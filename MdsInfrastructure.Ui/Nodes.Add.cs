using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MdsInfrastructure
{
    public static partial class Nodes
    {
        public static async Task<IResponse> Add(CommandContext commandContext, HttpContext requestData)
        {
            EditPage editPage = new EditPage()
            {
                EnvironmentTypes = await commandContext.Do(Api.LoadEnvironmentTypes),
            };
            return Page.Response(editPage, (b, clientModel) => b.Layout(b.InfraMenu(nameof(Nodes), requestData.User().IsSignedIn()), b.Render(b.Const(new Header.Props()
            {
                Main = new Header.Title() { Operation = "Add node" },
                User = requestData.User()
            })), b.RenderEditNodePage(clientModel)));
        }
    }
}
