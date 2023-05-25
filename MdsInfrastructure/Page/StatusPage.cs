using Metapsi;
using System.Collections.Generic;
//using System;
//using System.Linq;
using System.Threading.Tasks;
//using UI.Svelte;
//using System.Collections.Generic;
//using MdsInfrastructure.Behavior;
//using MdsInfrastructure.DataStructure;
//using MdsCommon;

namespace MdsInfrastructure
{


    //public partial class NodeStatusPage : StatusPage
    //{

    //}

    //public partial class ApplicationStatusPage : StatusPage
    //{

    //}

    //public static partial class MdsInfrastructureFunctions
    //{
    //}
}

//        public static UI.Svelte.View RenderStatusPage(StatusPage dataModel, UI.Svelte.ViewBuilder viewBuilder)
//        {
//            if (dataModel.Deployment == null)
//            {
//                viewBuilder.Label("lblNoDeployment", Guid.Empty, "No deployment yet! The infrastructure is not running any service!", -1, ViewBuilder.RootGroupId);
//                return viewBuilder.OutputView;
//            }
//            viewBuilder.OutputView.AutoRefresh.Add(new AutoRefresh()
//            {
//                IntervalSeconds = 5,
//                Area = "Status"
//            });

//            int totalServices = dataModel.GetDeployedServices().Count();
//            int totalNodes = dataModel.GetDeployedServices().Select(x => x.NodeName).Distinct().Count();

//            viewBuilder.InfoPanel("activeSnapshotPanel", Guid.Empty, UI.Svelte.ViewBuilder.RootGroupId, Guid.Empty,
//                $"Last deployment: {dataModel.GetLastDeployment().ConfigurationName}",
//                $"{dataModel.GetLastDeployment().Timestamp.ItalianFormat()}, total services {totalServices}, total infrastructure nodes {totalNodes}", "Primary", "Default");

//            Group nodesGroup = null;

//            int currentIndex = 0;
//            foreach (var nodeName in dataModel.GetDeployedServices().Select(x => x.NodeName).Distinct())
//            {
//                if (currentIndex % 4 == 0)
//                    nodesGroup = viewBuilder.Group("grpNodes", Guid.Empty, UI.Svelte.ViewBuilder.RootGroupId, "Horizontal");

//                var node = dataModel.InfrastructureConfiguration.InfrastructureNodes.Single(x => x.NodeName == nodeName);

//                RenderNodePanel<StatusPage, NodeStatusPage>(
//                    node,
//                    dataModel.HealthStatus,
//                    nodesGroup.Id,
//                    viewBuilder,
//                    async (cc, pageData, id) =>
//                    {
//                        NodeStatusPage nodesStatusPage = new NodeStatusPage()
//                        {
//                            HealthStatus = pageData.HealthStatus,
//                            InfrastructureConfiguration = pageData.InfrastructureConfiguration,
//                            InfrastructureEvents = pageData.InfrastructureEvents,
//                            Deployment = pageData.Deployment,
//                            Page = pageData.Page
//                        };

//                        nodesStatusPage.SetSelectedId<Record.InfrastructureNode>(node.Id);
//                        return nodesStatusPage;
//                    });

//                currentIndex++;
//            }

//            Group applicationsGroup = null;

//            currentIndex = 0;
//            foreach (var applicationName in dataModel.GetDeployedServices().Select(x => x.ApplicationName).Distinct())
//            {
//                if (currentIndex % 4 == 0)
//                    applicationsGroup = viewBuilder.Group("grpApplications", Guid.Empty, UI.Svelte.ViewBuilder.RootGroupId, "Horizontal");

//                RenderApplicationPanel<StatusPage, ApplicationStatusPage>(
//                    dataModel.Deployment,
//                    dataModel.HealthStatus,
//                    dataModel.InfrastructureEvents,
//                    applicationName,
//                    applicationsGroup.Id,
//                    viewBuilder,
//                    async (cc, pageData, id) =>
//                    {
//                        HealthStatus healthStatusResult = await cc.Do(MdsInfrastructureApplication.LoadHealthStatus);

