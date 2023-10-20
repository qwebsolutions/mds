using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Linq;
using MdsCommon.Controls;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<HyperNode> EditApplication(this LayoutBuilder b, Var<EditConfigurationPage> clientModel)
        {
            var appId = b.Get(clientModel, x => x.EditApplicationId);
            var app = b.Get(clientModel, appId, (x, appId) => x.Configuration.Applications.Single(x => x.Id == appId));

            var toolbar = b.Toolbar(b.OkButton(MainPage, x=>x.EditApplicationId));

            var form = b.Form(toolbar);
            b.AddClass(form, "rounded bg-white drop-shadow");
            b.Add(form, b.Text("Application name"));
            //b.Add(form, b.BoundInput(app, x => x.Name, b.Const("Application name")));
            b.Add(form, b.BoundInput(clientModel, appId, (x, appId) => x.Configuration.Applications.Single(x => x.Id == appId), x => x.Name, b.Const("")));

            return form;
        }
    }
}
