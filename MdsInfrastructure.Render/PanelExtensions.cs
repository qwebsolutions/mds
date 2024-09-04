using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using MdsCommon;
using System.Linq;
using System.Collections.Generic;
using System;
using MdsCommon.Controls;
using Metapsi.Html;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MdsInfrastructure.Render
{
    public static class PanelExtensions
    {

        public const string RestartIcon = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" >\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0 3.181 3.183a8.25 8.25 0 0 0 13.803-3.7M4.031 9.865a8.25 8.25 0 0 1 13.803-3.7l3.181 3.182m0-4.991v4.99\" />\r\n</svg>\r\n";


        public static Var<string> GetStatusBackgroundColorClass(this SyntaxBuilder b, Var<string> statusCode)
        {
            return b.Switch(
                statusCode,
                b => b.Const(string.Empty),
                ("warning", b => b.Const("bg-amber-500")),
                ("error", b => b.Const("bg-red-500")),
                ("ok", b => b.Const("bg-green-500")));
        }

        public static Var<IVNode> NodePanel(this LayoutBuilder b, Var<NodePanelModel> nodePanel)
        {
            return b.HtmlDiv(
               b =>
               {
                   b.SetClass("flex flex-col rounded-md p-4 shadow-md border border-gray-100 text-gray-100");
                   b.AddClass(b.GetStatusBackgroundColorClass(b.Get(nodePanel, x => x.NodeStatusCode)));
               },
               b.HtmlDiv(
                   b =>
                   {
                       b.SetClass("pb-2");
                   },
                   b.HtmlSpan(
                    b =>
                    {
                        b.SetClass("flex flex-row text-white");
                    },
                    b.HtmlA(
                        b =>
                        {
                            b.SetClass("hover:underline cursor-pointer font-bold");
                            b.SetHref(b.Get(nodePanel, x => x.NodeUiUrl));
                        },
                        b.TextSpan(b.Get(nodePanel, x => x.NodeName)),
                    b.HtmlImg(
                        b =>
                        {
                            b.SetClass("w-8 h-9 pl-2");
                            StaticFiles.Add(typeof(PanelExtensions).Assembly, "server-icon.png");
                            b.SetSrc("/server-icon.png");
                        })))),
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-col");
                    },
                    b.Optional(
                        b.HasValue(b.Get(nodePanel, x => x.AvailableHddGb)),
                        b =>
                        {
                            return b.HtmlSpan(
                                b =>
                                {
                                    b.If(
                                        b.Get(nodePanel, x => x.HddWarning),
                                        b =>
                                        {
                                            b.AddClass("font-bold");
                                        });
                                },
                                b.Text(
                                    b.Concat(
                                        b.Const("Available HDD: "),
                                        b.AsString(b.Get(nodePanel, x => x.AvailableHddGb)),
                                        b.Const(" GB ("),
                                        b.AsString(b.Get(nodePanel, x => x.AvailableHddPercent)),
                                        b.Const("%)"))));
                        }),
                    b.Optional(
                        b.HasValue(b.Get(nodePanel, x => x.AvailableRamGb)),
                        b =>
                        {
                            return
                                                b.HtmlSpan(
                                                    b =>
                                                    {
                                                        b.If(
                                                            b.Get(nodePanel, x => x.RamWarning),
                                                            b =>
                                                            {
                                                                b.AddClass("font-bold");
                                                            });
                                                    },
                                                    b.Text(
                                                        b.Concat(
                                                            b.Const("Available RAM: "),
                                                            b.AsString(b.Get(nodePanel, x => x.AvailableRamGb)),
                                                            b.Const(" GB ("),
                                                            b.AsString(b.Get(nodePanel, x => x.AvailableRamPercent)),
                                                            b.Const("%)"))));
                        }),
                    b.Optional(
                        b.HasValue(b.Get(nodePanel, x => x.ErrorMessage)),
                        b =>
                        {
                            return b.HtmlSpan(b.Text(b.Get(nodePanel, x => x.ErrorMessage)));
                        })));
        }

        public static Var<IVNode> ApplicationPanel(this LayoutBuilder b, Var<ApplicationPanelModel> panelData)
        {
            return b.HtmlDiv(
               b =>
               {
                   b.SetClass("flex flex-col rounded-md p-4 shadow-md border border-gray-100 text-gray-100");
                   b.AddClass(b.GetStatusBackgroundColorClass(b.Get(panelData, x => x.StatusCode)));
               },
               b.HtmlSpan(
                   b =>
                   {
                   },
                   b.Text(b.Get(panelData, x => x.ApplicationName))),
               b.HtmlSpan(
                   b =>
                   {

                   },
                   b.Text(b.Get(panelData, x => x.StatusText))));
        }

        public static Var<IVNode> ServicePanelHeader(this LayoutBuilder b, Var<ServicePanelModel> serviceData)
        {
            return b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row");
                    },
                    b.HtmlSpan(b => b.SetClass("font-bold w-full"), b.Text(b.Get(serviceData, x => x.ServiceName))),
                    b.HtmlA(
                        b =>
                        {
                            b.SetClass("flex flex-row justify-end text-gray-100 w-6 h-6 hover:text-white");
                            b.SetHref(b.Url<Routes.Docs.Service, string>(b.Get(serviceData, x => x.ServiceName)));
                        },
                        b.Svg(Icon.Info, "w-full h-full")));
        }

        public static Var<IVNode> ServicePanel(this LayoutBuilder b, Var<ServicePanelModel> serviceData)
        {
            return b.If(
                b.Get(serviceData, x => !x.Enabled),
                b =>
                {
                    return b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-col rounded-md p-4 shadow-md border border-gray-100 text-gray-100");
                            b.AddClass("bg-gray-500");
                        },
                        b.ServicePanelHeader(serviceData),
                        b.HtmlSpan(b.Text(b.Get(serviceData, x => x.StatusText))));
                },
                b =>
                {
                    return b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-col rounded-md p-4 shadow-md border border-gray-100 text-gray-100");
                            b.AddClass(b.GetStatusBackgroundColorClass(b.Get(serviceData, x => x.StatusCode)));
                        },
                        b.ServicePanelHeader(serviceData),
                        b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("flex flex-col gap-1");
                            },
                            b.Optional(
                                b.HasValue(
                                    b.Get(serviceData, x => x.StartedTimeUtc)),
                                b => b.HtmlSpan(
                                    b.Text(
                                        b.Concat(
                                            b.Const("Started "),
                                            //b.Get(serviceData, x => x.StartedTimeUtc),
                                            b.ItalianFormat(b.ParseDate(b.Get(serviceData, x => x.StartedTimeUtc))),
                                            b.Const(" ("),
                                            b.FormatDistanceAgo(b.Get(serviceData, x => x.StartedTimeUtc), b.Const("en")),
                                            b.Const(")")
                                            )))),
                            b.Optional(
                                b.HasValue(
                                    b.Get(serviceData, x => x.RamMb)),
                                b => b.HtmlSpan(
                                    b =>
                                    {
                                        b.If(
                                            b.Get(serviceData, x => x.RamWarning),
                                            b =>
                                            {
                                                b.AddClass("font-semibold");
                                            });
                                    },
                                    b.Text(
                                        b.Concat(
                                            b.Const("RAM: "),
                                            b.Get(serviceData, x => x.RamMb),
                                            b.Const(" MB"))))),
                            b.Optional(
                                b.HasValue(b.Get(serviceData, x => x.StatusText)),
                                b => b.HtmlSpan(b.Text(b.Get(serviceData, x => x.StatusText))))));
                });
        }
        //public static Var<IVNode> RenderServicePanel(
        //    this LayoutBuilder b,
        //    MdsInfrastructure.Deployment deployment,
        //    List<MachineStatus> healthStatus,
        //    MdsCommon.ServiceConfigurationSnapshot service,
        //    List<InfrastructureEvent> allInfrastructureEvents)
        //{
        //    FullStatus<MdsCommon.ServiceConfigurationSnapshot> serviceStatus = StatusExtensions.GetServiceStatus(deployment, healthStatus, service, allInfrastructureEvents);

        //    if (!serviceStatus.Entity.Enabled)
        //    {
        //        return b.InfoPanel(Panel.Style.Warning, $"Service: {service.ServiceName}", "Disabled", b.Const(Route.Path<Routes.Docs.Service, string>(service.ServiceName)));
        //    }
        //    else if (serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.NoData))
        //    {
        //        string serviceInfo = $"Infrastructure node: {service.NodeName}, service data not available!";
        //        return b.InfoPanel(Panel.Style.Error, $"Service: {service.ServiceName}", serviceInfo, b.Const(Route.Path<Routes.Docs.Service, string>(service.ServiceName)));
        //    }
        //    else
        //    {
        //        Panel.Style panelStyle = Panel.Style.Ok;

        //        var statusRows = b.NewCollection<IVNode>();

        //        var runningSince = serviceStatus.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.ServiceRunningSince);

        //        if (serviceStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceRunningFor).GeneralStatus == GeneralStatus.Danger)
        //        {
        //            return b.InfoPanel(Panel.Style.Error, $"{service.ServiceName}", "SERVICE NOT RUNNING!", b.Const(Route.Path<Routes.Docs.Service, string>(service.ServiceName)));
        //        }
        //        else
        //        {
        //            if (serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger && x.Name == StatusExtensions.HasErrors))
        //            {
        //                b.Push(statusRows, b.HtmlSpanText(b => b.AddClass("font-bold"), $"SERVICE STATUS: ERROR"));
        //            }

        //            b.Push(statusRows, b.TextSpan($"Started {runningSince.CurrentValue} ({serviceStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceRunningFor).CurrentValue})"));

        //            var lastChecked = serviceStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceSyncAgo);
        //            string lastCheckedLabel = $"({lastChecked.CurrentValue} seconds ago)";
        //            //if (lastChecked.GeneralStatus == GeneralStatus.Danger)
        //            //    lastCheckedLabel = $"<b>{lastCheckedLabel}</b>";

        //            var usedRam = serviceStatus.StatusValues.Single(x => x.Name == StatusExtensions.ServiceUsedRam);
        //            string usedRamLabel = $"RAM: {usedRam.CurrentValue} MB";

        //            if (usedRam.GeneralStatus == GeneralStatus.Danger)
        //            {
        //                panelStyle = Panel.Style.Warning;
        //            }

        //            b.Push(statusRows, b.TextSpan($"{usedRamLabel} {lastCheckedLabel}"));
        //        }

        //        var startCount = serviceStatus.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.StartCount);
        //        if (startCount != null && int.Parse(startCount.CurrentValue) > 1)
        //        {
        //            string startedLabel = $"Started {startCount.CurrentValue} times since last configured";
        //            if (startCount.GeneralStatus == GeneralStatus.Danger && panelStyle == Panel.Style.Ok)
        //            {
        //                panelStyle = Panel.Style.Warning;
        //                //startedLabel = $"<b>{startedLabel}</b>";
        //            }
        //            b.Push(statusRows, b.TextSpan(startedLabel));
        //        }
        //        else
        //        {
        //            var crashCount = serviceStatus.StatusValues.SingleOrDefault(x => x.Name == StatusExtensions.CrashCount);
        //            if (crashCount != null)
        //            {
        //                string crashLabel = $"Stopped {crashCount.CurrentValue} times since last configured";
        //                if (crashCount.GeneralStatus == GeneralStatus.Danger && panelStyle == Panel.Style.Ok)
        //                {
        //                    panelStyle = Panel.Style.Warning;
        //                }

        //                b.Push(statusRows, b.TextSpan(crashLabel));
        //            }
        //        }

        //        if (panelStyle == Panel.Style.Ok && serviceStatus.StatusValues.Any(x => x.GeneralStatus == GeneralStatus.Danger))
        //        {
        //            panelStyle = Panel.Style.Error;
        //        }

        //        var headerDiv = b.HtmlDiv(
        //            b =>
        //            {
        //                b.SetClass("flex flex-row");
        //            },
        //            b.HtmlSpanText(b => b.SetClass("font-bold w-full"), service.ServiceName),
        //            b.HtmlA(
        //                b =>
        //                {
        //                    b.SetClass("flex flex-row justify-end text-gray-100 w-6 h-6 hover:text-white");
        //                    b.SetHref(b.Const(Route.Path<Routes.Docs.Service, string>(service.ServiceName)));
        //                },
        //                b.Svg(Icon.Info, "w-full h-full")));

        //        return b.InfoPanel(
        //            b.Const(panelStyle),
        //            b => headerDiv,
        //            b => b.HtmlDiv(b => b.SetClass("flex flex-col w-full"), statusRows));
        //    }
        //}
    }
}
