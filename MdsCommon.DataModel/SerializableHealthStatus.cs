using System.Collections.Generic;

namespace MdsCommon
{
    public class SerializableHealthStatus
    {
        public System.Guid Id { get; set; }
        public System.Int32 HddTotalMb { get; set; }
        public System.Int32 HddAvailableMb { get; set; }
        public System.Int32 RamTotalMb { get; set; }
        public System.Int32 RamAvailableMb { get; set; }
        public System.String MachineName { get; set; } = System.String.Empty;
        public System.String NodeName { get; set; } = System.String.Empty;
        public System.String TimestampUtc { get; set; }

        public List<SerializableServiceStatus> ServiceStatuses { get; set; } = new List<SerializableServiceStatus>();
    }

}
