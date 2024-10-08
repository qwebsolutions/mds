﻿using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Hyperapp;
using Metapsi;

namespace MdsInfrastructure
{
    public class RenderInfrastructureEventsList
    {
        public static Var<IVNode> Render(LayoutBuilder b, ListInfrastructureEventsPage serverModel, Var<ListInfrastructureEventsPage> clientModel)
        {
            b.AddModuleStylesheet();

            var headerProps = b.NewObj<Header.Props>();
            b.Set(headerProps, x => x.Main, b.Const(new Header.Title() { Operation = "Infrastructure events" }));
            b.Set(headerProps, x => x.User, b.Get(clientModel, x => x.User));

            return b.Call(
                MdsCommon.Common.Layout,
                b.InfraMenu(nameof(MdsCommon.Routes.EventsLog), serverModel.User.IsSignedIn()),
                b.Render(headerProps),
                b.RenderListInfrastructureEventsPage(clientModel)).As<IVNode>();
        }
    }
}
