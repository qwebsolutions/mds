using MdsCommon;
using Metapsi;
using Metapsi.Dom;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
using System;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.Html;
using System.Collections.Generic;
using System.Drawing;

namespace MdsInfrastructure.Render
{
    public static class Deployment
    {
        public class List : MixedHyperPage<DeploymentHistory, DeploymentHistory>
        {
            public override DeploymentHistory ExtractClientModel(DeploymentHistory serverModel)
            {
                return serverModel;
            }

            public override Var<IVNode> OnRender(LayoutBuilder b, DeploymentHistory serverModel, Var<DeploymentHistory> clientModel)
            {
                b.AddModuleStylesheet();

                var headerProps = b.GetHeaderProps(b.Const("Deployments"), b.Const(string.Empty), b.Get(clientModel, x => x.User));

                return b.Layout(
                    b.InfraMenu(nameof(Routes.Deployment), serverModel.User.IsSignedIn()),
                    b.Render(headerProps),
                Render(b, serverModel)).As<IVNode>();
            }

            public Var<IVNode> Render(LayoutBuilder b, DeploymentHistory serverModel)
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

        public class Review : MixedHyperPage<DeploymentReview, DeploymentReview>
        {
            public override DeploymentReview ExtractClientModel(DeploymentReview serverModel)
            {
                return serverModel;
            }

            public override Var<IVNode> OnRender(LayoutBuilder b, DeploymentReview serverModel, Var<DeploymentReview> clientModel)
            {
                b.AddModuleStylesheet();

                var selectedDeployment = serverModel.Deployment.Timestamp.ItalianFormat();

                var headerProps = b.GetHeaderProps(b.Const("Deployment"), b.Const(selectedDeployment), b.Get(clientModel, x => x.User));

                var layout = b.Layout(
                    b.InfraMenu(
                        nameof(Routes.Deployment),
                        serverModel.User.IsSignedIn()),
                b.Render(headerProps),
                b.ReviewDeployment(serverModel.ChangesReport));
                return layout.As<IVNode>();
            }
        }

        public class Preview : MixedHyperPage<DeploymentPreview, DeploymentPreview>
        {
            public override DeploymentPreview ExtractClientModel(DeploymentPreview serverModel)
            {
                return serverModel;
            }

