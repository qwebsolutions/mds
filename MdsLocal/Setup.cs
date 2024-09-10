using MdsCommon;
using Metapsi;
using Metapsi.Sqlite;
using System;
using System.Collections.Generic;
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
            public SqliteQueue SqliteQueue { get; set; }
        }

        public static References Setup(InputArguments arguments, DateTime start)
        {

            string infrastructureUrl = arguments.InfrastructureApiUrl;
            string nodeName = arguments.NodeName;
            string fullDbPath = Metapsi.RelativePath.SearchUpfolder(RelativePath.From.EntryPath, arguments.DbPath);
            SqliteQueue sqliteQueue = new SqliteQueue(fullDbPath);

            Mds.LogToServiceText(arguments.LogFilePath, start, new LogMessage()
            {
                Message = new Metapsi.Log.Info($"MdsLocal: using {fullDbPath}")
            }).Wait();

            // This is initially empty, is filled at startup when the data is retrieved
            MdsCommon.InfrastructureNodeSettings infrastructureConfiguration = new MdsCommon.InfrastructureNodeSettings();

            ApplicationSetup applicationSetup = ApplicationSetup.New();

            ImplementationGroup implementationGroup = applicationSetup.AddImplementationGroup();
            var implementations = implementationGroup;

            var localAppState = applicationSetup.AddBusinessState(new MdsLocalApplication.State()
            {
                BaseDataFolder = arguments.ServicesDataPath,
                NodeName= arguments.NodeName,
                ServicesBasePath = arguments.ServicesBasePath,
            });

            var messagingHandler = applicationSetup.SetupMessagingApi(implementationGroup);
            applicationSetup.MapEvent<ApplicationRevived>(e =>
            {
                e.Using(messagingHandler, implementationGroup).EnqueueCommand(async (cc, state) =>
                {
                    var deploymentEventsUrl = arguments.InfrastructureApiUrl.Trim('/') + "/event";
                    cc.MapMessaging("global", deploymentEventsUrl);
                });
            });

            applicationSetup.PoolHealthStatus(implementationGroup, nodeName);
            applicationSetup.PoolServiceLog(implementationGroup, localAppState, sqliteQueue);

            applicationSetup.MapInternalEvents(
                implementationGroup,
                localAppState,
                sqliteQueue,
                arguments.BuildTarget);

            implementationGroup.MapRequest(MdsLocalApplication.GetFullLocalStatus, async (rc) =>
            {
                return await LocalDb.LoadFullLocalStatus(sqliteQueue, nodeName);
            });

            implementationGroup.MapRequest(MdsLocalApplication.GetSyncHistory, async (rc) =>
            {
                return await LocalDb.LoadSyncHistory(sqliteQueue);
            });

            implementationGroup.MapRequest(MdsLocalApplication.GetRunningProcesses, async (rc) =>
            {
                List<RunningServiceProcess> processes = new();

                foreach (var osProcess in System.Diagnostics.Process.GetProcesses())
                {
                    if (osProcess.ProcessName.StartsWith(ServiceProcessExtensions.ExePrefix(nodeName)))
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
                                    ServiceName = ServiceProcessExtensions.GuessServiceName(nodeName, exePath),
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
                return await MdsCommon.Db.LoadAllInfrastructureEvents(sqliteQueue);
            });

            //    implementationGroup.MapCommand(MdsLocalApplication.StoreSyncResult, async (rc, syncResult) =>
            //    {
            //        await LocalDb.RegisterSyncResult(fullDbPath, syncResult);
            //    });

            //    implementations.MapCommand(MdsLocalApplication.StopProcess, async (rc, runningProcess) =>
            //    {
            //        await Mds.WriteCommand(Mds.GetServiceCommandDbFile(arguments.ServicesDataPath, runningProcess.ServiceName), new Mds.Shutdown() { });

            //        var process = System.Diagnostics.Process.GetProcessById(runningProcess.Pid);
            //        var exited = process.WaitForExit(15000);

            //        if (!exited)
            //        {
            //            process.Kill();
            //            exited = process.WaitForExit(5000);
            //        }
            //    });

            //    #endregion Implementation mappings

            return new References()
            {
                ApplicationSetup = applicationSetup,
                ImplementationGroup = implementationGroup,
                SqliteQueue = sqliteQueue
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
