using MdsCommon;
using Metapsi;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure.Flow;

public static partial class Status
{
    public static async Task<InfrastructureStatus> LoadInfrastructureStatusPageModel(CommandContext commandContext)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        await DebugTo.File("c:\\github\\qwebsolutions\\mds\\debug\\InfraStatus.txt", DateTime.UtcNow.Roundtrip());
        var statusPage = await Load.FullStatusData(commandContext);
        var r = new MdsInfrastructure.InfrastructureStatus()
        {
            InfrastructureStatusData = statusPage,
            ApplicationPanels = Load.GetApplicationPanelData(statusPage, statusPage.Deployment.GetDeployedServices().Select(x => x.ApplicationName).Distinct()),
            NodePanels = Load.GetNodePanelsData(statusPage, statusPage.InfrastructureNodes.Select(x => x.NodeName))
        };

        await DebugTo.File("c:\\github\\qwebsolutions\\mds\\debug\\InfraStatus.txt", DateTime.UtcNow.Roundtrip());
        await DebugTo.File("c:\\github\\qwebsolutions\\mds\\debug\\InfraStatus.txt", $"{sw.ElapsedMilliseconds.ToString()} ms");
        return r;
    }

    public static async Task<ApplicationStatus> LoadApplicationStatusPageModel(CommandContext commandContext, string applicationName)
    {
        var fullStatus = await Load.FullStatusData(commandContext);
        return new MdsInfrastructure.ApplicationStatus()
        {
            InfrastructureStatus = fullStatus,
            ApplicationName = applicationName,
            ApplicationPanel = Load.GetApplicationPanelData(fullStatus, applicationName),
            ServicePanels = Load.GetServicePanelData(fullStatus, fullStatus.Deployment.GetDeployedServices().Where(x => x.ApplicationName == applicationName).Select(x => x.ServiceName))
        };
    }

    public static async Task<NodeStatus> LoadNodeStatusPageModel(CommandContext commandContext, string nodeName)
    {
        var fullStatus = await Load.FullStatusData(commandContext);
        return new MdsInfrastructure.NodeStatus()
        {
            InfrastructureStatus = fullStatus,
            NodeName = nodeName,
            NodePanel = Load.GetNodePanelData(fullStatus, nodeName),
            ServicePanels = Load.GetServicePanelData(fullStatus, fullStatus.Deployment.GetDeployedServices().Where(x => x.NodeName == nodeName).Select(x => x.ServiceName))
        };
    }

    public class Infra : Metapsi.Http.Get<Routes.Status.Infra>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
        {
            var statusPage = await LoadInfrastructureStatusPageModel(commandContext);
            statusPage.User = httpContext.User();
            return Page.Result(statusPage);
        }
    }

    public class Application : Metapsi.Http.Get<Routes.Status.Application, string>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext, string applicationName)
        {
            var pageData = await LoadApplicationStatusPageModel(commandContext, applicationName);
            pageData.User = httpContext.User();
            return Page.Result(pageData);
        }
    }

    public class Node : Metapsi.Http.Get<Routes.Status.Node, string>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext, string nodeName)
        {
            var pageData = await LoadNodeStatusPageModel(commandContext, nodeName);
            pageData.User = httpContext.User();

            return Page.Result(pageData);
        }
    }
}

internal static partial class Load
{
    public const decimal WarningAvailableHddGb = 1;
    public const decimal WarningAvailableRamGb = 0.75m;
    public const int WarningSyncSecondsAgo = 60;
    public const decimal WarningServiceUsedRamMb = 800;

    public static async Task<InfrastructureStatusData> FullStatusData(CommandContext commandContext)
    {
        string validation = await commandContext.Do(Backend.ValidateSchema);

        if (!string.IsNullOrEmpty(validation))
        {
            return new InfrastructureStatusData()
            {
                SchemaValidationMessage = validation
            };
        }

        var infrastructureStatus = await commandContext.Do(Backend.LoadInfraStatus);
        return infrastructureStatus;
    }

    public static NodePanelModel GetNodePanelData(InfrastructureStatusData infrastructureStatus, string nodeName)
    {
        var node = infrastructureStatus.InfrastructureNodes.SingleOrDefault(x => x.NodeName == nodeName);

        if (node == null)
        {
            return new NodePanelModel()
            {
                NodeStatusCode = "error",
                NodeName = nodeName,
                ErrorMessage = "Node data not available"
            };
        }

        // Should theoretically be only one anyway
        var nodeStatus = infrastructureStatus.HealthStatus.OrderByDescending(x => x.TimestampUtc).FirstOrDefault();
        if (nodeStatus == null)
        {
            return new NodePanelModel()
            {
                NodeStatusCode = "error",
                NodeName = nodeName,
                ErrorMessage = "Node data not available"
            };
        }

        var nodePanelModel = new NodePanelModel()
        {
            NodeName = nodeName,
            NodeUiUrl = $"http://{node.MachineIp}:{node.UiPort}"
        };

        decimal availableHddPercent = decimal.Round(decimal.Divide(nodeStatus.HddAvailableMb * 100, nodeStatus.HddTotalMb), 2, MidpointRounding.AwayFromZero);

        nodePanelModel.AvailableHddPercent = availableHddPercent.ToString();

        decimal availableHddGb = decimal.Round(decimal.Divide(nodeStatus.HddAvailableMb, 1024), 2, MidpointRounding.AwayFromZero);
        nodePanelModel.AvailableHddGb = availableHddGb.ToString();

        if (availableHddGb < WarningAvailableHddGb)
        {
            nodePanelModel.HddWarning = true;
            nodePanelModel.NodeStatusCode = "warning";
        }

        decimal availableRamPercent = decimal.Round(decimal.Divide(nodeStatus.RamAvailableMb * 100, nodeStatus.RamTotalMb), 2, MidpointRounding.AwayFromZero);
        nodePanelModel.AvailableRamPercent = availableRamPercent.ToString();

        decimal availableRamGb = decimal.Round(decimal.Divide(nodeStatus.RamAvailableMb, 1024), 2, MidpointRounding.AwayFromZero);
        nodePanelModel.AvailableRamGb = availableRamGb.ToString();

        if (availableRamGb < WarningAvailableRamGb)
        {
            nodePanelModel.RamWarning = true;
            nodePanelModel.NodeStatusCode = "warning";
        }

        return nodePanelModel;
    }

