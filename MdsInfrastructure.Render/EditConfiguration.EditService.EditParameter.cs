using Metapsi.Syntax;
using Metapsi.Hyperapp;
using System.Linq;
using System;
using MdsCommon.Controls;
using Metapsi.ChoicesJs;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<HyperNode> EditParameter(
            LayoutBuilder b,
            Var<EditConfigurationPage> clientModel)
        {
            var parameter = b.GetEditedParameter(clientModel);
            var paramId = b.Get(parameter, x => x.Id);
            var serviceId = b.Get(parameter, x => x.InfrastructureServiceId);
            var service = b.Get(clientModel, serviceId, (x, serviceId) => x.Configuration.InfrastructureServices.Single(x => x.Id == serviceId));

            var view = b.Div("flex flex-col gap-4 bg-white rounded p-4 drop-shadow");


            var top = b.Add(view, b.Div("flex flex-row justify-end"));
            var toolbar = b.Toolbar(b.OkButton(EditService, x => x.EditParameterId));

            b.Add(top, toolbar);

            var form = b.Add(view, b.Div("grid grid-cols-2 gap-4 items-center"));
            b.Add(form, b.Text("Parameter name"));
            b.Add(form, b.BoundInput(clientModel, paramId, (x, paramId) => x.Configuration.InfrastructureServices.SelectMany(x => x.InfrastructureServiceParameterDeclarations).Single(x => x.Id == paramId), x => x.ParameterName, b.Const("Parameter name")));
            b.Add(form, b.Text("Type"));

            //var transform =
            //    (BlockBuilder b, Var<ParameterType> t) =>
            //    {
            //        var description = b.Get(t, t => t.Description);
            //        var value = b.Get(t, t => t.Id.ToString());
            //        return b.NewObj<DropDown.Option>(b =>
            //        {
            //            b.Set(x => x.label, description);
            //            b.Set(x => x.value, value);
            //        });
            //    };


            //b.Add(form, b.BoundDropDown(
            //    b.Const("ddParameterType"),
            //    parameter,
            //    x => x.ParameterTypeId,
            //    b.Get(clientModel,x => x.ParameterTypes),
            //    b.Def(transform)));

            var parameterTypeChoices = b.MapChoices(b.Get(clientModel, x => x.ParameterTypes), x => x.Id, x => x.Description, b.Get(parameter, x => x.ParameterTypeId));

            var parameterTypeDd = b.Add(form, b.DropDown(parameterTypeChoices));
            Metapsi.ChoicesJs.Event.SetOnChange(b, parameterTypeDd, b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<string> payload) =>
            {
                b.Set(b.GetEditedParameter(page), x => x.ParameterTypeId, b.ParseId(payload));
                return b.Clone(page);
            }));

            var boundToVariable =
                b.Get(parameter, paramId, (x, paramId) => x.InfrastructureServiceParameterBindings.Count(x => x.InfrastructureServiceParameterDeclarationId == paramId) != 0);

            b.Add(form, b.Toggle(
                boundToVariable,
                b.MakeAction<EditConfigurationPage, bool>(
                    (SyntaxBuilder b, Var<EditConfigurationPage> state, Var<bool> isOn) => 
                    b.If(isOn,
                        b =>
                        {
                            var newId = b.NewId();
                            var emptyId = b.EmptyId();

                            var valueRemoved = b.Get(parameter, paramId, (x, paramId) => x.InfrastructureServiceParameterValues.Where(x => x.InfrastructureServiceParameterDeclarationId != paramId));
                            b.Set(parameter, x => x.InfrastructureServiceParameterValues, valueRemoved);
                            b.Modify(parameter, x => x.InfrastructureServiceParameterBindings, b =>
                            {
                                b.Add(b =>
                                {
                                    b.Set(x => x.Id, newId);
                                    b.Set(x => x.InfrastructureServiceParameterDeclarationId, paramId);
                                    b.Set(x => x.InfrastructureVariableId, emptyId);
                                });
                            });
                                
                            return b.Clone(clientModel);
                        },
                        b =>
                        {
                            var newId = b.NewId();

                            b.Modify(parameter, x => x.InfrastructureServiceParameterValues, b =>
                            {
                                b.Add(b =>
                                {
                                    b.Set(x => x.Id, newId);
                                    b.Set(x => x.InfrastructureServiceParameterDeclarationId, paramId);
                                    b.Set(x => x.ParameterValue, string.Empty);
                                });
                            });

                            var bindingsRemoved = b.Get(parameter, paramId, (x, paramId) => x.InfrastructureServiceParameterBindings.Where(x => x.InfrastructureServiceParameterDeclarationId != paramId));
                            b.Set(parameter, x => x.InfrastructureServiceParameterBindings, bindingsRemoved);
                            return b.Clone(clientModel);
                        })),
                b.Const("From variable"),
                b.Const("Value")));

            b.Add(form, b.Div());// Empty space div to the right of the toggle

            b.If(boundToVariable,
                b =>
                {
                    b.Add(form, b.Text("Variable"));

                    var binding = b.Get(parameter, paramId, (x, paramId) => x.InfrastructureServiceParameterBindings.Single(x => x.InfrastructureServiceParameterDeclarationId == paramId));

                    var variableChoices = b.MapChoices(
                        b.Get(clientModel, x => x.Configuration.InfrastructureVariables),
                        x => x.Id,
                        x => x.VariableName + "(" + x.VariableValue + ")",
                        b.Get(binding, x => x.InfrastructureVariableId));

                    var ddParamVar = b.Add(form, b.DropDown(variableChoices));
                    Metapsi.ChoicesJs.Event.SetOnChange(b, ddParamVar, b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<string> payload) =>
                    {
                        var binding = b.Get(parameter, paramId, (x, paramId) => x.InfrastructureServiceParameterBindings.Single(x => x.InfrastructureServiceParameterDeclarationId == paramId));
                        b.Set(binding, x => x.InfrastructureVariableId, b.ParseId(payload));
                        return b.Clone(page);
                    }));
                },
                b =>
                {
                    b.Add(form, b.Text("Value"));
                    b.Log("paramId", paramId);
                    var value = b.Get(parameter, x=> x.InfrastructureServiceParameterValues.Single());
                    b.Add(form, b.BoundInput(value, x => x.ParameterValue, b.Const("Value")));
                });

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
