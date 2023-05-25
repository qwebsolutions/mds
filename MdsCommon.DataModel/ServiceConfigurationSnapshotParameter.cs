using Metapsi;

namespace MdsCommon
{
    public partial class ServiceConfigurationSnapshotParameter : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.Guid ServiceConfigurationSnapshotId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public System.Guid ParameterTypeId { get; set; }
        public string ConfiguredValue { get; set; } = string.Empty;
        public string DeployedValue { get; set; } = string.Empty;
    }
}