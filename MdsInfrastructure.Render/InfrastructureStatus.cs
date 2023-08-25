using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
using MdsCommon;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.ChoicesJs;
using System.Collections.Generic;

namespace MdsInfrastructure.Render
{
    public class InfrastructureStatus: MixedHyperPage<MdsInfrastructure.InfrastructureStatus, MdsInfrastructure.InfrastructureStatus>
    {
        public override MdsInfrastructure.InfrastructureStatus ExtractClientModel(MdsInfrastructure.InfrastructureStatus serverData)
        {
            return serverData;
        }

        public override Var<HyperNode> OnRender(BlockBuilder b, MdsInfrastructure.InfrastructureStatus serverModel, Var<MdsInfrastructure.InfrastructureStatus> clientModel)
        {
            b.AddModuleStylesheet();

            return b.Layout(
                b.InfraMenu(nameof(Routes.Status), serverModel.User.IsSignedIn()),
                b.Render(
                    b.Const(
                        new MdsCommon.Header.Props()
                        {
                            Main = new MdsCommon.Header.Title() { Operation = "Infrastructure status" },
                            User = serverModel.User
                        })),
                Render(b, serverModel));
        }

        public static Var<HyperNode> Render(BlockBuilder b, MdsInfrastructure.InfrastructureStatus dataModel)
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
                var nodePanel = b.RenderNodePanel<MdsInfrastructure.InfrastructureStatus, MdsInfrastructure.InfrastructureStatus>(node, dataModel.HealthStatus);
                b.Add(nodesContainer, nodePanel);
            }

            Var<HyperNode> appsContainer = b.Add(page, b.PanelsContainer(4));

            foreach (var applicationName in dataModel.Deployment.GetDeployedServices().Select(x => x.ApplicationName).Distinct())
            {
                var appPanel = b.Add(appsContainer, b.RenderApplicationPanel<MdsInfrastructure.InfrastructureStatus, MdsInfrastructure.InfrastructureStatus>(
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
    }
}
