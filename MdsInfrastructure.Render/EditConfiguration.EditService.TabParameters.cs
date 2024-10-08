﻿using Metapsi.Syntax;
using Metapsi.Hyperapp;
using System.Linq;
using System;
using MdsCommon;
using MdsCommon.Controls;
using Metapsi.Html;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<IVNode> TabParameters(
            LayoutBuilder b,
            Var<EditConfigurationPage> clientModel)
        {
            var allParameters = b.Get(b.GetSelectedService(clientModel), x => x.InfrastructureServiceParameterDeclarations);
            var filteredParameters = b.FilterList(allParameters, b.Get(clientModel, x => x.ParametersFilter));
            return b.ListParametersGrid(clientModel, filteredParameters);


            //var rc= b.RenderCell<InfrastructureServiceParameterDeclaration>(
            //            (b, row, col) =>
            //            {
            //                var typeId = b.Get(row, x => x.ParameterTypeId);

            //                var typeLabel = b.Get(clientModel, typeId, (x, typeId) => x.ParameterTypes.SingleOrDefault(x => x.Id == typeId, new ParameterType() { Description = "(not selected)" }).Description);

            //                return b.VPadded4(b.Switch(
            //                    b.Get(col, x => x.Name),
            //                    b => b.Link(
            //                        b.Get(row, x => x.ParameterName, "(not set)"), 
            //                        b.MakeAction<EditConfigurationPage>(
            //                            (b, clientModel) =>
            //                            {
            //                                b.Set(clientModel, x => x.EditParameterId, b.Get(row, x => x.Id));
            //                                return b.EditView<EditConfigurationPage>(clientModel, EditParameter);
            //                            })),
            //                    ("Type", b => b.Text(typeLabel)),
            //                    ("Value", b => b.Text(b.Call(GetParameterValue, clientModel, row)))));
            //            });

            //return b.DataGrid<InfrastructureServiceParameterDeclaration>(
            //    new()
            //    {
            //        b=>b.AddClass(b.CommandButton<EditConfigurationPage>(b=>
            //        {
            //            b.Set(x=>x.Label, "Add parameter");
            //            b.Set(x => x.OnClick, onAddParameter);
            //        }), "text-white")
            //    },
            //    b =>
            //    {
            //        b.SetRows(parameters);
            //        b.AddColumn("Parameter");
            //        b.AddColumn("Type");
            //        b.AddColumn("Value");
            //        b.SetRenderCell<InfrastructureServiceParameterDeclaration>(rc);
            //    },
            //    (b, actions, item) =>
            //    {
            //        var removeIcon = Icon.Remove;

            //        var onRemove = b.Def((SyntaxBuilder b, Var<InfrastructureServiceParameterDeclaration> parameter) =>
            //        {
            //            var paramId = b.Get(parameter, x => x.Id);
            //            var parameterRemoved = b.Get(service, paramId, (x, paramId) => x.InfrastructureServiceParameterDeclarations.Where(x => x.Id != paramId).ToList());
            //            b.Set(service, x => x.InfrastructureServiceParameterDeclarations, parameterRemoved);
            //            var bindingRemoved = b.Get(parameter, paramId, (x, paramId) => x.InfrastructureServiceParameterBindings.Where(x => x.InfrastructureServiceParameterDeclarationId != paramId).ToList());
            //            b.Set(parameter, x => x.InfrastructureServiceParameterBindings, bindingRemoved);
            //            var valueRemoved = b.Get(parameter, paramId, (x, paramId) => x.InfrastructureServiceParameterValues.Where(x => x.InfrastructureServiceParameterDeclarationId != paramId).ToList());
            //            b.Set(parameter, x => x.InfrastructureServiceParameterValues, valueRemoved);
            //            var notesRemoved = b.Get(service, x => x.InfrastructureServiceNotes.Where(x => x.Reference != paramId.ToString()).ToList());
            //            b.Set(service, x => x.InfrastructureServiceNotes, notesRemoved);
            //        });

            //        b.Modify(actions, x => x.Commands, b =>
            //        {
            //            b.Add(b =>
            //            {
            //                b.Set(x => x.IconHtml, removeIcon);
            //                b.Set(x => x.OnCommand, onRemove);
            //            });
            //        });
            //    });
        }

        public static Var<string> GetParameterValue(this SyntaxBuilder b, Var<EditConfigurationPage> page, Var<InfrastructureServiceParameterDeclaration> parameter)
        {
            var parameterId = b.Get(parameter, x => x.Id);
            var parameterValue = b.Get(parameter, parameterId, (x, parameterId) => x.InfrastructureServiceParameterValues.SingleOrDefault(x => x.InfrastructureServiceParameterDeclarationId == parameterId));
            return b.If(
                b.HasObject(parameterValue),
                b => b.Get(parameterValue, x => x.ParameterValue),
                b =>
                {
                    var bindingVarId = b.Get(parameter, parameterId,
                        (x, parameterId) => x.InfrastructureServiceParameterBindings.Single(x => x.InfrastructureServiceParameterDeclarationId == parameterId).InfrastructureVariableId);
                    var variable = b.Get(page, bindingVarId, (x, bindingVarId) => x.Configuration.InfrastructureVariables.SingleOrDefault(x => x.Id == bindingVarId, new InfrastructureVariable() { VariableValue = "(not selected)" }));
                    return b.Get(variable, x => x.VariableValue);
                });
        }

        public static Var<IVNode> ListParametersGrid(
            this LayoutBuilder b, 
            Var<EditConfigurationPage> clientModel,
            Var<System.Collections.Generic.List<InfrastructureServiceParameterDeclaration>> displayedParameters)
        {
            var parametersGrid = MdsDefaultBuilder.DataGrid<InfrastructureServiceParameterDeclaration>();
            parametersGrid.DataTableBuilder.OverrideDataCell(
                "Parameter",
                (b, parameter) =>
                {
                    return b.Link(
                        b.Get(parameter, x => x.ParameterName, "(not set)"),
                        b.MakeAction<EditConfigurationPage>(
                            (b, clientModel) =>
                            {
                                b.Set(clientModel, x => x.EditParameterId, b.Get(parameter, x => x.Id));
                                return b.EditView<EditConfigurationPage>(clientModel, EditParameter);
                            }));
                });
            parametersGrid.DataTableBuilder.OverrideDataCell(
                "Type",
                (b, parameter) =>
                {
                    var typeId = b.Get(parameter, x => x.ParameterTypeId);
                    var typeLabel = b.Get(clientModel, typeId, (x, typeId) => x.ParameterTypes.SingleOrDefault(x => x.Id == typeId, new ParameterType() { Description = "(not selected)" }).Description);
                    return b.Text(typeLabel);
                });

            parametersGrid.DataTableBuilder.OverrideDataCell(
                "Value",
                (b, parameter) =>
                {
                    return b.Text(b.Call(GetParameterValue, clientModel, parameter));
                });

            parametersGrid.CreateToolbarActions = (b) =>
            {
                return b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row items-center justify-between");
                    },
                    b.HtmlButton(
                        b =>
                        {
                            b.AddPrimaryButtonStyle();
                            b.OnClickAction<EditConfigurationPage, HtmlButton>(AddParameter);
                        },
                        b.Text("Add parameter")),
                    b.Filter(clientModel, x => x.ParametersFilter));
            };

            parametersGrid.AddRowAction((b, parameter) =>
            {
                return b.DeleteRowIconAction(b =>
                {
                    b.OnClickAction(b.MakeActionDescriptor<EditConfigurationPage, InfrastructureServiceParameterDeclaration>(RemoveParameter, parameter));
                });
            });

            return b.DataGrid(
                parametersGrid, 
                displayedParameters,
                b.Const(new System.Collections.Generic.List<string>()
                {
                    "Parameter",
                    "Type",
                    "Value"
                }));
        }

        public static Var<EditConfigurationPage> RemoveParameter(SyntaxBuilder b, Var<EditConfigurationPage> clientModel, Var<InfrastructureServiceParameterDeclaration> parameter)
        {
            var service = b.GetSelectedService(clientModel);
            var paramId = b.Get(parameter, x => x.Id);
            var parameterRemoved = b.Get(service, paramId, (x, paramId) => x.InfrastructureServiceParameterDeclarations.Where(x => x.Id != paramId).ToList());
            b.Set(service, x => x.InfrastructureServiceParameterDeclarations, parameterRemoved);
            var bindingRemoved = b.Get(parameter, paramId, (x, paramId) => x.InfrastructureServiceParameterBindings.Where(x => x.InfrastructureServiceParameterDeclarationId != paramId).ToList());
            b.Set(parameter, x => x.InfrastructureServiceParameterBindings, bindingRemoved);
            var valueRemoved = b.Get(parameter, paramId, (x, paramId) => x.InfrastructureServiceParameterValues.Where(x => x.InfrastructureServiceParameterDeclarationId != paramId).ToList());
            b.Set(parameter, x => x.InfrastructureServiceParameterValues, valueRemoved);
            var notesRemoved = b.Get(service, paramId, (x, paramId) => x.InfrastructureServiceNotes.Where(x => x.Reference != paramId.ToString()).ToList());
            b.Set(service, x => x.InfrastructureServiceNotes, notesRemoved);
            return b.Clone(clientModel);
        }

        public static Var<EditConfigurationPage> AddParameter(SyntaxBuilder b, Var<EditConfigurationPage> clientModel)
        {
            var selectedService = b.GetSelectedService(clientModel);
            var selectedServiceId = b.Get(selectedService, x => x.Id);
            var parameters = b.Get(selectedService, x => x.InfrastructureServiceParameterDeclarations);

            var newParameterId = b.NewId();
            var newValueId = b.NewId();
            var newParam = b.NewObj<InfrastructureServiceParameterDeclaration>(b =>
            {
                b.Set(x => x.Id, newParameterId);
                b.Set(x => x.ParameterName, b.Const(""));
                b.Set(x => x.InfrastructureServiceId, selectedServiceId);
            });

            b.Push(
                b.Get(newParam, x => x.InfrastructureServiceParameterValues),
                b.NewObj<InfrastructureServiceParameterValue>(
                    b =>
                    {
                        b.Set(x => x.Id, newValueId);
                        b.Set(x => x.InfrastructureServiceParameterDeclarationId, newParameterId);
                    }));

            b.Push(parameters, newParam);
            b.Set(clientModel, x => x.EditParameterId, newParameterId);
            return b.EditView<EditConfigurationPage>(clientModel, EditParameter);
        }
    }
}
