using Metapsi;
using Metapsi.Hyperapp;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Metapsi.Syntax;
using MdsCommon;
using static Metapsi.Hyperapp.HyperType;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Metapsi.Ui;
using Dapper;
using Metapsi.Sqlite;

namespace MdsLocal
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            DateTime start = DateTime.UtcNow;

            if (args.Length < 1)
            {
                throw new Exception("Provide input configuration file!");
            }

            string inputFileName = args[0];

            var inputFullPath = Mds.GetParametersFilePath(inputFileName);
            Console.WriteLine($"Using parameters file {inputFullPath}");
            var parameters = Mds.LoadParameters(inputFullPath);
            Console.WriteLine(Serialize.ToJson(parameters));
            string inputFileFolder = System.IO.Path.GetDirectoryName(inputFullPath);

            MdsCommon.PathParameter.SetRelativeToFolder(inputFileFolder, parameters, nameof(InputArguments.DbPath));
            MdsCommon.PathParameter.SetRelativeToFolder(inputFileFolder, parameters, nameof(InputArguments.LogFilePath));
            MdsCommon.PathParameter.SetRelativeToFolder(inputFileFolder, parameters, nameof(InputArguments.WebRootPath));
            MdsCommon.PathParameter.SetRelativeToFolder(inputFileFolder, parameters, nameof(InputArguments.BinariesRepositoryFolder));
            MdsCommon.PathParameter.SetRelativeToFolder(inputFileFolder, parameters, nameof(InputArguments.ServicesBasePath));
            MdsCommon.PathParameter.SetRelativeToFolder(inputFileFolder, parameters, nameof(InputArguments.ServicesDataPath));

            await Mds.ValidateMissingParameters(parameters, new List<string>()
            {
                nameof(InputArguments.BinariesRepositoryFolder),
                nameof(InputArguments.BuildTarget),
                nameof(InputArguments.DbPath),
                nameof(InputArguments.InfrastructureApiUrl),
                nameof(InputArguments.NodeName),
                nameof(InputArguments.ServicesBasePath),
                nameof(InputArguments.WebRootPath),
                nameof(InputArguments.ServicesDataPath)
            },
            async (logMessage) =>
            {
                Console.Error.WriteLine(logMessage);

                if (parameters.ContainsKey(nameof(InputArguments.LogFilePath)))
                {
                    await Mds.LogToServiceText(parameters[nameof(InputArguments.LogFilePath)], start, logMessage);
                }
            });

            InputArguments arguments = Mds.ParametersAs<InputArguments>(parameters);

            int uiPort = 0;

            if (parameters.ContainsKey("UiPort"))
            {
                uiPort = int.Parse(parameters["UiPort"]);
            }

            await ReplaceOldDateTimeFormat(arguments.DbPath);

            var localReferences = await SetupLocalController(arguments, uiPort, start);

            var application = localReferences.ApplicationSetup.Revive();
            //h.State.WebApplication.Lifetime.ApplicationStopped.Register(async () =>
            //{
            //    Console.WriteLine("Stop triggered from web app");
            //    await application.Suspend();
            //});
            await application.SuspendComplete;
        }

        public static async Task ReplaceOldDateTimeFormat(string fullDbPath)
        {
            var replaceSql = async (
                OpenTransaction t,
                string tableName,
                string dateTimeField) =>
            {
                var allRecords = await t.Connection.QueryAsync<(string Id, string Timestamp)>($"select Id, {dateTimeField} from {tableName}", transaction: t.Transaction);
                foreach (var record in allRecords)
                {
                    // Do not update if format is already correct
                    if (record.Timestamp.Contains(" "))
                    {
                        var snapshotTimestamp = DateTime.Parse(record.Timestamp, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
                        snapshotTimestamp = DateTime.SpecifyKind(snapshotTimestamp, DateTimeKind.Utc);
                        await t.Connection.ExecuteAsync(
                            $"update {tableName} set {dateTimeField}=@timestamp where Id=@Id",
                            new { 
                                Id = record.Id,
                                timestamp = snapshotTimestamp.ToString("O", System.Globalization.CultureInfo.InvariantCulture) },
                            transaction: t.Transaction);
                    }
                }
            };

            await Metapsi.Sqlite.Db.WithCommit(fullDbPath,
                async t =>
                {
                    await replaceSql(t, nameof(InfrastructureEvent), nameof(InfrastructureEvent.Timestamp));
                    await replaceSql(t, nameof(ServiceConfigurationSnapshot), nameof(ServiceConfigurationSnapshot.SnapshotTimestamp));
                    await replaceSql(t, nameof(SyncResult), nameof(SyncResult.Timestamp));
                });
        }

        public static async Task<WebServer.References> SetupLocalController(InputArguments arguments, int uiPort, DateTime start)
        {
            Metapsi.Sqlite.Converters.RegisterAll();

            var localReferences = MdsLocalApplication.Setup(arguments, start);

            WebServer.References webServer = null;

            if (uiPort != 0)
            {
                webServer = localReferences.ApplicationSetup.AddWebServer(
                    localReferences.ImplementationGroup,
                    uiPort,
                    arguments.WebRootPath);
            }
            else
            {
                webServer = localReferences.ApplicationSetup.AddWebServer(
                    localReferences.ImplementationGroup,
                    webRootPath: arguments.WebRootPath);

                localReferences.ApplicationSetup.MapEvent<MdsLocalApplication.Event.GlobalControllerReached>(
                    e =>
                    {
                        var _ = webServer.RunAsync(e.EventData.InfrastructureConfiguration.NodeUiPort);
                    });
            }

            webServer.WebApplication.RegisterGetHandler<ListProcessesHandler, Overview.ListProcesses>();
            webServer.WebApplication.RegisterGetHandler<SyncHistoryHandler, SyncHistory.List>();
            webServer.WebApplication.RegisterGetHandler<MdsCommon.EventsLogHandler, MdsCommon.Routes.EventsLog.List>();
            webServer.RegisterPageBuilder<OverviewPage>(new RenderOverviewListProcesses().Render);
            webServer.RegisterPageBuilder<ListInfrastructureEventsPage>(new RenderInfrastructureEventsList().Render);
            webServer.RegisterPageBuilder<SyncHistory.DataModel>(new RenderSyncHistory().Render);
            webServer.RegisterStaticFiles(typeof(MdsCommon.Render).Assembly);
            webServer.RegisterStaticFiles(typeof(MdsCommon.Controls.Controls).Assembly);
            webServer.RegisterStaticFiles(typeof(SyntaxBuilder).Assembly);
            webServer.RegisterStaticFiles(typeof(Metapsi.Hyperapp.IVNode).Assembly);
            webServer.RegisterStaticFiles(typeof(Metapsi.Dom.DomElement).Assembly);
            webServer.RegisterStaticFiles(typeof(MdsLocal.RenderSyncHistory).Assembly);

            webServer.WebApplication.MapGet("/", () => Results.Redirect(WebServer.Url<Overview.ListProcesses>())).AllowAnonymous().ExcludeFromDescription();

            var api = webServer.WebApplication.MapGroup("api");
            api.MapRequest(Frontend.KillProcessByPid, async (CommandContext commandContext, HttpContext httpContext, string pid) =>
            {
                try
                {
                    var intPid = Convert.ToInt32(pid);
                    var process = System.Diagnostics.Process.GetProcessById(intPid);
                    process.Kill();
                    process.WaitForExit(5000);
                    await Task.Delay(5000);

                    return new ApiResponse();
                }
                catch (Exception ex)
                {
                    return new ApiResponse()
                    {
                        ErrorMessage = ex.Message,
                        ResultCode = ApiResultCode.Error
                    };
                }
            }, WebServer.Authorization.Public);

            api.MapRequest(Frontend.ReloadProcesses, async (CommandContext commandContext, HttpContext httpContext) =>
            {
                var reloadedModel = await ListProcessesHandler.Load(commandContext, httpContext);
                return new ReloadedOverviewModel()
                {
                    Model = reloadedModel,
                };
            }, WebServer.Authorization.Public);

            return webServer;
        }
    }
}
