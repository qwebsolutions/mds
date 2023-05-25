//using Metapsi;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using UI.Svelte;

//namespace MdsInfrastructure
//{
//    public class EditServicePage : EditConfigurationPage
//    {
//        public DataStructure.InfrastructureService PendingService { get; set; } = new DataStructure.InfrastructureService();
//        //public List<Record.ParameterType> ParameterTypes { get; set; }
//        //public List<Record.NoteType> NoteTypes { get; set; }

//        public Record.InfrastructureService Service => PendingService.Service;
//    }


//    public static partial class MdsInfrastructureFunctions
//    {
//        public static View RenderListServices(EditConfigurationPage dataModel, UI.Svelte.ViewBuilder viewBuilder, Guid inGroupId)
//        {
//            var grid = viewBuilder.ActionSelectionGrid(
//                "grdListServices",
//                Guid.Empty,
//                dataModel,
//                inGroupId,
//                (pageData, _) => pageData.Configuration.InfrastructureServices.OrderBy(x => x.ServiceName),
//                GetListServicesColumns,
//                GetServiceCellValue,
//                "Add service",
//                AddService,
//                "Edit service",
//                EditService,
//                "Remove service",
//                DeleteService);

//            var disabledServices = dataModel.Configuration.InfrastructureServices.Where(x => !x.Enabled);
//            grid.Badges.Clear();
//            foreach (var s in disabledServices)
//            {
//                grid.Badges.Add(new DataGridBadge()
//                {
//                    ColumnName = nameof(Record.InfrastructureService.ServiceName),
//                    RowReferencedId = s.Id.ToString(),
//                    Badge = new Badge()
//                    {
//                        Name = "bdg_disabled" + s.Id,
//                        ReferencedId = s.Id,
//                        Styling = "muted",
//                        Text = "Disabled"
//                    }
//                });
//            }

//            return viewBuilder.OutputView;
//        }

//        public static List<CaptionMapping> GetListServicesColumns()
//        {
//            List<CaptionMapping> captionMappings = new List<CaptionMapping>();

//            captionMappings.Add(new CaptionMapping()
//            {
//                CaptionText = "Name",
//                FieldName = nameof(Record.InfrastructureService.ServiceName)
//            });

//            captionMappings.Add(new CaptionMapping()
//            {
//                CaptionText = "Project",
//                FieldName = nameof(Record.InfrastructureService.ProjectId)
//            });

//            captionMappings.Add(new CaptionMapping()
//            {
//                CaptionText = "Application",
//                FieldName = nameof(Record.InfrastructureService.ApplicationId)
//            });

//            captionMappings.Add(new CaptionMapping()
//            {
//                CaptionText = "Node",
//                FieldName = nameof(Record.InfrastructureService.InfrastructureNodeId)
//            });

//            return captionMappings;
//        }


//        public static string GetServiceCellValue(EditConfigurationPage dataModel, Record.InfrastructureService service, string fieldName)
//        {
//            if (fieldName == nameof(service.ServiceName))
//            {
//                string serviceName = service.ServiceName;

//                //if (!service.Enabled)
//                //    serviceName += " (disabled)";
//                return serviceName;
//            }

//            if (fieldName == nameof(service.ProjectId))
//            {
//                string projectName = "(not selected)";
//                string versionTag = "(not selected)";
//                var project = dataModel.AllProjects.Projects.SingleOrDefault(x => x.Id == service.ProjectId);
//                var version = dataModel.AllProjects.Versions.SingleOrDefault(x => x.Id == service.ProjectVersionId);

//                if (project == null)
//                {
//                    return projectName;
//                }
//                else
//                {
//                    projectName = project.Name;
//                }

//                if (version != null)
//                    versionTag = version.VersionTag;

//                return $"{projectName} {versionTag}";
//            }

//            if (fieldName == nameof(service.ApplicationId))
//            {
//                var app = dataModel.Configuration.Applications.SingleOrDefault(x => x.Id == service.ApplicationId);
//                return app != null ? app.Name : "(not selected)";
//            }

//            if (fieldName == nameof(service.InfrastructureNodeId))
//            {
//                var node = dataModel.Configuration.InfrastructureNodes.SingleOrDefault(x => x.Id == service.InfrastructureNodeId);
//                if (node == null)
//                    return "(not selected)";

