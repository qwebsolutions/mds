using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting.Systemd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure.Flow;

public static class GetModel
{
    public static async Task<InfrastructureStatus> InfrastructureStatus(CommandContext commandContext, HttpContext httpContext)
    {
        var statusPage = await Load.Status(commandContext);
        statusPage.User = httpContext.User();
        return statusPage;
    }

    public static void RegisterModelApi(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet(nameof(MdsInfrastructure.InfrastructureStatus), InfrastructureStatus).AllowAnonymous();
    }
}

public static partial class Status
{
    public class Infra : Metapsi.Http.Get<Routes.Status.Infra>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
        {
            var statusPage = await Load.Status(commandContext);
            statusPage.User = httpContext.User();
            return Page.Result(statusPage);
        }
    }

    public class Application : Metapsi.Http.Get<Routes.Status.Application, string>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext, string applicationName)
        {
            var pageData = await Load.Status(commandContext);
            pageData.User = httpContext.User();
            //Guid selectedApplicationId = pageData.InfrastructureConfiguration.Applications.Single(x => x.Name == applicationName).Id;

            return Page.Result<ApplicationStatus>(new ApplicationStatus()
            {
                ApplicationName = applicationName,
                InfrastructureStatus = pageData
            });
        }
    }

    public class Node : Metapsi.Http.Get<Routes.Status.Node, string>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext, string nodeName)
        {
            var pageData = await Load.Status(commandContext);
            pageData.User = httpContext.User();

            return Page.Result<NodeStatus>(new NodeStatus()
            {
                NodeName = nodeName,
                InfrastructureStatus = pageData
            });
        }
    }
}

internal static partial class Load
{
    public static async Task<InfrastructureStatus> Status(CommandContext commandContext)
    {
        string validation = await commandContext.Do(Backend.ValidateSchema);

        if (!string.IsNullOrEmpty(validation))
        {
            return new InfrastructureStatus()
            {
                SchemaValidationMessage = validation
            };
        }

        var infrastructureStatus = await commandContext.Do(Backend.LoadInfraStatus);
        infrastructureStatus.NodePanels = GetNodePanelsData(infrastructureStatus);
        infrastructureStatus.ApplicationPanels = GetApplicationPanelData(infrastructureStatus);
        infrastructureStatus.ServicePanels = GetServicePanelData(infrastructureStatus);
        return infrastructureStatus;
    }

    private static List<NodePanelModel> GetNodePanelsData(InfrastructureStatus infrastructureStatus)
    {
        List<NodePanelModel> nodePanels = new List<NodePanelModel>();

        foreach (var node in infrastructureStatus.InfrastructureNodes)
        {
            var nodeHealthStatus = infrastructureStatus.HealthStatus.SingleOrDefault(x => x.NodeName == node.NodeName);
            if (nodeHealthStatus == null)
            {
                nodePanels.Add(new NodePanelModel()
                {
                    NodeName = node.NodeName,
                    NodeUiUrl = $"http://{node.MachineIp}:{node.UiPort}",
                    NodeStatusCode = "error",
                    ErrorMessage = "Could not retrieve status"
                });
            }
            else
            {
                var nodePanel = new NodePanelModel()
                {
                    NodeName = node.NodeName,
                    NodeUiUrl = $"http://{node.MachineIp}:{node.UiPort}",
                };

                nodePanels.Add(nodePanel);

                FullStatus<string> status = StatusExtensions.GetNodeStatus(infrastructureStatus.HealthStatus, node.NodeName);

                var availableHddGb = status.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.AvailableHddGb);
                var availableHddPercent = status.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.AvailableHddPercent);
                var availableRamGb = status.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.AvailableRamGb);
                var availableRamPercent = status.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.AvailableRamPercent);


                if (availableHddGb != null)
                {
                    nodePanel.AvailableHddGb = availableHddGb.CurrentValue;

                    if (availableHddGb.GeneralStatus == GeneralStatus.Danger)
                    {
                        nodePanel.HddWarning = true;
                    }
                }

                if (availableHddPercent != null)
                {
                    nodePanel.AvailableHddPercent = availableHddPercent.CurrentValue;
                    if (availableHddPercent.GeneralStatus == GeneralStatus.Danger)
                    {
                        nodePanel.HddWarning = true;
                    }
                }

                if (availableRamGb != null)
                {
                    nodePanel.AvailableRamGb = availableRamGb.CurrentValue;
                    if (availableRamGb.GeneralStatus == GeneralStatus.Danger)
                    {
                        nodePanel.RamWarning = true;
                    }
                }

                if (availableHddPercent != null)
                {
                    nodePanel.AvailableRamPercent = availableRamPercent.CurrentValue;
                    if (availableRamPercent.GeneralStatus == GeneralStatus.Danger)
                    {
                        nodePanel.RamWarning = true;
                    }
                }
            }
        }

        return nodePanels;
    }

    private static ApplicationPanelModel GetApplicationPanelData(InfrastructureStatus status, string applicationName)
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
            var serviceStatus = StatusExtensions.GetServiceStatus(status.Deployment, status.HealthStatus, service, status.InfrastructureEvents);
            if (serviceStatus.Entity.Enabled)
            {
                if (serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger || x.GeneralStatus == GeneralStatus.NoData))
                {
                    dangerServicesCount++;
                }
                else
                {
                    if (serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Warning))
                    {
                        warningServicesCount++;
                    }
                }
            }
            else
            {
                warningServicesCount++;
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

    private static List<ApplicationPanelModel> GetApplicationPanelData(InfrastructureStatus status)
    {
        var applications = status.Deployment.GetDeployedServices().Select(x => x.ApplicationName).Distinct();
        return applications.Select(x => GetApplicationPanelData(status, x)).ToList();
    }

    private static ServicePanelModel GetServicePanelData(InfrastructureStatus status, string serviceName)
    {

        ServicePanelModel servicePanelData = new ServicePanelModel()
        {
            ServiceName = serviceName
        };

        var serviceSnapshot = status.Deployment.GetDeployedServices().Single(x => x.ServiceName == serviceName);



        servicePanelData.FullStatus = StatusExtensions.GetServiceStatus(status.Deployment, status.HealthStatus, serviceSnapshot, status.InfrastructureEvents);

        if (servicePanelData.FullStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.NoData))
        {
            servicePanelData.StatusCode = "warning";
            servicePanelData.StatusText = "Waiting for data";
        }
        else if (servicePanelData.FullStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceRunningFor).GeneralStatus == GeneralStatus.Danger)
        {
            servicePanelData.StatusCode = "error";
            servicePanelData.StatusText = "Service not running!";
        }
        else
        {
            var runningSince = servicePanelData.FullStatus.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.ServiceRunningSince);
            servicePanelData.StartedDateIso = runningSince.CurrentValue;
        }

        return servicePanelData;
    }

    private static List<ServicePanelModel> GetServicePanelData(InfrastructureStatus status)
    {
        var deployedServiceNames = status.Deployment.GetDeployedServices().Select(x => x.ServiceName);
        return deployedServiceNames.Select(x => GetServicePanelData(status, x)).ToList();
    }
}
