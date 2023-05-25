using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MdsInfrastructure
{
    public static partial class Nodes
    {
        public static async Task<IResponse> Save(CommandContext commandContext, HttpContext requestData)
        {
            var node = Metapsi.Serialize.FromJson<InfrastructureNode>(await requestData.Payload());
            //var uiState = Metapsi.Serialize.FromJson<UiState.StateContainer>(await requestData.UiState());
            var validationMessage = ValidateNode(node);

            if (!string.IsNullOrEmpty(validationMessage))
            {
                EditPage editPage = new EditPage()
                {
                    EnvironmentTypes = await commandContext.Do(Api.LoadEnvironmentTypes),
                    InfrastructureNode = node,
                    ValidationMessage = validationMessage
                };

                return Page.Response(
                    editPage,
                    (b, clientModel) => b.Layout(b.InfraMenu(nameof(Nodes), requestData.User().IsSignedIn()),
                    b.Render(b.Const(new Header.Props()
                    {
                        Main = new Header.Title() { Operation = "Node" },
                        User = requestData.User()
                    })), b.RenderEditNodePage(clientModel)));
            }
            else
            {
                await commandContext.Do(Api.SaveNode, node);
                return Page.Redirect(List);
            }
        }


        public static string ValidateNode(
            InfrastructureNode node)
        {
            if (string.IsNullOrWhiteSpace(node.NodeName))
            {
                return "Node name not set!";
            }

            if (string.IsNullOrWhiteSpace(node.MachineIp))
            {
                return "Node address not set!";
            }

            if (node.EnvironmentTypeId == Guid.Empty)
            {
                return "Node type not set!";
            }

            return null;
        }
    }
}
