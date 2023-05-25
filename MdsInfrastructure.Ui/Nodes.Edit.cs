using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure
{
    public static partial class Nodes
    {
        public class EditPage: IHasValidationPanel
        {
            public InfrastructureNode InfrastructureNode { get; set; } = new InfrastructureNode();
            public List<EnvironmentType> EnvironmentTypes { get; set; } = new List<EnvironmentType>();
            public string ValidationMessage { get; set; } = string.Empty;
        }

        public static async Task<IResponse> Edit(CommandContext commandContext, HttpContext requestData)
        {
            var nodeId = Guid.Parse(requestData.EntityId());
            var allNodes = await commandContext.Do(Api.LoadAllNodes);
            EditPage editPage = new EditPage()
            {
                EnvironmentTypes = await commandContext.Do(Api.LoadEnvironmentTypes),
                InfrastructureNode = allNodes.Single(x => x.Id == nodeId)
            };
            return Page.Response(editPage, (b, clientModel) => {
                var nodeName = b.Get(clientModel, x => x.InfrastructureNode.NodeName);

                var header = b.NewObj(new Header.Props()
                {
                    Main = new Header.Title() { Operation = "Edit node" },
                    User = requestData.User()
                });
                b.Set(b.Get(header, x => x.Main), x => x.Entity, nodeName);

                var layout = b.Layout(
                    b.InfraMenu(nameof(Nodes), requestData.User().IsSignedIn()),
                    b.Render(header), 
                    b.RenderEditNodePage(clientModel));

                return layout;
            });
        }

        public static Var<HyperNode> RenderEditNodePage(
            this BlockBuilder b,
            Var<EditPage> clientModel)
        {
            var node = b.Get(clientModel, x => x.InfrastructureNode);
            var container = b.Div("flex flex-col w-full");
            b.Add(container, b.ValidationPanel(clientModel));
            b.Add(container, b.RenderEditNodeForm(clientModel));
            return container;
        }

        public static Var<HyperNode> RenderEditNodeForm(
            this BlockBuilder b,
            Var<EditPage> clientModel)
        {
            var node = b.Get(clientModel, x => x.InfrastructureNode);
            var envId = b.Get(node, x => x.EnvironmentTypeId);
            var envTypes = b.Get(clientModel, m => m.EnvironmentTypes.ToList());
            var saveUrl = b.Url(Save);
            var toolbar = b.Toolbar(
                b => b.SubmitButton<InfrastructureNode>(b =>
                {
                    b.Set(x => x.Label, "Save");
                    b.Set(x => x.Href, saveUrl);
                    b.Set(x => x.Payload, node);
                    b.Set(x => x.ButtonClass, "rounded text-white py-2 px-4 shadow");
                }));

            var form = b.Form(toolbar);
            b.AddClass(form, "p-4");
            b.AddClass(form, "bg-white rounded drop-shadow");
            b.FormField(form, "Node name", b.BoundInput(clientModel, x => x.InfrastructureNode, x => x.NodeName));
            b.FormField(form, "Machine address", b.BoundInput(clientModel, x => x.InfrastructureNode, x => x.MachineIp));
            b.FormField(form, "Node UI port", b.BoundInput(clientModel, x => x.InfrastructureNode, x => x.UiPort, b.Const(string.Empty)));

            b.FormField(form, "OS type",
                b.BoundDropDown(
                    b.Const("ddOsType"),
                    node,
                    x => x.EnvironmentTypeId,
                    envTypes,
                    x => x.Id,
                    x => x.Name));

            return form;
        }
    }
}
