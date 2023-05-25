using Metapsi.Syntax;
using System.Linq;
using Metapsi;
using System;
using System.Threading.Tasks;
using Metapsi.Hyperapp;
using MdsCommon;
using Microsoft.AspNetCore.Http;

namespace MdsInfrastructure
{
    public static partial class Status
    {
        public static async Task<IResponse> Node(CommandContext commandContext, HttpContext requestData)
        {
            var pageData = await LoadStatus(commandContext);

            //NodeStatusPage nodesStatusPage = new NodeStatusPage()
            //{
            //    HealthStatus = pageData.HealthStatus,
            //    InfrastructureConfiguration = pageData.InfrastructureConfiguration,
            //    InfrastructureEvents = pageData.InfrastructureEvents,
            //    Deployment = pageData.Deployment,
            //};

            var selectedNodeId = Guid.Parse(requestData.EntityId());

            return Page.Response(pageData,
                (b, clientModel) => b.Layout(b.InfraMenu(nameof(Status), requestData.User().IsSignedIn()),
                b.Render(b.Const(new Header.Props()
                {
                    Main = new Header.Title() { Operation = "Node status" },
                    User = requestData.User()
                })), RenderNodeStatus(b, pageData, selectedNodeId)));
        }

        public static Var<HyperNode> RenderNodeStatus(
            this BlockBuilder b, 
            InfrastructureStatus nodesStatusPage,
            System.Guid selectedNodeId)
        {
            var selectedNode = nodesStatusPage.InfrastructureNodes.Single(x => x.Id == selectedNodeId);

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