//                string envDescription = "not selected";

//                var envType = dataModel.EnvironmentTypes.SingleOrDefault(x => x.Id == node.EnvironmentTypeId);
//                if (envType != null)
//                {
//                    envDescription = $"{envType.Name}";
//                }
//                return $"{node.NodeName} ({node.MachineIp}, {envDescription})";
//            }

//            return string.Empty;
//        }

//        public static async Task<EditServicePage> AddService(CommandContext commandContext, EditConfigurationPage dataModel, Guid referencedId)
//        {
//            EditServicePage editServicePage = Copy.To<EditServicePage>(dataModel);
//            editServicePage.ParameterTypes = await commandContext.Do(MdsInfrastructureApplication.GetAllParameterTypes);
//            editServicePage.NoteTypes = await commandContext.Do(MdsInfrastructureApplication.GetAllNoteTypes);
//            editServicePage.PendingService = new DataStructure.InfrastructureService();
//            editServicePage.PendingService.InfrastructureServices.Add(new Record.InfrastructureService()
//            {
//                ConfigurationHeaderId = dataModel.ConfigurationHeader.Id,
//            });

//            return editServicePage;
//        }

//        public static async Task<EditServicePage> EditService(CommandContext commandContext, EditConfigurationPage dataModel, System.Guid referencedId)
//        {
//            dataModel.RegisterSelectionId<Record.InfrastructureService>();
//            EditServicePage editServicePage = Copy.To<EditServicePage>(dataModel);
//            editServicePage.ParameterTypes = await commandContext.Do(MdsInfrastructureApplication.GetAllParameterTypes);
//            editServicePage.NoteTypes = await commandContext.Do(MdsInfrastructureApplication.GetAllNoteTypes);

//            var editedService = editServicePage.GetSelected(dataModel.Configuration.InfrastructureServices);

//            //var pendingService = new DataStructure.InfrastructureService();
//            //pendingService.InfrastructureServices.Add(editServicePage.GetSelected(dataModel.Configuration.InfrastructureServices).Clone());
//            //pendingService.InfrastructureServiceParameterDeclarations.AddRange(editServicePage.Configuration.InfrastructureServiceParameterDeclarations.Where(x => x.InfrastructureServiceId == pendingService.Service.Id));
//            //HashSet<Guid> paramDecIds = new HashSet<Guid>(pendingService.InfrastructureServiceParameterDeclarations.Select(x => x.Id));
//            //pendingService.InfrastructureServiceParameterValues.AddRange(editServicePage.Configuration.InfrastructureServiceParameterValues.Where(x => paramDecIds.Contains(x.InfrastructureServiceParameterDeclarationId)));
//            //pendingService.InfrastructureServiceParameterBindings.AddRange(editServicePage.Configuration.InfrastructureServiceParameterBindings.Where(x => paramDecIds.Contains(x.InfrastructureServiceParameterDeclarationId)));
//            //pendingService.InfrastructureServiceNotes.AddRange(editServicePage.Configuration.InfrastructureServiceNotes.Where(x => x.InfrastructureServiceId == pendingService.InfrastructureServices.Single().Id));

//            editServicePage.PendingService = dataModel.Configuration.Extract<DataStructure.InfrastructureService>(editedService.Id);
//            return editServicePage;
//        }

//        public static async Task<EditConfigurationPage> DeleteService(CommandContext commandContext, EditConfigurationPage dataModel, Guid referencedId)
//        {
//            dataModel.RegisterSelectionId<Record.InfrastructureService>();

//            dataModel.Configuration.Remove<DataStructure.InfrastructureService>(dataModel.GetSelectedId<Record.InfrastructureService>());

//            //var selectedService = dataModel.GetSelected(dataModel.Configuration.InfrastructureServices);

