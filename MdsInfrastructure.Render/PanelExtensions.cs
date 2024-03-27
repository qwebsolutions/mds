using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using MdsCommon;
using System.Linq;
using System.Collections.Generic;
using System;
using MdsCommon.Controls;
using Metapsi.Html;

namespace MdsInfrastructure.Render
{
    public static class PanelExtensions
    {

        public const string RestartIcon = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" >\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0 3.181 3.183a8.25 8.25 0 0 0 13.803-3.7M4.031 9.865a8.25 8.25 0 0 1 13.803-3.7l3.181 3.182m0-4.991v4.99\" />\r\n</svg>\r\n";

        public static Var<IVNode> RenderNodePanel<TFromPage, TToPage>(
            this LayoutBuilder b,
            InfrastructureNode node,
            List<MdsCommon.MachineStatus> healthStatus)
        {
            string nodeName = node.NodeName;
            string nodeUrl = $"http://{node.MachineIp}:{node.UiPort}";

            Func<LayoutBuilder, Var<IVNode>> nodeHeader = (b) =>
            {
                return b.HtmlSpan(
                    b =>
                    {
                        b.SetClass("flex flex-row text-white");
                    },
                    b.HtmlA(
                        b =>
                        {
                            b.SetClass("hover:underline cursor-pointer font-bold");
                            b.SetHref(nodeUrl);
                        },
                        b.T(nodeName)),
                    b.HtmlImg(
                        b =>
                        {
                            b.SetClass("w-8 h-9 pl-2");
                            b.SetSrc("/server-icon.png");
                        }));
            };

            FullStatus<string> status = StatusExtensions.GetNodeStatus(healthStatus, nodeName);

            if (!healthStatus.Where(x => x.NodeName == nodeName).Any())
            {
                return b.InfoPanel(
                    b.Const(Panel.Style.Error),
                    nodeHeader,
                    (b) => b.T("Could not retrieve status"));
            }
            else if (Math.Abs((healthStatus.Where(x => x.NodeName == nodeName).Max(x => x.TimestampUtc) - DateTime.Now).TotalMinutes) > 1)
            {
                var timespan = healthStatus.Where(x => x.NodeName == nodeName).Max(x => x.TimestampUtc) - DateTime.Now;
                return b.InfoPanel(
                    b.Const(Panel.Style.Error),
                    nodeHeader,
                    b => b.T($"Status not received for {Convert.ToInt32(Math.Abs(timespan.TotalMinutes))} minutes!"));
            }
            else if (status.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.NoData))
            {
                return b.InfoPanel(
                    b.Const(Panel.Style.Error),
                    nodeHeader,
                    b => b.T("Data not available!"));
            }
            else
            {
                var availableHddGb = status.StatusValues.Single(x => x.Name == StatusExtensions.AvailableHddGb);
                var availableHddPercent = status.StatusValues.Single(x => x.Name == StatusExtensions.AvailableHddPercent);

                var hddInfo = b.HtmlDiv(
                    b =>
                    {
                        if (availableHddGb.GeneralStatus == GeneralStatus.Danger || availableHddPercent.GeneralStatus == GeneralStatus.Danger)
                        {
                            b.AddClass(b.Const("font-bold"));
                        }
                    }, 
                    b.T($"Available HDD: {availableHddGb.CurrentValue} GB ({availableHddPercent.CurrentValue}%)"));


                var availableRamGb = status.StatusValues.Single(x => x.Name == StatusExtensions.AvailableRamGb);
                var availableRamPercent = status.StatusValues.Single(x => x.Name == StatusExtensions.AvailableRamPercent);

                var ramInfo =b.HtmlDiv(
                    b=>
                    {
                        if (availableRamGb.GeneralStatus == GeneralStatus.Danger || availableRamPercent.GeneralStatus == GeneralStatus.Danger)
                        {
                            b.AddClass(b.Const("font-bold"));
                        }
                    },
                    b.T($"Available RAM: {availableRamGb.CurrentValue} GB ({availableRamPercent.CurrentValue}%)"));

                Panel.Style panelStyling = Panel.Style.Ok;

                if (status.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger))
                {
                    panelStyling = Panel.Style.Error;
                }

                var infoPanelContent = b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-col");
                    },
                    hddInfo,
                    ramInfo);

                return b.HtmlA(
                    b =>
                    {
                        b.SetHref(b.Const(Route.Path<Routes.Status.Node, string>(node.NodeName)));
                    },
                    b.InfoPanel(
                        b.Const(panelStyling),
                        nodeHeader,
                        b => infoPanelContent));
            }
        }

        public static Var<IVNode> RenderApplicationPanel<TFromPage, TToPage>(
            this LayoutBuilder b,
            MdsInfrastructure.Deployment deployment,
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

            Var<IVNode> statusLabel = b.T($"{applicationServices.Count()} services (all ok)");

            if (dangerServicesCount > 0)
            {
                statusLabel = b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("font-bold");
                    },
                    b.T($"{applicationServices.Count()} services ({dangerServicesCount} in error)"));
                panelStyling = Panel.Style.Error;
            }
           
            return b.HtmlA(
                b=>
                {
                    b.SetHref(b.Const(Route.Path<Routes.Status.Application, string>(applicationName)));
                },
                b.InfoPanel(
                b.Const(panelStyling),
                b => b.T(applicationName),
                b => statusLabel));
        }

        public static Var<IVNode> RenderServicePanel(
            this LayoutBuilder b,
            MdsInfrastructure.Deployment deployment,
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
                return b.InfoPanel(Panel.Style.Error, $"Service: {service.ServiceName}", serviceInfo, b.Const(Route.Path<Routes.Docs.Service, string>(service.ServiceName)));
            }
            else
            {
                Panel.Style panelStyle = Panel.Style.Ok;

                var statusRows = b.NewCollection<IVNode>();

                var runningSince = serviceStatus.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.ServiceRunningSince);

                if (serviceStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceRunningFor).GeneralStatus == GeneralStatus.Danger)
                {
                    return b.InfoPanel(Panel.Style.Error, $"{service.ServiceName}", "SERVICE NOT RUNNING!", b.Const(Route.Path<Routes.Docs.Service, string>(service.ServiceName)));
                }
                else
                {
                    if (serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger && x.Name == StatusExtensions.HasErrors))
                    {
                        b.Push(statusRows, b.HtmlSpanText(b => b.AddClass("font-bold"), $"SERVICE STATUS: ERROR"));
                    }

                    b.Push(statusRows, b.T($"Started {runningSince.CurrentValue} ({serviceStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceRunningFor).CurrentValue})"));

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

                    b.Push(statusRows, b.T($"{usedRamLabel} {lastCheckedLabel}"));
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
                    b.Push(statusRows, b.T(startedLabel));
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

                        b.Push(statusRows, b.T(crashLabel));
                    }
                }

                if (panelStyle == Panel.Style.Ok && serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger))
                {
                    panelStyle = Panel.Style.Error;
                }

                var headerDiv = b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row");
                    },
                    b.HtmlSpanText(b => b.SetClass("font-bold w-full"), service.ServiceName),
                    b.HtmlA(
                        b =>
                        {
                            b.SetClass("flex flex-row justify-end text-gray-100 w-6 h-6 hover:text-white");
                            b.SetHref(b.Const(Route.Path<Routes.Docs.Service, string>(service.ServiceName)));
                        },
                        b.Svg(Icon.Info, "w-full h-full")));

                return b.InfoPanel(
                    b.Const(panelStyle),
                    b => headerDiv,
                    b => b.HtmlDiv(b => b.SetClass("flex flex-col w-full"), statusRows));
            }
        }
    }
}
