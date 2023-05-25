using Metapsi;

namespace MdsInfrastructure
{
    public partial class EnvironmentType : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.String Name { get; set; } = System.String.Empty;
        public System.String OsType { get; set; } = System.String.Empty;
    }
}
