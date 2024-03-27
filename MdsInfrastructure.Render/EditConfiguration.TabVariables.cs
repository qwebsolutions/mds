﻿using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;
using MdsCommon.Controls;
using MdsCommon.HtmlControls;
using static MdsCommon.Controls.DataTable;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Metapsi.Html;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<IVNode> TabVariables(
           LayoutBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            throw new System.NotImplementedException();
            //var container = b.Div("w-full h-full");

            //var configId = b.Get(clientModel, x => x.Configuration.Id);

            //var onRemove = (SyntaxBuilder b, Var<EditConfigurationPage> clientModel, Var<InfrastructureVariable> variable) =>
            //{
            //    var typed = variable.As<InfrastructureVariable>();
            //    var removed = b.Get(clientModel, typed, (x, typed) => x.Configuration.InfrastructureVariables.Where(x => x != typed).ToList());
            //    b.Set(b.Get(clientModel, x => x.Configuration), x => x.InfrastructureVariables, removed);
            //    return b.Clone(clientModel);
            //};

            //var allVariables = b.Get(clientModel, x => x.Configuration.InfrastructureVariables.OrderBy(x => x.VariableName).ToList());
            //var filteredVariables = b.FilterList<InfrastructureVariable>(allVariables, b.Get(clientModel, x => x.VariablesFilter));

            //b.OnModel(
            //    clientModel,
            //    (bParent, context) =>
            //    {
            //        var b = new LayoutBuilder(bParent);

            //        b.Add(container, b.DataGrid<InfrastructureVariable>(
            //            b =>
            //            {
            //                b.OnTable(b =>
            //                {
            //                    b.FillFrom(filteredVariables, exceptColumns: new()
            //                    {
            //                        nameof(InfrastructureVariable.Id),
            //                        nameof(InfrastructureVariable.ConfigurationHeaderId),
            //                    });

            //                    b.SetCommonStyle();

            //                    b.OverrideColumnCell(
            //                        nameof(InfrastructureVariable.VariableName),
            //                        (b, data) => b.RenderVariableNameCell(b.Get(data, x => x.Row)));
            //                });

            //                b.AddHoverRowAction(onRemove, Icon.Remove, (b, data, props) =>
            //                {
            //                    b.AddClass(props, "text-red-500");
            //                },
            //                visible: (b, row) =>
            //                {
            //                    var isInUse = b.Get(
            //                        clientModel,
            //                        b.Get(row, x => x.Id),
            //                        (x, variableId) => x.Configuration.InfrastructureServices.SelectMany(x => x.InfrastructureServiceParameterDeclarations.SelectMany(x => x.InfrastructureServiceParameterBindings)).Any(x => x.InfrastructureVariableId == variableId));

            //                    return b.Not(isInUse);
            //                });

            //                b.AddToolbarChild(AddVariableButton);

            //                b.AddToolbarChild(
            //                    b => b.Filter(
            //                        b =>
            //                        {
            //                            b.BindFilter(context, x => x.VariablesFilter);
            //                        }),
            //                    HorizontalPlacement.Right);

            //            }));
            //    });

            //return container;
        }

        public static Var<IVNode> RenderVariableNameCell(this LayoutBuilder b, Var<InfrastructureVariable> row)
        {
            var variableName = b.Get(row, x => x.VariableName);

            var goToVariable = (SyntaxBuilder b, Var<EditConfigurationPage> clientModel) =>
            {
                b.Set(clientModel, x => x.EditVariableId, b.Get(row, x => x.Id));
                return b.EditView<EditConfigurationPage>(clientModel, EditVariable);
            };

            return b.HtmlSpan(
                b =>
                {

                },
                b.Link<EditConfigurationPage>(b.WithDefault(variableName), b.MakeAction<EditConfigurationPage>(goToVariable)));
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
            return b.EditView(clientModel, b.GetViewName<EditConfigurationPage>(EditConfiguration.EditVariable));
        }

        public static Var<IVNode> AddVariableButton(this LayoutBuilder b)
        {
            return b.Render(ControlDefinition.New<object>(
                "button",
                (b, data, props) =>
                {
                    b.AddPrimaryButtonStyle(props);
                    b.OnClickAction<EditConfigurationPage>(props, OnAddVariable);
                },
                (b, data) => b.T("Add variable")),
                b.Const(new object()));

        }
    }
}