    public static List<NodePanelModel> GetNodePanelsData(InfrastructureStatusData infrastructureStatus, IEnumerable<string> nodeNames)
    {
        List<NodePanelModel> nodePanels = new List<NodePanelModel>();

        foreach (var node in nodeNames)
        {
            nodePanels.Add(GetNodePanelData(infrastructureStatus, node));
        }

        return nodePanels;
    }

    public static ApplicationPanelModel GetApplicationPanelData(InfrastructureStatusData status, string applicationName)
    {
        ApplicationPanelModel panelModel = new ApplicationPanelModel()
        {
            ApplicationName = applicationName,
            StatusCode = "ok"
        };

        var appServices = status.Deployment.GetDeployedServices().Where(x => x.ApplicationName == applicationName);

        int dangerServicesCount = 0;
        int warningServicesCount = 0;

        foreach (var service in appServices)
        {
            var servicePanelData = GetServicePanelData(status, service.ServiceName);
            if (servicePanelData.Enabled)
            {
                if (servicePanelData.StatusCode == "error")
                {
                    dangerServicesCount++;
                }
                else if (servicePanelData.StatusCode == "warning")
                {
                    warningServicesCount++;
                }
            }
        }

        panelModel.StatusText = $"{appServices.Count()} services (all ok)";

        if (dangerServicesCount > 0)
        {
            panelModel.StatusText = $"{appServices.Count()} services ({dangerServicesCount} in error)";
            panelModel.StatusCode = "error";
        }
        else if (warningServicesCount > 0)
        {
            panelModel.StatusText = $"{appServices.Count()} services";
            panelModel.StatusCode = "warning";
        }

        return panelModel;
    }

    public static List<ApplicationPanelModel> GetApplicationPanelData(InfrastructureStatusData status, IEnumerable<string> applicationNames)
    {
        return applicationNames.Select(x => GetApplicationPanelData(status, x)).ToList();
    }

    public static ServicePanelModel GetServicePanelData(InfrastructureStatusData status, string serviceName)
    {

        ServicePanelModel servicePanelData = new ServicePanelModel()
        {
            ServiceName = serviceName
        };

        var serviceSnapshot = status.Deployment.GetDeployedServices().Single(x => x.ServiceName == serviceName);
        if (!serviceSnapshot.Enabled)
        {
            servicePanelData.Enabled = false;
            servicePanelData.StatusText = "Disabled";
            return servicePanelData;
        }

        var serviceStatus = status.HealthStatus.SelectMany(x => x.ServiceStatuses).Where(x => x.ServiceName == serviceName).OrderByDescending(x => x.StatusTimestamp).FirstOrDefault();

        if (serviceStatus == null)
        {
            servicePanelData.StatusCode = "error";
            servicePanelData.StatusText = "Service data not available";
            return servicePanelData;
        }

        var chronologicalEvents = status.InfrastructureEvents.Where(x => x.Source == serviceName).OrderBy(x => x.Timestamp);
        var lastConfigurationChange = status.Deployment.LastConfigurationChanges.SingleOrDefault(x => x.ServiceName == serviceName);
        if (lastConfigurationChange != null)
        {
            // Kinda flimsy based on timestamp, ain't it?
            var eventsSinceLastReconfigured = chronologicalEvents.Where(x => x.Timestamp > lastConfigurationChange.LastConfigurationChangeTimestamp);

            // If started multiple times since last deployment, could be a problem
            var exitEvents = eventsSinceLastReconfigured.Where(x => x.Type == MdsCommon.InfrastructureEventType.ProcessExit);

            if (exitEvents.Count() > 30)
            {
                servicePanelData.StatusCode = "warning";
                servicePanelData.StatusText = $"Crashed {exitEvents.Count()} times since deployed";
            }
        }

        servicePanelData.StartedTimeUtc = serviceStatus.StartTimeUtc.Roundtrip();

        TimeSpan syncAgo = TimeSpan.FromSeconds((int)(DateTime.UtcNow - serviceStatus.StatusTimestamp.ToUniversalTime()).TotalSeconds);

        if (syncAgo.TotalSeconds > WarningSyncSecondsAgo)
        {
            servicePanelData.StatusCode = "warning";
            servicePanelData.StatusText = "Waiting for status update...";
        }
        servicePanelData.RamMb = serviceStatus.UsedRamMb.ToString();

        if (serviceStatus.UsedRamMb > WarningServiceUsedRamMb)
        {
            servicePanelData.StatusCode = "warning";
            servicePanelData.RamWarning = true;
        }

        return servicePanelData;
    }

    public static List<ServicePanelModel> GetServicePanelData(InfrastructureStatusData status, IEnumerable<string> serviceNames)
    {
        List<ServicePanelModel> servicePanels = new List<ServicePanelModel>();
        foreach (var serviceName in serviceNames)
        {
            servicePanels.Add(GetServicePanelData(status, serviceName));
        }

        return servicePanels;
        //var deployedServiceNames = status.Deployment.GetDeployedServices().Select(x => x.ServiceName);
        //return deployedServiceNames.Select(x => GetServicePanelData(status, x)).ToList();
    }
}
