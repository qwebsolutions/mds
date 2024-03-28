using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using MdsCommon.Controls;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<IVNode> EditService(
           this LayoutBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            var toolbar = b.Toolbar(b => { }, b.OkButton(MainPage, x => x.EditServiceId));

            var tabs = b.Tabs(
                clientModel,
                b.Const("serviceTabs"),
                toolbar,
                new TabRenderer()
                {
                    TabHeader = b => b.TextSpan("Service"),
                    TabContent = b => b.Call(TabService, clientModel),
                    TabPageCode = "Service"
                },
                new TabRenderer()
                {
                    TabHeader = b => b.TextSpan("Parameters"),
                    TabContent = b => b.Call(TabParameters, clientModel),
                    TabPageCode = "Parameters"
                },
                new TabRenderer()
                {
                    TabHeader = b => b.TextSpan("Notes"),
                    TabContent = b => b.Call(TabNotes, clientModel),
                    TabPageCode = "Notes"
                });

            return tabs;
        }
    }
}
