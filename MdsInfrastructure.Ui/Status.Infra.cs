﻿using Metapsi.Syntax;
using System.Collections.Generic;
using System.Linq;
using Metapsi;
using System;
using System.Threading.Tasks;
using MdsCommon;
using Metapsi.Hyperapp;
using Microsoft.AspNetCore.Http;

namespace MdsInfrastructure
{
    public static partial class Status
    {
        public static async Task<InfrastructureStatus> LoadStatus(CommandContext commandContext)
        {
            string validation = await commandContext.Do(Api.ValidateSchema);

            if(!string.IsNullOrEmpty(validation))
            {
                return new InfrastructureStatus()
                {
                    SchemaValidationMessage = validation
                };
            }

            return await commandContext.Do(Api.LoadInfraStatus);
        }

        public static async Task<IResponse> Infra(CommandContext commandContext, HttpContext requestData)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var statusPage = await LoadStatus(commandContext);
            var dbTime = sw.ElapsedMilliseconds;
            return Page.Response(statusPage,
                (b, clientModel) => b.Layout(b.InfraMenu(nameof(Status), requestData.User().IsSignedIn()),
                b.Render(b.Const(new Header.Props()
                {
                    Main = new Header.Title() { Operation = "Infrastructure status" },
                    User = requestData.User()
                })), b.Render(statusPage)));
        }

        public static Var<HyperNode> Render(this BlockBuilder b, InfrastructureStatus dataModel)
        {
            if (!string.IsNullOrEmpty(dataModel.SchemaValidationMessage))
            {
                var container = b.Div("text-red-500");
                b.Add(container, b.Bold(dataModel.SchemaValidationMessage));
                return container;
            }

            if (dataModel.Deployment == null)
            {
                return b.Text("No deployment yet! The infrastructure is not running any service!").As<HyperNode>();
            }

            var page = b.Div("flex flex-col space-y-4");

            int totalServices = dataModel.Deployment.GetDeployedServices().Count();
            int totalNodes = dataModel.Deployment.GetDeployedServices().Select(x => x.NodeName).Distinct().Count();

            b.Add(
                page,
                b.InfoPanel(
                    Panel.Style.Info,
                    $"Last deployment: {dataModel.Deployment.ConfigurationName}",
                    $"{dataModel.Deployment.Timestamp.ItalianFormat()}, total services {totalServices}, total infrastructure nodes {totalNodes}"));

            var nodesContainer = b.Add(page, b.PanelsContainer(4));

            foreach (var nodeName in dataModel.Deployment.GetDeployedServices().Select(x => x.NodeName).Distinct())
            {
                var node = dataModel.InfrastructureNodes.Single(x => x.NodeName == nodeName);
                var nodePanel = b.RenderNodePanel<InfrastructureStatus, InfrastructureStatus>(node, dataModel.HealthStatus);
                b.Add(nodesContainer, nodePanel);
            }

            Var<HyperNode> appsContainer = b.Add(page, b.PanelsContainer(4));

            foreach (var applicationName in dataModel.Deployment.GetDeployedServices().Select(x => x.ApplicationName).Distinct())
            {
                var appPanel = b.Add(appsContainer, b.RenderApplicationPanel<InfrastructureStatus, InfrastructureStatus>(
                    dataModel.Deployment,
                    dataModel.HealthStatus,
                    dataModel.InfrastructureEvents,
                    applicationName));
            }

            // TODO: Not displayed
            //var events = dataModel.InfrastructureEvents.GetRecentEvents();

            //var fatalEvents = events.Where(x => x.Criticality == MdsCommon.InfrastructureEventCriticality.Fatal);
            //var criticalEvents = events.Where(x => x.Criticality == MdsCommon.InfrastructureEventCriticality.Critical);

            return page;
        }