//            //dataModel.Configuration.InfrastructureServices.Remove(selectedService);
//            //dataModel.Configuration.InfrastructureServiceNotes.RemoveAll(x => x.InfrastructureServiceId == selectedService.Id);
//            //var declarations = new HashSet<Guid>(dataModel.Configuration.InfrastructureServiceParameterDeclarations.Where(x => x.InfrastructureServiceId == selectedService.Id).Select(x => x.Id));
//            //dataModel.Configuration.InfrastructureServiceParameterDeclarations.RemoveAll(x => declarations.Contains(x.Id));
//            //dataModel.Configuration.InfrastructureServiceParameterValues.RemoveAll(x => declarations.Contains(x.InfrastructureServiceParameterDeclarationId));
//            //dataModel.Configuration.InfrastructureServiceParameterBindings.RemoveAll(x => declarations.Contains(x.InfrastructureServiceParameterDeclarationId));

//            return dataModel;
//        }

//        public static View RenderEditServicePage(EditServicePage dataModel, UI.Svelte.ViewBuilder viewBuilder)
//        {
//            var editedService = dataModel.PendingService.Service;
//            var configuration = dataModel.ConfigurationHeader;
//            viewBuilder.AddOkBackHeader<EditServicePage, EditConfigurationPage, DataStructure.InfrastructureService>(
//                dataModel, $"Configuration {configuration.Name}, service {editedService.ServiceName}",
//                page => Metapsi.DataStructure.Different(page.PendingService, page.Configuration.Extract<DataStructure.InfrastructureService>(page.Service.Id)),
//                //dataModel.NoValidation(),
//                async (cc, page, id) =>
//                {
//                    page.Configuration.Merge(page.PendingService);
//                    return page;
//                },
//                async (cc, page, id) => Copy.To<EditConfigurationPage>(page));

//            var mainTab = viewBuilder.TabControl("tabEditConfiguration", editedService.Id, ViewBuilder.RootGroupId);
//            var serviceDataTab = viewBuilder.TabPage<EditServicePage>("tabServiceData", editedService.Id, "Service", mainTab.Id, SetSelectedServiceTab);
//            var serviceParametersTab = viewBuilder.TabPage<EditServicePage>("tabServiceParameters", editedService.Id, "Parameters", mainTab.Id, SetSelectedServiceTab);
//            var serviceNotesTab = viewBuilder.TabPage<EditServicePage>("tabServiceNotes", editedService.Id, "Notes", mainTab.Id, SetSelectedServiceTab);

//            string selectedTabName = dataModel.GetString(Keys.SelectedServiceTabName);
//            if (string.IsNullOrEmpty(selectedTabName))
//            {
//                selectedTabName = serviceDataTab.Name;
//            }
//            mainTab.SelectedTabPageName = selectedTabName;

//            //var firstRow = viewBuilder.Group("grpFirstLine", editedService.Id, serviceDataTab.ContainedGroupId, "Horizontal");
//            //var secondRow = viewBuilder.Group("grpFirstLine", editedService.Id, serviceDataTab.ContainedGroupId, "Horizontal");
//            //var secondRow = viewBuilder.Group("grpFirstLine", editedService.Id, serviceDataTab.ContainedGroupId, "Horizontal");

//            viewBuilder.TextBox<EditServicePage>(
//                "txtServiceName",
//                editedService.Id,
//                editedService.ServiceName,
//                "Service name",
//                EditServiceName,
//                400,
//                serviceDataTab.ContainedGroupId);

//            viewBuilder.AddDropDown<EditServicePage, MdsCommon.Project>(
//                "ddServiceProject",
//                editedService.Id,
//                editedService.ProjectId,
//                serviceDataTab.ContainedGroupId,
//                "Project",
//                dataModel,
//                EditServiceProject,
//                GetEnabledProjects,
//                (project) => project.Name);

//            viewBuilder.AddDropDown<EditServicePage, MdsCommon.ProjectVersion>(
//                "ddProjectVersion",
//                editedService.Id,
//                editedService.ProjectVersionId,
//                serviceDataTab.ContainedGroupId,
//                "Version",
//                dataModel,
//                EditProjectVersion,
//                (dataModel) => dataModel.AllProjects.Versions.Where(x => x.ProjectId == editedService.ProjectId && x.Enabled),
//                (x) => x.VersionTag);

//            //var secondRow = viewBuilder.Group("grpSecondRow", editedService.Id, serviceDataTab.ContainedGroupId, "Horizontal");

