﻿using Metapsi;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MdsCommon;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Metapsi.Sqlite;
using static MdsLocal.MdsLocalApplication;

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

            var localReferences = await SetupLocalController(arguments, uiPort, start);

            var application = localReferences.ApplicationSetup.Revive();
            //h.State.WebApplication.Lifetime.ApplicationStopped.Register(async () =>
            //{
            //    Console.WriteLine("Stop triggered from web app");
            //    await application.Suspend();
            //});
            await application.SuspendComplete;
        }


        public static async Task<WebServer.References> SetupLocalController(InputArguments arguments, int uiPort, DateTime start)
        {
            Metapsi.Sqlite.Converters.RegisterAll();

            await Migrate.All(arguments.DbPath);


            var localReferences = MdsLocalApplication.Setup(arguments, start);
            var dbQueue = localReferences.SqliteQueue;

            WebServer.References webServer = null;

            if (uiPort != 0)
            {
                webServer = localReferences.ApplicationSetup.AddWebServer(
                    localReferences.ImplementationGroup,
                    uiPort);
            }
            else
            {
                webServer = localReferences.ApplicationSetup.AddWebServer(
                    localReferences.ImplementationGroup);

                localReferences.ApplicationSetup.MapEvent<MdsLocalApplication.Event.GlobalControllerReached>(
                    e =>
                    {
                        var _ = webServer.RunAsync(e.EventData.InfrastructureConfiguration.NodeUiPort);
                    });
            }

            //webServer.WebApplication.RegisterGetHandler<ListProcessesHandler, Overview.ListProcesses>();
            //webServer.WebApplication.RegisterGetHandler<SyncHistoryHandler, SyncHistory.List>();
            //webServer.WebApplication.RegisterGetHandler<MdsCommon.EventsLogHandler, MdsCommon.Routes.EventsLog.List>();

            webServer.WebApplication.MapGet(Metapsi.WebServer.Url<Overview.ListProcesses>(), async () => await ListProcessesHandler.Get(localReferences.SqliteQueue, localReferences.LocalControllerSettings));
            webServer.WebApplication.MapGet(Metapsi.WebServer.Url<SyncHistory.List>(), async () => await SyncHistoryHandler.Get(localReferences.SqliteQueue));
            webServer.WebApplication.MapGet(Metapsi.WebServer.Url<MdsCommon.Routes.EventsLog.List>(), async (HttpContext httpContext) => await MdsCommon.EventsLogHandler.Get(httpContext, localReferences.SqliteQueue));

            webServer.WebApplication.UseRenderer<OverviewPage>(RenderOverviewListProcesses.Render);
            webServer.WebApplication.UseRenderer<ListInfrastructureEventsPage>(RenderInfrastructureEventsList.Render);
            webServer.WebApplication.UseRenderer<SyncHistory.DataModel>(RenderSyncHistory.Render);

            webServer.WebApplication.MapGet("/", () => Results.Redirect(WebServer.Url<Overview.ListProcesses>())).AllowAnonymous().ExcludeFromDescription();
            webServer.WebApplication.MapGroup("event").MapIncomingEvents(
                localReferences.LocalControllerSettings,
                localReferences.SqliteQueue,
                localReferences.OsProcessTracker,
                localReferences.GlobalNotifier);

            await MdsLocalApplication.NotifyStartStatus(
                localReferences.SqliteQueue,
                localReferences.LocalControllerSettings,
                localReferences.GlobalNotifier,
                localReferences.OsProcessTracker);

            var api = webServer.WebApplication.MapGroup("api");
            api.MapGetCommand(Frontend.KillProcessByPid, async (CommandContext commandContext, HttpContext httpContext, string pid) =>
            {
                var intPid = Convert.ToInt32(pid);
                var process = System.Diagnostics.Process.GetProcessById(intPid);
                process.Kill();
                process.WaitForExit(5000);
                await Task.Delay(5000);
            });

            api.MapGetRequest(Frontend.ReloadProcesses, async (CommandContext commandContext, HttpContext httpContext) =>
            {
                var reloadedModel = await ListProcessesHandler.Load(localReferences.SqliteQueue, localReferences.LocalControllerSettings);
                return new ReloadedOverviewModel()
                {
                    Model = reloadedModel,
                };
            });

            api.MapGetRequest(Frontend.LoadFullSyncResult, async (CommandContext commandContext, HttpContext httpContext, Guid syncResultId) =>
            {

                var fullSyncResult = await LocalDb.LoadFullSyncResult(dbQueue, syncResultId);

                return new FullSyncResultResponse()
                {
                    SyncResult = fullSyncResult
                };

            });

            await StaticFiles.AddAll(typeof(Metapsi.Html.Binding).Assembly);
            await StaticFiles.AddAll(typeof(MdsCommon.Controls.Controls).Assembly);
            await StaticFiles.AddAll(typeof(MdsCommon.Render).Assembly);
            await StaticFiles.AddAll(typeof(Program).Assembly);

            return webServer;
        }
    }
}
