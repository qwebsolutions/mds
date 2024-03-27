using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Linq;
using MdsCommon.Controls;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<IVNode> EditVariable(LayoutBuilder b, Var<EditConfigurationPage> clientModel)
        {
            var varId = b.Get(clientModel, x => x.EditVariableId);
            var toolbar = b.Toolbar(b => { }, b.OkButton(MainPage, x => x.EditVariableId));

            var form = b.Form(toolbar);
            b.AddClass(form, "rounded bg-white drop-shadow");
            b.FormField(form, "Name", b.BoundInput(clientModel, varId, (x, varId) => x.Configuration.InfrastructureVariables.Single(x => x.Id == varId), x => x.VariableName, b.Const(string.Empty)));
            b.FormField(form, "Value", b.BoundInput(clientModel, varId, (x, varId) => x.Configuration.InfrastructureVariables.Single(x => x.Id == varId), x => x.VariableValue, b.Const(string.Empty)));
            return form;
        }
    }
}
