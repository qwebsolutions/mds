using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Threading.Tasks;
using System.Linq;
using MdsCommon;
using Microsoft.AspNetCore.Http;

namespace MdsInfrastructure
{

    public static partial class Deployments
    {

        public static async Task<IResponse> Review(CommandContext commandContext, HttpContext requestData)
        {
            var deployment = await commandContext.Do(
                Api.LoadDeploymentById,
                System.Guid.Parse(requestData.EntityId()));

            var fromSnapshotIds = deployment.Transitions.Select(x => x.FromServiceConfigurationSnapshotId);
            var toSnapshotIds = deployment.Transitions.Select(x => x.ToServiceConfigurationSnapshotId);

            var changes = ChangesReport.Get(deployment.Transitions.Select(x => x.FromSnapshot).Where(x => x != null).ToList(), deployment.GetDeployedServices().ToList());

            return Page.Response(new object(), (b, clientModel) =>
            {
                var selectedDeployment = deployment.Timestamp.ItalianFormat();
                var layout = b.Layout(b.InfraMenu(nameof(Deployments), requestData.User().IsSignedIn()),
                b.Render(b.Const(new Header.Props()
                {
                    Main = new Header.Title() { Operation = "Deployment", Entity = selectedDeployment },
                    User = requestData.User()
                })),
                b.ReviewDeployment(changes)); ;
                return layout;
            });
        }

        public static Var<HyperNode> ReviewDeployment(this BlockBuilder b, ChangesReport serverModel)
        {
            var view = b.Div("flex flex-col");
            var toolbarContainer = b.Add(view, b.Div("flex flex-row justify-end"));
            //b.Add(toolbarContainer, b.Div("w-full"));
            //var toolbar = b.Add(toolbarContainer, b.Toolbar((b, t) =>
            //{
            //    //t.Command("Deploy again", b => { });
            //    //t.Command("Just differences", b => { });
            //}));

            var pageContainer = b.Add(view, b.Div("py-4"));
            b.Add(pageContainer, b.ChangesReport(serverModel.ServiceChanges));

            return view;
        }
    }
}
