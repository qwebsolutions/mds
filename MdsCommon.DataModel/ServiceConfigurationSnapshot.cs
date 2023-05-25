using Metapsi;

namespace MdsCommon
{
    public partial class ServiceConfigurationSnapshot : IRecord
    {
        // Record

        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public string ServiceName { get; set; } = string.Empty;
        public System.DateTime SnapshotTimestamp { get; set; }
        public string ApplicationName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectVersionTag { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;

        // Data structure

        public System.Collections.Generic.List<ServiceConfigurationSnapshotParameter> ServiceConfigurationSnapshotParameters { get; set; } = new();

        public static UptreeRelation<ServiceConfigurationSnapshot>
            Data = Relation.On<ServiceConfigurationSnapshot>(
                x => x.FromParentId(x => x.Id, x =>
                {
                    x.Children(x => x.ServiceConfigurationSnapshotParameters, x => x.ServiceConfigurationSnapshotId, x => { });
                }));
    }
}