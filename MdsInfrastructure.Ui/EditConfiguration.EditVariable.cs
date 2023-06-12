using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Linq;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        public static Var<HyperNode> EditVariable(BlockBuilder b, Var<EditConfigurationPage> clientModel, Var<Guid> varId)
        {
            var toolbar = b.Toolbar(Controls.OkButton<EditConfigurationPage>);

            var form = b.Form(toolbar);
            b.AddClass(form, "rounded bg-white drop-shadow");
            b.FormField(form, "Name", b.BoundInput(clientModel, varId, (x, varId) => x.Configuration.InfrastructureVariables.Single(x => x.Id == varId), x => x.VariableName, b.Const(string.Empty)));
            b.FormField(form, "Value", b.BoundInput(clientModel, varId, (x, varId) => x.Configuration.InfrastructureVariables.Single(x => x.Id == varId), x => x.VariableValue, b.Const(string.Empty)));
            b.Call(ApplyVariablesFilter, clientModel);
            
            return form;
        }
    }
}
