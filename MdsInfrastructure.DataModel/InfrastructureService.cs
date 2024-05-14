using Metapsi;
using System.Collections.Generic;

namespace MdsInfrastructure
{
    public partial class InfrastructureService : IRecord
    {
        // Record
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.String ServiceName { get; set; } = System.String.Empty;
        public System.Guid ProjectId { get; set; }
        public System.Guid ProjectVersionId { get; set; }
        public System.Guid InfrastructureNodeId { get; set; }
        public System.Boolean Enabled { get; set; }
        public System.Guid ApplicationId { get; set; }
        public System.Guid ConfigurationHeaderId { get; set; }


        // Data structure
        public List<InfrastructureServiceParameterDeclaration> InfrastructureServiceParameterDeclarations { get; set; } = new();
        public List<InfrastructureServiceNote> InfrastructureServiceNotes { get; set; } = new();

        public static void Loader(DowntreeBuilder<InfrastructureService> service)
        {
            service.FromParentId(x => x.Id, service =>
            {
                service.Children(x => x.InfrastructureServiceNotes, x => x.InfrastructureServiceId, b => { });
                service.Children(x => x.InfrastructureServiceParameterDeclarations, x => x.InfrastructureServiceId, InfrastructureServiceParameterDeclaration.Loader);
            });
        }

        public static UptreeRelation<InfrastructureService> Service = Relation.On<InfrastructureService>(Loader);
    }



    public class ParameterType : IRecord
    {
        public System.Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class NoteType : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public partial class InfrastructureServiceNote : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.Guid InfrastructureServiceId { get; set; }
        public System.Guid NoteTypeId { get; set; }
        public System.String Reference { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}
