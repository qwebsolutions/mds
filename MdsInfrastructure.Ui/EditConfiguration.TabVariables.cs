﻿using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        public static void ApplyVariablesFilter(this BlockBuilder b, Var<EditConfigurationPage> clientModel)
        {
            b.Set(
                 clientModel,
                 x => x.FilteredVariables,
                 b.ContainingText(
                     b.Get(clientModel, x => x.Configuration.InfrastructureVariables),
                     b.Get(clientModel, x => x.VariablesFilterValue)
                     ));
        }
        public static Var<HyperNode> TabVariables(
           BlockBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            var configId = b.Get(clientModel, x => x.Configuration.Id);

            var onAddVariable = b.MakeAction((BlockBuilder b, Var<EditConfigurationPage> clientModel) =>
            {
                var newId = b.NewId();
                var newVar = b.NewObj<InfrastructureVariable>(b =>
                {
                    b.Set(x => x.ConfigurationHeaderId, configId);
                    b.Set(x => x.Id, newId);
                });
                b.Push(b.Get(clientModel, x => x.Configuration.InfrastructureVariables), newVar);
                b.Call(ApplyVariablesFilter, clientModel);
                b.GoTo(clientModel, EditVariable, newVar);

                return b.Clone(clientModel);
            });

            var rows = b.Get(clientModel, x => x.FilteredVariables.OrderBy(x => x.VariableName).ToList());
            var rc = b.RenderCell<InfrastructureVariable>((b, row, column) =>
            {
                var columnName = b.Get(column, x => x.Name);
                return b.VPadded4(b.Switch(columnName,
                    b => b.Link(b.GetProperty<string>(row, columnName), b.MakeAction((BlockBuilder b, Var<EditConfigurationPage> clientModel) => b.GoTo(clientModel, EditVariable, row))),
                    (nameof(InfrastructureVariable.VariableValue), b => b.Text(b.Get(row, x => x.VariableValue)))));
            });

            return b.DataGrid<InfrastructureVariable>(
                new()
                {
                    b=> b.CommandButton<EditConfigurationPage>(b=>
                    {
                        b.Set(x => x.Label, "Add variable");
                        b.Set(x => x.OnClick, onAddVariable);
                    }),
                      b => b.Filter<EditConfigurationPage>(
                                clientModel,
                                x => x.VariablesFilterValue,
                                ApplyVariablesFilter,
                                "")
                },
                b =>
                {
                    b.AddColumn(nameof(InfrastructureVariable.VariableName), "Name");
                    b.AddColumn(nameof(InfrastructureVariable.VariableValue), "Value");
                    b.SetRows(rows);
                    b.SetRenderCell<InfrastructureVariable>(rc);
                },
                (b, actions, row) =>
                {
                    var variableId = b.Get(row.As<InfrastructureVariable>(), x => x.Id);
                    b.Comment("isInUse");
                    var isInUse = b.Get(clientModel, variableId, (x, variableId) => x.Configuration.InfrastructureServices.SelectMany(x => x.InfrastructureServiceParameterDeclarations.SelectMany(x => x.InfrastructureServiceParameterBindings)).Any(x => x.InfrastructureVariableId == variableId));

                    var removeIcon = Icon.Remove;

                    var onRemove = b.Def((BlockBuilder b, Var<InfrastructureVariable> variable) =>
                    {
                        var typed = variable.As<InfrastructureVariable>();
                        var removed = b.Get(clientModel, typed, (x, typed) => x.Configuration.InfrastructureVariables.Where(x => x != typed).ToList());
                        b.Set(b.Get(clientModel, x => x.Configuration), x => x.InfrastructureVariables, removed);
                        b.Set(clientModel, x => x.FilteredVariables, removed);
                    });

                    b.If(b.Not(isInUse), b =>
                    {
                        b.Modify(actions, x => x.Commands, b =>
                        {
                            b.Add(b =>
                                {
                                    b.Set(x => x.IconHtml, removeIcon);
                                    b.Set(x => x.OnCommand, onRemove);
                                });
                        });
                    });
                });
        }
    }
}
