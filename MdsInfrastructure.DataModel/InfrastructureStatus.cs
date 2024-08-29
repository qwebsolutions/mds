using MdsCommon;
using System.Collections.Generic;

namespace MdsInfrastructure
{
    public partial class InfrastructureStatusData
    {
        public InfrastructureConfiguration InfrastructureConfiguration { get; set; } = new();
        public List<InfrastructureNode> InfrastructureNodes { get; set; } = new();
        public List<MachineStatus> HealthStatus { get; set; } = new();
        public Deployment Deployment { get; set; } = new();
        public List<InfrastructureEvent> InfrastructureEvents { get; set; } = new();
        public string SchemaValidationMessage { get; set; } = string.Empty;
    }

    public partial class InfrastructureStatus 
    {
        public MdsCommon.User User { get; set; } = new();
        public InfrastructureStatusData InfrastructureStatusData { get; set; } = new();
        public List<ApplicationPanelModel> ApplicationPanels { get; set; } = new();
        public List<NodePanelModel> NodePanels { get; set; } = new();
        public List<ServicePanelModel> ServicePanels { get; set; } = new();
    }

    public class ApplicationStatus
    {
        public MdsCommon.User User { get; set; } = new();
        public string ApplicationName { get; set; } = string.Empty;
        public InfrastructureStatusData InfrastructureStatus { get; set; } = new();

        public ApplicationPanelModel ApplicationPanel { get; set; } = new();
        public List<ServicePanelModel> ServicePanels { get; set; } = new();
    }

    public class NodeStatus
    {
        public MdsCommon.User User { get; set; } = new();
        public string NodeName { get; set; } = string.Empty;
        public InfrastructureStatusData InfrastructureStatus { get; set; } = new();

        public NodePanelModel NodePanel { get; set; } = new();
        public List<ServicePanelModel> ServicePanels { get; set; } = new();
    }

    public class NodePanelModel
    {
        public string NodeName { get; set; }
        public string NodeUiUrl { get; set; } = string.Empty;
        public string NodeStatusCode { get; set; } = "ok";
        public string AvailableHddGb { get; set; }
        public string AvailableHddPercent { get; set; }
        public bool HddWarning { get; set; }
        public string AvailableRamGb { get; set; }
        public string AvailableRamPercent { get; set; }
        public bool RamWarning { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class ApplicationPanelModel
    {
        public string ApplicationName { get; set; }
        public string StatusCode { get; set; }
        public string StatusText { get; set; }
        public int DangerServicesCount { get; set; }
        public int WarningServicesCount { get; set; }
    }

    public class ServicePanelModel
    {
        public string ServiceName { get; set; }
        public bool Enabled { get; set; } = true;
        //public FullStatus<MdsCommon.ServiceConfigurationSnapshot> FullStatus { get; set; }
        public string StatusCode { get; set; } = "ok";
        public string StatusText { get; set; }
        public string StartedTimeUtc { get; set; }
        public string RamMb { get; set; }
        public bool RamWarning { get; set; }
    }
}
