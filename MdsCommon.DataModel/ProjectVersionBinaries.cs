using Metapsi;

namespace MdsCommon
{
    public partial class ProjectVersionBinaries : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.Guid ProjectVersionId { get; set; }
        public System.String BuildNumber { get; set; } = System.String.Empty;
        public System.String Target { get; set; } = System.String.Empty;
    }
}
