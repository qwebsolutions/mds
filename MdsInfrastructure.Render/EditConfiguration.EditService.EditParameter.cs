using Metapsi.Syntax;
using Metapsi.Hyperapp;
using System.Linq;
using System;
using MdsCommon.Controls;
using Metapsi.Html;
using Metapsi.TomSelect;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<IVNode> EditParameter(
            LayoutBuilder b,
            Var<EditConfigurationPage> clientModel)
        {
            var parameter = b.GetEditedParameter(clientModel);
            var paramId = b.Get(parameter, x => x.Id);
            var serviceId = b.Get(parameter, x => x.InfrastructureServiceId);
            var service = b.Get(clientModel, serviceId, (x, serviceId) => x.Configuration.InfrastructureServices.Single(x => x.Id == serviceId));

            var boundToVariable =
                b.Get(parameter, paramId, (x, paramId) => x.InfrastructureServiceParameterBindings.Count(x => x.InfrastructureServiceParameterDeclarationId == paramId) != 0);

            var fromVariableToggle = b.Toggle(
                boundToVariable,
                b.MakeAction<EditConfigurationPage, bool>(
                    (SyntaxBuilder b, Var<EditConfigurationPage> state, Var<bool> isOn) =>
                    b.If(isOn,
                        b =>
                        {
                            var newId = b.NewId();
                            var emptyId = b.EmptyId();

                            var valueRemoved = b.Get(parameter, paramId, (x, paramId) => x.InfrastructureServiceParameterValues.Where(x => x.InfrastructureServiceParameterDeclarationId != paramId).ToList());
                            b.Set(parameter, x => x.InfrastructureServiceParameterValues, valueRemoved);
                            b.Push(
                                b.Get(parameter, x => x.InfrastructureServiceParameterBindings),
                                b.NewObj<InfrastructureServiceParameterBinding>(
                                    b =>
                                    {
                                        b.Set(x => x.Id, newId);
                                        b.Set(x => x.InfrastructureServiceParameterDeclarationId, paramId);
                                        b.Set(x => x.InfrastructureVariableId, emptyId);
                                    }));

                            return b.Clone(clientModel);
                        },
                        b =>
                        {
                            var newId = b.NewId();

                            b.Push(b.Get(parameter, x => x.InfrastructureServiceParameterValues),
                                b.NewObj<InfrastructureServiceParameterValue>(b =>
                                {
                                    b.Set(x => x.Id, newId);
                                    b.Set(x => x.InfrastructureServiceParameterDeclarationId, paramId);
                                    b.Set(x => x.ParameterValue, string.Empty);
                                }));

                            var bindingsRemoved = b.Get(parameter, paramId, (x, paramId) => x.InfrastructureServiceParameterBindings.Where(x => x.InfrastructureServiceParameterDeclarationId != paramId).ToList());
                            b.Set(parameter, x => x.InfrastructureServiceParameterBindings, bindingsRemoved);
                            return b.Clone(clientModel);
                        })),
                b.Const("From variable"),
                b.Const("Value"),
                b=>
                {
                    b.Set(x => x.ExtraRootCss, "col-span-2");
                });

            var view = b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col gap-4 bg-white rounded p-4 drop-shadow");
                },
                b.HtmlDiv( // top
                    b =>
                    {
                        b.SetClass("flex flex-row justify-end");
                    },
                    b.Toolbar(b => { }, b.OkButton(EditService, x => x.EditParameterId))
                    ),
                b.HtmlDiv(
                    b=>
                    {
                        b.SetClass("grid grid-cols-2 gap-4 items-center");
                    },
                    b.TextSpan("Parameter name"),
                    b.MdsInputText(
                        b=>
                        {
                            b.BindTo(clientModel, GetEditedParameter, x => x.ParameterName);
                        }),
                    b.TextSpan("Type"),
                    b.MdsDropDown(
                        b=>
                        {
                            b.SetOptions(b.Get(clientModel, x => x.ParameterTypes), x => x.Id, x => x.Description);
                            b.SetItem(b.Get(parameter, x => x.ParameterTypeId));
                            b.OnChange(b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<string> value) =>
                            {
                                b.Set(b.GetEditedParameter(page), x => x.ParameterTypeId, b.ParseId(value));
                                return b.Clone(page);
                            }));
                        }),
                    fromVariableToggle,
                    b.Optional(
                        boundToVariable, 
                        b=> b.TextSpan("Variable")),
                    b.Optional(
                        boundToVariable,
                        b =>
                        {
                            var binding = b.Get(parameter, paramId, (x, paramId) => x.InfrastructureServiceParameterBindings.Single(x => x.InfrastructureServiceParameterDeclarationId == paramId));

                            return b.MdsDropDown(b =>
                            {
                                b.SetOptions(
                                    b.Get(clientModel, x => x.Configuration.InfrastructureVariables),
                                    x => x.Id,
                                    x => x.VariableName + "(" + x.VariableValue + ")");
                                b.SetItem(b.Get(binding, x => x.InfrastructureVariableId));
                                b.OnChange(b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<string> payload) =>
                                {
                                    var binding = b.Get(parameter, paramId, (x, paramId) => x.InfrastructureServiceParameterBindings.Single(x => x.InfrastructureServiceParameterDeclarationId == paramId));
                                    b.Set(binding, x => x.InfrastructureVariableId, b.ParseId(payload));
                                    return b.Clone(page);
                                }));
                            });
                        }),
                    b.Optional(
                        b.Not(boundToVariable),
                        b=> b.TextSpan("Value")),
                    b.Optional(
                        b.Not(boundToVariable),
                        b=>
                        {
                            return b.MdsInputText(
                                b =>
                                {
                                    b.BindTo(
                                        clientModel,
                                        (b, model) =>
                                        b.Get(
                                            b.GetEditedParameter(clientModel),
                                            x => x.InfrastructureServiceParameterValues.Single()),
                                        x => x.ParameterValue);
                                });
                        }))
                );

            return view;
        }

        public static Var<InfrastructureServiceParameterDeclaration> GetEditedParameter(this SyntaxBuilder b, Var<EditConfigurationPage> clientModel)
        {
            var paramId = b.Get(clientModel, x => x.EditParameterId);
            var parameter = b.Get(clientModel, paramId, (x, paramId) => x.Configuration.InfrastructureServices.SelectMany(x => x.InfrastructureServiceParameterDeclarations).Single(x => x.Id == paramId));
            return parameter;
        }
    }
}