//                        ApplicationStatusPage applicationStatusPage = new ApplicationStatusPage()
//                        {
//                            HealthStatus = healthStatusResult,
//                            InfrastructureConfiguration = pageData.InfrastructureConfiguration,
//                            InfrastructureEvents = pageData.InfrastructureEvents,
//                            Deployment = pageData.Deployment,
//                            Page = pageData.Page
//                        };

//                        applicationStatusPage.SetSelectedId<Application>(pageData.InfrastructureConfiguration.Applications.Single(x => x.Name == applicationName).Id);
//                        return applicationStatusPage;
//                    });
//                currentIndex++;
//            }

//            var events = dataModel.GetRecentEvents();

//            var fatalEvents = events.Where(x => x.Criticality == MdsCommon.InfrastructureEventCriticality.Fatal);
//            var criticalEvents = events.Where(x => x.Criticality == MdsCommon.InfrastructureEventCriticality.Critical);

//            if (fatalEvents.Count() != 0 || criticalEvents.Count() != 0)
//            {
//                viewBuilder.OutputView.ValidationMessages.Add(new ValidationMessage()
//                {
//                    MessageType = "Danger",
//                    ValidationMessageText = $"{fatalEvents.Count()} fatal / {criticalEvents.Count()} critical events in the last 15 minutes"
//                });
//            }

//            viewBuilder.OutputView.HeaderText = "Status";

//            return viewBuilder.OutputView;
//        }


//        static void RenderNodePanel<TFromPage, TToPage>(
//            Record.InfrastructureNode node,
//            MdsCommon.HealthStatus healthStatus,
//            Guid intoGroupId,
//            UI.Svelte.ViewBuilder viewBuilder,
//            Func<CommandContext, TFromPage, Guid, Task<TToPage>> command = null)
//        {
//            string nodeName = node.NodeName;
//            string nodeUrl = $"http://{node.MachineIp}:{node.UiPort}";

//            string nodeLabel = $"<a href='{nodeUrl}' style='color:white'>{nodeName}</a> <img src=\"/server-icon.png\" style=\"width:1.5em;height:2em;padding-left:10px;\"/>";
//            //nodeName = $"{nodeName} <object data=\"ServerIcon.svg\" type=\"image / svg + xml\" style=\"width:1.5em;height:2em;padding-left:10px;\">";

//            FullStatus<string> status = StatusExtensions.GetNodeStatus(healthStatus, nodeName);

//            if (!healthStatus.MachineStatus.Where(x => x.NodeName == nodeName).Any())
//            {
//                viewBuilder.InfoPanel("nodePanel", Guid.NewGuid(), intoGroupId, Guid.Empty, $"{nodeLabel}", "Could not retrieve status", "Light", "Danger");
//            }
//            else if (Math.Abs((healthStatus.MachineStatus.Where(x => x.NodeName == nodeName).Max(x => x.TimestampUtc) - DateTime.Now).TotalMinutes) > 1)
//            {
//                var timespan = healthStatus.MachineStatus.Where(x => x.NodeName == nodeName).Max(x => x.TimestampUtc) - DateTime.Now;
//                viewBuilder.InfoPanel("nodePanel", Guid.NewGuid(), intoGroupId, Guid.Empty, $"{nodeLabel}", $"Status not received for {Convert.ToInt32(Math.Abs(timespan.TotalMinutes))} minutes!", "Light", "Danger");
//            }
//            else if (status.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.NoData))
//            {
//                viewBuilder.InfoPanel("nodePanel", Guid.NewGuid(), intoGroupId, Guid.Empty, $"Infrastructure node: {status.Entity}", "Data not available!", "Danger", "Default");
//            }
//            else
//            {
//                string nodeStyling = "Light";

//                //if (status.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger))
//                //{
//                //    nodeStyling = "Danger";
//                //}

//                var availableHddGb = status.StatusValues.Single(x => x.Name == StatusExtensions.AvailableHddGb);
//                var availableHddPercent = status.StatusValues.Single(x => x.Name == StatusExtensions.AvailableHddPercent);

//                List<string> panelData = new List<string>();
//                string hddInfo = $"Available HDD: {availableHddGb.CurrentValue} GB ({availableHddPercent.CurrentValue}%)";

