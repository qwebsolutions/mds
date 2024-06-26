﻿using Metapsi.Hyperapp;
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

            var headerProps = b.GetHeaderProps(
                b.Const("Node status"),
                b.Get(clientModel, x => x.NodeName),
                b.Get(clientModel, x => x.InfrastructureStatus.User));

            return b.Layout(
                b.InfraMenu(
                    nameof(Routes.Status),
                    serverData.InfrastructureStatus.User.IsSignedIn()),
                b.Render(headerProps),
                RenderNodeStatus(b, serverData.InfrastructureStatus, serverData.NodeName)).As<IVNode>();
        }

        public static Var<IVNode> RenderNodeStatus(
            LayoutBuilder b,
            MdsInfrastructure.InfrastructureStatus nodesStatusPage,
            string selectedNodeName)
        {
            var selectedNode = nodesStatusPage.InfrastructureNodes.SingleOrDefault(x => x.NodeName == selectedNodeName);

            if (selectedNode == null)
            {
                return b.TextSpan("Node not found");
            }

            var nodeServices = nodesStatusPage.Deployment.GetDeployedServices().Where(x => x.NodeName == selectedNode.NodeName);

            var servicesGroup = b.PanelsContainer(4,
                nodeServices.Select(service =>
                {
                    return b.RenderServicePanel(
                        nodesStatusPage.Deployment,
                        nodesStatusPage.HealthStatus,
                        service,
                        nodesStatusPage.InfrastructureEvents);
                }));

            return b.StyledDiv(
                "flex flex-col space-y-4",
                b.RenderNodePanel<InfrastructureStatus, InfrastructureStatus>(selectedNode, nodesStatusPage.HealthStatus),
                servicesGroup);
        }
    }
}
