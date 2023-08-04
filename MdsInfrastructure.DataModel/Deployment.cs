using Metapsi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsInfrastructure
{
    public partial class DeploymentServiceTransition : IRecord
    {
        // Record

        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.Guid DeploymentId { get; set; }
        public System.Guid FromServiceConfigurationSnapshotId { get; set; }
        public System.Guid ToServiceConfigurationSnapshotId { get; set; }

        public MdsCommon.ServiceConfigurationSnapshot FromSnapshot { get; set; }
        public MdsCommon.ServiceConfigurationSnapshot ToSnapshot { get; set; }
    }

    public partial class ConfigurationTime
    {
        public System.String ServiceName { get; set; } = System.String.Empty;
        public System.DateTime LastConfigurationChangeTimestamp { get; set; }
    }

    public partial class Deployment : IRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; }
        public string ConfigurationName { get; set; } = string.Empty;
        public Guid ConfigurationHeaderId { get; set; }

        public static UptreeRelation<Deployment> Data =
            Relation.On<Deployment>(x =>
            {
                x.FromParentId(x => x.Id, x =>
                {
                    x.Children(x => x.Transitions, x => x.DeploymentId, x => { });
                });
            });
    }

    public partial class Deployment : IDataStructure//, IServiceTransitionDataSet
    {
        public List<DeploymentServiceTransition> Transitions { get; set; } = new();
        public List<ConfigurationTime> LastConfigurationChanges { get; set; } = new();

        public IEnumerable<MdsCommon.ServiceConfigurationSnapshot> GetDeployedServices()
        {
            return Transitions.Where(x => x.ToSnapshot != null).Select(x => x.ToSnapshot);
        }
    }

    public class DeploymentHistory
    {
        public System.Collections.Generic.List<Deployment> Deployments { get; set; } = new System.Collections.Generic.List<Deployment>();
        public Metapsi.Ui.User User { get; set; }
    }

    public class DeploymentReview
    {
        public Deployment Deployment { get; set; }
        public ChangesReport ChangesReport { get; set; }
        public Metapsi.Ui.User User { get; set; }
    }

    public class DeploymentPreview
    {
        public Deployment Deployment { get; set; }
        public ChangesReport ChangesReport { get; set; }
        public InfrastructureConfiguration SavedConfiguration { get; set; }
        public Metapsi.Ui.User User { get; set; }
    }
}
