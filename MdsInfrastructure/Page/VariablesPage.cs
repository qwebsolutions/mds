//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using UI.Svelte;
//using Metapsi;
//using System.Linq;
//using MdsInfrastructure.Record;

//namespace MdsInfrastructure
//{
//    public class EditVariablePage : EditConfigurationPage
//    {
//        public Record.InfrastructureVariable PendingVariable { get; set; }

//        public IEnumerable<Record.InfrastructureService> UsedByServices
//        {
//            get
//            {
//                List<InfrastructureServiceParameterDeclaration> parameterDeclarations = new List<InfrastructureServiceParameterDeclaration>();

//                var boundDeclarationIds = new HashSet<Guid>(
//                    Configuration.InfrastructureServiceParameterBindings.Where(x => x.InfrastructureVariableId == PendingVariable.Id)
//                    .Select(x => x.InfrastructureServiceParameterDeclarationId));

//                var valueDeclarationIds = new HashSet<Guid>(
//                    Configuration.InfrastructureServiceParameterValues.Where(x => x.ParameterValue.Contains($"$({PendingVariable.VariableName})"))
//                    .Select(x => x.InfrastructureServiceParameterDeclarationId));

//                var variableParameters = Configuration.InfrastructureServiceParameterDeclarations.Where(x => boundDeclarationIds.Contains(x.Id) || valueDeclarationIds.Contains(x.Id));

//                var serviceIds = new HashSet<Guid>(variableParameters.Select(x => x.InfrastructureServiceId));

//                return Configuration.InfrastructureServices.Where(x => serviceIds.Contains(x.Id));
//            }
//        }
//    }

//    //[Relation(nameof(InfrastructureVariables), nameof(InfrastructureVariable.Id), nameof(InfrastructureServiceParameterBindings), nameof(InfrastructureServiceParameterBinding.InfrastructureVariableId))]
//    //[Relation(nameof(InfrastructureServiceParameterBindings), nameof(InfrastructureServiceParameterBinding.InfrastructureServiceParameterDeclarationId), nameof(InfrastructureServiceParameterDeclarations), nameof(InfrastructureServiceParameterDeclaration.Id))]
//    //[Relation(nameof(InfrastructureServiceParameterDeclarations), nameof(InfrastructureServiceParameterDeclaration.InfrastructureServiceId), nameof(InfrastructureServices), nameof(InfrastructureService.Id))]

//    //public class VariableServices : IDataStructure
//    //{
//    //    [Order(1)] public RecordCollection<InfrastructureVariable> InfrastructureVariables { get; set; } = new();
//    //    [Order(2)] public RecordCollection<InfrastructureServiceParameterBinding> InfrastructureServiceParameterBindings { get; set; } = new();
//    //    [Order(3)] public RecordCollection<InfrastructureServiceParameterDeclaration> InfrastructureServiceParameterDeclarations { get; set; } = new();
//    //    [Order(4)] public RecordCollection<InfrastructureService> InfrastructureServices { get; set; } = new();
//    //}

//    public static partial class MdsInfrastructureFunctions
//    {
//        public static View RenderEditVariables(EditConfigurationPage dataModel, ViewBuilder viewBuilder, Guid inGroupId)
//        {
//            viewBuilder.ActionSelectionGrid(
//                "grdListVariables",
//                Guid.Empty,
//                dataModel,
//                inGroupId,
//                (localModel, _) => localModel.Configuration.InfrastructureVariables,
//                () =>
//                {
//                    List<CaptionMapping> columns = new List<CaptionMapping>();

//                    columns.Add(new CaptionMapping()
//                    {
//                        CaptionText = "Name",
//                        FieldName = nameof(Record.InfrastructureVariable.VariableName)
//                    });

//                    columns.Add(new CaptionMapping()
//                    {
//                        CaptionText = "Value",
//                        FieldName = nameof(Record.InfrastructureVariable.VariableValue)
//                    });

