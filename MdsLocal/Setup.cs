using Dapper;
using Metapsi;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MdsLocal
{

    public static partial class MdsLocalApplication
    {
        //public static string DebugFile = System.IO.Path.Combine(Metapsi.RelativePath.SearchUpfolder(RelativePath.From.EntryPath, "mds"), "mdslocal.log");

        public class References
        {
            public ApplicationSetup ApplicationSetup { get; set; }
            public ImplementationGroup ImplementationGroup { get; set; }
        }

        public static References Setup(InputArguments arguments, DateTime start)
        {

            string infrastructureUrl = arguments.InfrastructureApiUrl;
            string nodeName = arguments.NodeName;
            string fullDbPath = Metapsi.RelativePath.SearchUpfolder(RelativePath.From.EntryPath, arguments.DbPath);

            Mds.LogToServiceText(arguments.LogFilePath, start, new LogMessage()
            {
                Message = new Metapsi.Log.Info($"MdsLocal: using {fullDbPath}")
            }).Wait();

            // This is initially empty, is filled at startup when the data is retrieved
            MdsCommon.InfrastructureNodeSettings infrastructureConfiguration = new MdsCommon.InfrastructureNodeSettings();

            #region Application setup

            ApplicationSetup applicationSetup = ApplicationSetup.New();

            var infraEventNotifier = applicationSetup.AddBusinessState(new RedisNotifier.State());

            applicationSetup.OnLogMessage = async (logMessage) =>
            {
                System.Console.Error.Write(logMessage);

                await Mds.LogToServiceText(arguments.LogFilePath, start, logMessage);

                MdsCommon.InfrastructureEvent infrastructureEvent = null;

                switch (logMessage.Message)
                {
                    case Metapsi.Log.Exception ex:
                        {
                            infrastructureEvent = new MdsCommon.InfrastructureEvent()
                            {
                                Type = "Error",
                                Criticality = "Error",
                                Source = arguments.NodeName,
                                FullDescription = logMessage.ToString(),
                                ShortDescription = "Internal error",
                                Timestamp = System.DateTime.UtcNow
                            };
                        }
                        break;
                    case Metapsi.Log.Error error:
                        {
                            infrastructureEvent = new MdsCommon.InfrastructureEvent()
                            {
                                Type = "Error",
                                Criticality = "Error",
                                Source = arguments.NodeName,
                                FullDescription = logMessage.ToString(),
                                ShortDescription = "Internal error",
                                Timestamp = System.DateTime.UtcNow
                            };
                        }
                        break;
                }

                if (infrastructureEvent != null)
                {
                    await MdsCommon.Db.SaveInfrastructureEvent(fullDbPath, infrastructureEvent);

                    if (!string.IsNullOrEmpty(infrastructureConfiguration.InfrastructureEventsOutputChannel))
                    {
                        await RedisNotifier.RaiseNotification(
                            infraEventNotifier,
                            new RedisChannelMessage(
                                infrastructureConfiguration.InfrastructureEventsOutputChannel,
                                infrastructureEvent.GetType().Name,
                                Serialize.ToJson(infrastructureEvent)));
                    }
                }
            };

            ImplementationGroup implementationGroup = applicationSetup.AddImplementationGroup();
            var implementations = implementationGroup;

            #endregion Application setup

            var eventBroadcaster = SetupEventBroadcaster(
                applicationSetup, 
                implementationGroup, 
                fullDbPath, 
                infraEventNotifier,
                infrastructureConfiguration);

            var mdsLocalApplication = SetupLocalApp(
                applicationSetup, 
                implementationGroup, 
                infrastructureConfiguration, 
                eventBroadcaster, 
                arguments.ServicesBasePath, 
                arguments.NodeName,
                arguments.ServicesDataPath,
                start);

            SetupDeployment(
                applicationSetup,
                implementationGroup,
                infrastructureConfiguration,
                mdsLocalApplication,
                eventBroadcaster,
                arguments.BinariesRepositoryFolder,
                arguments.BuildTarget);

            SetupHealth(applicationSetup, implementationGroup, infrastructureConfiguration, mdsLocalApplication, arguments.NodeName);

            List<string> warnings = new List<string>();


            #region Implementation mappings

            System.Net.Http.HttpClient apiClient = new System.Net.Http.HttpClient();
            apiClient.BaseAddress = new Uri(infrastructureUrl);

            implementationGroup.MapRequest(Api.GetInfrastructureNodeSettings, async (rc) =>
            {
                return await apiClient.GetFromJsonAsync<MdsCommon.InfrastructureNodeSettings>(
                    $"{MdsCommon.Api.GetInfrastructureNodeSettings.Name}/{arguments.NodeName}");
            });

            implementationGroup.MapRequest(Api.GetUpToDateConfiguration, async (rc) =>
            {
                return await apiClient.GetFromJsonAsync<List<MdsCommon.ServiceConfigurationSnapshot>>(
                    $"{MdsCommon.Api.GetCurrentNodeSnapshot.Name}/{arguments.NodeName}");
            });

            implementationGroup.MapRequest(MdsLocalApplication.GetLocalKnownConfiguration, async (rc) =>
            {
                return await LocalDb.LoadKnownConfiguration(fullDbPath);
            });

            implementationGroup.MapCommand(MdsLocalApplication.OverwriteLocalConfiguration, async (rc, snapshot) =>
            {
                await LocalDb.SetNewConfiguration(fullDbPath, snapshot);
            });

            implementationGroup.MapRequest(MdsLocalApplication.PerformStartupValidations, async (rc) =>
            {
                var fieldsDiff = await LocalDb.ValidateSchema(fullDbPath);
                warnings = FieldsDiffAsWarnings(fieldsDiff);
                return warnings;
            });

            implementationGroup.MapRequest(MdsLocalApplication.GetWarnings, async (rc) => warnings);

            implementationGroup.MapRequest(MdsLocalApplication.GetLocalSettings,
                async (rc) => new LocalSettings()
                {
                    FullDbPath = fullDbPath,
                    InfrastructureApiUrl = infrastructureUrl,
                    NodeName = nodeName
                });

            implementationGroup.MapRequest(MdsLocalApplication.GetFullLocalStatus, async (rc) =>
            {
                return await LocalDb.LoadFullLocalStatus(fullDbPath, nodeName);
            });

            implementationGroup.MapRequest(MdsLocalApplication.GetSyncHistory, async (rc) =>
            {
                return await LocalDb.LoadSyncHistory(fullDbPath);
            });

            implementationGroup.MapRequest(MdsLocalApplication.GetRunningProcesses, async (rc) =>
            {
                List<RunningServiceProcess> processes = new();

                foreach (var osProcess in System.Diagnostics.Process.GetProcesses())
                {
                    if (osProcess.ProcessName.StartsWith(MdsLocalApplication.ExePrefix(nodeName)))
                    {
                        var maxRetries = 5;
                        int retryCount = 0;
                        while (true)
                        {
                            try
                            {
                                string exePath = osProcess.MainModule.FileName;

                                processes.Add(new RunningServiceProcess()
                                {
                                    FullExePath = exePath,
                                    ServiceName = MdsLocalApplication.GuessServiceName(nodeName, exePath),
                                    Pid = osProcess.Id,
                                    StartTimestampUtc = osProcess.StartTime.ToUniversalTime(),
                                    UsedRamMB = (int)(osProcess.WorkingSet64 / (1024 * 1024))
                                });
                                break;
                            }
                            catch (Exception ex)
                            {
                                retryCount++;
                                if (retryCount >= maxRetries)
                                {
                                    rc.Logger.LogException(ex);
                                    break;
                                }
                                else
                                {
                                    await Task.Delay(1000);
                                }
                            }
                        }
                    }
                }

                return processes;
            });

            implementationGroup.MapRequest(MdsCommon.Api.GetAllInfrastructureEvents, async (rc) =>
            {
                return await MdsCommon.Db.LoadAllInfrastructureEvents(fullDbPath);
            });

            implementationGroup.MapCommand(MdsLocalApplication.StoreSyncResult, async (rc, syncResult) =>
            {
                await LocalDb.RegisterSyncResult(fullDbPath, syncResult);
            });

            implementations.MapCommand(MdsLocalApplication.StopProcess, async (rc, runningProcess) =>
            {
                await Mds.WriteCommand(Mds.GetServiceCommandDbFile(arguments.ServicesDataPath, runningProcess.ServiceName), new Mds.Shutdown() { });

                var process = System.Diagnostics.Process.GetProcessById(runningProcess.Pid);
                var exited = process.WaitForExit(15000);

                if (!exited)
                {
                    process.Kill();
                    exited = process.WaitForExit(5000);
                }
            });

            #endregion Implementation mappings

            return new References()
            {
                ApplicationSetup = applicationSetup,
                ImplementationGroup = implementationGroup
            };
        }

        private static List<string> FieldsDiffAsWarnings(Metapsi.Sqlite.Validate.FieldsDiff fieldsDiff)
        {
            List<string> warnings = new List<string>();

            if (!fieldsDiff.SameFields)
            {
                if (fieldsDiff.MissingFields.Any())
                {
                    var missingFields = $" Missing fields: {string.Join(", ", fieldsDiff.MissingFields)}";
                    warnings.Add(missingFields);
                }

                if (fieldsDiff.ExtraFields.Any())
                {
                    var extraFields = $" Extra fields: {string.Join(", ", fieldsDiff.ExtraFields)}";
                    warnings.Add(extraFields);
                }
            }

            return warnings;
        }
    }
}
