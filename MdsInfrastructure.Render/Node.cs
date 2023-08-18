using Metapsi.Hyperapp;
using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Ui;
using M = MdsInfrastructure.Node;
using System.Linq;
using System;
using MdsCommon.Controls;
using Metapsi.ChoicesJs;

namespace MdsInfrastructure.Render
{
    public static class Node
    {
        public class List : MixedHyperPage<M.List, M.List>
        {
            public override MdsInfrastructure.Node.List ExtractClientModel(M.List serverModel)
            {
                return serverModel;
            }

            public override Var<HyperNode> OnRender(BlockBuilder b, M.List serverModel, Var<M.List> clientModel)
            {
                b.AddModuleStylesheet();
                return b.Layout(
                    b.InfraMenu(nameof(Routes.Node), serverModel.User.IsSignedIn()),
                    b.Render(
                        b.Const(
                            new Header.Props()
                            {
                                Main = new Header.Title() { Operation = "Nodes" },
                                User = serverModel.User,
                            })),
                    b.RenderNodesList(clientModel));
            }
        }

        public class Edit : MixedHyperPage<M.EditPage, M.EditPage>
        {
            public override M.EditPage ExtractClientModel(M.EditPage serverModel)
            {
                return serverModel;
            }

            public override Var<HyperNode> OnRender(BlockBuilder b, M.EditPage serverModel, Var<M.EditPage> clientModel)
            {
                var nodeName = b.Get(clientModel, x => x.InfrastructureNode.NodeName);

                var header = b.NewObj(new Header.Props()
                {
                    Main = new Header.Title() { Operation = "Edit node" },
                    User = serverModel.User,
                });
                b.Set(b.Get(header, x => x.Main), x => x.Entity, nodeName);

                var layout = b.Layout(
                    b.InfraMenu(nameof(Routes.Node), serverModel.User.IsSignedIn()),
                    b.Render(header),
                    b.RenderEditNodePage(clientModel));

                b.AddModuleStylesheet();

                return layout;
            }
        }

        public static Var<HyperNode> RenderNodesList(this BlockBuilder b, Var<M.List> clientModel)
        {
            var addUrl = b.Const("Not implemented");


            var rows = b.Get(clientModel, x => x.InfrastructureNodes.ToList());

            var rc = b.RenderCell((BlockBuilder b, Var<InfrastructureNode> node, Var<DataTable.Column> col) =>
            {
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

                var nameLink = b.Link(b.GetRef(nodeNameRef), b.Url<Routes.Node.Edit, Guid>(b.Get(node, x => x.Id)));

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
                    b=> b.AddClass(b.NavigateButton(b=>
                    {
                        b.Set(x=>x.Label, "Add node");
                        b.Set(x => x.Href, addUrl);
                    }), "text-white")
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


        public static Var<HyperNode> RenderEditNodePage(
            this BlockBuilder b,
            Var<M.EditPage> clientModel)
        {
            var node = b.Get(clientModel, x => x.InfrastructureNode);
            var container = b.Div("flex flex-col w-full");
            b.Add(container, b.ValidationPanel(clientModel));
            b.Add(container, b.RenderEditNodeForm(clientModel));
            return container;
        }

        public static Var<HyperNode> RenderEditNodeForm(
            this BlockBuilder b,
            Var<M.EditPage> clientModel)
        {
            var node = b.Get(clientModel, x => x.InfrastructureNode);
            var envId = b.Get(node, x => x.EnvironmentTypeId);
            var envTypes = b.Get(clientModel, m => m.EnvironmentTypes.ToList());
            var saveUrl = b.Const("Not implemented");
            var toolbar = b.Toolbar(
                b => b.SubmitButton<InfrastructureNode>(b =>
                {
                    b.Set(x => x.Label, "Save");
                    b.Set(x => x.Href, saveUrl);
                    b.Set(x => x.Payload, node);
                    b.Set(x => x.ButtonClass, "rounded text-white py-2 px-4 shadow bg-sky-500");
                }));

            var form = b.Form(toolbar);
            b.AddClass(form, "p-4");
            b.AddClass(form, "bg-white rounded drop-shadow");
            b.FormField(form, "Node name", b.BoundInput(clientModel, x => x.InfrastructureNode, x => x.NodeName));
            b.FormField(form, "Machine address", b.BoundInput(clientModel, x => x.InfrastructureNode, x => x.MachineIp));
            b.FormField(form, "Node UI port", b.BoundInput(clientModel, x => x.InfrastructureNode, x => x.UiPort, b.Const(string.Empty)));

            var osChoicesDd = b.DropDown(b.MapChoices(envTypes, x => x.Id, x => x.Name, b.Get(node, x => x.EnvironmentTypeId)));
            b.SetOnChoiceSelected<M.EditPage, Guid>(osChoicesDd, (b, state, value) =>
            {
                var node = b.Get(clientModel, x => x.InfrastructureNode);
                b.Set(node, x => x.EnvironmentTypeId, value);
            });

            return form;
        }

        public static Var<HyperNode> Form(this BlockBuilder b, Var<HyperNode> toolbar)
        {
            //var container = b.Div("flex flex-col p-4 gap-4");
            //var top = b.Add(container, b.Div("flex flex-row justify-end"));
            //b.Add(top, toolbar);
            //var grid = b.Add(container, b.Div("grid grid-cols-2 gap-4 items-center"));
            //return grid;

            var grid = b.Div("grid grid-cols-2 gap-4 items-center p-4");

            var toolbarRow = b.Add(grid, b.Div("col-span-2 flex flex-row justify-end items-center"));
            b.Add(toolbarRow, toolbar);

            return grid;
        }

        public static void FormField(this BlockBuilder b, Var<HyperNode> form, string label, Var<HyperNode> fieldControl)
        {
            b.Add(form, b.Text(label));
            b.Add(form, fieldControl);
        }

        public static Var<string> WithDefault(this BlockBuilder b, Var<string> value)
        {
            return b.If(b.HasValue(value), b => value, b => b.Const("(not set)"));
        }
    }
}
