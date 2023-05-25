using Metapsi;
using System.Collections.Generic;

namespace MdsCommon
{
    public partial class Project : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.String Name { get; set; } = System.String.Empty;
        public System.Boolean Enabled { get; set; }

        public List<ProjectVersion> Versions { get; set; } = new List<ProjectVersion>();

        public static UptreeRelation<Project> Data = Relation.On<Project>(x =>
        {
            x.FromParentId(x => x.Id, x =>
            {
                x.Children(x => x.Versions, x => x.ProjectId, x =>
                {
                    x.FromParentId(x => x.Id, x =>
                    {
                        x.Children(x => x.Binaries, x => x.ProjectVersionId, x => { });
                    });
                });
            });
        });
    }

}
