using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        public static async Task<IResponse> ReviewConfiguration(CommandContext commandContext, HttpContext requestData)
        {
#if DEBUG
            WebServer.WebRootPaths.Add("D:\\qweb\\mes\\Mds\\MdsInfrastructure\\inline");
#endif

            var savedConfiguration = await commandContext.Do(Api.LoadConfiguration, System.Guid.Parse(requestData.EntityId()));
            var allProjects = await commandContext.Do(Api.LoadAllProjects);
            var allNodes = await commandContext.Do(Api.LoadAllNodes);

            var snapshot = await MdsInfrastructureFunctions.TakeConfigurationSnapshot(
                commandContext,
                savedConfiguration,
                allProjects,
                allNodes);

            return Page.Response(new object(), (b, clientModel) => b.Layout(b.InfraMenu(nameof(Configuration), requestData.User().IsSignedIn()),
                b.Render(b.Const(new Header.Props()
                {
                    Main = new Header.Title() { Operation = "Review configuration" },
                    User = requestData.User()
                })), RenderDeploymentConfiguration(b, snapshot, savedConfiguration)));
        }

        public class ConfigurationRow
        {
            public string ServiceName { get; set; }
            public string Property { get; set; }
            public string Value { get; set; }
        }

        public static Var<HyperNode> RenderDeploymentConfiguration(
            this BlockBuilder b,
            List<MdsCommon.ServiceConfigurationSnapshot> infrastructureSnapshot,
            InfrastructureConfiguration infrastructureConfiguration)
        {
            var view = b.Div();

            var deploymentReportUrl = b.Url(DeploymentReport, b.Const(infrastructureConfiguration.Id));
            var confirmDeploymentUrl = b.Url(ConfirmDeployment, b.Const(infrastructureConfiguration.Id));

            var swapIcon = Icon.Swap;

            var toolbar = b.Add(view,
                b.Toolbar(
                    b => b.NavigateButton(b=> 
                    {
                        b.Set(x => x.Label, "Review deployment actions");
                        b.Set(x => x.Href, deploymentReportUrl);
                        b.Set(x => x.SvgIcon, swapIcon);
                    }),
                    b => b.NavigateButton(b=>
                    {
                        b.Set(x => x.Label, "Deploy now");
                        b.Set(x => x.Href, confirmDeploymentUrl);
                        b.Set(x => x.Style, Button.Style.Danger);
                    })));

            b.AddClass(toolbar, "justify-end");

            List<ConfigurationRow> configurationRows = new List<ConfigurationRow>();
            foreach (ServiceConfigurationSnapshot serviceSnapshot in infrastructureSnapshot.OrderBy(x => x.ServiceName))
            {
                configurationRows.Add(new ConfigurationRow() { ServiceName = serviceSnapshot.ServiceName, Property = "Node", Value = serviceSnapshot.NodeName });
                configurationRows.Add(new ConfigurationRow() { ServiceName = serviceSnapshot.ServiceName, Property = "Project", Value = serviceSnapshot.ProjectName });
                configurationRows.Add(new ConfigurationRow() { ServiceName = serviceSnapshot.ServiceName, Property = "Version", Value = serviceSnapshot.ProjectVersionTag });
                foreach (var param in serviceSnapshot.ServiceConfigurationSnapshotParameters)
                {
                    configurationRows.Add(new ConfigurationRow() { ServiceName = serviceSnapshot.ServiceName, Property = param.ParameterName, Value = param.DeployedValue });
                }
            }

            var rc = b.Def((BlockBuilder b, Var<ConfigurationRow> row, Var<DataTable.Column> column) =>
            {
                return b.VPadded4(b.Text(b.GetProperty<string>(row, b.Get(column, x => x.Name))));
            });

            var dataTableProps = b.NewObj<DataTable.Props<ConfigurationRow>>(b =>
            {
                b.SetRows(b.Const(configurationRows.ToList()));
                b.AddColumn(nameof(ConfigurationRow.ServiceName), "Service name");
                b.AddColumn(nameof(ConfigurationRow.Property));
                b.AddColumn(nameof(ConfigurationRow.Value));
                b.SetRenderCell(rc);
            });

            b.Add(view, b.DataTable(dataTableProps));

            return view;
        }
    }
}
