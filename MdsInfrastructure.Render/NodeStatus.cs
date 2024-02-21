using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
using MdsCommon;
using System.Linq;
using MdsCommon.Controls;

namespace MdsInfrastructure.Render
{
    public class NodeStatus : MixedHyperPage<MdsInfrastructure.NodeStatus, MdsInfrastructure.NodeStatus>
    {
        public override MdsInfrastructure.NodeStatus ExtractClientModel(MdsInfrastructure.NodeStatus serverData)
        {
            return serverData;
        }

        public override Var<IVNode> OnRender(LayoutBuilder b, MdsInfrastructure.NodeStatus serverData, Var<MdsInfrastructure.NodeStatus> clientModel)
        {
            b.AddModuleStylesheet();

            return b.Layout(b.InfraMenu(nameof(Routes.Status), serverData.InfrastructureStatus.User.IsSignedIn()),
                b.Render(b.Const(new Header.Props()
                {
                    Main = new Header.Title() { Operation = "Node status" },
                    User = serverData.InfrastructureStatus.User,
                })), RenderNodeStatus(b, serverData.InfrastructureStatus, serverData.NodeName)).As<IVNode>();
        }

        public static Var<HyperNode> RenderNodeStatus(
            LayoutBuilder b,
            MdsInfrastructure.InfrastructureStatus nodesStatusPage,
            string selectedNodeName)
        {
            var selectedNode = nodesStatusPage.InfrastructureNodes.SingleOrDefault(x => x.NodeName == selectedNodeName);

            if (selectedNode == null)
            {
                return b.Text("Node not found");
            }

            //var headerBuilder = viewBuilder.CreatePageHeader($"Deployment: {nodesStatusPage.GetLastDeployment().Timestamp.ItalianFormat()}, configuration name: {nodesStatusPage.GetLastDeployment().ConfigurationName}");
            //var backButton = headerBuilder.AddHeaderCommand<NodeStatusPage, StatusPage>(MdsCommon.MdsCommonFunctions.BackLabel, (context, model, id) => Status(context), Guid.Empty, true);
            //backButton.Styling = "Secondary";

            //var nodesGroup = viewBuilder.Group("grpNodes", Guid.Empty, UI.Svelte.ViewBuilder.RootGroupId, "Horizontal");

            var page = b.Div("flex flex-col space-y-4");

            b.Add(page, b.RenderNodePanel<InfrastructureStatus, InfrastructureStatus>(selectedNode, nodesStatusPage.HealthStatus));

            var nodeServices = nodesStatusPage.Deployment.GetDeployedServices().Where(x => x.NodeName == selectedNode.NodeName);

            var servicesGroup = b.Add(page, b.PanelsContainer(4));

            foreach (var service in nodeServices)
            {
                var serviceCard = b.Add(servicesGroup, b.RenderServicePanel(
                    nodesStatusPage.Deployment,
                    nodesStatusPage.HealthStatus,
                    service,
                    nodesStatusPage.InfrastructureEvents));
            }

            return page;
        }
    }
}