//                if (availableHddGb.GeneralStatus == GeneralStatus.Danger || availableHddPercent.GeneralStatus == GeneralStatus.Danger)
//                {
//                    hddInfo = $"<b>{hddInfo}</b>";
//                }

//                panelData.Add(hddInfo);

//                var availableRamGb = status.StatusValues.Single(x => x.Name == StatusExtensions.AvailableRamGb);
//                var availableRamPercent = status.StatusValues.Single(x => x.Name == StatusExtensions.AvailableRamPercent);

//                string ramInfo = $"Available RAM: {availableRamGb.CurrentValue} GB ({availableRamPercent.CurrentValue}%)";

//                if (availableRamGb.GeneralStatus == GeneralStatus.Danger || availableRamPercent.GeneralStatus == GeneralStatus.Danger)
//                {
//                    ramInfo = $"<b>{ramInfo}</b>";
//                }

//                panelData.Add(ramInfo);

//                string nodeInfo = string.Join("<br>", panelData);

//                string panelStyling = "Success";

//                if (status.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger))
//                {
//                    panelStyling = "Danger";
//                }

//                if (command == null)
//                {
//                    viewBuilder.InfoPanel("nodePanel", Guid.NewGuid(), intoGroupId, Guid.Empty, $"{nodeLabel}", nodeInfo, nodeStyling, panelStyling);
//                }
//                else
//                {
//                    viewBuilder.InfoPanel<TFromPage, TToPage>("nodePanel", Guid.NewGuid(), intoGroupId, command, $"{nodeLabel}", nodeInfo, nodeStyling, panelStyling);
//                }
//            }
//        }

//        static void RenderApplicationPanel<TFromPage, TToPage>(
//            Deployment deployment,
//            HealthStatus healthStatus,
//            InfrastructureEventsList allInfrastructureEvents,
//            string applicationName,
//            Guid intoGroupId,
//            UI.Svelte.ViewBuilder viewBuilder,
//            Func<CommandContext, TFromPage, Guid, Task<TToPage>> command = null)
//        {
//            string applicationInfo = "Data not available!";
//            string styling = "Light";
//            string panelStyling = "Success";


//            IEnumerable<MdsCommon.ServiceConfigurationSnapshot> applicationServices = deployment.GetDeployedServices().Where(x => x.ApplicationName == applicationName);

//            int dangerServicesCount = 0;

//            foreach (var service in applicationServices)
//            {
//                var serviceStatus = StatusExtensions.GetServiceStatus(deployment, healthStatus, service, allInfrastructureEvents);
//                if (serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger || x.GeneralStatus == GeneralStatus.NoData))
//                {
//                    dangerServicesCount++;
//                }
//            }

//            string dangerServicesLabel = "(all ok)";

//            if (dangerServicesCount > 0)
//            {
//                dangerServicesLabel = $"<b>({dangerServicesCount} in error)</b>";
//                panelStyling = "Danger";
//            }

//            applicationInfo = $"{applicationServices.Count()} services {dangerServicesLabel} ";

//            if (command == null)
//            {
//                viewBuilder.InfoPanel("applicationPanel", Guid.NewGuid(), intoGroupId, Guid.Empty, $"{applicationName}", applicationInfo, styling, panelStyling);
//            }
//            else
//            {
//                viewBuilder.InfoPanel<TFromPage, TToPage>("applicationPanel", Guid.NewGuid(), intoGroupId, command, $"{applicationName}", applicationInfo, styling, panelStyling);
//            }
//        }

//        static void RenderServicePanel(
//            Deployment deployment,
//                    MdsCommon.HealthStatus healthStatus,
//                    MdsCommon.ServiceConfigurationSnapshot service,
//                    MdsCommon.InfrastructureEventsList allInfrastructureEvents,
//                    Guid intoGroupId,
//                    UI.Svelte.ViewBuilder viewBuilder)
//        {
//            FullStatus<MdsCommon.ServiceConfigurationSnapshot> serviceStatus = StatusExtensions.GetServiceStatus(deployment, healthStatus, service, allInfrastructureEvents);

