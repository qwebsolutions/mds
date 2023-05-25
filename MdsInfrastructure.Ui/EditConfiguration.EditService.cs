using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        public static Var<HyperNode> EditService(
           this BlockBuilder b,
           Var<EditConfigurationPage> clientModel,
           Var<Guid> serviceId)
        {
            var toolbar = b.Toolbar(Controls.OkButton<EditConfigurationPage>);

            var tabs = b.Tabs(
                clientModel, 
                b.Const("serviceTabs"),
                toolbar,
                new Metapsi.Hyperapp.Controls.TabRenderer()
                {
                    TabHeader = b=>b.Text("Service"),
                    TabContent = b => b.Call(TabService, clientModel, serviceId),
                    TabPageCode = "Service"
                },
                new Metapsi.Hyperapp.Controls.TabRenderer()
                {
                    TabHeader = b => b.Text("Parameters"),
                    TabContent = b => b.Call(TabParameters, clientModel, serviceId),
                    TabPageCode = "Parameters"
                },
                new Metapsi.Hyperapp.Controls.TabRenderer()
                {
                    TabHeader = b => b.Text("Notes"),
                    TabContent = b => b.Call(TabNotes, clientModel, serviceId),
                    TabPageCode = "Notes"
                });

            return tabs;
        }
    }
}
