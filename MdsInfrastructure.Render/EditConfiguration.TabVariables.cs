using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.Html;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<IVNode> TabVariables(
           LayoutBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            var allVariables = b.Get(clientModel, x => x.Configuration.InfrastructureVariables.OrderBy(x => x.VariableName).ToList());
            var filteredVariables = b.FilterList(allVariables, b.Get(clientModel, x => x.VariablesFilter));

            var gridBuilder = MdsDefaultBuilder.DataGrid<InfrastructureVariable>();
            gridBuilder.DataTableBuilder.OverrideHeaderCell(nameof(InfrastructureVariable.VariableName), b => b.Text("Variable name"));
            gridBuilder.DataTableBuilder.OverrideDataCell(nameof(InfrastructureVariable.VariableName), (b, variable) => b.RenderVariableNameCell(variable));
            gridBuilder.DataTableBuilder.OverrideHeaderCell("value", b => b.Text("Variable value"));

            gridBuilder.CreateToolbarActions = b =>
            {
                return b.HtmlDiv(b =>
                {
                    b.SetClass("flex flex-row items-center justify-between");
                },
                b.HtmlButton(
                    b =>
                    {
                        b.AddPrimaryButtonStyle();
                        b.OnClickAction<EditConfigurationPage, HtmlButton>(OnAddVariable);
                    },
                    b.Text("Add variable")),
                b.Filter(clientModel, x => x.VariablesFilter));
            };

            gridBuilder.AddRowAction((b, row) =>
            {
                var isInUse = b.Get(
                    clientModel,
                    b.Get(row, x => x.Id),
                    (x, variableId) => x.Configuration.InfrastructureServices.SelectMany(x => x.InfrastructureServiceParameterDeclarations.SelectMany(x => x.InfrastructureServiceParameterBindings)).Any(x => x.InfrastructureVariableId == variableId));

                return b.Optional(
                    b.Not(isInUse),
                    b =>
                    {
                        return b.DeleteRowIconAction(b =>
                        {
                            b.OnClickAction(
                                (SyntaxBuilder b, Var<EditConfigurationPage> clientModel) =>
                                {
                                    b.Remove(b.Get(clientModel, x => x.Configuration.InfrastructureVariables), row);
                                    return b.Clone(clientModel);
                                });
                        });
                    });
            });

            return b.DataGrid(
                gridBuilder,
                filteredVariables,
                nameof(InfrastructureVariable.VariableName),
                nameof(InfrastructureVariable.VariableValue));
        }

        public static Var<IVNode> RenderVariableNameCell(this LayoutBuilder b, Var<InfrastructureVariable> row)
        {
            var variableName = b.Get(row, x => x.VariableName);

            var goToVariable = (SyntaxBuilder b, Var<EditConfigurationPage> clientModel) =>
            {
                b.Set(clientModel, x => x.EditVariableId, b.Get(row, x => x.Id));
                return b.EditView<EditConfigurationPage>(clientModel, EditVariable);
            };

            return b.Link(b.WithDefault(variableName), b.MakeAction(goToVariable));
        }

        public static Var<EditConfigurationPage> OnAddVariable(SyntaxBuilder b, Var<EditConfigurationPage> clientModel)
        {
            var configHeaderId = b.Get(clientModel, x => x.Configuration.Id);
            var newId = b.NewId();
            var newVar = b.NewObj<InfrastructureVariable>(b =>
            {
                b.Set(x => x.ConfigurationHeaderId, configHeaderId);
                b.Set(x => x.Id, newId);
            });
            b.Push(b.Get(clientModel, x => x.Configuration.InfrastructureVariables), newVar);
            b.Set(clientModel, x => x.EditVariableId, newId);
            return b.EditView<EditConfigurationPage>(clientModel, EditConfiguration.EditVariable);
        }
    }
}
