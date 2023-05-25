using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MdsInfrastructure
{
    public static partial class Nodes
    {
        public class NodesList
        {
            public List<EnvironmentType> EnvironmentTypes { get; set; } = new List<EnvironmentType>();
            public List<InfrastructureNode> InfrastructureNodes { get; set; } = new List<InfrastructureNode>();
            public List<InfrastructureService> InfrastructureServices { get; set; } = new List<InfrastructureService>();
        }

        public static async Task<IResponse> List(CommandContext commandContext, HttpContext requestData)
        {
            NodesList nodesList = new NodesList()
            {
                EnvironmentTypes = await commandContext.Do(Api.LoadEnvironmentTypes),
                InfrastructureNodes = await commandContext.Do(Api.LoadAllNodes),
                InfrastructureServices = await commandContext.Do(Api.LoadAllServices)
            };

            return Page.Response(nodesList, (b, clientModel) => b.Layout(b.InfraMenu(nameof(Nodes), requestData.User().IsSignedIn()),
                b.Render(b.Const(new Header.Props()
                {
                    Main = new Header.Title() { Operation = "Nodes" },
                    User = requestData.User()
                })), b.RenderNodesList(clientModel)));
        }

        public static Var<HyperNode> RenderNodesList(this BlockBuilder b, Var<NodesList> clientModel)
        {
            var addUrl = b.Url(Nodes.Add);

            var rows = b.Get(clientModel, x => x.InfrastructureNodes.ToList());

            var rc = b.RenderCell((BlockBuilder b, Var<InfrastructureNode> node, Var<DataTable.Column> col) =>
            {
                b.Log("Nodes.List original renderer node", node);
                b.Log("Nodes.List original renderer col", col);

                var envTypes = b.Get(clientModel, x => x.EnvironmentTypes);
                var nodeNameRef = b.Ref(b.Get(node, x => x.NodeName));

                b.If(b.IsEmpty(b.GetRef(nodeNameRef)), b => b.SetRef(nodeNameRef, b.Const("(not set)")));

                var columnName = b.Get(col, x => x.Name);

                var machineIp = b.Get(node, x => x.MachineIp);
                var uiPort = b.Get(node, x => x.UiPort);
                var uiUrl = b.Concat(b.Const("http://"), machineIp, b.Const(":"), uiPort.As<string>());
                var envTypeId = b.Get(node, x => x.EnvironmentTypeId);
                var envType = b.Get(envTypes, envTypeId, (envTypes, envTypeId) => envTypes.SingleOrDefault(y => y.Id == envTypeId));

                var envTypeLabel = b.If(b.HasObject(envType),
                    b => b.Get(envType, x => x.Name),
                    b => b.Const("(not selected)"));

                var nameLink = b.Link(b.GetRef(nodeNameRef), b.Url(Edit, b.Get(node, x => x.Id)));

                return b.VPadded4(b.Switch(
                    columnName, b => b.Text("Not supported"),
                    (nameof(InfrastructureNode.MachineIp), b => b.Text(machineIp)),
                    (nameof(InfrastructureNode.NodeName), b => nameLink),
                    (nameof(InfrastructureNode.UiPort), b => b.Text(uiUrl)),
                    (nameof(InfrastructureNode.EnvironmentTypeId), b => b.Text(envTypeLabel))));
            });

            var dg = b.DataGrid<InfrastructureNode>(
                new()
                {
                    b=> b.NavigateButton(b=>
                    {
                        b.Set(x=>x.Label, "Add node");
                        b.Set(x => x.Href, addUrl);
                    })
                },
                b =>
                {
                    b.AddColumn(nameof(InfrastructureNode.NodeName), "Name");
                    b.AddColumn(nameof(InfrastructureNode.MachineIp), "IP");
                    b.AddColumn(nameof(InfrastructureNode.UiPort), "Controller UI");
                    b.AddColumn(nameof(InfrastructureNode.EnvironmentTypeId), "Type");
                    b.SetRows(rows);
                    b.SetRenderCell(rc);
                });
            b.AddClass(dg, "drop-shadow");
            return dg;
        }
    }
}