//            //InfrastructureService service = serviceStatus.Entity;
//            //InfrastructureNode node = infrastructureConfiguration.InfrastructureNodes.ById(service.InfrastructureNodeId);

//            if (serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.NoData))
//            {
//                string serviceInfo = $"Infrastructure node: {service.NodeName}, service data not available!";
//                viewBuilder.InfoPanel("servicePanel", service.Id, intoGroupId, Guid.Empty, $"Service: {service.ServiceName}", serviceInfo, "Danger", "Default");
//            }
//            else
//            {
//                string serviceStyling = "Light";

//                var runningSince = serviceStatus.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.ServiceRunningSince);

//                List<string> statusRows = new List<string>();

//                if (serviceStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceRunningFor).GeneralStatus == GeneralStatus.Danger)
//                {
//                    statusRows.Add($"<b>SERVICE NOT RUNNING!</b>");
//                }
//                else
//                {
//                    if (serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger && x.Name == StatusExtensions.HasErrors))
//                    {
//                        statusRows.Add($"<b>SERVICE STATUS: ERROR</b> ");
//                    }
//                    statusRows.Add($"Running since {runningSince.CurrentValue} ({serviceStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceRunningFor).CurrentValue})");

//                    var lastChecked = serviceStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceSyncAgo);
//                    string lastCheckedLabel = $"(checked {lastChecked.CurrentValue} seconds ago)";
//                    if (lastChecked.GeneralStatus == GeneralStatus.Danger)
//                        lastCheckedLabel = $"<b>{lastCheckedLabel}</b>";

//                    var usedRam = serviceStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceUsedRam);
//                    string usedRamLabel = $"RAM: {usedRam.CurrentValue} MB";

//                    if (usedRam.GeneralStatus == GeneralStatus.Danger)
//                        usedRamLabel = $"<b>{usedRamLabel}</b>";

//                    statusRows.Add($"{usedRamLabel} {lastCheckedLabel}");
//                }

//                var startCount = serviceStatus.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.StartCount);
//                if (startCount != null && int.Parse(startCount.CurrentValue) > 1)
//                {
//                    string startedLabel = $"Started {startCount.CurrentValue} times since last configured";
//                    if (startCount.GeneralStatus == GeneralStatus.Danger)
//                    {
//                        startedLabel = $"<b>{startedLabel}</b>";
//                    }
//                    statusRows.Add(startedLabel);
//                }
//                else
//                {
//                    var crashCount = serviceStatus.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.CrashCount);
//                    if (crashCount != null)
//                    {
//                        string crashLabel = $"Stopped {crashCount.CurrentValue} times since last configured";
//                        if (crashCount.GeneralStatus == GeneralStatus.Danger)
//                            crashLabel = $"<b>{crashLabel}</b>";

//                        statusRows.Add(crashLabel);
//                    }
//                }

//                if (statusRows.Count() < 3)
//                    statusRows.Add("&nbsp;");

//                string serviceInfo = string.Join("<br>", statusRows);

//                string panelStyling = "Success";

//                if (serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger))
//                {
//                    panelStyling = "Danger";
//                }

//                viewBuilder.InfoPanel("servicePanel", service.Id, intoGroupId, Guid.Empty, $"{service.ServiceName}", serviceInfo, serviceStyling, panelStyling);
//            }
//        }

//        public static UI.Svelte.View RenderNodeStatusPage(NodeStatusPage nodesStatusPage, UI.Svelte.ViewBuilder viewBuilder)
//        {
//            var selectedNode = nodesStatusPage.GetSelected(nodesStatusPage.InfrastructureConfiguration.InfrastructureNodes);

//            var headerBuilder = viewBuilder.CreatePageHeader($"Deployment: {nodesStatusPage.GetLastDeployment().Timestamp.ItalianFormat()}, configuration name: {nodesStatusPage.GetLastDeployment().ConfigurationName}");
//            var backButton = headerBuilder.AddHeaderCommand<NodeStatusPage, StatusPage>(MdsCommon.MdsCommonFunctions.BackLabel, (context, model, id) => Status(context), Guid.Empty, true);
//            backButton.Styling = "Secondary";

