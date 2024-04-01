using MdsCommon;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.Html;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<IVNode> TabNotes(
            LayoutBuilder b,
            Var<EditConfigurationPage> clientModel)
        {
            var serviceId = b.Get(clientModel, x => x.EditServiceId);
            var service = b.Get(clientModel, serviceId, (x, serviceId) => x.Configuration.InfrastructureServices.Single(x => x.Id == serviceId));

            var removeIcon = Icon.Remove;
            var addCommand = b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> state) =>
            {
                var newId = b.NewId();
                var newNote = b.NewObj<InfrastructureServiceNote>(b =>
                {
                    b.Set(x => x.Id, newId);
                    b.Set(x => x.InfrastructureServiceId, serviceId);
                    b.Set(x => x.Reference, string.Empty);
                    b.Set(x => x.NoteTypeId, System.Guid.Empty);
                });

                b.Push(b.Get(service, x => x.InfrastructureServiceNotes), newNote);
                b.Set(clientModel, x => x.EditServiceNoteId, newId);
                return b.EditView<EditConfigurationPage>(clientModel, EditNote);
            });

            var notes = b.Get(service, x => x.InfrastructureServiceNotes.ToList());
            var noteTypes = b.Get(clientModel, x => x.NoteTypes);

            var gridBuilder = MdsDefaultBuilder.DataGrid<InfrastructureServiceNote>();

            gridBuilder.CreateToolbarActions = b =>
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
                            b.OnClickAction(addCommand);
                        },
                        b.T("Add note")));
            };

            gridBuilder.DataTableBuilder.OverrideHeaderCell(nameof(InfrastructureServiceNote.NoteTypeId), b => b.T("Note type"));
            gridBuilder.DataTableBuilder.OverrideDataCell(
                nameof(InfrastructureServiceNote.NoteTypeId),
                (b, note) =>
                {
                    var noteTypeId = b.Get(note, x => x.NoteTypeId);
                    var noteType = b.Get(noteTypes, noteTypeId, (x, noteTypeId) => x.SingleOrDefault(x => x.Id == noteTypeId, new NoteType() { Description = "", Code = "" }));
                    var noteTypeLabel = b.Get(noteType, x => x.Description, "(not set)");
                    return b.Link(
                        noteTypeLabel,
                        b.MakeAction(
                            (SyntaxBuilder b, Var<EditConfigurationPage> clientModel) =>
                            {
                                b.Set(clientModel, x => x.EditServiceNoteId, b.Get(note, x => x.Id));
                                return b.EditView<EditConfigurationPage>(clientModel, EditNote);
                            }));
                });

            gridBuilder.DataTableBuilder.OverrideDataCell(
                nameof(InfrastructureServiceNote.Reference),
                (b, note) =>
                {
                    var noteTypeId = b.Get(note, x => x.NoteTypeId);
                    var noteType = b.Get(noteTypes, noteTypeId, (x, noteTypeId) => x.SingleOrDefault(x => x.Id == noteTypeId, new NoteType() { Description = "", Code = "" }));
                    var noteTypeCode = b.ToLowercase(b.Get(noteType, x => x.Code));
                    var refAsId = b.Get(note, x => x.Reference).As<System.Guid>();
                    var reference = b.If(
                        b.AreEqual(noteTypeCode, b.Const("parameter")),
                        b => b.Get(service, refAsId, (x, refAsId) => x.InfrastructureServiceParameterDeclarations.SingleOrDefault(
                            x => x.Id == refAsId,
                            new InfrastructureServiceParameterDeclaration()
                            {
                                ParameterName = "(not set)"
                            }).ParameterName),
                        b => b.Get(note, x => x.Reference));
                    return b.T(reference);
                });

            gridBuilder.AddRowAction((b, note) =>
            {
                return b.DeleteRowIconAction(b =>
                {
                    var removeNote = b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> clientModel, Var<Guid> noteId) =>
                    {
                        var selectedService = b.GetSelectedService(clientModel);
                        var note = b.Get(selectedService, noteId, (service, noteId) => service.InfrastructureServiceNotes.Single(x => x.Id == noteId));
                        b.Remove(b.Get(service, x => x.InfrastructureServiceNotes), note);
                        return b.Clone(clientModel);
                    });

                    b.OnClickAction(b.MakeActionDescriptor(removeNote, b.Get(note, x => x.Id)));
                });
            });

            return b.DataGrid(
                gridBuilder,
                notes,
                nameof(InfrastructureServiceNote.NoteTypeId),
                nameof(InfrastructureServiceNote.Reference),
                nameof(InfrastructureServiceNote.Note));


            //var rc = b.RenderCell<InfrastructureServiceNote>(
            //            (b, row, col) =>
            //            {
            //                var noteTypeId = b.Get(row, x => x.NoteTypeId);
            //                var noteType = b.Get(noteTypes, noteTypeId, (x, noteTypeId) => x.SingleOrDefault(x => x.Id == noteTypeId, new NoteType() { Description = "", Code = "" }));
            //                var noteTypeLabel = b.Get(noteType, x => x.Description, "(not set)");
            //                var noteTypeCode = b.ToLowercase(b.Get(noteType, x => x.Code));
            //                var refAsId = b.Get(row, x => x.Reference).As<System.Guid>();

            //                var reference = b.If(
            //                    b.AreEqual(noteTypeCode, b.Const("parameter")),
            //                    b => b.Get(service, refAsId, (x, refAsId) => x.InfrastructureServiceParameterDeclarations.SingleOrDefault(
            //                        x => x.Id == refAsId,
            //                        new InfrastructureServiceParameterDeclaration()
            //                        {
            //                            ParameterName = "(not set)"
            //                        }).ParameterName),
            //                    b => b.Get(row, x => x.Reference));

            //                return b.VPadded4(b.Switch(b.Get(col, x => x.Name),
            //                    b => b.Link(
            //                        noteTypeLabel,
            //                        b.MakeAction(
            //                            (SyntaxBuilder b, Var<EditConfigurationPage> clientModel) =>
            //                            {
            //                                b.Set(clientModel, x => x.EditServiceNoteId, b.Get(row, x => x.Id));
            //                                return b.EditView<EditConfigurationPage>(clientModel, EditNote);
            //                            })),
            //                    ("Reference", b => b.Text(reference)),
            //                    ("Note", b => b.Text(b.Get(row, x => x.Note)))));
            //            });

            //return b.DataGrid<InfrastructureServiceNote>(
            //    new()
            //    {
            //        b=> b.AddClass(b.CommandButton<EditConfigurationPage>(b=>
            //        {
            //            b.Set(x=>x.Label, "Add note");
            //            b.Set(x => x.OnClick, addCommand);
            //        }),"text-white")
            //    },
            //    b =>
            //    {
            //        b.SetRows(notes);
            //        b.AddColumn("NoteType", "Note type", b => b.Set(x => x.Class, b.Const("w-1/6")));
            //        b.AddColumn("Reference", b => b.Set(x => x.Class, b.Const("w-1/6")));
            //        b.AddColumn("Note");
            //        b.SetRenderCell(rc);
            //    },
            //    (b, actions, item) =>
            //    {
            //        var onCommand = b.Def((SyntaxBuilder b, Var<InfrastructureServiceNote> note) =>
            //        {
            //            var noteId = b.Get(note, x => x.Id);
            //            var noteRemoved = b.Get(service, noteId, (x, noteId) => x.InfrastructureServiceNotes.Where(x => x.Id != noteId).ToList());
            //            b.Set(service, x => x.InfrastructureServiceNotes, noteRemoved);
            //        });

            //        b.Modify(actions, x => x.Commands, b =>
            //        {
            //            b.Add(b =>
            //            {
            //                b.Set(x => x.IconHtml, removeIcon);
            //                b.Set(x => x.OnCommand, onCommand);
            //            });
            //        });
            //    });
        }
    }
}
