using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.Html;
using System.Collections.Generic;
using Metapsi.Shoelace;
using Metapsi.SignalR;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MdsInfrastructure.Render
{
    public static class Deployment
    {
        public class List
        {
            public static Var<IVNode> Render(LayoutBuilder b, DeploymentHistory serverModel, Var<DeploymentHistory> clientModel)
            {
                b.AddModuleStylesheet();

                var headerProps = b.GetHeaderProps(b.Const("Deployments"), b.Const(string.Empty), b.Get(clientModel, x => x.User));

                return b.Layout(
                    b.InfraMenu(nameof(Routes.Deployment), serverModel.User.IsSignedIn()),
                    b.Render(headerProps),
                Render(b, serverModel)).As<IVNode>();
            }

            public static Var<IVNode> Render(LayoutBuilder b, DeploymentHistory serverModel)
            {
                var deploymentsTableBuilder = MdsDefaultBuilder.DataTable<MdsInfrastructure.Deployment>();
                deploymentsTableBuilder.OverrideHeaderCell(
                    nameof(MdsInfrastructure.Deployment.Timestamp),
                    b => b.Text("Deployment timestamp"));
                deploymentsTableBuilder.OverrideDataCell(
                    nameof(MdsInfrastructure.Deployment.Timestamp),
                    (b, deployment) =>
                    {
                        var dateStringLocale = b.ItalianFormat(b.Get(deployment, x => x.Timestamp));
                        return b.Link(dateStringLocale, b.Url<Routes.Deployment.Review, Guid>(b.Get(deployment, x => x.Id)));
                    });

                deploymentsTableBuilder.OverrideHeaderCell(nameof(MdsInfrastructure.Deployment.ConfigurationName), b => b.Text("Configuration name"));

                return b.MdsMainPanel(b => { },
                    b.DataTable(
                        deploymentsTableBuilder,
                        b.Const(serverModel.Deployments.ToList()),
                        nameof(MdsInfrastructure.Deployment.Timestamp),
                        nameof(MdsInfrastructure.Deployment.ConfigurationName)));

            }
        }

        public class Review
        {
            public static void Render(HtmlBuilder b, DeploymentReview serverModel)
            {
                b.BodyAppend(
                    b.DeploymentToasts(),
                    b.Hyperapp(
                        b => b.MakeInit(
                            b.MakeStateWithEffects(
                                b.Const(serverModel),
                                b.SignalRConnect(
                                    DefaultMetapsiSignalRHub.Path, 
                                    null, 
                                    (b, dispatch) =>
                                    {
                                        b.DispatchCustomEvent(b.NewObj<RefreshDeploymentReviewModel>());
                                    }),
                                b.MakeEffect((SyntaxBuilder b) =>
                                {
                                    if (serverModel.DeploymentInProgress)
                                    {
                                        b.ShowDeploymentToast(MdsCommon.Controls.Controls.IdDeploymentStartedToast);
                                    }
                                }))),
                        (LayoutBuilder b, Var<DeploymentReview> model) => RenderClient(b, serverModel, model),
                        (b, model) => b.Listen(b.MakeAction((SyntaxBuilder b, Var<MdsInfrastructure.DeploymentReview> model, Var<RefreshDeploymentReviewModel> e) =>
                        {
                            return b.MakeStateWithEffects(
                                model, 
                                b.RefreshModelEffect<MdsInfrastructure.DeploymentReview>(
                                    b.AsString(b.Get(model, x => x.Deployment.Id)),
                                    b.MakeAction((SyntaxBuilder b, Var<DeploymentReview> model, Var<DeploymentReview> newModel) =>
                                    {
                                        return b.MakeStateWithEffects(
                                            newModel,
                                            b =>
                                            {
                                                b.If(b.And(
                                                    b.Get(model, x => x.DeploymentInProgress),
                                                    b.Get(newModel, x => !x.DeploymentInProgress)),
                                                    b =>
                                                    {
                                                        b.ShowDeploymentToast(MdsCommon.Controls.Controls.IdDeploymentSuccessToast);
                                                    });
                                            });
                                    })));
                        }))));
            }

            public static Var<IVNode> RenderClient(LayoutBuilder b, DeploymentReview serverModel, Var<DeploymentReview> clientModel)
            {
                b.AddModuleStylesheet();

                var selectedDeployment = serverModel.Deployment.Timestamp.ItalianFormat();

                var headerProps = b.GetHeaderProps(b.Const("Deployment"), b.Const(selectedDeployment), b.Get(clientModel, x => x.User));

                return b.LayoutWithProgressBar(
                    b.InfraMenu(
                        nameof(Routes.Deployment),
                        serverModel.User.IsSignedIn()),
                    b.Render(headerProps),
                    b.Optional(
                        b.Get(clientModel, x => x.DeploymentInProgress),
                        b =>
                        {
                            return b.SlProgressBar(b =>
                            {
                                b.SetIndeterminate();
                                b.AddStyle("--height", "1px");
                            });
                        }),

                    b.ReviewDeployment(clientModel));
            }
        }

        public class Preview
        {
            public static Var<IVNode> Render(LayoutBuilder b, DeploymentPreview serverModel, Var<DeploymentPreview> clientModel)
            {
                b.AddModuleStylesheet();

                var selectedDeployment = serverModel.Deployment.Timestamp.ItalianFormat();

                var headerProps = b.GetHeaderProps(
                    b.Const("Review deployment"),
                    b.Const(selectedDeployment),
                    b.Get(clientModel, x => x.User));

                return b.Layout(
                    b.InfraMenu(
                        nameof(Routes.Deployment),
                        serverModel.User.IsSignedIn()),
                        b.Render(headerProps),
                        RenderDeploymentReport(b, b.Get(clientModel, x => x.ChangesReport), b.Get(clientModel, x => x.SavedConfiguration)));
            }
        }

        public static Var<IVNode> DeployNowButton<TModel>(this LayoutBuilder b, Var<Guid> configurationId)
        {
            return b.HtmlButton(
                b =>
                {
                    b.SetClass("bg-red-500 rounded px-4 py-2 text-white");
                    b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<TModel> model) =>
                    {
                        return b.MakeStateWithEffects(
                            model,
                            b.GetJson<DeploymentPreview, Guid>(
                                b.GetApiUrl(Frontend.ConfirmDeployment, b.AsString(configurationId)),
                                b.MakeAction((SyntaxBuilder b, Var<DeploymentPreview> model, Var<Guid> deploymentId) =>
                                {
                                    b.SetUrl(b.Url<Routes.Deployment.Review, Guid>(deploymentId));
                                    return model;
                                }),
                                b.MakeAction((SyntaxBuilder b, Var<DeploymentPreview> model, Var<ClientSideException> ex) =>
                                {
                                    b.Alert(ex);
                                    return model;
                                })));
                    }));
                },
                b.TextSpan("Deploy now"));
        }

        public static Var<IVNode> RenderDeploymentReport(
            this LayoutBuilder b,
            Var<ChangesReport> changesReport,
            Var<InfrastructureConfiguration> infrastructureConfiguration)
        {
            StaticFiles.Add(typeof(Render.Deployment).Assembly, "nowork.png");
            var hasServiceChanges = b.If(
                b.Not(b.Get(changesReport, x => x.ServiceChanges.Any())),
                b => b.Const(false),
                b => b.If(
                    b.Get(changesReport, x => x.ServiceChanges.All(x => x.ServiceChangeType == ChangeType.None)),
                    b => b.Const(false),
                    b => b.Const(true)));

            return b.If(
                b.Not(hasServiceChanges),
                b =>
                {
                    return b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-col w-full h-full items-center justify-center");
                        },
                        b.HtmlImg(
                            b =>
                            {
                                b.SetSrc("/nowork.png");
                            }),
                        b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("flex flex-row space-x-2");
                            },
                            b.TextSpan("There are no changes to deploy."),
                            b.Link(b.Const("Edit"), b.Url<Routes.Configuration.Edit, Guid>(b.Get(infrastructureConfiguration, x => x.Id)))));
                },
                b =>
                {
                    var reviewConfigurationUrl = b.Url<Routes.Configuration.Review, Guid>(b.Get(infrastructureConfiguration, x => x.Id));
                    var swapIcon = Icon.Swap;

                    return b.HtmlDiv(
                        b =>
                        {

                        },
                        b.Toolbar(
                            b =>
                            {
                                b.AddClass("justify-end");
                            },
                            b.NavigateButton(
                                b =>
                                {
                                    b.Set(x => x.Label, "Review configuration");
                                    b.Set(x => x.Href, reviewConfigurationUrl);
                                    b.Set(x => x.SvgIcon, swapIcon);
                                }),
                            b.DeployNowButton<DeploymentPreview>(b.Get(infrastructureConfiguration, x => x.Id))),
                        b.ChangesReport(
                            b.Get(changesReport, x => x.ServiceChanges.Where(x=>x.ServiceChangeType != ChangeType.None).ToList()),
                            b.NewCollection<DbDeploymentEvent>()));
                });
        }

        public static Var<IVNode> ReviewDeployment(this LayoutBuilder b, Var<DeploymentReview> model)
        {
            var view = b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col");
                },
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row justify-end");
                    }),
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("py-4");
                    },
                    b.ChangesReport(
                        b.Get(model, x => x.ChangesReport.ServiceChanges),
                        b.Get(model, x => x.DeploymentEvents))));

            return view;
        }
    }

    public static class DeploymentControls
    {

        const string CloudArrowUp = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"w-6 h-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M12 16.5V9.75m0 0 3 3m-3-3-3 3M6.75 19.5a4.5 4.5 0 0 1-1.41-8.775 5.25 5.25 0 0 1 10.233-2.33 3 3 0 0 1 3.758 3.848A3.752 3.752 0 0 1 18 19.5H6.75Z\" />\r\n</svg>\r\n";
        const string ArrowPath = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"w-6 h-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0 3.181 3.183a8.25 8.25 0 0 0 13.803-3.7M4.031 9.865a8.25 8.25 0 0 1 13.803-3.7l3.181 3.182m0-4.991v4.99\" />\r\n</svg>\r\n";
        const string XMark = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"w-6 h-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M6 18 18 6M6 6l12 12\" />\r\n</svg>\r\n";
        const string Server = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M21.75 17.25v-.228a4.5 4.5 0 0 0-.12-1.03l-2.268-9.64a3.375 3.375 0 0 0-3.285-2.602H7.923a3.375 3.375 0 0 0-3.285 2.602l-2.268 9.64a4.5 4.5 0 0 0-.12 1.03v.228m19.5 0a3 3 0 0 1-3 3H5.25a3 3 0 0 1-3-3m19.5 0a3 3 0 0 0-3-3H5.25a3 3 0 0 0-3 3m16.5 0h.008v.008h-.008v-.008Zm-3 0h.008v.008h-.008v-.008Z\" />\r\n</svg>\r\n";
        const string Play = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M5.25 5.653c0-.856.917-1.398 1.667-.986l11.54 6.347a1.125 1.125 0 0 1 0 1.972l-11.54 6.347a1.125 1.125 0 0 1-1.667-.986V5.653Z\" />\r\n</svg>\r\n";
        const string PlayCircle = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"w-6 h-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z\" />\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M15.91 11.672a.375.375 0 0 1 0 .656l-5.603 3.113a.375.375 0 0 1-.557-.328V8.887c0-.286.307-.466.557-.327l5.603 3.112Z\" />\r\n</svg>\r\n";
        const string Pause = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"w-6 h-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M15.75 5.25v13.5m-7.5-13.5v13.5\" />\r\n</svg>\r\n";
        const string PauseCircle = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"w-6 h-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M14.25 9v6m-4.5 0V9M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z\" />\r\n</svg>\r\n";
        const string Stop = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"w-6 h-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M5.25 7.5A2.25 2.25 0 0 1 7.5 5.25h9a2.25 2.25 0 0 1 2.25 2.25v9a2.25 2.25 0 0 1-2.25 2.25h-9a2.25 2.25 0 0 1-2.25-2.25v-9Z\" />\r\n</svg>\r\n";
        const string StopCircle = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"w-6 h-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z\" />\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M9 9.563C9 9.252 9.252 9 9.563 9h4.874c.311 0 .563.252.563.563v4.874c0 .311-.252.563-.563.563H9.564A.562.562 0 0 1 9 14.437V9.564Z\" />\r\n</svg>\r\n";

        public static Var<IVNode> ChangesReport(this LayoutBuilder b, Var<List<ServiceChange>> serviceChanges, Var<List<DbDeploymentEvent>> deploymentEvents)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-row flex-wrap gap-4 p-4 ");
                },
                b.Map(
                    serviceChanges,
                    (b, change) =>
                    {
                        var serviceEvents = b.Get(deploymentEvents, change, (deploymentEvents, change) => deploymentEvents.Where(x => x.ServiceName == change.ServiceName).ToList());
                        //var serviceStatus = b.Get(servicesState, change, (servicesState, change) => servicesState.SingleOrDefault(x => x.ServiceName == change.ServiceName));
                        return b.Switch(
                            b.Get(change, x => x.ServiceChangeType),
                            b => b.NewService(change, serviceEvents),
                            (ChangeType.None, b => b.VoidNode()),
                            (ChangeType.Added, b => b.NewService(change, serviceEvents)),
                            (ChangeType.Changed, b => b.ChangedService(change, serviceEvents)),
                            (ChangeType.Removed, b => b.RemovedService(change)));
                    }));
        }

        public static Var<IVNode> DefaultNewServiceStatus(this LayoutBuilder b, Var<ServiceChange> serviceChange)
        {
            b.Log("serviceChange", serviceChange);
            return b.If(
                b.Get(serviceChange, x => x.Enabled.NewValue == "False"),
                b => b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row gap-2 text-red-800");
                    },
                    b.Svg(PauseCircle, "w-6 h-6"),
                    b.Text($"Service disabled, will not be started")),
                b => b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row gap-2 text-green-600");
                    },
                    b.Svg(Play, "w-6 h-6"),
                    b.Text($"Service will be started")));
        }

        public static Var<IVNode> LiveServiceStatus(this LayoutBuilder b, Var<List<DbDeploymentEvent>> deploymentEvents)
        {
            var lastEvent = b.Get(deploymentEvents, x => x.Where(x => x.EventType == nameof(DeploymentEvent.ServiceStart) || x.EventType == nameof(DeploymentEvent.ServiceStop)).OrderByDescending(x => x.TimestampIso).FirstOrDefault());
            return b.If(
                b.Not(b.HasObject(lastEvent)),
                b =>
                {
                    return b.HtmlDiv(b.Text("Waiting for status..."));
                },
                b => b.If(
                    b.Get(lastEvent, x => x.EventType == nameof(DeploymentEvent.ServiceStop)),
                    b =>
                    {
                        return b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("flex flex-row gap-2 text-gray-800");
                            },
                            b.Svg(StopCircle, "w-6 h-6"),
                            b.Text($"Service stopped"));
                    },
                    b =>
                    {
                        return b.If(
                            b.Get(lastEvent, x => x.EventType == nameof(DeploymentEvent.ServiceStart)),
                            b =>
                            {
                                return b.HtmlDiv(
                                    b =>
                                    {
                                        b.SetClass("flex flex-row gap-2 text-green-800");
                                    },
                                    b.Svg(PlayCircle, "w-6 h-6"),
                                    b.Text($"Service started"));
                            },
                            b => b.HtmlDiv(b.Text("Waiting for status...")));
                    }));
        }

        public static Var<IVNode> NewService(this LayoutBuilder b, Var<ServiceChange> serviceChange, Var<List<DbDeploymentEvent>> serviceDeploymentEvents)
        {
            var processStatus = b.If(
                b.Get(serviceDeploymentEvents, x=>x.Any()),
                b => b.LiveServiceStatus(serviceDeploymentEvents),
                b => b.DefaultNewServiceStatus(serviceChange));

            return b.ServiceChangeCard(
                b.Get(serviceChange, x => x.ServiceName),
                b.Get(serviceChange, x => x.NodeName.NewValue),
                b.Const("bg-green-100"),
                b.Const("bg-green-200"),
                b.ListParameterChanges(serviceChange),
                b.ListBinariesChanges(serviceChange),
                processStatus);
        }

        public static Var<IVNode> RemovedService(this LayoutBuilder b, Var<ServiceChange> serviceChange)
        {
            return b.ServiceChangeCard(
                b.Get(serviceChange, x => x.ServiceName),
                b.Get(serviceChange, x => x.NodeName.OldValue),
                b.Const("bg-red-100"),
                b.Const("bg-red-200"),
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row gap-2 text-red-800");
                    },
                    b.Svg(XMark),
                    b.Text($"Service deleted, will be stopped and uninstalled")));
        }

        public static Var<IVNode> ServiceChangeCard(
            this LayoutBuilder b,
            Var<string> serviceName,
            Var<string> nodeName,
            Var<string> bgColor,
            Var<string> titleBgColor,
            params Var<IVNode>[] children)
        {
            var container = b.HtmlDiv(
               b =>
               {
                   b.SetClass($"flex flex-col rounded text-sky-800 min-w-[24rem]");
                   b.AddClass(bgColor);
               },
               b.HtmlSpan(
                   b =>
                   {
                       b.SetClass($"flex flex-row items-center justify-between p-4 rounded-t");
                       b.AddClass(titleBgColor);
                   },
                   b.HtmlSpanText(
                       b =>
                       {
                           b.SetClass($"font-semibold");
                       },
                       serviceName),
                   b.HtmlSpan(
                       b =>
                       {
                           b.SetClass("flex flex-row gap-1 items-center");
                       },
                       b.Svg(Server, "w-5 h-5"),
                       b.HtmlSpanText(
                           b =>
                           {
                               b.SetClass($" text-sm");
                           },
                           nodeName))),
               b.HtmlDiv(
                   b =>
                   {
                       b.SetClass("flex flex-col gap-2 p-4");
                   },
                   children
                   ));

            return container;
        }

        public static Var<IVNode> DefaultChangedServiceStatus(this LayoutBuilder b, Var<ServiceChange> serviceChange)
        {
            var shouldEnable = b.Get(serviceChange, serviceChange => serviceChange.Enabled.NewValue == "True" && serviceChange.Enabled.OldValue == "False");
            var shouldDisable = b.Get(serviceChange, serviceChange => serviceChange.Enabled.NewValue == "False" && serviceChange.Enabled.OldValue == "True");
            var isDisabled = b.Get(serviceChange, serviceChange => serviceChange.Enabled.NewValue == "False" && serviceChange.Enabled.OldValue == "False");

            var processChanges = b.If(
                shouldEnable,
                b =>
                {
                    return b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-row gap-2 text-green-600");
                        },
                        b.Svg(ArrowPath),
                        b.Text($"Service enabled, process will be started"));
                },
                b => b.If(
                    shouldDisable,
                    b =>
                    {
                        return b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("flex flex-row gap-2 text-red-800");
                            },
                            b.Svg(StopCircle),
                            b.Text($"Service disabled, process will be stopped"));
                    },
                    b =>
                    {
                        return b.If(
                            isDisabled,
                            b =>
                            {
                                return b.HtmlDiv(
                                    b =>
                                    {
                                        b.SetClass("flex flex-row gap-2 text-red-800");
                                    },
                                    b.Svg(PauseCircle, "w-6 h-6"),
                                    b.Text($"Service disabled, will not be started"));
                            },
                            b =>
                            {
                                return b.HtmlDiv(
                                    b =>
                                    {
                                        b.SetClass("flex flex-row gap-2");
                                    },
                                    b.Svg(ArrowPath),
                                    b.Text($"Process will be restarted"));
                            });
                    }));

            return processChanges;
        }

        public static Var<IVNode> ChangedService(this LayoutBuilder b, Var<ServiceChange> serviceChange, Var<List<DbDeploymentEvent>> deploymentEvents)
        {
            var serviceStatus = b.If(
                b.Get(deploymentEvents, x => x.Any()),
                b => b.LiveServiceStatus(deploymentEvents),
                b => b.DefaultChangedServiceStatus(serviceChange));

            return b.ServiceChangeCard(
                b.Get(serviceChange, x => x.ServiceName),
                b.Get(serviceChange, x => x.NodeName.NewValue),
                b.Const("bg-sky-100"),
                b.Const("bg-sky-200"),
                b.ListParameterChanges(serviceChange),
                b.ListBinariesChanges(serviceChange),
                serviceStatus);
        }

        public static Var<IVNode> ListParameterChanges(this LayoutBuilder b, Var<ServiceChange> serviceChange)
        {
            var nodes = b.NewCollection<IVNode>();
            b.Foreach(
                b.Get(serviceChange, x => x.ServiceParameterChanges),
                (b, parameterChange) =>
                {
                    b.If(
                        b.Get(parameterChange, x => x.OldValue != x.NewValue),
                        b =>
                        {
                            b.Push(nodes, b.ParameterChange(parameterChange));
                        });
                });

            return b.Optional(
                b.Get(nodes, x => x.Any()),
                b =>
                {
                    return b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-col gap-2");
                        },
                        b.HtmlDiv(
                            b =>
                           {
                               b.SetClass("flex flex-row gap-2 items-center");
                           },
                           b.Svg(Icon.Changed, "w-h h-6"),
                           b.Text("Changed parameters")),
                        b.HtmlDiv(
                            b => b.SetClass("flex flex-row gap-2"),
                            b.HtmlDiv(b => b.SetClass("w-6")),
                            b.HtmlDiv(
                                b =>
                                {
                                    b.SetClass("flex flex-col gap-1 text-sm");
                                },
                                nodes)));
                });
        }

        public static Var<IVNode> ListBinariesChanges(this LayoutBuilder b, Var<ServiceChange> serviceChange)
        {
            b.Log("ListBinariesChanges", serviceChange);
            return b.Optional(
                b.Get(serviceChange, serviceChange => !(serviceChange.ProjectName.OldValue == serviceChange.ProjectName.NewValue && serviceChange.ProjectVersionTag.OldValue == serviceChange.ProjectVersionTag.NewValue)),
                b =>
                {
                    return b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-row gap-2");
                        },
                        b.Svg(CloudArrowUp),
                        b.Text(
                            b.Concat(
                                b.Const("Version "),
                                b.Get(serviceChange, x => x.ProjectName.NewValue),
                                b.Const(" "),
                                b.Get(serviceChange, x => x.ProjectVersionTag.NewValue))));
                });
        }

        public static Var<IVNode> ParameterChange(this LayoutBuilder b, Var<ServicePropertyChange> parameterChange)
        {
            // null is different from string.Empty in this case.
            // null = parameter does not even exist, while string.Empty is valid parameter value

            // removed, no new value
            return b.If(
                b.Get(parameterChange, x => x.NewValue == null),
                b =>
                {
                    return b.HtmlSpan(
                        b =>
                        {
                            b.SetClass("text-red-800 line-through flex flex-row space-x-4");
                        },
                        b.HtmlSpanText(Font.Bold(), b.Get(parameterChange, x => x.PropertyName)),
                        b.TextSpan(b.Get(parameterChange, x => x.OldValue)));
                },
                b =>
                {
                    return b.If(
                        b.Get(parameterChange, x => x.OldValue == null),
                        b =>
                        {
                            return b.HtmlSpan(
                                b =>
                                {
                                    b.SetClass("text-green-800 flex flex-row space-x-4");
                                },
                                b.HtmlSpanText(Font.Bold(), b.Get(parameterChange, x => x.PropertyName)),
                                b.TextSpan(b.Get(parameterChange, x => x.NewValue)));
                        },
                        b =>
                        {
                            return b.HtmlSpan(
                                b =>
                                {
                                    b.SetClass("text-sky-800 flex flex-row space-x-4");
                                },
                                b.HtmlSpanText(Font.Bold(), b.Get(parameterChange, x => x.PropertyName)),
                                b.TextSpan(b.Get(parameterChange, x => x.OldValue)),
                                b.TextSpan("➔"),
                                b.TextSpan(b.Get(parameterChange, x => x.NewValue)));
                        });
                });
        }

    }

    public static class Font
    {
        public static Action<PropsBuilder<HtmlSpan>> Bold()
        {
            return b => b.SetClass("font-bold");
        }
    }
}
