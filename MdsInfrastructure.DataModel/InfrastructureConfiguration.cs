using Metapsi;
using System.Collections.Generic;

namespace MdsInfrastructure
{
    public class InfrastructureConfiguration : IRecord, IDataStructure
    {
        // Record

        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public string Name { get; set; } = string.Empty;


        // Data structure

        public List<InfrastructureService> InfrastructureServices { get; set; } = new();
        public List<Application> Applications { get; set; } = new();
        public List<InfrastructureVariable> InfrastructureVariables { get; set; } = new();

        public static UptreeRelation<InfrastructureConfiguration>

            Data = Relation.On<InfrastructureConfiguration>(configHeader =>
            {
                configHeader.FromParentId(x => x.Id, configurationChildren =>
                {
                    configurationChildren.Children(x => x.InfrastructureServices, x => x.ConfigurationHeaderId, InfrastructureService.Loader);
                    configurationChildren.Children(x => x.Applications, x => x.ConfigurationHeaderId, b => { });
                    configurationChildren.Children(x => x.InfrastructureVariables, x => x.ConfigurationHeaderId, b => { });
                });
            });
    }
}