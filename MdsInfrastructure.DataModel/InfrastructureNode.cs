using Metapsi;

namespace MdsInfrastructure
{
    public partial class InfrastructureNode : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.String MachineIp { get; set; } = System.String.Empty;
        public System.String NodeName { get; set; } = System.String.Empty;
        public System.Int32 UiPort { get; set; } = 9234;
        public System.Boolean Active { get; set; }
        public System.Guid EnvironmentTypeId { get; set; }
    }
}
