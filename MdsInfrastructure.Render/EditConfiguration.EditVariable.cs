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
            var toolbar = b.Toolbar(b => { }, b.OkButton(MainPage, x => x.EditVariableId));

            return b.Form(b =>
            {
                b.AddClass("rounded bg-white drop-shadow");
            },
            toolbar,
            ("Name", b.MdsInputText(b =>
            {
                b.BindTo(clientModel, GetSelectedVariable, x => x.VariableName);
            })),
            ("Value", b.MdsInputText(
                b =>
                {
                    b.BindTo(clientModel, GetSelectedVariable, x => x.VariableValue);
                })));
        }

        private static Var<InfrastructureVariable> GetSelectedVariable(this SyntaxBuilder b, Var<EditConfigurationPage> clientModel)
        {
            var varId = b.Get(clientModel, x => x.EditVariableId);
            return b.Get(clientModel, varId, (model, varId) => model.Configuration.InfrastructureVariables.Single(x => x.Id == varId));
        }
    }
}
