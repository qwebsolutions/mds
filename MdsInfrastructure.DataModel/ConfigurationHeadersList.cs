using System.Collections.Generic;

namespace MdsInfrastructure
{
    public class ConfigurationHeadersList
    {
        public List<InfrastructureConfiguration> ConfigurationHeaders { get; set; } = new();
        public List<InfrastructureService> Services { get; set; } = new();
    }
}
