using Metapsi;

namespace MdsCommon
{
    public partial class ServiceStatus : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.String ServiceName { get; set; } = System.String.Empty;
        public System.Int32 Pid { get; set; }
        public System.DateTime StartTimeUtc { get; set; }
        public System.Int32 UsedRamMb { get; set; }
        public System.Guid MachineStatusId { get; set; }
        public System.DateTime StatusTimestamp { get; set; }

    }
}