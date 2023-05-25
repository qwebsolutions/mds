namespace MdsCommon
{
    public partial class SerializableServiceStatus
    {
        public System.Guid Id { get; set; }
        public System.String ServiceName { get; set; } = System.String.Empty;
        public System.Int32 Pid { get; set; }
        public System.String StartTimeUtc { get; set; }
        public System.Int32 UsedRamMb { get; set; }
        public System.Guid MachineStatusId { get; set; }
        public System.String StatusTimestamp { get; set; }
    }

}
