using MdsCommon.Controls;
using Metapsi.ChoicesJs;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<Func<TIn, TOut>> Def<TIn, TOut>(this SyntaxBuilder b, System.Linq.Expressions.Expression<Func<TIn, TOut>> getProperty)
        {
            return b.Def<SyntaxBuilder, TIn, TOut>((SyntaxBuilder b, Var<TIn> input) => b.Get(input, getProperty));
        }

        public static Var<IVNode> EditNote(
            LayoutBuilder b,
            Var<EditConfigurationPage> clientModel)
        {
            var noteId = b.Get(clientModel, x => x.EditServiceNoteId);
            var note = b.Get(clientModel, noteId, (x, noteid) => x.Configuration.InfrastructureServices.SelectMany(x => x.InfrastructureServiceNotes).Single(x => x.Id == noteid));
            var serviceId = b.Get(note, x => x.InfrastructureServiceId);
            var service = b.Get(clientModel, serviceId, (x, serviceId) => x.Configuration.InfrastructureServices.Single(x => x.Id == serviceId));
            var noteTypes = b.Get(clientModel, x => x.NoteTypes);
            var noteTypeId = b.Get(note,x => x.NoteTypeId);
            var currentNoteType = b.Get(clientModel, noteTypeId, (x, noteTypeId) => x.NoteTypes.SingleOrDefault(x => x.Id == noteTypeId, new NoteType() { Code = "" }));

            //var view = b.Div();
            //var noteOptions = b.Get(noteTypes, x => x.Select(x => new DropDown.Option() { value = x.Id.ToString(), label = x.Description }).ToList());
            var noteOptions = b.MapChoices(noteTypes, x => x.Id, x => x.Description, b.Get(note, x => x.NoteTypeId));

            var toolbar = b.Toolbar(b => { }, b.OkButton(EditService, x => x.EditServiceNoteId));

            //var form = b.Add(view, b.Form(toolbar));
            //b.AddClass(form, "bg-white rounded");
            //b.Add(form, b.Text("Note type"));
            //b.Add(form, b.DropDown(
            //    b.Const("noteType"),
            //    b.Get(note, x => x.NoteTypeId).As<string>(),
            //    noteOptions,
            //    b.Def<string>(
            //    (b, noteType) =>
            //    {
            //        b.Modify(note, b =>
            //        {
            //            b.Set(x => x.NoteTypeId, noteType.As<System.Guid>());
            //            b.Set(x => x.Reference, b.Const(string.Empty));
            //        });
            //    }),
            //    b.Const("Note type")));

            

            var formFields = b.NewCollection<IVNode>();

            var noteTypeDd = b.Choices(b =>
            {
                b.SetChoices(noteTypes, x => x.Id, x => x.Description, noteTypeId);
                b.OnChange(b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> clientModel, Var<string> value) =>
                {
                    var editedNote = b.GetEditedNote(clientModel);
                    b.Set(editedNote, x => x.NoteTypeId, b.ParseId(value));
                    // This is a rather ugly workaround
                    b.Set(editedNote, x => x.Reference, b.Const(string.Empty));
                    return b.Clone(clientModel);
                }));
            });

            b.AddFormField(formFields, "Note type", noteTypeDd);

            //var noteTypeDd = 
            //    b.BoundDropDown<EditConfigurationPage, InfrastructureServiceNote, NoteType, Guid>(
            //        clientModel,
            //        b.Def<SyntaxBuilder, EditConfigurationPage, InfrastructureServiceNote>(GetEditedNote),
            //        x => x.NoteTypeId,
            //        b.Def<EditConfigurationPage, List<NoteType>>(x => x.NoteTypes),
            //        b.Def<NoteType, Guid>(x => x.Id),
            //        b.Def<NoteType, string>(x => x.Description)));

            //Metapsi.ChoicesJs.Event.SetOnChoice(b, noteTypeDd, b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<Choice> payload) =>
            //{
            //    // This is a rather ugly workaround
            //    b.Set(b.GetEditedNote(page), x => x.Reference, b.Const(string.Empty));
            //    return b.Clone(page);
            //}));

            var noteTypeCode = b.ToLowercase(b.Get(currentNoteType, x => x.Code));
            var isParameterNote = b.AreEqual(noteTypeCode, b.Const("parameter"));

            b.If(isParameterNote, b =>
            {
                var serviceParams = b.Get(clientModel, serviceId, (x, serviceId) => x.Configuration.InfrastructureServices.SelectMany(x => x.InfrastructureServiceParameterDeclarations).Where(x => x.InfrastructureServiceId == serviceId).ToList());

                b.AddFormField(
                    formFields,
                    "Referenced parameter",
                    b.Choices(b =>
                    {
                        b.SetChoices(serviceParams, x => x.Id, x => x.ParameterName, b.ParseId(b.Get(note, x => x.Reference)));
                        b.OnChange(b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> clientModel, Var<string> value) =>
                        {
                            b.Set(b.GetEditedNote(clientModel), x => x.Reference, value);
                            return b.Clone(clientModel);
                        }));
                    }));               
            },
            b =>
            {
                b.AddFormField(
                    formFields, 
                    "Reference",
                    b.BoundInput(clientModel, noteId, (x, noteid) => x.Configuration.InfrastructureServices.SelectMany(x => x.InfrastructureServiceNotes).Single(x => x.Id == noteid), x => x.Reference, b.Const("Reference")));
            });

            b.AddFormField(
                formFields, 
                "Note",
                b.BoundInput(clientModel, noteId, (x, noteid) => x.Configuration.InfrastructureServices.SelectMany(x => x.InfrastructureServiceNotes).Single(x => x.Id == noteid), x => x.Note, b.Const("Note")));

            return b.HtmlDiv(
                b =>
                {
                },
                b.Form(
                    b =>
                    {
                        b.SetClass("bg-white rounded");
                    },
                    toolbar,
                    formFields));
        }

        public static Var<InfrastructureServiceNote> GetEditedNote(this SyntaxBuilder b, Var<EditConfigurationPage> clientModel)
        {
            var noteId = b.Get(clientModel, x => x.EditServiceNoteId);
            var note = b.Get(clientModel, noteId, (x, noteid) => x.Configuration.InfrastructureServices.SelectMany(x => x.InfrastructureServiceNotes).Single(x => x.Id == noteid));
            return note;
        }
    }

    public static partial class Controls
    {
        public static Var<IVNode> OkButton(
            this LayoutBuilder b,
            Func<LayoutBuilder, Var<EditConfigurationPage>, Var<IVNode>> areaRenderer,
            System.Linq.Expressions.Expression<Func<EditConfigurationPage, Guid>> editedId)
        {
            var popPage = b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> state) =>
            {
                b.Set(state, editedId, b.EmptyId());
                return b.EditView<EditConfigurationPage>(state, areaRenderer);
            });

            var button = b.HtmlButton(
                b =>
                {
                    b.AddClass("rounded py-2 px-4 shadow bg-white text-sky-600 text-sm border font-semibold");
                    b.OnClickAction(popPage);
                },
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row space-x-2 items-center");
                    },
                    b.T("OK")));

            return button;
        }
    }
}
