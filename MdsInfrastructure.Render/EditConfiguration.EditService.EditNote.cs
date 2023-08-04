using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<HyperNode> EditNote(
            BlockBuilder b,
            Var<EditConfigurationPage> clientModel)
        {
            var noteId = b.Get(clientModel, x => x.EditServiceNoteId);
            var note = b.Get(clientModel, noteId, (x, noteid) => x.Configuration.InfrastructureServices.SelectMany(x => x.InfrastructureServiceNotes).Single(x => x.Id == noteid));
            var serviceId = b.Get(note, x => x.InfrastructureServiceId);
            var service = b.Get(clientModel, serviceId, (x, serviceId) => x.Configuration.InfrastructureServices.Single(x => x.Id == serviceId));
            var noteTypes = b.Get(clientModel, x => x.NoteTypes);
            var noteTypeId = b.Get(note,x => x.NoteTypeId);
            var currentNoteType = b.Get(clientModel, noteTypeId, (x, noteTypeId) => x.NoteTypes.SingleOrDefault(x => x.Id == noteTypeId, new NoteType() { Code = "" }));

            var view = b.Div();
            var noteOptions = b.Get(noteTypes, x => x.Select(x => new DropDown.Option() { value = x.Id.ToString(), label = x.Description }).ToList());

            var toolbar = b.Toolbar(b.OkButton(EditService, x => x.EditServiceNoteId));

            var form = b.Add(view, b.Form(toolbar));
            b.AddClass(form, "bg-white rounded");
            b.Add(form, b.Text("Note type"));
            b.Add(form, b.DropDown(
                b.Const("noteType"),
                b.Get(note, x => x.NoteTypeId).As<string>(),
                noteOptions,
                b.Def<string>(
                (b, noteType) =>
                {
                    b.Modify(note, b =>
                    {
                        b.Set(x => x.NoteTypeId, noteType.As<System.Guid>());
                        b.Set(x => x.Reference, b.Const(string.Empty));
                    });
                }),
                b.Const("Note type")));

            var noteTypeCode = b.ToLowercase(b.Get(currentNoteType, x => x.Code));
            var isParameterNote = b.AreEqual(noteTypeCode, b.Const("parameter"));
            b.Log("noteTypeCode", noteTypeCode);

            b.If(isParameterNote, b =>
            {
                var serviceParams = b.Get(clientModel, serviceId, (x, serviceId) => x.Configuration.InfrastructureServices.SelectMany(x => x.InfrastructureServiceParameterDeclarations).Where(x => x.InfrastructureServiceId == serviceId).ToList());
                b.Add(form, b.Text("Referenced parameter"));
                b.Add(form, b.BoundDropDown(
                    b.Const("noteParamRef"),
                    note,
                    x => x.Reference, serviceParams,
                    b.Def(
                        (BlockBuilder b, Var<InfrastructureServiceParameterDeclaration> param) =>

                        {
                            var paramName = b.Get(param, x => x.ParameterName);
                            var paramId = b.Get(param, x => x.Id).As<string>();
                            return b.NewObj<DropDown.Option>(b =>
                        {
                            b.Set(x => x.label, paramName);
                            b.Set(x => x.value, paramId);

                        });
                        })));
            },
            b =>
            {
                b.Add(form, b.Text("Reference"));
                b.Add(form, b.BoundInput(clientModel, noteId, (x, noteid) => x.Configuration.InfrastructureServices.SelectMany(x => x.InfrastructureServiceNotes).Single(x => x.Id == noteid), x => x.Reference, b.Const("Reference")));
            });

            b.Add(form, b.Text("Note"));
            b.Add(form, b.BoundInput(clientModel, noteId, (x, noteid) => x.Configuration.InfrastructureServices.SelectMany(x => x.InfrastructureServiceNotes).Single(x => x.Id == noteid), x => x.Note, b.Const("Note")));

            return view;
        }
    }

    public static partial class Controls
    {
        public static Var<HyperNode> OkButton(
            this BlockBuilder b,
            Func<BlockBuilder, Var<EditConfigurationPage>, Var<HyperNode>> areaRenderer,
            System.Linq.Expressions.Expression<Func<EditConfigurationPage, Guid>> editedId)
        {
            var popPage = b.MakeAction((BlockBuilder b, Var<EditConfigurationPage> state) =>
            {
                b.Set(state, editedId, b.EmptyId());
                return b.EditView<EditConfigurationPage>(state, areaRenderer);
            });

            var okButton = b.CommandButton<EditConfigurationPage>(b =>
            {
                b.Set(x => x.Label, "OK");
                b.Set(x => x.OnClick, popPage);
                b.Set(x => x.Style, Button.Style.Light);
            });
            b.AddClass(okButton, "text-sky-600 text-sm border font-semibold");
            return okButton;
        }
    }
}
