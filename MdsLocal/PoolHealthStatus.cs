using MdsCommon;
using Metapsi;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MdsLocal
{
    static partial class MdsLocalApplication
    {
        private class HealthTick : IData
        {

        }

        public static void PoolHealthStatus(this ApplicationSetup applicationSetup, ImplementationGroup ig, string nodeName)
        {
            var timer = applicationSetup.AddBusinessState(new System.Timers.Timer(TimeSpan.FromMinutes(1)));

            applicationSetup.MapEvent<ApplicationRevived>(e =>
            {
                e.Using(timer, ig).EnqueueCommand(async (cc, state) =>
                {
                    timer.Start();
                    timer.Elapsed += (s, e) =>
                    {
                        cc.PostEvent(new HealthTick());
                    };
                });
            });

            applicationSetup.MapEvent<HealthTick>(e =>
            {
                e.Using(timer, ig).EnqueueCommand(async (cc, state) =>
                {
                    var healthStatus = await GetNodeStatus(cc, nodeName);
                    cc.NotifyGlobal(healthStatus);
                });
            });
        }

        public static async Task<MachineStatus> GetNodeStatus(CommandContext commandContext, string nodeName)
        {
            string drivePath = string.Empty;

            string entryLocation = System.Reflection.Assembly.GetEntryAssembly().Location;

            switch (ServiceProcessExtensions.GetOs())
            {
                case Os.Windows:
                    {
                        drivePath = entryLocation.First().ToString();
                    }
                    break;
                case Os.Linux:
                    {
                        // StackOverflow says any path should work 
                        drivePath = entryLocation;
                    }
                    break;
            }
            var driveInfo = new System.IO.DriveInfo(drivePath);

            var machineStatus = new MdsCommon.MachineStatus()
            {
                HddAvailableMb = (int)(driveInfo.AvailableFreeSpace / (1024 * 1024)),
                HddTotalMb = (int)(driveInfo.TotalSize / (1024 * 1024)),
                MachineName = Environment.MachineName,
                NodeName = nodeName,
                TimestampUtc = DateTime.UtcNow
            };

            MemoryMetrics memoryMetrics = new MemoryMetricsClient().GetMetrics();
            machineStatus.RamTotalMb = (int)memoryMetrics.Total;
            machineStatus.RamAvailableMb = (int)memoryMetrics.Free;

            var allServiceProcesses = ServiceProcessExtensions.IdentifyOwnedProcesses(nodeName);

            foreach (var serviceProcess in allServiceProcesses)
            {
                try
                {
                    machineStatus.ServiceStatuses.Add(new MdsCommon.ServiceStatus()
                    {
                        MachineStatusId = machineStatus.Id,
                        Pid = serviceProcess.Id,
                        ServiceName = ServiceProcessExtensions.GuessServiceName(nodeName, serviceProcess.ProcessName),
                        StartTimeUtc = serviceProcess.StartTime.ToUniversalTime(),
                        UsedRamMb = (int)(serviceProcess.WorkingSet64 / (1024 * 1024)),
                        StatusTimestamp = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }

            return machineStatus;
        }
    }
}
