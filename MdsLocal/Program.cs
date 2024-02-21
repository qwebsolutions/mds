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

namespace MdsLocal
{
    class Program
    {
        static async Task Main(string[] args)
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

            Metapsi.Sqlite.IdConverter.Register();

            var localReferences = MdsLocalApplication.Setup(arguments, start);

            WebServer.References webServer = null;

            if (parameters.ContainsKey("UiPort"))
            {
                webServer = localReferences.ApplicationSetup.AddWebServer(
                    localReferences.ImplementationGroup,
                    int.Parse(parameters["UiPort"]),
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
            webServer.RegisterStaticFiles(typeof(MdsLocal.RenderSyncHistory).Assembly);
            webServer.RegisterStaticFiles(typeof(MdsCommon.Render).Assembly);
            webServer.RegisterStaticFiles(typeof(MdsCommon.Controls.Controls).Assembly);
            webServer.RegisterStaticFiles(typeof(SyntaxBuilder).Assembly);
            webServer.RegisterStaticFiles(typeof(HyperNode).Assembly);
            webServer.RegisterStaticFiles(typeof(Metapsi.Dom.DomElement).Assembly);

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

            var application = localReferences.ApplicationSetup.Revive();
            //h.State.WebApplication.Lifetime.ApplicationStopped.Register(async () =>
            //{
            //    Console.WriteLine("Stop triggered from web app");
            //    await application.Suspend();
            //});
            await application.SuspendComplete;
        }
    }
}
