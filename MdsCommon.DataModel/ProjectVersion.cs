using Metapsi;
using System.Collections.Generic;

namespace MdsCommon
{
    public partial class ProjectVersion : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.Guid ProjectId { get; set; }
        public System.String VersionTag { get; set; } = System.String.Empty;
        public System.Boolean Enabled { get; set; }

        public List<ProjectVersionBinaries> Binaries { get; set; } = new List<ProjectVersionBinaries>();

        public static UptreeRelation<ProjectVersion> Data = Relation.On<ProjectVersion>(x =>
        {
            x.FromParentId(x => x.Id, x =>
            {
                x.Children(x => x.Binaries, x => x.ProjectVersionId, x => { });
            });
        });
    }

}
