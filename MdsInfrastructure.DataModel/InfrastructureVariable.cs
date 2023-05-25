using Metapsi;

namespace MdsInfrastructure
{
    public partial class InfrastructureVariable : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.String VariableName { get; set; } = System.String.Empty;
        public System.String VariableValue { get; set; } = System.String.Empty;
        public System.Guid ConfigurationHeaderId { get; set; }
    }
}