            public override Var<IVNode> OnRender(LayoutBuilder b, DeploymentPreview serverModel, Var<DeploymentPreview> clientModel)
            {
                b.AddModuleStylesheet();

                var selectedDeployment = serverModel.Deployment.Timestamp.ItalianFormat();

                var headerProps = b.GetHeaderProps(
                    b.Const("Review deployment"),
                    b.Const(selectedDeployment),
                    b.Get(clientModel, x => x.User));

                var layout = b.Layout(
                    b.InfraMenu(
                        nameof(Routes.Deployment),
                        serverModel.User.IsSignedIn()),
                        b.Render(headerProps),
                        RenderDeploymentReport(b, serverModel.ChangesReport, serverModel.SavedConfiguration));
                return layout.As<IVNode>();
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
                            b.ShowPanel(model),
                            b.GetRequest(
                                Frontend.ConfirmDeployment,
                                configurationId,
                                b.MakeAction((SyntaxBuilder b, Var<DeploymentPreview> model, Var<Frontend.ConfirmDeploymentResponse> response) =>
                                {
                                    b.SetUrl(b.Const("/"));
                                    return model;
                                }))
                            );
                    }));
                },
                b.TextSpan("Deploy now"));
        }

        public static Var<IVNode> RenderDeploymentReport(
            this LayoutBuilder b,
            MdsInfrastructure.ChangesReport serverModel,
            InfrastructureConfiguration infrastructureConfiguration)
        {
            StaticFiles.Add(typeof(Render.Deployment).Assembly, "nowork.png");
            var serviceChanges = serverModel.ServiceChanges;
            if (!serviceChanges.Any() || serviceChanges.All(x => x.ServiceChangeType == ChangeType.None))
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
                        b.Link(b.Const("Edit"), b.Url<Routes.Configuration.Edit, Guid>(b.Const(infrastructureConfiguration.Id)))));
            }
            else
            {
                var reviewConfigurationUrl = b.Url<Routes.Configuration.Review, Guid>(b.Const(infrastructureConfiguration.Id));
                var swapIcon = Icon.Swap;

                var view = b.HtmlDiv(
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
                        b.DeployNowButton<DeploymentPreview>(b.Const(infrastructureConfiguration.Id))),
                    b.ChangesReport(serviceChanges));

                return view;
            }
        }


        public static Var<IVNode> ReviewDeployment(this LayoutBuilder b, ChangesReport serverModel)
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
                    b.ChangesReport(serverModel.ServiceChanges)));

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

        public static Var<IVNode> ChangesReport(this LayoutBuilder b, System.Collections.Generic.List<ServiceChange> serviceChanges)
        {
            List<Var<IVNode>> serviceNodes = new();

            foreach (var service in serviceChanges)
            {
                switch (service.ServiceChangeType)
                {
                    case ChangeType.Added:
                        serviceNodes.Add(b.NewService(service));
                        break;
                    case ChangeType.Changed:
                        serviceNodes.Add(b.ChangedService(service));
                        break;
                    case ChangeType.Removed:
                        serviceNodes.Add(b.RemovedService(service));
                        break;
                }
            }

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-row flex-wrap gap-4 p-4 ");
                },
                serviceNodes.ToArray());
        }

        public static Var<IVNode> NewService(this LayoutBuilder b, ServiceChange serviceChange)
        {
            var processStatus =
                serviceChange.Enabled.NewValue == "False" ?
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row gap-2 text-red-800");
                    },
                    b.Svg(PauseCircle, "w-6 h-6"),
                    b.Text($"Service disabled, will not be started"))
                :
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row gap-2 text-green-600");
                    },
                    b.Svg(Play, "w-6 h-6"),
                    b.Text($"Service will be started"));


            return b.ServiceChangeCard(
                serviceChange.ServiceName,
                serviceChange.NodeName.NewValue,
                "bg-green-100",
                "bg-green-200",
                b.ListParameterChanges(serviceChange),
                b.ListBinariesChanges(serviceChange),
                processStatus);
        }

        public static Var<IVNode> RemovedService(this LayoutBuilder b, ServiceChange serviceChange)
        {
            return b.ServiceChangeCard(
                serviceChange.ServiceName,
                serviceChange.NodeName.OldValue,
                "bg-red-100",
                "bg-red-200",
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
            string serviceName,
            string nodeName,
            string bgColor,
            string titleBgColor,
            params Var<IVNode>[] children)
        {
            var container = b.HtmlDiv(
               b =>
               {
                   b.SetClass($"flex flex-col {bgColor} rounded text-sky-800 min-w-[24rem]");
               },
               b.HtmlSpan(
                   b=>
                   {
                       b.SetClass($"flex flex-row items-center justify-between {titleBgColor} p-4 rounded-t");
                   },
                   b.HtmlSpanText(
                       b =>
                       {
                           b.SetClass($"font-semibold");
                       },
                       serviceName),
                   b.HtmlSpan(
                       b=>
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

        public static Var<IVNode> ChangedService(this LayoutBuilder b, ServiceChange serviceChange)
        {
            var processChanges = b.VoidNode();

            var shouldEnable = serviceChange.Enabled.NewValue == "True" && serviceChange.Enabled.OldValue == "False";
            var shouldDisable = serviceChange.Enabled.NewValue == "False" && serviceChange.Enabled.OldValue == "True";
            var isDisabled = serviceChange.Enabled.NewValue == "False" && serviceChange.Enabled.OldValue == "False";

            if (shouldEnable)
            {
                processChanges = b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row gap-2 text-green-600");
                    },
                    b.Svg(ArrowPath),
                    b.Text($"Service enabled, process will be started"));
            }
            else if (shouldDisable)
            {
                processChanges = b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row gap-2 text-red-800");
                    },
                    b.Svg(StopCircle),
                    b.Text($"Service disabled, process will be stopped"));
            }
            else if (isDisabled)
            {
                processChanges = b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row gap-2 text-red-800");
                    },
                    b.Svg(PauseCircle, "w-6 h-6"),
                    b.Text($"Service disabled, will not be started"));
            }
            else
            {

                processChanges = b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row gap-2");
                    },
                    b.Svg(ArrowPath),
                    b.Text($"Process will be restarted"));
            }

            return b.ServiceChangeCard(
                serviceChange.ServiceName,
                serviceChange.NodeName.NewValue,
                "bg-sky-100",
                "bg-sky-200",
                b.ListParameterChanges(serviceChange),
                b.ListBinariesChanges(serviceChange),
                processChanges);
        }

        public static Var<IVNode> ListParameterChanges(this LayoutBuilder b, ServiceChange serviceChange)
        {
            List<Var<IVNode>> nodes = new();

            foreach (var parameterChange in serviceChange.ServiceParameterChanges)
            {
                if (parameterChange.OldValue != parameterChange.NewValue)
                {
                    nodes.Add(b.ParameterChange(parameterChange));
                }
            }

            if (nodes.Any())
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
                        b=> b.SetClass("flex flex-row gap-2"),
                        b.HtmlDiv(b=>b.SetClass("w-6")),
                        b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("flex flex-col gap-1 text-sm");
                            },
                            nodes.ToArray())));
            }
            else return b.VoidNode();
        }

        public static Var<IVNode> ListBinariesChanges(this LayoutBuilder b, ServiceChange serviceChange)
        {
            var binariesChanges = b.VoidNode();
            var binariesChanged = !(serviceChange.ProjectName.OldValue == serviceChange.ProjectName.NewValue && serviceChange.ProjectVersionTag.OldValue == serviceChange.ProjectVersionTag.NewValue);
            if (binariesChanged)
            {
                binariesChanges = b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row gap-2");
                    },
                    b.Svg(CloudArrowUp),
                    b.Text($"Version {serviceChange.ProjectName.NewValue} {serviceChange.ProjectVersionTag.NewValue}"));
            }

            return binariesChanges;
        }

        public static Var<IVNode> ParameterChange(this LayoutBuilder b, ServicePropertyChange parameterChange)
        {
            // null is different from string.Empty in this case.
            // null = parameter does not even exist, while string.Empty is valid parameter value

            // removed, no new value
            if (parameterChange.NewValue == null)
            {
                return b.HtmlSpan(
                    b =>
                    {
                        b.SetClass("text-red-800 line-through flex flex-row space-x-4");
                    },
                    b.HtmlSpanText(Font.Bold(), parameterChange.PropertyName),
                    b.TextSpan(parameterChange.OldValue));
            }

            // added, no old value
            if (parameterChange.OldValue == null)
            {
                return b.HtmlSpan(
                    b =>
                    {
                        b.SetClass("text-green-800 flex flex-row space-x-4");
                    },
                    b.HtmlSpanText(Font.Bold(), parameterChange.PropertyName),
                    b.TextSpan(parameterChange.NewValue));
            }

            // value changed
            {
                return b.HtmlSpan(
                    b =>
                    {
                        b.SetClass("text-sky-800 flex flex-row space-x-4");
                    },
                    b.HtmlSpanText(Font.Bold(), parameterChange.PropertyName),
                    b.TextSpan(parameterChange.OldValue),
                    b.TextSpan("➔"),
                    b.TextSpan(parameterChange.NewValue));
            }
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
