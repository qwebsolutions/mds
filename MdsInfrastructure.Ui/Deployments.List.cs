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
        public class DeploymentHistory
        {
            public System.Collections.Generic.List<Deployment> Deployments { get; set; } = new System.Collections.Generic.List<Deployment>();
        }

        public static async Task<IResponse> List(CommandContext commandContext, HttpContext requestData)
        {
            var deploymentsHistory = new DeploymentHistory()
            {
                Deployments = await commandContext.Do(Api.LoadDeploymentsHistory)
            };
            return Page.Response(
                new object(), (b, clientModel) => b.Layout(
                b.InfraMenu(nameof(Deployments), requestData.User().IsSignedIn()),
                b.Render(b.Const(new Header.Props()
                {
                    Main = new Header.Title() { Operation = "Deployments" },
                    User = requestData.User()
                })),
                Render(b, deploymentsHistory)),
                string.Empty);
        }

        public static Var<HyperNode> Render(this BlockBuilder b, DeploymentHistory serverModel)
        {
            var rc = b.RenderCell<Deployment>((b, row, col) =>
            {
                var dateStringLocale = b.ItalianFormat(b.Get(row, x => x.Timestamp));
                return b.VPadded4(b.If(b.AreEqual(b.Get(col, x => x.Name), b.Const("timestamp")),
                    b => b.Link(dateStringLocale, b.Url(Deployments.Review, b.Get(row, x => x.Id).As<string>())),
                    b => b.Text(b.Get(row, x => x.ConfigurationName))));
            });

            var props = b.NewObj<DataTable.Props<Deployment>>(b =>
            {
                b.AddColumn("timestamp", "Deployment timestamp");
                b.AddColumn("name", "Configuration name");
                b.SetRows(b.Const(serverModel.Deployments.ToList()));
                b.SetRenderCell(rc);
            });

            var table = b.DataTable(props);
            b.AddClass(table, "drop-shadow");
            return table;
        }
    }
}
