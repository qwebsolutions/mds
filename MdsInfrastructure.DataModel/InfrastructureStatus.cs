using MdsCommon;
using System.Collections.Generic;

namespace MdsInfrastructure
{
    public partial class InfrastructureStatus
    {
        public InfrastructureConfiguration InfrastructureConfiguration { get; set; } = new();
        public List<InfrastructureNode> InfrastructureNodes { get; set; } = new();
        public List<MachineStatus> HealthStatus { get; set; } = new();
        public Deployment Deployment { get; set; } = new();
        public List<InfrastructureEvent> InfrastructureEvents { get; set; } = new();
        public string SchemaValidationMessage { get; set; } = string.Empty;

        public Metapsi.Ui.User User { get; set; } = new();
    }

    public class ApplicationStatus
    {
        public string ApplicationName { get; set; } = string.Empty;
        public InfrastructureStatus InfrastructureStatus { get; set; } = new();
    }
}
