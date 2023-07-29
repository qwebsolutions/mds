using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Hyperapp;
using Metapsi;

namespace MdsLocal
{
    public class RenderInfrastructureEventsList : HyperPage<ListInfrastructureEventsPage>
    {
        public override Var<HyperNode> OnRender(BlockBuilder b, Var<ListInfrastructureEventsPage> clientModel)
        {
            b.AddStylesheet("metapsi.hyperapp.css");

            var headerProps = b.NewObj<Header.Props>();
            b.Set(headerProps, x => x.Main, b.Const(new Header.Title() { Operation = "Infrastructure events" }));
            b.Set(headerProps, x => x.User, b.Get(clientModel, x => x.User));

            return b.Call(
                MdsCommon.Common.Layout,
                b.LocalMenu(nameof(MdsCommon.Routes.EventsLog)),
                b.Render(headerProps),
                b.RenderListInfrastructureEventsPage(clientModel));
        }
    }
}
