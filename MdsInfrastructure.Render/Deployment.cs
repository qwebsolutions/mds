﻿using MdsCommon;
using Metapsi;
using Metapsi.Dom;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
using System;
using System.Linq;
using MdsCommon.Controls;

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


            public Var<HyperNode> Render(LayoutBuilder b, DeploymentHistory serverModel)
            {
                var rc = b.RenderCell<MdsInfrastructure.Deployment>((b, row, col) =>
                {
                    var dateStringLocale = b.ItalianFormat(b.Get(row, x => x.Timestamp));
                    return b.VPadded4(b.If(b.AreEqual(b.Get(col, x => x.Name), b.Const("timestamp")),
                        b => b.Link(dateStringLocale, b.Url<Routes.Deployment.Review, Guid>(b.Get(row, x => x.Id))),
                        b => b.Text(b.Get(row, x => x.ConfigurationName))));
                });

                var props = b.NewObj<DataTable.Props<MdsInfrastructure.Deployment>>(b =>
                {
                    b.AddColumn("timestamp", "Deployment timestamp");
                    b.AddColumn("name", "Configuration name");
                    b.SetRows(b.Const(serverModel.Deployments.ToList()));
                    b.SetRenderCell(rc);
                });

                var table = b.DataTable(props);
                b.AddClass(table, "drop-shadow");
                return table;
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


        public static Var<HyperNode> RenderDeploymentReport(
            this LayoutBuilder b,
            MdsInfrastructure.ChangesReport serverModel,
            InfrastructureConfiguration infrastructureConfiguration)
        {
            var serviceChanges = serverModel.ServiceChanges;
            if (!serviceChanges.Any() || serviceChanges.All(x => x.ServiceChangeType == ChangeType.None))
            {
                var view = b.Div("flex flex-col w-full h-full items-center justify-center");

                var img = b.Node("img");
                b.SetAttr(img, Html.src, "/nowork.png");
                b.Add(view, img);

                var messageLine = b.Add(view, b.Div("flex flex-row space-x-2"));
                b.Add(messageLine, b.Text("There are no changes to deploy."));
                //b.Add(messageLine, b.Link(b.Const("Edit"), EditConfiguration.Edit, b.Const(infrastructureConfiguration.Id.ToString())));
                b.Add(messageLine, b.Link(b.Const("Edit"), b.Url<Routes.Configuration.Edit, Guid>(b.Const(infrastructureConfiguration.Id))));

                return view;
            }
            else
            {
                var view = b.Div();

                var reviewConfigurationUrl = b.Url<Routes.Configuration.Review, Guid>(b.Const(infrastructureConfiguration.Id));
                var confirmDeploymentUrl = b.Const(string.Empty); //b.Url(ConfirmDeployment, b.Const(infrastructureConfiguration.Id));
                var swapIcon = Icon.Swap;

                var deployNowButton = b.Node(
                    "button",
                    "bg-red-500 rounded px-4 py-2 text-white",
                    b => b.Text("Deploy now"));

                b.SetOnClick(deployNowButton, b.MakeAction((SyntaxBuilder b, Var<DeploymentPreview> model) =>
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

                var toolbar = b.Add(view, b.Toolbar(b =>
                    b.NavigateButton(b =>
                    {
                        b.Set(x => x.Label, "Review configuration");
                        b.Set(x => x.Href, reviewConfigurationUrl);
                        b.Set(x => x.SvgIcon, swapIcon);
                    }),
                    b=> deployNowButton
                    //b => b.NavigateButton(b =>
                    //{
                    //    b.Set(x => x.Label, "Not implemented -  Deploy now");
                    //    b.Set(x => x.Href, confirmDeploymentUrl);
                    //    b.Set(x => x.Style, Button.Style.Danger);
                    //})
                    ));
                b.AddClass(toolbar, "justify-end");

                b.Add(view, b.ChangesReport(serviceChanges));

                return view;
            }
        }


        public static Var<HyperNode> ReviewDeployment(this LayoutBuilder b, ChangesReport serverModel)
        {
            var view = b.Div("flex flex-col");
            var toolbarContainer = b.Add(view, b.Div("flex flex-row justify-end"));
            //b.Add(toolbarContainer, b.Div("w-full"));
            //var toolbar = b.Add(toolbarContainer, b.Toolbar((b, t) =>
            //{
            //    //t.Command("Deploy again", b => { });
            //    //t.Command("Just differences", b => { });
            //}));

            var pageContainer = b.Add(view, b.Div("py-4"));
            b.Add(pageContainer, b.ChangesReport(serverModel.ServiceChanges));

            return view;
        }
    }

    public static class DeploymentControls
    {

        public static Var<HyperNode> ChangesReport(this LayoutBuilder b, System.Collections.Generic.List<ServiceChange> serviceChanges)
        {
            var container = b.Div("flex flex-col space-y-4 pt-4");

            //var serviceNames = serviceChanges.Select(x => x.ServiceName).Distinct();

            foreach (var service in serviceChanges)
            {
                switch (service.ServiceChangeType)
                {
                    case ChangeType.Added:
                        b.Add(container, b.NewService(service));
                        break;
                    case ChangeType.Changed:
                        b.Add(container, b.ChangedService(service));
                        break;
                    case ChangeType.Removed:
                        b.Add(container, b.RemovedService(service));
                        break;
                }
            }

            return container;
        }

        public static Var<HyperNode> NewService(this LayoutBuilder b, ServiceChange serviceChange)
        {
            var container = b.Div("flex flex-col bg-green-100 p-4 rounded text-green-800");

            var header = b.Add(container, b.Div("flex flex-row items-center  space-x-4"));
            b.Add(header, b.Bold(b.Const(serviceChange.ServiceName)));
            var icon = b.Add(header, b.Div());
            b.SetInnerHtml(icon, Icon.Enter);

            var operationSummary = b.Add(header, b.Text($"Install {serviceChange.ProjectName.NewValue} {serviceChange.ProjectVersionTag.NewValue} on {serviceChange.NodeName.NewValue}"));
            b.AddClass(operationSummary, "grid w-full justify-end");

            b.Add(container, b.ListParameterChanges(serviceChange));

            return container;
        }

        public static Var<HyperNode> RemovedService(this LayoutBuilder b, ServiceChange serviceChange)
        {
            var container = b.Div("flex flex-col bg-red-100 p-4 rounded text-red-800");
            var header = b.Add(container, b.Div("flex flex-row items-center space-x-4"));
            b.Add(header, b.Bold(serviceChange.ServiceName));
            var icon = b.Add(header, b.Div());
            b.SetInnerHtml(icon, MdsCommon.Icon.Remove);
            var operationSummary = b.Add(header, b.Text($"Uninstall {serviceChange.ProjectName.OldValue} {serviceChange.ProjectVersionTag.OldValue} from {serviceChange.NodeName.OldValue}"));
            b.AddClass(operationSummary, "grid w-full justify-end");

            b.Add(container, b.ListParameterChanges(serviceChange));
            return container;
        }

        public static Var<HyperNode> ChangedService(this LayoutBuilder b, ServiceChange serviceChange)
        {
            var container = b.Div("flex flex-col bg-sky-200 p-4 rounded text-sky-800");
            var header = b.Add(container, b.Div("flex flex-row items-center space-x-4"));
            b.Add(header, b.Bold(serviceChange.ServiceName));
            var icon = b.Add(header, b.Div());
            b.SetInnerHtml(icon, Icon.Changed);

            // There are no changes in project version, so probably just in parameters
            if (serviceChange.ProjectName.OldValue == serviceChange.ProjectName.NewValue && serviceChange.ProjectVersionTag.OldValue == serviceChange.ProjectVersionTag.NewValue)
            {
                var operationSummary = b.Add(header, b.Text($"Restart {serviceChange.ProjectName.NewValue} {serviceChange.ProjectVersionTag.NewValue} on {serviceChange.NodeName.NewValue}"));
                b.AddClass(operationSummary, "grid w-full justify-end");
            }
            else
            {
                var operationSummary = b.Add(header, b.Text($"Upgrade {serviceChange.ProjectName.NewValue} from {serviceChange.ProjectVersionTag.OldValue} to {serviceChange.ProjectVersionTag.NewValue} on {serviceChange.NodeName.NewValue}"));
                b.AddClass(operationSummary, "grid w-full justify-end");
            }

            b.Add(container, b.ListParameterChanges(serviceChange));

            return container;
        }

        public static Var<HyperNode> ListParameterChanges(this LayoutBuilder b, ServiceChange serviceChange)
        {
            var l = b.Div("flex flex-col space-y-1 text-sm py-2");

            foreach (var parameterChange in serviceChange.ServiceParameterChanges)
            {
                if (parameterChange.OldValue != parameterChange.NewValue)
                {
                    b.Add(l, b.ParameterChange(parameterChange));
                }
            }

            return l;
        }

        public static Var<HyperNode> ParameterChange(this LayoutBuilder b, ServicePropertyChange parameterChange)
        {
            // null is different from string.Empty in this case.
            // null = parameter does not even exist, while string.Empty is valid parameter value

            // removed, no new value
            if (parameterChange.NewValue == null)
            {
                var paramSpan = b.Span("text-red-800 line-through flex flex-row space-x-4");
                b.Add(paramSpan, b.Bold(parameterChange.PropertyName));
                b.Add(paramSpan, b.Text(parameterChange.OldValue));
                return paramSpan;
            }

            // added, no old value
            if (parameterChange.OldValue == null)
            {
                var paramSpan = b.Span("text-green-800 flex flex-row space-x-4");
                b.Add(paramSpan, b.Bold(parameterChange.PropertyName));
                b.Add(paramSpan, b.Text(parameterChange.NewValue));
                return paramSpan;
            }

            // value changed
            {
                var paramSpan = b.Span("text-sky-800 flex flex-row space-x-4");
                b.Add(paramSpan, b.Bold(parameterChange.PropertyName));
                b.Add(paramSpan, b.Text(parameterChange.OldValue));
                b.Add(paramSpan, b.Text("➔"));
                b.Add(paramSpan, b.Text(parameterChange.NewValue));
                return paramSpan;
            }
        }

    }
}
