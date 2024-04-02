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

                return b.Layout(
                    b.InfraMenu(nameof(Routes.Deployment), serverModel.User.IsSignedIn()),
                    b.Render(b.Const(new Header.Props()
                    {
                        Main = new Header.Title() { Operation = "Deployments" },
                        User = serverModel.User
                    })),
                Render(b, serverModel)).As<IVNode>();
            }

            public Var<IVNode> Render(LayoutBuilder b, DeploymentHistory serverModel)
            {
                var deploymentsTableBuilder = MdsDefaultBuilder.DataTable<MdsInfrastructure.Deployment>();
                deploymentsTableBuilder.OverrideHeaderCell(
                    nameof(MdsInfrastructure.Deployment.Timestamp),
                    b => b.T("Deployment timestamp"));
                deploymentsTableBuilder.OverrideDataCell(
                    nameof(MdsInfrastructure.Deployment.Timestamp),
                    (b, deployment) =>
                    {
                        var dateStringLocale = b.ItalianFormat(b.Get(deployment, x => x.Timestamp));
                        return b.Link(dateStringLocale, b.Url<Routes.Deployment.Review, Guid>(b.Get(deployment, x => x.Id)));
                    });

                deploymentsTableBuilder.OverrideHeaderCell(nameof(MdsInfrastructure.Deployment.ConfigurationName), b => b.T("Configuration name"));

                return b.MdsMainPanel(b => { },
                    b.DataTable(
                        deploymentsTableBuilder,
                        b.Const(serverModel.Deployments.ToList()),
                        nameof(MdsInfrastructure.Deployment.Timestamp),
                        nameof(MdsInfrastructure.Deployment.ConfigurationName)));

                throw new NotImplementedException();
                //var rc = b.RenderCell<MdsInfrastructure.Deployment>((b, row, col) =>
                //{
                //    var dateStringLocale = b.ItalianFormat(b.Get(row, x => x.Timestamp));
                //    return b.VPadded4(b.If(b.AreEqual(b.Get(col, x => x.Name), b.Const("timestamp")),
                //        b => b.Link(dateStringLocale, b.Url<Routes.Deployment.Review, Guid>(b.Get(row, x => x.Id))),
                //        b => b.Text(b.Get(row, x => x.ConfigurationName))));
                //});

                //var props = b.NewObj<DataTable.Props<MdsInfrastructure.Deployment>>(b =>
                //{
                //    b.AddColumn("timestamp", "Deployment timestamp");
                //    b.AddColumn("name", "Configuration name");
                //    b.SetRows(b.Const(serverModel.Deployments.ToList()));
                //    b.SetRenderCell(rc);
                //});

                //var table = b.DataTable(props);
                //b.AddClass(table, "drop-shadow");
                //return table;
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
                var layout = b.Layout(b.InfraMenu(nameof(Routes.Deployment), serverModel.User.IsSignedIn()),
                b.Render(b.Const(new Header.Props()
                {
                    Main = new Header.Title() { Operation = "Deployment", Entity = selectedDeployment },
                    User = serverModel.User
                })),
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
                var layout = b.Layout(
                    b.InfraMenu(
                        nameof(Routes.Deployment),
                        serverModel.User.IsSignedIn()),
                        b.Render(b.Const(new Header.Props()
                        {
                            Main = new Header.Title() { Operation = "Review deployment", Entity = selectedDeployment },
                            User = serverModel.User
                        })),
                        RenderDeploymentReport(b, serverModel.ChangesReport, serverModel.SavedConfiguration));
                return layout.As<IVNode>();
            }
        }


        public static Var<IVNode> RenderDeploymentReport(
            this LayoutBuilder b,
            MdsInfrastructure.ChangesReport serverModel,
            InfrastructureConfiguration infrastructureConfiguration)
        {
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
                        b.HtmlButton(b =>
                        {
                            b.SetClass("bg-red-500 rounded px-4 py-2 text-white");
                            b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<DeploymentPreview> model) =>
                            {
                                return b.MakeStateWithEffects(
                                    b.ShowPanel(model),
                                    b.MakeEffect(
                                        b.Def(
                                            b.Request(
                                                Frontend.ConfirmDeployment,
                                                b.Get(model, x => x.SavedConfiguration.Id),
                                                b.MakeAction((SyntaxBuilder b, Var<DeploymentPreview> model, Var<Frontend.ConfirmDeploymentResponse> response) =>
                                                {
                                                    b.SetUrl(b.Const("/"));
                                                    return model;
                                                }))
                                            )));
                            }));
                        },
                        b.TextSpan("Deploy now"))),
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
                    b.SetClass("flex flex-col space-y-4 pt-4");
                },
                serviceNodes.ToArray());
        }

        public static Var<IVNode> NewService(this LayoutBuilder b, ServiceChange serviceChange)
        {
            var container = b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col bg-green-100 p-4 rounded text-green-800");
                },
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row items-center  space-x-4");
                    },
                    b.HtmlSpanText(Font.Bold(), serviceChange.ServiceName),
                    b.Svg(Icon.Enter),
                    b.HtmlSpanText(b => b.SetClass("grid w-full justify-end"), $"Install {serviceChange.ProjectName.NewValue} {serviceChange.ProjectVersionTag.NewValue} on {serviceChange.NodeName.NewValue}")),
                b.ListParameterChanges(serviceChange));

            return container;
        }

        public static Var<IVNode> RemovedService(this LayoutBuilder b, ServiceChange serviceChange)
        {
            var container = b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col bg-red-100 p-4 rounded text-red-800");
                },
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row items-center space-x-4");
                    },
                    b.HtmlSpanText(Font.Bold(), serviceChange.ServiceName),
                    b.Svg(MdsCommon.Icon.Remove),
                    b.HtmlSpanText(b => b.SetClass("grid w-full justify-end"), $"Uninstall {serviceChange.ProjectName.OldValue} {serviceChange.ProjectVersionTag.OldValue} from {serviceChange.NodeName.OldValue}")),
                b.ListParameterChanges(serviceChange));
            return container;
        }

        public static Var<IVNode> ChangedService(this LayoutBuilder b, ServiceChange serviceChange)
        {
            Var<IVNode> operationsSummary = null;
            // There are no changes in project version, so probably just in parameters
            if (serviceChange.ProjectName.OldValue == serviceChange.ProjectName.NewValue && serviceChange.ProjectVersionTag.OldValue == serviceChange.ProjectVersionTag.NewValue)
            {
                var operationSummary = b.HtmlSpanText(
                    b => b.SetClass("grid w-full justify-end"),
                    $"Restart {serviceChange.ProjectName.NewValue} {serviceChange.ProjectVersionTag.NewValue} on {serviceChange.NodeName.NewValue}");
            }
            else
            {
                var operationSummary = b.HtmlSpanText(
                    b => b.SetClass("grid w-full justify-end"),
                    $"Upgrade {serviceChange.ProjectName.NewValue} from {serviceChange.ProjectVersionTag.OldValue} to {serviceChange.ProjectVersionTag.NewValue} on {serviceChange.NodeName.NewValue}");
            }

            var container = b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col bg-sky-200 p-4 rounded text-sky-800");
                },
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row items-center space-x-4");
                    },
                    b.HtmlSpanText(Font.Bold(), serviceChange.ServiceName),
                    b.Svg(Icon.Changed),
                    operationsSummary),
                b.ListParameterChanges(serviceChange));

            return container;
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

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col space-y-1 text-sm py-2");
                },
                nodes.ToArray());
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