        public static Var<HyperNode> RenderNodePanel<TFromPage, TToPage>(
            this BlockBuilder b,
            InfrastructureNode node,
            List<MdsCommon.MachineStatus> healthStatus)
        {
            string nodeName = node.NodeName;
            string nodeUrl = $"http://{node.MachineIp}:{node.UiPort}";

            Func<BlockBuilder, Var<HyperNode>> nodeHeader = (b) =>
            {
                var header = b.Span("flex flex-row text-white");
                var link = b.Add(header, b.Node("a", "hover:underline cursor-pointer font-bold"));
                b.SetAttr(link, Html.href, b.Const(nodeUrl));
                b.Add(link, b.Text(nodeName));

                var img = b.Add(header, b.Node("img", "w-8 h-9 pl-2"));
                b.SetAttr(img, Html.src, "/server-icon.png");
                return header;
            };

            //string nodeLabel = $"<a href='{nodeUrl}' style='color:white'>{nodeName}</a> <img src=\"/server-icon.png\" style=\"width:1.5em;height:2em;padding-left:10px;\"/>";
            //nodeName = $"{nodeName} <object data=\"ServerIcon.svg\" type=\"image / svg + xml\" style=\"width:1.5em;height:2em;padding-left:10px;\">";

            FullStatus<string> status = StatusExtensions.GetNodeStatus(healthStatus, nodeName);

            if (!healthStatus.Where(x => x.NodeName == nodeName).Any())
            {
                return b.InfoPanel(
                    b.Const(Panel.Style.Error),
                    nodeHeader,
                    (b) => b.Text("Could not retrieve status"));
            }
            else if (Math.Abs((healthStatus.Where(x => x.NodeName == nodeName).Max(x => x.TimestampUtc) - DateTime.Now).TotalMinutes) > 1)
            {
                var timespan = healthStatus.Where(x => x.NodeName == nodeName).Max(x => x.TimestampUtc) - DateTime.Now;
                return b.InfoPanel(
                    b.Const(Panel.Style.Error),
                    nodeHeader,
                    b => b.Text($"Status not received for {Convert.ToInt32(Math.Abs(timespan.TotalMinutes))} minutes!"));
            }
            else if (status.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.NoData))
            {
                return b.InfoPanel(
                    b.Const(Panel.Style.Error),
                    nodeHeader,
                    b => b.Text("Data not available!"));
            }
            else
            {
                var infoPanelContent = b.Div("flex flex-col");

                var availableHddGb = status.StatusValues.Single(x => x.Name == StatusExtensions.AvailableHddGb);
                var availableHddPercent = status.StatusValues.Single(x => x.Name == StatusExtensions.AvailableHddPercent);

                var hddInfo = b.Add(infoPanelContent, b.Div());
                b.Add(hddInfo, b.Text($"Available HDD: {availableHddGb.CurrentValue} GB ({availableHddPercent.CurrentValue}%)"));

                if (availableHddGb.GeneralStatus == GeneralStatus.Danger || availableHddPercent.GeneralStatus == GeneralStatus.Danger)
                {
                    b.AddClass(hddInfo, b.Const("font-bold"));
                }

                var availableRamGb = status.StatusValues.Single(x => x.Name == StatusExtensions.AvailableRamGb);
                var availableRamPercent = status.StatusValues.Single(x => x.Name == StatusExtensions.AvailableRamPercent);

                var ramInfo = b.Add(infoPanelContent, b.Div());
                b.Add(ramInfo, b.Text($"Available RAM: {availableRamGb.CurrentValue} GB ({availableRamPercent.CurrentValue}%)"));

                if (availableRamGb.GeneralStatus == GeneralStatus.Danger || availableRamPercent.GeneralStatus == GeneralStatus.Danger)
                {
                    b.AddClass(ramInfo, b.Const("font-bold"));
                }

                //string nodeInfo = string.Join("<br>", panelData);

                Panel.Style panelStyling = Panel.Style.Ok;

                if (status.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger))
                {
                    panelStyling = Panel.Style.Error;
                }

                var asLink = b.Node("a");
                b.SetAttr(asLink, Html.href, b.Url(Status.Node, b.Const(node.Id.ToString())));

                b.Add(asLink,
                    b.InfoPanel(
                        b.Const(panelStyling),
                        nodeHeader,
                        b => infoPanelContent));

