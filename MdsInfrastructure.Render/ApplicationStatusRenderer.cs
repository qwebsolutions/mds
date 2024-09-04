using Metapsi.Hyperapp;
using Metapsi.Syntax;
using MdsCommon;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.Html;

namespace MdsInfrastructure.Render
{
    public static class ApplicationStatus
    {
        public static void Render(this HtmlBuilder b, MdsInfrastructure.ApplicationStatus serverModel)
        {
            b.BodyAppend(b.Hyperapp(serverModel, (b, model) =>
            {
                return OnRender(b, serverModel, model);
            }));
        }

        public static Var<IVNode> OnRender(
            LayoutBuilder b, 
            MdsInfrastructure.ApplicationStatus serverData, 
            Var<MdsInfrastructure.ApplicationStatus> clientModel)
        {
            b.AddModuleStylesheet();

            return b.Layout(
                b.InfraMenu(nameof(Routes.Status), serverData.User.IsSignedIn()),
                b.Render(
                    b.GetHeaderProps(
                        b.Const("Application status"),
                        b.Const(serverData.ApplicationName),
                        b.Get(clientModel, x => x.User))),
                RenderClient(
                    b,
                    clientModel));
        }


        public static Var<IVNode> RenderClient(
            LayoutBuilder b,
            Var<MdsInfrastructure.ApplicationStatus> clientModel)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.AddClass("flex flex-col space-y-4");
                },
                b.ApplicationPanel(b.Get(clientModel, x=>x.ApplicationPanel)),
                b.PanelsContainer(
                    4,
                    b.Map(
                        b.Get(clientModel, x => x.ServicePanels),
                        (b, panel) => b.ServicePanel(panel))));
        }
    }
}
