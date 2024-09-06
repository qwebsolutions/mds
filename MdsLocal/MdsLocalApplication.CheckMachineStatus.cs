using Metapsi;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MdsLocal
{
    static partial class MdsLocalApplication
    {
        public static async Task CheckMachineStatus(CommandContext commandContext, State state)
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
                NodeName = state.NodeName,
                TimestampUtc = DateTime.UtcNow
            };

            MemoryMetrics memoryMetrics = new MemoryMetricsClient().GetMetrics();
            machineStatus.RamTotalMb = (int)memoryMetrics.Total;
            machineStatus.RamAvailableMb = (int)memoryMetrics.Free;

            var allServiceProcesses = await commandContext.Do(GetRunningProcesses);

            foreach (var serviceProcess in allServiceProcesses)
            {
                machineStatus.ServiceStatuses.Add(new MdsCommon.ServiceStatus()
                {
                    MachineStatusId = machineStatus.Id,
                    Pid = serviceProcess.Pid,
                    ServiceName = serviceProcess.ServiceName,
                    StartTimeUtc = serviceProcess.StartTimestampUtc,
                    UsedRamMb = serviceProcess.UsedRamMB,
                    StatusTimestamp = DateTime.UtcNow
                });
            }

            Event.HealthPing healthPing = new Event.HealthPing() { HealthStatus = machineStatus };

            commandContext.PostEvent(healthPing);
        }
    }
}