//                    return columns;
//                },
//                (localModel, variable, columnName) =>
//                {
//                    switch (columnName)
//                    {
//                        case nameof(Record.InfrastructureVariable.VariableName): return variable.VariableName;
//                        case nameof(Record.InfrastructureVariable.VariableValue): return variable.VariableValue;
//                    }
//                    return string.Empty;
//                },
//                "Add variable",
//                AddVariable,
//                "Edit variable",
//                EditVariable,
//                "Delete variable",
//                DeleteVariable);

//            return viewBuilder.OutputView;
//        }

//        public static async Task<EditVariablePage> AddVariable(CommandContext commandContext, EditConfigurationPage dataModel, Guid _)
//        {
//            EditVariablePage editVariablePage = Copy.To<EditVariablePage>(dataModel);
//            editVariablePage.PendingVariable = new Record.InfrastructureVariable()
//            {
//                ConfigurationHeaderId = dataModel.Configuration.ConfigurationHeader.Single().Id
//            };

//            return editVariablePage;
//        }

//        public static async Task<EditVariablePage> EditVariable(CommandContext commandContext, EditConfigurationPage dataModel, Guid _)
//        {
//            dataModel.RegisterSelectionId<InfrastructureVariable>();
//            EditVariablePage editVariablePage = Copy.To<EditVariablePage>(dataModel);
//            editVariablePage.PendingVariable = dataModel.GetSelected(dataModel.Configuration.InfrastructureVariables).Clone();
//            return editVariablePage;
//        }

//        public static async Task<EditConfigurationPage> DeleteVariable(CommandContext commandContext, EditConfigurationPage dataModel, Guid _)
//        {
//            var variableId = dataModel.RegisterSelectionId<InfrastructureVariable>();
//            dataModel.Configuration.InfrastructureVariables.Remove(variableId);
//            return dataModel;
//        }

//        public static View RenderEditVariablePage(EditVariablePage dataModel, UI.Svelte.ViewBuilder viewBuilder)
//        {
//            var configuration = dataModel.ConfigurationHeader;
//            var infrastructureVariable = dataModel.PendingVariable;

//            viewBuilder.AddOkBackHeader<EditVariablePage, EditConfigurationPage, Record.InfrastructureVariable>(
//                dataModel,
//                $"Configuration {configuration.Name}, variable {infrastructureVariable.VariableName}",
//                (page) => Metapsi.Record.Different(page.PendingVariable, page.Configuration.InfrastructureVariables.ByIdOrDefault(page.PendingVariable.Id)),
//                //dataModel.NoValidation(),
//                async (cc, page, _) =>
//                {
//                    page.Configuration.InfrastructureVariables.Set(page.PendingVariable);
//                    return page;
//                },
//                async (cc, page, _) =>
//                {
//                    return Copy.To<EditConfigurationPage>(page);
//                });

//            viewBuilder.TextBox<EditVariablePage>(
//                "txtVarName",
//                infrastructureVariable.Id,
//                infrastructureVariable.VariableName,
//                "Variable name",
//                (page, id, newValue) =>
//                {
//                    page.PendingVariable.VariableName = newValue;
//                    return page;
//                },
//                300,
//                ViewBuilder.RootGroupId);

//            viewBuilder.TextBox<EditVariablePage>(
//                "txtVarValue",
//                infrastructureVariable.Id,
//                infrastructureVariable.VariableValue,
//                "Variable value",
//                (page, id, newValue) =>
//                {
//                    page.PendingVariable.VariableValue = newValue;
//                    return page;
//                },
//                300,
//                ViewBuilder.RootGroupId);

//            viewBuilder.Group("spacer", Guid.Empty, ViewBuilder.RootGroupId, "Horizontal");

//            foreach (var service in dataModel.UsedByServices)
//            {
//                viewBuilder.TextBox<EditVariablePage>(
//                    "txtUsedBy",
//                    service.Id,
//                    service.ServiceName,
//                    "Used by",
//                    (page, id, newValue) => page,
//                    300,
//                    ViewBuilder.RootGroupId,
//                    enabled: false);
//            }

//            return viewBuilder.OutputView;
//        }
//    }
//}