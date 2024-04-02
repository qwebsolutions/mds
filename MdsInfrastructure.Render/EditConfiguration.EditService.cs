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
                toolbar,
                b.TabPair("Service", b.Call(TabService, clientModel)),
                b.TabPair("Parameters", b.Call(TabParameters, clientModel)),
                b.TabPair("Notes", b.Call(TabNotes, clientModel)));

            return tabs;
        }
    }
}