//            var dd = viewBuilder.AddDropDown<EditServicePage, Record.Application>(
//                "ddApplication",
//                editedService.Id,
//                editedService.ApplicationId,
//                serviceDataTab.ContainedGroupId,
//                "Application",
//                dataModel,
//                EditServiceApplicationId,
//                page => page.Configuration.Applications,
//                (app) => app.Name);
//            dd.LabelForEmpty = "Select application";


//            dd = viewBuilder.AddDropDown(
//                "ddNode",
//                editedService.Id,
//                editedService.InfrastructureNodeId,
//                serviceDataTab.ContainedGroupId,
//                "Deployed on node",
//                dataModel,
//                EditServiceNodeId,
//                GetConfigurationNodes,
//                (node) => GetNodeDescription(dataModel.Configuration, node, dataModel.EnvironmentTypes));
//            dd.LabelForEmpty = "Select node";

//            var checkBoxRow = viewBuilder.Group("grpcheckBoxRow", editedService.Id, serviceDataTab.ContainedGroupId, "Horizontal");

//            viewBuilder.Label("lblCheckboxStatus", editedService.Id, "Service status", -1, checkBoxRow.Id);

//            string enabledStatus = editedService.Enabled ? "Enabled" : "Disabled";

//            viewBuilder.CheckBox<EditServicePage>(
//                "chkEnabled",
//                editedService.Id,
//                checkBoxRow.Id,
//                enabledStatus,
//                async (cc, page, editedServiceId, enabled) =>
//                {
//                    page.PendingService.InfrastructureServices.ById(editedServiceId).Enabled = enabled;
//                    return page;
//                },
//                editedService.Enabled,
//                -1);

//            viewBuilder.ActionSelectionGrid(
//                "grdServiceParameters",
//                editedService.Id,
//                dataModel,
//                serviceParametersTab.ContainedGroupId,
//                (localModel, _) => localModel.PendingService.InfrastructureServiceParameterDeclarations,
//                () =>
//                {
//                    List<CaptionMapping> columns = new List<CaptionMapping>();
//                    columns.Add(new CaptionMapping() { CaptionText = "Parameter", FieldName = nameof(Record.InfrastructureServiceParameterDeclaration.ParameterName) });
//                    columns.Add(new CaptionMapping() { CaptionText = "Type", FieldName = nameof(Record.ParameterType.Description) });
//                    columns.Add(new CaptionMapping() { CaptionText = "Value", FieldName = "Value" });
//                    return columns;
//                },
//                (localModel, parameter, columnName) =>
//                {
//                    if (columnName == nameof(Record.InfrastructureServiceParameterDeclaration.ParameterName))
//                        return parameter.ParameterName;

//                    if (columnName == nameof(Record.ParameterType.Description))
//                    {
//                        var t = localModel.ParameterTypes.SingleOrDefault(x => x.Id == parameter.ParameterTypeId);

//                        if (t == null)
//                            return String.Empty;

//                        return t.Description;
//                    }

//                    if (columnName == "Value")
//                        return localModel.PendingService.GetParameterValue(parameter, localModel.Configuration.InfrastructureVariables);

//                    return string.Empty;
//                },
//                "Add parameter", AddParameter,
//                "Edit parameter", EditParameter,
//                "Delete parameter", DeleteParameter);

//            viewBuilder.ActionSelectionGrid(
//                "grdServiceNotes",
//                editedService.Id,
//                dataModel,
//                serviceNotesTab.ContainedGroupId,
//                (localModel, _) => localModel.PendingService.InfrastructureServiceNotes,
//                () =>
//                {
//                    List<CaptionMapping> columns = new List<CaptionMapping>();
//                    columns.Add(new CaptionMapping() { CaptionText = "Note type", FieldName = nameof(Record.NoteType.Code) });
//                    columns.Add(new CaptionMapping() { CaptionText = "Reference", FieldName = nameof(Record.InfrastructureServiceNote.Reference) });
//                    columns.Add(new CaptionMapping() { CaptionText = "Note", FieldName = nameof(Record.InfrastructureServiceNote.Note) });
//                    return columns;
//                },
//                (localModel, note, columnName) =>
//                {
//                    if (columnName == nameof(Record.NoteType.Code))
//                    {
//                        if (note.NoteTypeId == Guid.Empty)
//                            return "Not selected";