                return asLink;
            }
        }

        public static Var<HyperNode> RenderApplicationPanel<TFromPage, TToPage>(
            this BlockBuilder b,
            Deployment deployment,
            List<MachineStatus> healthStatus,
            List<InfrastructureEvent> allInfrastructureEvents,
            string applicationName)
        {
            Panel.Style panelStyling = Panel.Style.Ok;

            var applicationServices = deployment.GetDeployedServices().Where(x => x.ApplicationName == applicationName);

            int dangerServicesCount = 0;

            foreach (var service in applicationServices)
            {
                var serviceStatus = StatusExtensions.GetServiceStatus(deployment, healthStatus, service, allInfrastructureEvents);
                if (serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger || x.GeneralStatus == GeneralStatus.NoData))
                {
                    dangerServicesCount++;
                }
            }

            Var<HyperNode> statusLabel = b.Text($"{applicationServices.Count()} services (all ok)");

            if (dangerServicesCount > 0)
            {
                statusLabel = b.Div("font-bold");
                b.Add(statusLabel, b.Text($"{applicationServices.Count()} services ({dangerServicesCount} in error)"));
                panelStyling = Panel.Style.Error;
            }

            var asLink = b.Node("a");
            b.SetAttr(asLink, Html.href, b.Url(Status.Application, b.Const(applicationName)));

            b.Add(asLink,
                b.InfoPanel(
                b.Const(panelStyling),
                b => b.Text(applicationName),
                b => statusLabel));


            return asLink;
        }

        public static Var<HyperNode> RenderServicePanel(
            this BlockBuilder b,
            Deployment deployment,
            List<MachineStatus> healthStatus,
            MdsCommon.ServiceConfigurationSnapshot service,
            List<InfrastructureEvent> allInfrastructureEvents)
        {
            FullStatus<MdsCommon.ServiceConfigurationSnapshot> serviceStatus = StatusExtensions.GetServiceStatus(deployment, healthStatus, service, allInfrastructureEvents);

            //InfrastructureService service = serviceStatus.Entity;
            //InfrastructureNode node = infrastructureConfiguration.InfrastructureNodes.ById(service.InfrastructureNodeId);

            if (serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.NoData))
            {
                string serviceInfo = $"Infrastructure node: {service.NodeName}, service data not available!";
                return b.InfoPanel(Panel.Style.Error, $"Service: {service.ServiceName}", serviceInfo, b.Url(Docs.Service, b.Const(service.ServiceName)));
            }
            else
            {
                Panel.Style panelStyle = Panel.Style.Ok;

                var statusRows = b.Div("flex flex-col");

                var runningSince = serviceStatus.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.ServiceRunningSince);

                if (serviceStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceRunningFor).GeneralStatus == GeneralStatus.Danger)
                {
                    return b.InfoPanel(Panel.Style.Error, $"{service.ServiceName}", "SERVICE NOT RUNNING!", b.Url(Docs.Service, b.Const(service.ServiceName)));
                }
                else
                {
                    if (serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger && x.Name == StatusExtensions.HasErrors))
                    {
                        b.Add(statusRows, b.Bold($"SERVICE STATUS: ERROR"));
                    }
                    b.Add(statusRows, b.Text($"Started {runningSince.CurrentValue} ({serviceStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceRunningFor).CurrentValue})"));

                    var lastChecked = serviceStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceSyncAgo);
                    string lastCheckedLabel = $"({lastChecked.CurrentValue} seconds ago)";
                    //if (lastChecked.GeneralStatus == GeneralStatus.Danger)
                    //    lastCheckedLabel = $"<b>{lastCheckedLabel}</b>";

                    var usedRam = serviceStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceUsedRam);
                    string usedRamLabel = $"RAM: {usedRam.CurrentValue} MB";

                    if (usedRam.GeneralStatus == GeneralStatus.Danger)
                    {
                        panelStyle = Panel.Style.Warning;
                    }

                    b.Add(statusRows, b.Text($"{usedRamLabel} {lastCheckedLabel}"));
                }

                var startCount = serviceStatus.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.StartCount);
                if (startCount != null && int.Parse(startCount.CurrentValue) > 1)
                {
                    string startedLabel = $"Started {startCount.CurrentValue} times since last configured";
                    if (startCount.GeneralStatus == GeneralStatus.Danger && panelStyle == Panel.Style.Ok)
                    {
                        panelStyle = Panel.Style.Warning;
                        //startedLabel = $"<b>{startedLabel}</b>";
                    }
                    b.Add(statusRows, b.Text(startedLabel));
                }
                else
                {
                    var crashCount = serviceStatus.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.CrashCount);
                    if (crashCount != null)
                    {
                        string crashLabel = $"Stopped {crashCount.CurrentValue} times since last configured";
                        if (crashCount.GeneralStatus == GeneralStatus.Danger && panelStyle == Panel.Style.Ok)
                        {
                            panelStyle = Panel.Style.Warning;
                        }

                        b.Add(statusRows, b.Text(crashLabel));
                    }
                }

                if (panelStyle == Panel.Style.Ok && serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger))
                {
                    panelStyle = Panel.Style.Error;
                }

                var headerDiv = b.Div("flex flex-row");
                var serviceNameSpan = b.Add(headerDiv, b.Bold(service.ServiceName));
                b.AddClass(serviceNameSpan, "w-full");

                var a = b.Add(headerDiv, b.Node("a", "flex flex-row justify-end text-gray-100 w-6 h-6 hover:text-white"));
                b.SetAttr(a, Html.href, b.Url(Docs.Service, b.Const(service.ServiceName)));
                b.Add(a, b.Svg(Icon.Info, "w-full h-full"));

                return b.InfoPanel(
                    b.Const(panelStyle),
                    b => headerDiv,
                    b => statusRows);
            }
        }
    }
}

