using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {

        public static async Task<IResponse> DeploymentReport(CommandContext commandContext, HttpContext requestData, Guid entityId)
        {
#if DEBUG
            WebServer.WebRootPaths.Add("D:\\qweb\\mes\\Mds\\MdsInfrastructure\\inline");
#endif

            var savedConfiguration = await commandContext.Do(Api.LoadConfiguration, entityId);
            var serverModel = await MdsInfrastructureFunctions.InitializeEditConfiguration(commandContext, savedConfiguration);

            var currentDeployment = await commandContext.Do(Api.LoadCurrentDeployment);
            if (currentDeployment == null)
                currentDeployment = new Deployment();

            var snapshot = await MdsInfrastructureFunctions.TakeConfigurationSnapshot(
                commandContext,
                savedConfiguration,
                serverModel.AllProjects,
                serverModel.InfrastructureNodes);

            var changesReport = MdsInfrastructure.ChangesReport.Get(currentDeployment.GetDeployedServices().ToList(), snapshot);
            return Page.Response(changesReport, (b, clientModel) =>
            {
                var layout = b.Layout(b.InfraMenu(nameof(Configuration), requestData.User().IsSignedIn()),
                    b.Render(b.Const(new Header.Props()
                    {
                        Main = new Header.Title() { Operation = "Review deployment", Entity = currentDeployment.Timestamp.ItalianFormat() },
                        User = requestData.User()
                    })), RenderDeploymentReport(b, changesReport, savedConfiguration));
                return layout;
            });
        }

        public static Var<HyperNode> RenderDeploymentReport(
            this BlockBuilder b,
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
                b.Add(messageLine, b.Link(b.Const("Edit"), EditConfiguration.Edit, b.Const(infrastructureConfiguration.Id.ToString())));

                return view;
            }
            else
            {
                var view = b.Div();

                var reviewConfigurationUrl = b.Url(ReviewConfiguration, b.Const(infrastructureConfiguration.Id));
                var confirmDeploymentUrl = b.Url(ConfirmDeployment, b.Const(infrastructureConfiguration.Id));
                var swapIcon = Icon.Swap;

                var toolbar = b.Add(view, b.Toolbar(b =>
                    b.NavigateButton(b =>
                    {
                        b.Set(x => x.Label, "Review configuration");
                        b.Set(x => x.Href, reviewConfigurationUrl);
                        b.Set(x => x.SvgIcon, swapIcon);
                    }),
                    b => b.NavigateButton(b =>
                    {
                        b.Set(x => x.Label, "Deploy now");
                        b.Set(x => x.Href, confirmDeploymentUrl);
                        b.Set(x => x.Style, Button.Style.Danger);
                    })));
                b.AddClass(toolbar, "justify-end");

                b.Add(view, b.ChangesReport(serviceChanges));

                return view;
            }
        }

        public static Var<HyperNode> ChangesReport(this BlockBuilder b, System.Collections.Generic.List<ServiceChange> serviceChanges)
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

        public static Var<HyperNode> NewService(this BlockBuilder b, ServiceChange serviceChange)
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

        public static Var<HyperNode> RemovedService(this BlockBuilder b, ServiceChange serviceChange)
        {
            var container = b.Div("flex flex-col bg-red-100 p-4 rounded text-red-800");
            var header = b.Add(container, b.Div("flex flex-row items-center space-x-4"));
            b.Add(header, b.Bold(serviceChange.ServiceName));
            var icon = b.Add(header, b.Div());
            b.SetInnerHtml(icon, Icon.Remove);
            var operationSummary = b.Add(header, b.Text($"Uninstall {serviceChange.ProjectName.OldValue} {serviceChange.ProjectVersionTag.OldValue} from {serviceChange.NodeName.OldValue}"));
            b.AddClass(operationSummary, "grid w-full justify-end");

            b.Add(container, b.ListParameterChanges(serviceChange));
            return container;
        }

        public static Var<HyperNode> ChangedService(this BlockBuilder b, ServiceChange serviceChange)
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

        public static Var<HyperNode> ListParameterChanges(this BlockBuilder b, ServiceChange serviceChange)
        {
            var l = b.Div("flex flex-col space-y-1 text-sm py-2");

            foreach (var parameterChange in serviceChange.ServiceParameterChanges)
            {
                if(parameterChange.OldValue != parameterChange.NewValue)
                {
                    b.Add(l, b.ParameterChange(parameterChange));
                }
            }

            return l;
        }

        public static Var<HyperNode> ParameterChange(this BlockBuilder b, ServicePropertyChange parameterChange)
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
            if(parameterChange.OldValue == null)
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
