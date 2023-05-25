using Metapsi;
using System.Collections.Generic;

namespace MdsInfrastructure
{
    public partial class InfrastructureServiceParameterDeclaration : IRecord, IDataStructure
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.Guid InfrastructureServiceId { get; set; }
        public System.String ParameterName { get; set; } = System.String.Empty;
        public System.Guid ParameterTypeId { get; set; }

        public List<InfrastructureServiceParameterValue> InfrastructureServiceParameterValues { get; set; } = new();
        public List<InfrastructureServiceParameterBinding> InfrastructureServiceParameterBindings { get; set; } = new();

        public static void Loader(DowntreeBuilder<InfrastructureServiceParameterDeclaration> parameter)
        {
            parameter.FromParentId(x => x.Id, parameter =>
            {
                parameter.Children(x => x.InfrastructureServiceParameterBindings, x => x.InfrastructureServiceParameterDeclarationId, p => { });
                parameter.Children(x => x.InfrastructureServiceParameterValues, x => x.InfrastructureServiceParameterDeclarationId, p => { });
            });
        }

        public static UptreeRelation<InfrastructureServiceParameterDeclaration>
            ParameterData = Relation.On<InfrastructureServiceParameterDeclaration>(Loader);
    }

    public partial class InfrastructureServiceParameterValue : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.Guid InfrastructureServiceParameterDeclarationId { get; set; }
        public System.String ParameterValue { get; set; } = System.String.Empty;
    }
    public partial class InfrastructureServiceParameterBinding : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.Guid InfrastructureServiceParameterDeclarationId { get; set; }
        public System.Guid InfrastructureVariableId { get; set; }
    }
}
