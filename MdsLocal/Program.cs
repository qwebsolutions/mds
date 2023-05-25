using Metapsi;
using Metapsi.Hyperapp;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

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

#if DEBUG
            WebServer.WebRootPaths.Add("D:\\qweb\\mes\\Mds\\MdsInfrastructure\\inline");
            WebServer.WebRootPaths.Add("D:\\qweb\\mes\\Metapsi.Hyperapp\\inline");
#endif

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

            var h = webServer.AddHyperapp(Overview.ListProcesses);
            h.RegisterModule(typeof(SyncHistory));
            h.RegisterModule(typeof(EventsLog));

            var application = localReferences.ApplicationSetup.Revive();
            h.State.WebApplication.Lifetime.ApplicationStopped.Register(async () =>
            {
                Console.WriteLine("Stop triggered from web app");
                await application.Suspend();
            });
            await application.SuspendComplete;
        }
    }
}