//                        return dataModel.NoteTypes.Single(x => x.Id == note.NoteTypeId).Description;
//                    }

//                    if (columnName == nameof(Record.InfrastructureServiceNote.Reference))
//                    {
//                        if (note.NoteTypeId != Guid.Empty)
//                        {
//                            var noteType = dataModel.NoteTypes.SingleOrDefault(x => x.Id == note.NoteTypeId);
//                            if (noteType != null && noteType.Code.ToLower() == "parameter")
//                            {
//                                var referencedParam = dataModel.PendingService.InfrastructureServiceParameterDeclarations.SingleOrDefault(x => x.Id.ToString() == note.Reference);

//                                if (referencedParam != null)
//                                {
//                                    return referencedParam.ParameterName;
//                                }
//                            }
//                        }

//                        return note.Reference;
//                    }

//                    if (columnName == nameof(Record.InfrastructureServiceNote.Note))
//                    {
//                        return note.Note;
//                    }

//                    return string.Empty;
//                },
//                "Add note", AddInfrastructureServiceNote,
//                "Edit note", EditInfrastructureServiceNote,
//                "Delete note", DeleteServiceNote);

//            return viewBuilder.OutputView;
//        }

//        private static EditServicePage SetSelectedServiceTab(EditServicePage model, Guid _, string tabName)
//        {
//            model.SetValue(Keys.SelectedServiceTabName, tabName);
//            return model;
//        }

//        public static EditServicePage EditServiceName(EditServicePage dataModel, System.Guid referencedId, string commandValue)
//        {
//            dataModel.PendingService.Service.ServiceName = commandValue;
//            return dataModel;
//        }

//        public static UI.Svelte.DropDown AddDropDown<TModel, TOption>(
//            this UI.Svelte.ViewBuilder viewBuilder,
//            string name,
//            System.Guid referencedId,
//            System.Guid selectedId,
//            System.Guid groupId,
//            string label,
//            TModel model,
//            Func<TModel, System.Guid, System.Guid, TModel> callback,
//            System.Func<TModel, IEnumerable<TOption>> getOptions,
//            System.Func<TOption, object> getDescription)
//            where TOption : IRecord
//        {

//            var dropDown = viewBuilder.DropDown<TModel>(name,
//                referencedId,
//                groupId,
//                callback,
//                selectedId,
//                label,
//                -1,
//                string.Empty);

//            foreach (TOption record in getOptions(model))
//            {
//                viewBuilder.DropDownItem(dropDown.Id, record.Id, getDescription(record).ToString());
//            }

//            return dropDown;
//        }

//        public static EditServicePage EditServiceProject(EditServicePage dataModel, System.Guid referencedId, Guid selectedId)
//        {
//            dataModel.PendingService.Service.ProjectId = selectedId;
//            dataModel.PendingService.Service.ProjectVersionId = Guid.Empty;
//            return dataModel;
//        }

//        public static IEnumerable<MdsCommon.Project> GetEnabledProjects(EditServicePage editConfigurationPage)
//        {
//            return editConfigurationPage.AllProjects.Projects.Where(x => x.Enabled);
//        }

//        public static EditServicePage EditProjectVersion(EditServicePage dataModel, System.Guid referencedId, Guid selectedId)
//        {
//            dataModel.Service.ProjectVersionId = selectedId;

//            if (dataModel.Service.InfrastructureNodeId != Guid.Empty)
//            {
//                var selectedNode = dataModel.Configuration.InfrastructureNodes.ById(dataModel.Service.InfrastructureNodeId);
//                var nodeOs = dataModel.EnvironmentTypes.ById(selectedNode.EnvironmentTypeId);

//                var compatibleNodes = NodeExtensions.GetSupportedNodesForProjectVersion(
//                    dataModel.Configuration, 
//                    dataModel.AllProjects,
//                    dataModel.EnvironmentTypes,
//                    dataModel.ConfigurationHeader.Id,
//                    dataModel.Service.ProjectVersionId);

//                if (!compatibleNodes.Any(x => x.Id == selectedNode.Id))
//                {
//                    dataModel.Service.InfrastructureNodeId = Guid.Empty;
//                }
//            }

//            return dataModel;
//        }

