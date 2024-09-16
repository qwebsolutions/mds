using Metapsi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsLocal
{
    public static partial class MdsLocalApplication
    {
        public static Request<LocalSettings> GetLocalSettings { get; set; } = new Request<LocalSettings>(nameof(GetLocalSettings));
        public static Request<FullLocalStatus> GetFullLocalStatus { get; set; } = new Request<FullLocalStatus>(nameof(GetFullLocalStatus));
        public static Request<IEnumerable<SyncResult>> GetSyncHistory { get; set; } = new Request<IEnumerable<SyncResult>>(nameof(GetSyncHistory));
        public static Request<List<RunningServiceProcess>> GetRunningProcesses { get; set; } = new Request<List<RunningServiceProcess>>(nameof(GetRunningProcesses));
        //public static Command<SyncResult> StoreSyncResult { get; set; } = new Command<SyncResult>(nameof(StoreSyncResult));
        //public static Command<RunningServiceProcess> StopProcess { get; set; } = new Command<RunningServiceProcess>(nameof(StopProcess));

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
            //public List<ServiceCrashEvent> ServiceCrashEvents = new List<ServiceCrashEvent>();
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
    }
}
