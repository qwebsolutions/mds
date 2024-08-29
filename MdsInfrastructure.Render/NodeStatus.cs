using Metapsi.Hyperapp;
using Metapsi.Syntax;
using MdsCommon;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.Html;

namespace MdsInfrastructure.Render
{
    public static class NodeStatus
    {
        public static void Render(this HtmlBuilder b, MdsInfrastructure.NodeStatus serverModel)
        {
            b.BodyAppend(b.Hyperapp(serverModel, (b, model) =>
            {
                return OnRender(b, serverModel, model);
            }));
        }
        public static Var<IVNode> OnRender(LayoutBuilder b, MdsInfrastructure.NodeStatus serverData, Var<MdsInfrastructure.NodeStatus> clientModel)
        {
            b.AddModuleStylesheet();

            var headerProps = b.GetHeaderProps(
                b.Const("Node status"),
                b.Get(clientModel, x => x.NodeName),
                b.Get(clientModel, x => x.User));

            return b.Layout(
                b.InfraMenu(
                    nameof(Routes.Status),
                    serverData.User.IsSignedIn()),
                b.Render(headerProps),
                RenderNodeStatus(b, clientModel));
        }

        public static Var<IVNode> RenderNodeStatus(
            LayoutBuilder b,
            Var<MdsInfrastructure.NodeStatus> clientModel)
        {
            return b.If(
                b.Get(clientModel, clientModel => !clientModel.InfrastructureStatus.InfrastructureNodes.Any(x => x.NodeName == clientModel.NodeName)),
                b =>
                {
                    return b.Text("Node not found!");
                },
                b =>
                {
                    return b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-col gap-4");
                        },
                        b.NodePanel(b.Get(clientModel, x => x.NodePanel)),
                        b.PanelsContainer(
                            4,
                            b.Map(
                                b.Get(clientModel, x => x.ServicePanels),
                                (b, service) => b.ServicePanel(service))));
                });
        }
    }
}
