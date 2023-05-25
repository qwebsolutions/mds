using Metapsi;

namespace MdsInfrastructure
{
    public partial class Application : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.String Name { get; set; } = System.String.Empty;
        public System.Guid ConfigurationHeaderId { get; set; }

        public static UptreeRelation<InfrastructureConfiguration>
            Data = Relation.On<InfrastructureConfiguration>(application =>
            {
                application.FromParentId(x => x.Id, application =>
                {
                    application.Children(x => x.InfrastructureServices, x => x.ApplicationId, service => { });
                });
            });
    }
}
