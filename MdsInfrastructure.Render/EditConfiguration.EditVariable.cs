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
        public static Var<IVNode> EditVariable(LayoutBuilder b, Var<EditConfigurationPage> clientModel)
        {
            var varId = b.Get(clientModel, x => x.EditVariableId);
            var toolbar = b.Toolbar(b => { }, b.OkButton(MainPage, x => x.EditVariableId));

            return b.Form(b =>
            {
                b.AddClass("rounded bg-white drop-shadow");
            },
            toolbar,
            ("Name", b.BoundInput(clientModel, varId, (x, varId) => x.Configuration.InfrastructureVariables.Single(x => x.Id == varId), x => x.VariableName, b.Const(string.Empty))),
            ("Value", b.BoundInput(clientModel, varId, (x, varId) => x.Configuration.InfrastructureVariables.Single(x => x.Id == varId), x => x.VariableValue, b.Const(string.Empty))));
        }
    }
}