//        public static EditServicePage EditServiceApplicationId(EditServicePage dataModel, System.Guid referencedId, Guid selectedId)
//        {
//            dataModel.Service.ApplicationId = selectedId;
//            return dataModel;
//        }

//        public static EditServicePage EditServiceNodeId(EditServicePage dataModel, System.Guid referencedId, Guid selectedId)
//        {
//            dataModel.Service.InfrastructureNodeId = selectedId;
//            return dataModel;
//        }

//        public static IEnumerable<Record.InfrastructureNode> GetConfigurationNodes(EditServicePage editServicePage)
//        {
//            return NodeExtensions.GetSupportedNodesForProjectVersion(
//                editServicePage.Configuration, 
//                editServicePage.AllProjects, 
//                editServicePage.EnvironmentTypes,
//                editServicePage.ConfigurationHeader.Id, 
//                editServicePage.Service.ProjectVersionId);
//        }

//        public static string GetNodeDescription(
//            DataStructure.InfrastructureConfiguration infrastructureConfiguration,
//            Record.InfrastructureNode infrastructureNode,
//            RecordCollection<Record.EnvironmentType> environmentTypes)
//        {
//            var node = infrastructureConfiguration.InfrastructureNodes.ById(infrastructureNode.Id);
//            var env = environmentTypes.ById(node.EnvironmentTypeId);
//            return $"{node.NodeName} ({env.OsType})";
//        }

//        public static async Task<EditServiceParameterPage> AddParameter(CommandContext commandContext, EditServicePage dataModel, Guid _)
//        {
//            return Copy.To<EditServiceParameterPage>(dataModel);
//        }

//        public static async Task<EditServiceParameterPage> EditParameter(CommandContext commandContext, EditServicePage dataModel, Guid _)
//        {
//            var parameterId = RegisterSelectionId<Record.InfrastructureServiceParameterDeclaration>(dataModel);
//            EditServiceParameterPage editServiceParameterPage = Copy.To<EditServiceParameterPage>(dataModel);
//            editServiceParameterPage.PendingParameter = dataModel.PendingService.GetPendingParameter(parameterId);
//            return editServiceParameterPage;
//        }

//        public static PendingParameter GetPendingParameter(this DataStructure.InfrastructureService infraService, string parameterName)
//        {
//            var p = infraService.InfrastructureServiceParameterDeclarations.SingleOrDefault(x => x.ParameterName == parameterName);
//            if (p != null)
//            {
//                return GetPendingParameter(infraService, p.Id);
//            }
//            return new PendingParameter();
//        }

//        public static PendingParameter GetPendingParameter(this DataStructure.IParameters parametersStore, Guid id)
//        {
//            var pendingParameter = new PendingParameter();

//            var declaration = parametersStore.InfrastructureServiceParameterDeclarations.ByIdOrDefault(id);

//            if (Metapsi.Record.IsValid(declaration))
//            {
//                pendingParameter.ParameterName = declaration.ParameterName;
//                pendingParameter.ParameterTypeId = declaration.ParameterTypeId;

//                var binding = parametersStore.InfrastructureServiceParameterBindings.SingleOrDefault(x => x.InfrastructureServiceParameterDeclarationId == id);
//                if (Metapsi.Record.IsValid(binding))
//                {
//                    pendingParameter.BoundVariableId = binding.InfrastructureVariableId;
//                    pendingParameter.IsBound = true;
//                }
//                else
//                {
//                    pendingParameter.ParameterValue = parametersStore.InfrastructureServiceParameterValues.Single(x => x.InfrastructureServiceParameterDeclarationId == id).ParameterValue;
//                    pendingParameter.IsBound = false;
//                }
//            }

//            return pendingParameter;
//        }

//        public static async Task<EditServicePage> DeleteParameter(CommandContext commandContext, EditServicePage dataModel, Guid _)
//        {
//            RegisterSelectionId<Record.InfrastructureServiceParameterDeclaration>(dataModel);
//            Guid selectedParameterId = dataModel.GetSelectedId<Record.InfrastructureServiceParameterDeclaration>();
//            dataModel.PendingService.Remove<DataStructure.ServiceParameter>(selectedParameterId);
//            return dataModel;
//        }
//    }
//}
