using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using MdsCommon;
using System.Linq;
using MdsCommon.Controls;
using System.Collections.Generic;
using Metapsi.Html;

namespace MdsInfrastructure.Render
{
    public static class InfrastructureStatus
    {
        public static void Render(this HtmlBuilder b, MdsInfrastructure.InfrastructureStatus serverModel)
        {
            b.BodyAppend(b.Hyperapp(serverModel, (b, model) =>
            {
                return OnRender(b, serverModel, model);
            }));
        }

        public static Var<IVNode> OnRender(LayoutBuilder b, MdsInfrastructure.InfrastructureStatus serverModel, Var<MdsInfrastructure.InfrastructureStatus> clientModel)
        {
            b.AddModuleStylesheet();

            var headerProps = b.GetHeaderProps(b.Const("Infrastructure status"), b.Const(string.Empty), b.Get(clientModel, x => x.User));

            return b.Layout(
                b.InfraMenu(nameof(Routes.Status), serverModel.User.IsSignedIn()),
                b.Render(headerProps),
                Render(b, serverModel)).As<IVNode>();
        }

        public static Var<IVNode> Render(LayoutBuilder b, MdsInfrastructure.InfrastructureStatus dataModel)
        {

            if (!string.IsNullOrEmpty(dataModel.SchemaValidationMessage))
            {
                return b.StyledDiv("text-red-500", b.Bold(dataModel.SchemaValidationMessage));
            }

            if (dataModel.Deployment == null)
            {
                return b.TextSpan("No deployment yet! The infrastructure is not running any service!");
            }

            int totalServices = dataModel.Deployment.GetDeployedServices().Count();
            int totalNodes = dataModel.Deployment.GetDeployedServices().Select(x => x.NodeName).Distinct().Count();

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col space-y-4");
                },
                b.InfoPanel(
                    Panel.Style.Info,
                    $"Last deployment: {dataModel.Deployment.ConfigurationName}",
                    $"{dataModel.Deployment.Timestamp.ItalianFormat()}, total services {totalServices}, total infrastructure nodes {totalNodes}"),
                b.PanelsContainer(
                    4,
                    dataModel.Deployment.GetDeployedServices().Select(x => x.NodeName).Distinct().Select(nodeName =>
                    {
                        var node = dataModel.InfrastructureNodes.Single(x => x.NodeName == nodeName);
                        var nodePanel = b.RenderNodePanel<MdsInfrastructure.InfrastructureStatus, MdsInfrastructure.InfrastructureStatus>(node, dataModel.HealthStatus);
                        return nodePanel;
                    })),
                b.PanelsContainer(
                    4,
                    dataModel.Deployment.GetDeployedServices().Select(x => x.ApplicationName).Distinct().Select(applicationName =>
                    {
                        return b.RenderApplicationPanel<MdsInfrastructure.InfrastructureStatus, MdsInfrastructure.InfrastructureStatus>(
                            dataModel.Deployment,
                            dataModel.HealthStatus,
                            dataModel.InfrastructureEvents,
                            applicationName);
                    })));
        }
    }
}
