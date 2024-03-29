using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.Html;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<IVNode> EditApplication(this LayoutBuilder b, Var<EditConfigurationPage> clientModel)
        {
            var appId = b.Get(clientModel, x => x.EditApplicationId);
            var app = b.Get(clientModel, appId, (x, appId) => x.Configuration.Applications.Single(x => x.Id == appId));

            var toolbar = b.Toolbar(
                b =>
                {
                }, 
                b.OkButton(MainPage, x => x.EditApplicationId));

            return b.Form(
                b =>
                {
                    b.AddClass("rounded bg-white drop-shadow");
                },
                toolbar,
                ("Application name", b.BoundInput(clientModel, appId, (x, appId) => x.Configuration.Applications.Single(x => x.Id == appId), x => x.Name, b.Const(""))));
        }
    }
}
