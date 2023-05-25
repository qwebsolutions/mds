//using Metapsi;
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using UI.Svelte;

//namespace MdsInfrastructure
//{
//    public class EditServiceNotePage : EditServicePage
//    {
//        public Record.InfrastructureServiceNote PendingNote { get; set; } = new();
//    }

//    public static partial class MdsInfrastructureFunctions
//    {
//        public static async Task<EditServiceNotePage> AddInfrastructureServiceNote(CommandContext commandContext, EditServicePage dataModel, Guid _)
//        {
//            EditServiceNotePage editServiceNotePage = Copy.To<EditServiceNotePage>(dataModel);
//            editServiceNotePage.PendingNote = new Record.InfrastructureServiceNote()
//            {
//                InfrastructureServiceId = dataModel.Service.Id
//            };

//            return editServiceNotePage;
//        }

//        public static async Task<EditServiceNotePage> EditInfrastructureServiceNote(CommandContext commandContext, EditServicePage dataModel, Guid _)
//        {
//            dataModel.RegisterSelectionId<Record.InfrastructureServiceNote>();
//            EditServiceNotePage editServiceNotePage = Copy.To<EditServiceNotePage>(dataModel);
//            editServiceNotePage.PendingNote = editServiceNotePage.GetSelected(dataModel.PendingService.InfrastructureServiceNotes).Clone();

//            return editServiceNotePage;
//        }

//        public static async Task<EditServicePage> DeleteServiceNote(CommandContext commandContext, EditServicePage dataModel, Guid _)
//        {
//            dataModel.RegisterSelectionId<Record.InfrastructureServiceNote>();
//            dataModel.PendingService.InfrastructureServiceNotes.Remove(dataModel.GetSelectedId<Record.InfrastructureServiceNote>());
//            return dataModel;
//        }

//        public static UI.Svelte.View RenderEditServiceNotePage(EditServiceNotePage dataModel, UI.Svelte.ViewBuilder viewBuilder)
//        {
//            var serviceNote = dataModel.PendingNote;
//            var service = dataModel.Service;
//            var configuration = dataModel.ConfigurationHeader;
//            var currentNoteType = dataModel.NoteTypes.SingleOrDefault(x => x.Id == serviceNote.NoteTypeId);

//            viewBuilder.AddOkBackHeader<EditServiceNotePage, EditServicePage, Record.InfrastructureServiceNote>(
//                dataModel,
//                $"Configuration {configuration.Name}, service {service.ServiceName}",
//                (page) => Metapsi.Record.Different(
//                    page.PendingNote,
//                    page.PendingService.InfrastructureServiceNotes.ByIdOrDefault(page.PendingNote.Id)),
//                //dataModel.NoValidation(),
//                async (cc, page, id) =>
//                {
//                    page.PendingService.InfrastructureServiceNotes.Set(page.PendingNote);
//                    return page;
//                },
//                async (cc, page, id) => Copy.To<EditServicePage>(page));

//            var ddNoteType = viewBuilder.DropDown<EditServiceNotePage>(
//                "ddNoteType",
//                serviceNote.Id,
//                ViewBuilder.RootGroupId,
//                (page, refId, selectedId) =>
//                {
//                    page.PendingNote.NoteTypeId = selectedId;
//                    return page;
//                },
//                serviceNote.NoteTypeId,
//                "Note type",
//                -1,
//                "Select note type");

//            foreach (var noteType in dataModel.NoteTypes)
//            {
//                viewBuilder.DropDownItem(ddNoteType.Id, noteType.Id, noteType.Description);
//            }

//            if (currentNoteType != null && currentNoteType.Code.ToLower() == "parameter")
//            {
//                var selectedNoteParameter = dataModel.PendingService.InfrastructureServiceParameterDeclarations.SingleOrDefault(x => x.Id.ToString() == serviceNote.Reference);

//                string parameterName = "Not selected";
//                Guid selectedParameterId = Guid.Empty;

//                if (selectedNoteParameter != null)
//                {
//                    parameterName = selectedNoteParameter.ParameterName;
//                    selectedParameterId = selectedNoteParameter.Id;
//                }

//                var ddParameter = viewBuilder.DropDown<EditServiceNotePage>(
//                    "ddNoteParameter",
//                    serviceNote.Id,
//                    ViewBuilder.RootGroupId,
//                    (page, refId, selectedId) =>
//                    {
//                        page.PendingNote.Reference = selectedId.ToString();
//                        return page;
//                    },
//                    selectedParameterId,
//                    "Referenced parameter",
//                    -1,
//                    "Referenced parameter");

//                foreach (var parameter in dataModel.PendingService.InfrastructureServiceParameterDeclarations.Where(x => x.InfrastructureServiceId == service.Id))
//                {
//                    viewBuilder.DropDownItem(ddParameter.Id, parameter.Id, parameter.ParameterName);
//                }
//            }
//            else
//            {

//                viewBuilder.TextBox<EditServiceNotePage>(
//                    "txtReference",
//                    serviceNote.Id,
//                    serviceNote.Reference,
//                    "Reference",
//                    (page, refId, newString) =>
//                    {
//                        page.PendingNote.Reference = newString;
//                        return page;
//                    },
//                    -1,
//                    ViewBuilder.RootGroupId);
//            }

//            viewBuilder.TextBox<EditServiceNotePage>(
//                "txtNote",
//                serviceNote.Id,
//                serviceNote.Note,
//                "Note",
//                (page, refId, newString) =>
//                {
//                    page.PendingNote.Note = newString;
//                    return page;
//                },
//                -1,
//                ViewBuilder.RootGroupId);

//            return viewBuilder.OutputView;

//        }

//    }
//}