//            var nodesGroup = viewBuilder.Group("grpNodes", Guid.Empty, UI.Svelte.ViewBuilder.RootGroupId, "Horizontal");

//            RenderNodePanel<StatusPage, NodeStatusPage>(selectedNode, nodesStatusPage.HealthStatus, nodesGroup.Id, viewBuilder);

//            var nodeServices = nodesStatusPage.GetDeployedServices().Where(x => x.NodeName == selectedNode.NodeName);

//            viewBuilder.Group("grpNodeSpacer", Guid.Empty, ViewBuilder.RootGroupId, "Horizontal");

//            Group servicesGroup = null;

//            int serviceIndex = 0;
//            foreach (var service in nodeServices)
//            {
//                if (serviceIndex % 4 == 0)
//                    servicesGroup = viewBuilder.Group("grpServices" + serviceIndex, Guid.Empty, UI.Svelte.ViewBuilder.RootGroupId, "Horizontal");

//                // hm.. type is not used
//                RenderServicePanel(
//                    nodesStatusPage.Deployment,
//                    nodesStatusPage.HealthStatus,
//                    service,
//                    nodesStatusPage.InfrastructureEvents,
//                    servicesGroup.Id,
//                    viewBuilder);
//                serviceIndex++;
//            }

//            if (serviceIndex % 4 != 0)
//            {
//                for (int i = 0; i < 4 - (serviceIndex % 4); i++)
//                {
//                    viewBuilder.Label("lblSpacer" + i, Guid.Empty, "", -1, servicesGroup.Id);
//                }
//            }

//            return viewBuilder.OutputView;
//        }

//        public static UI.Svelte.View RenderApplicationStatusPage(ApplicationStatusPage applicationStatusPage, UI.Svelte.ViewBuilder viewBuilder)
//        {
//            var selectedApplication = applicationStatusPage.GetSelected(applicationStatusPage.InfrastructureConfiguration.Applications);
//            var headerBuilder = viewBuilder.CreatePageHeader($"Deployment: {applicationStatusPage.GetLastDeployment().Timestamp.ItalianFormat()}, configuration name: {applicationStatusPage.GetLastDeployment().ConfigurationName}");
//            var backButton = headerBuilder.AddHeaderCommand<ApplicationStatusPage, StatusPage>(MdsCommon.MdsCommonFunctions.BackLabel, (context, model, id) => Status(context), Guid.Empty, true);
//            backButton.Styling = "Secondary";

//            RenderApplicationPanel<ApplicationStatusPage, ApplicationStatusPage>(
//                applicationStatusPage.Deployment,
//                applicationStatusPage.HealthStatus,
//                applicationStatusPage.InfrastructureEvents,
//                selectedApplication.Name,
//                ViewBuilder.RootGroupId,
//                viewBuilder);

//            viewBuilder.Group("grpAppSpacer", Guid.Empty, ViewBuilder.RootGroupId, "Horizontal");

//            Group servicesGroup = null;// viewBuilder.Group("grpServices", Guid.Empty, UI.Svelte.ViewBuilder.RootGroupId, "Vertical");

//            int serviceIndex = 0;
//            foreach (var service in applicationStatusPage.GetDeployedServices().Where(x => x.ApplicationName == selectedApplication.Name))
//            {
//                if (serviceIndex % 4 == 0)
//                    servicesGroup = viewBuilder.Group("grpServices" + serviceIndex, Guid.Empty, UI.Svelte.ViewBuilder.RootGroupId, "Horizontal");

//                // hm.. type is not used
//                RenderServicePanel(
//                    applicationStatusPage.Deployment,
//                    applicationStatusPage.HealthStatus,
//                    service,
//                    applicationStatusPage.InfrastructureEvents,
//                    servicesGroup.Id,
//                    viewBuilder);
//                serviceIndex++;
//            }
//            if (serviceIndex % 4 != 0)
//            {
//                for (int i = 0; i < 4 - (serviceIndex % 4); i++)
//                {
//                    viewBuilder.Label("lblSpacer" + i, Guid.Empty, "", -1, servicesGroup.Id);
//                }
//            }

//            return viewBuilder.OutputView;
//        }
//    }

//}