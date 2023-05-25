//using Metapsi;
//using System;
//using System.Linq;
//using UI.Svelte;

//namespace MdsInfrastructure
//{
//    public record PendingParameter
//    {
//        public string ParameterName { get; set; } = String.Empty;
//        public Guid ParameterTypeId { get; set; }
//        public Guid BoundVariableId { get; set; }
//        public string ParameterValue { get; set; } = String.Empty;

//        public bool IsBound { get; set; }
//    }

//    public class EditServiceParameterPage : EditServicePage
//    {
//        public PendingParameter PendingParameter { get; set; } = new();
//    }


//    public static partial class MdsInfrastructureFunctions
//    {
//        public static UI.Svelte.View RenderEditServiceParameterPage(EditServiceParameterPage dataModel, ViewBuilder viewBuilder)
//        {
//            var localBuilder = viewBuilder;
//            var page = dataModel;

//            viewBuilder.AddOkBackHeader<EditServiceParameterPage, EditServicePage, DataStructure.ServiceParameter>(
//                dataModel,
//                $"Configuration {dataModel.ConfigurationHeader.Name}, service {dataModel.PendingService.Service.ServiceName}, parameter {dataModel.PendingParameter.ParameterName}",
//                (page) =>
//                {
//                    var commitedParameter = page.PendingService.GetPendingParameter(page.PendingParameter.ParameterName);
//                    return commitedParameter != page.PendingParameter;
//                },
//                //dataModel.NoValidation(),
//                async (cc, page, id) =>
//                {
//                    var removedParameter = page.PendingService.InfrastructureServiceParameterDeclarations.SingleOrDefault(x => x.ParameterName == page.PendingParameter.ParameterName);
//                    if (Metapsi.Record.IsValid(removedParameter))
//                    {
//                        page.PendingService.InfrastructureServiceParameterDeclarations.RemoveAll(x => x.Id == removedParameter.Id);
//                        page.PendingService.InfrastructureServiceParameterBindings.RemoveAll(x => x.InfrastructureServiceParameterDeclarationId == removedParameter.Id);
//                        page.PendingService.InfrastructureServiceParameterValues.RemoveAll(x => x.InfrastructureServiceParameterDeclarationId == removedParameter.Id);
//                    }

//                    var parameterDeclaration = new Record.InfrastructureServiceParameterDeclaration()
//                    {
//                        InfrastructureServiceId = page.PendingService.Service.Id,
//                        ParameterName = page.PendingParameter.ParameterName,
//                        ParameterTypeId = page.PendingParameter.ParameterTypeId,
//                    };

//                    page.PendingService.InfrastructureServiceParameterDeclarations.Add(parameterDeclaration);

//                    if (page.PendingParameter.IsBound)
//                    {
//                        page.PendingService.InfrastructureServiceParameterBindings.Add(new Record.InfrastructureServiceParameterBinding()
//                        {
//                            InfrastructureServiceParameterDeclarationId = parameterDeclaration.Id,
//                            InfrastructureVariableId = page.PendingParameter.BoundVariableId
//                        });
//                    }
//                    else
//                    {
//                        page.PendingService.InfrastructureServiceParameterValues.Add(new Record.InfrastructureServiceParameterValue()
//                        {
//                            InfrastructureServiceParameterDeclarationId = parameterDeclaration.Id,
//                            ParameterValue = page.PendingParameter.ParameterValue
//                        });
//                    }

//                    return page;
//                },
//                async (cc, page, id) =>
//                {
//                    return Copy.To<EditServicePage>(page);
//                });

//            var pageGroup = viewBuilder.Group("grpPage", Guid.Empty, ViewBuilder.RootGroupId, "Vertical");

//            var groupId = pageGroup.Id;

//            localBuilder.TextBox<EditServiceParameterPage>(
//                "txtServiceName",
//                Guid.Empty,
//                dataModel.PendingParameter.ParameterName,
//                "Parameter name",
//                (editPage, referencedId, commandValue) =>
//                {
//                    editPage.PendingParameter.ParameterName = commandValue;
//                    return editPage;
//                },
//                -1,
//                groupId);

//            Guid selectedParameterTypeId = Guid.Empty;
//            var paramType = page.ParameterTypes.SingleOrDefault(x => x.Id == page.PendingParameter.ParameterTypeId);
//            if (paramType != null)
//            {
//                selectedParameterTypeId = paramType.Id;
//            }

//            localBuilder.AddDropDown<EditServiceParameterPage, Record.ParameterType>(
//                "ddParamType",
//                Guid.Empty,
//                selectedParameterTypeId,
//                groupId,
//                "Type",
//                dataModel,
//                (page, referencedId, selectedId) =>
//                {
//                    page.PendingParameter.ParameterTypeId = selectedId;
//                    return page;
//                },
//                (page) => page.ParameterTypes,
//                (pt) => pt.Description);

//            localBuilder.CheckBox<EditServiceParameterPage>(
//                "chkBound",
//                Guid.Empty,
//                groupId,
//                "From variable",
//                async (cc, page, parameterId, isBound) =>
//                {
//                    if (isBound)
//                    {
//                        page.PendingParameter.ParameterValue = String.Empty;
//                        page.PendingParameter.IsBound = true;
//                    }
//                    else
//                    {
//                        page.PendingParameter.BoundVariableId = Guid.Empty;
//                        page.PendingParameter.IsBound = false;
//                    }
//                    return page;
//                },
//                dataModel.PendingParameter.IsBound,
//                -1);

//            if (dataModel.PendingParameter.IsBound)
//            {
//                localBuilder.AddDropDown(
//                    "ddBoundVar",
//                    Guid.Empty,
//                    page.PendingParameter.BoundVariableId,
//                    groupId,
//                    "Variable",
//                    page,
//                    (ddModel, referencedId, selectedId) =>
//                    {
//                        ddModel.PendingParameter.BoundVariableId = selectedId;
//                        return ddModel;
//                    },
//                    ddModel => ddModel.Configuration.InfrastructureVariables,
//                    (v) => $"{v.VariableName} (current value: {v.VariableValue})");
//            }
//            else
//            {
//                localBuilder.TextBox<EditServiceParameterPage>(
//                    "txtVarVal", 
//                    Guid.Empty, 
//                    page.PendingParameter.ParameterValue,
//                    "Value",
//                    (txtPage, refId, newVal) =>
//                    {
//                        txtPage.PendingParameter.ParameterValue = newVal;
//                        return txtPage;
//                    }, -1, groupId);
//            }

//            return viewBuilder.OutputView;
//        }
//    }
//}
