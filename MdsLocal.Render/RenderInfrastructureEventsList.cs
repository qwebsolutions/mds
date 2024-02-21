using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Hyperapp;
using Metapsi;

namespace MdsLocal
{
    public class RenderInfrastructureEventsList : HyperPage<ListInfrastructureEventsPage>
    {
        public override Var<IVNode> OnRender(LayoutBuilder b, Var<ListInfrastructureEventsPage> clientModel)
        {
            b.AddModuleStylesheet();
            var headerProps = b.NewObj<Header.Props>();
            b.Set(headerProps, x => x.Main, b.Const(new Header.Title() { Operation = "Infrastructure events" }));
            b.Set(headerProps, x => x.User, b.Get(clientModel, x => x.User));
            b.Set(headerProps, x => x.UseSignIn, b.Const(false));

            return b.Call(
                MdsCommon.Common.Layout,
                b.LocalMenu(nameof(MdsCommon.Routes.EventsLog)),
                b.Render(headerProps),
                b.RenderListInfrastructureEventsPage(clientModel)).As<IVNode>();
        }
    }
}
