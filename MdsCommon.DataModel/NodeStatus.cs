using System;

namespace MdsCommon
{
    public static class NodeStatus
    {
        public static SerializableHealthStatus ToSerializable(MachineStatus machineStatus)
        {
            if (machineStatus == null)
                return new SerializableHealthStatus();

            MdsCommon.SerializableHealthStatus serializableHealthStatus = new MdsCommon.SerializableHealthStatus()
            {
                Id = machineStatus.Id,
                HddAvailableMb = machineStatus.HddAvailableMb,
                HddTotalMb = machineStatus.HddTotalMb,
                MachineName = machineStatus.MachineName,
                NodeName = machineStatus.NodeName,
                RamAvailableMb = machineStatus.RamAvailableMb,
                RamTotalMb = machineStatus.RamTotalMb,
                TimestampUtc = machineStatus.TimestampUtc.ToString("O")
            };

            foreach (var serviceStatus in machineStatus.ServiceStatuses)
            {
                serializableHealthStatus.ServiceStatuses.Add(new MdsCommon.SerializableServiceStatus()
                {
                    Id = serviceStatus.Id,
                    MachineStatusId = serviceStatus.MachineStatusId,
                    Pid = serviceStatus.Pid,
                    ServiceName = serviceStatus.ServiceName,
                    StartTimeUtc = serviceStatus.StartTimeUtc.ToString("O"),
                    StatusTimestamp = serviceStatus.StatusTimestamp.ToString("O"),
                    UsedRamMb = serviceStatus.UsedRamMb
                });
            }

            return serializableHealthStatus;
        }

        public static MachineStatus FromSerializable(SerializableHealthStatus serHs)
        {
            MachineStatus machineStatus = new MachineStatus()
            {
                Id = serHs.Id,
                HddAvailableMb = serHs.HddAvailableMb,
                HddTotalMb = serHs.HddTotalMb,
                MachineName = serHs.MachineName,
                NodeName = serHs.NodeName,
                RamAvailableMb = serHs.RamAvailableMb,
                RamTotalMb = serHs.RamTotalMb,
                TimestampUtc = DateTime.Parse(serHs.TimestampUtc, null, System.Globalization.DateTimeStyles.RoundtripKind)
            };

            foreach (var serviceStatus in serHs.ServiceStatuses)
            {
                machineStatus.ServiceStatuses.Add(new ServiceStatus()
                {
                    Id = serviceStatus.Id,
                    MachineStatusId = serviceStatus.MachineStatusId,
                    UsedRamMb = serviceStatus.UsedRamMb,
                    Pid = serviceStatus.Pid,
                    ServiceName = serviceStatus.ServiceName,
                    StartTimeUtc = DateTime.Parse(serviceStatus.StartTimeUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
                    StatusTimestamp = DateTime.Parse(serviceStatus.StatusTimestamp, null, System.Globalization.DateTimeStyles.RoundtripKind)
                });
            }

            return machineStatus;
        }
    }
}