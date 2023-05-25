using Dapper;
using Metapsi;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace MdsLocal
{
    public static partial class MdsLocalApplication
    {
        //public static Command<MdsCommon.InfrastructureEvent> BroadcastEvent { get; set; } = new Command<MdsCommon.InfrastructureEvent>(nameof(BroadcastEvent));
        public static Request<List<MdsCommon.ServiceConfigurationSnapshot>> GetLocalKnownConfiguration { get; set; } = new(nameof(GetLocalKnownConfiguration));
        public static Command<List<MdsCommon.ServiceConfigurationSnapshot>> OverwriteLocalConfiguration { get; set; } = new(nameof(OverwriteLocalConfiguration));
        public static Request<List<string>> PerformStartupValidations { get; set; } = new Request<List<string>>(nameof(PerformStartupValidations));
        public static Request<List<string>> GetWarnings { get; set; } = new Request<List<string>>(nameof(GetWarnings));
        public static Request<LocalSettings> GetLocalSettings { get; set; } = new Request<LocalSettings>(nameof(GetLocalSettings));
        public static Request<FullLocalStatus> GetFullLocalStatus { get; set; } = new Request<FullLocalStatus>(nameof(GetFullLocalStatus));
        public static Request<IEnumerable<SyncResult>> GetSyncHistory { get; set; } = new Request<IEnumerable<SyncResult>>(nameof(GetSyncHistory));
        public static Request<RecordCollection<RunningServiceProcess>> GetRunningProcesses { get; set; } = new Request<RecordCollection<RunningServiceProcess>>(nameof(GetRunningProcesses));
        public static Command<SyncResult> StoreSyncResult { get; set; } = new Command<SyncResult>(nameof(StoreSyncResult));
        public static Request<ProjectBinary, /*project name*/ string, /*project version*/string, /*into path*/ string> GetProjectBinaries { get; set; } = new Request<ProjectBinary, string, string, string>(nameof(GetProjectBinaries));
        public static Command<RunningServiceProcess> StopProcess { get; set; } = new Command<RunningServiceProcess>(nameof(StopProcess));

        public class State
        {
            public Dictionary<int, System.Diagnostics.Process> LiveProcesses { get; set; } = new Dictionary<int, System.Diagnostics.Process>();

            public int DropAtCrashesPerMinute { get; set; } = 5;

            // The path into which to copy project binaries & rename the exe
            public string ServicesBasePath { get; set; }

            public string BaseDataFolder { get; set; }

            public MdsCommon.InfrastructureNodeSettings InfrastructureConfiguration { get; set; }

            //public string InfrastructureName { get; set; } = string.Empty;
            //public string InfrastructureRedis { get; set; } = string.Empty;
            //public string BinariesApiUrl { get; set; } = string.Empty;
            public string NodeName { get; set; } = string.Empty;
            public bool RestartProcessAfterCrash { get; set; }
            public List<string> StartupWarnings = new List<string>();
            public List<ServiceCrashEvent> ServiceCrashEvents = new List<ServiceCrashEvent>();
            public HashSet<string> DroppedServices { get; set; } = new HashSet<string>();
        }

        public class LogEntry
        {
            public int Id { get; set; }
            public string ProcessStartTimestamp { get; set; }
            public string LogTimestamp { get; set; }
            public string LogMessageType { get; set; }
            public string LogMessage { get; set; }
            public string CallStack { get; set; }
            public int Processed { get; set; }
        }


        

        public static async Task SendCommand(CommandContext commandContext, State state, string serviceName, string command)
        {
            switch (command)
            {
                case MdsCommon.NodeCommand.StopService:
                    commandContext.Logger.LogInfo($"Attempting to restart service {serviceName}");
                    var serviceProcesses = await commandContext.Do(GetRunningProcesses);
                    var runningService = serviceProcesses.SingleOrDefault(x => x.ServiceName == serviceName);

                    if (runningService == null)
                    {
                        commandContext.Logger.LogError($"Cannot restart! Service {serviceName} is not running!");
                        return;
                    }

                    await commandContext.Do(StopProcess, runningService);
                    break;
            }
        }
    }
}
