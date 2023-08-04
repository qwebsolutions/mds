using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<HyperNode> EditService(
           this BlockBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            var toolbar = b.Toolbar(b.OkButton(MainPage, x => x.EditServiceId));

            var tabs = b.Tabs(
                clientModel,
                b.Const("serviceTabs"),
                toolbar,
                new Metapsi.Hyperapp.Controls.TabRenderer()
                {
                    TabHeader = b => b.Text("Service"),
                    TabContent = b => b.Call(TabService, clientModel),
                    TabPageCode = "Service"
                },
                new Metapsi.Hyperapp.Controls.TabRenderer()
                {
                    TabHeader = b => b.Text("Parameters"),
                    TabContent = b => b.Call(TabParameters, clientModel),
                    TabPageCode = "Parameters"
                },
                new Metapsi.Hyperapp.Controls.TabRenderer()
                {
                    TabHeader = b => b.Text("Notes"),
                    TabContent = b => b.Call(TabNotes, clientModel),
                    TabPageCode = "Notes"
                });

            return tabs;
        }
    }
}
