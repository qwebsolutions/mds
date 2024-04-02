using Metapsi.Hyperapp;
using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Ui;
using M = MdsInfrastructure.Node;
using System.Linq;
using System;
using MdsCommon.Controls;
using System.Diagnostics.Contracts;
using Metapsi.Html;
using Metapsi.Dom;

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

            public override Var<IVNode> OnRender(LayoutBuilder b, M.List serverModel, Var<M.List> clientModel)
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
                    b.RenderNodesList(clientModel)).As<IVNode>();
            }
        }

        public class Edit : MixedHyperPage<M.EditPage, M.EditPage>
        {
            public override M.EditPage ExtractClientModel(M.EditPage serverModel)
            {
                return serverModel;
            }

            public override Var<IVNode> OnRender(LayoutBuilder b, M.EditPage serverModel, Var<M.EditPage> clientModel)
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

                return layout.As<IVNode>();
            }
        }

        public static Var<IVNode> RenderNodesList(this LayoutBuilder b, Var<M.List> clientModel)
        {
            var nodesGridBuilder = MdsDefaultBuilder.DataGrid<InfrastructureNode>();

            nodesGridBuilder.CreateToolbarActions = b =>
            {
                return b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row");
                        b.OnClickAction((SyntaxBuilder b, Var<M.List> clientModel) =>
                        {
                            b.CallOnObject(b.Window(), "alert", b.Const("Adding nodes is not supported at the moment"));
                            return b.Clone(clientModel);
                        });
                    },
                    b.HtmlButton(
                        b =>
                        {
                            b.AddPrimaryButtonStyle();
                        },
                        b.T("Add node")));
            };

            nodesGridBuilder.DataTableBuilder.OverrideHeaderCell(nameof(InfrastructureNode.NodeName), (b) => b.T("Name"));
            nodesGridBuilder.DataTableBuilder.OverrideDataCell(
                nameof(InfrastructureNode.NodeName),
                (b, node) =>
                {
                    var nodeNameRef = b.Ref(b.Get(node, x => x.NodeName));
                    b.If(b.IsEmpty(b.GetRef(nodeNameRef)), b => b.SetRef(nodeNameRef, b.Const("(not set)")));
                    var nameLink = b.Link(b.GetRef(nodeNameRef), b.Url<Routes.Node.Edit, Guid>(b.Get(node, x => x.Id)));
                    return nameLink;
                });

            nodesGridBuilder.DataTableBuilder.OverrideHeaderCell(nameof(InfrastructureNode.MachineIp), (b) => b.T("IP"));

            nodesGridBuilder.DataTableBuilder.OverrideHeaderCell(nameof(InfrastructureNode.UiPort), (b) => b.T("Controller UI"));
            nodesGridBuilder.DataTableBuilder.OverrideDataCell(
                nameof(InfrastructureNode.UiPort),
                (b, node) =>
                {
                    var machineIp = b.Get(node, x => x.MachineIp);
                    var uiPort = b.Get(node, x => x.UiPort);
                    var uiUrl = b.Concat(b.Const("http://"), machineIp, b.Const(":"), b.AsString(uiPort));
                    return b.HtmlA(
                        b =>
                        {
                            b.SetClass("flex flex-row gap-2");
                            b.UnderlineBlue();
                            b.SetHref(uiUrl);
                        },
                        b.T(uiUrl),
                        b.Svg(Metapsi.Heroicons.Mini.ArrowTopRightOnSquare, "w-5 h-5"));
                });

            nodesGridBuilder.DataTableBuilder.OverrideHeaderCell(nameof(InfrastructureNode.EnvironmentTypeId), (b) => b.T("Type"));
            nodesGridBuilder.DataTableBuilder.OverrideDataCell(
                nameof(InfrastructureNode.EnvironmentTypeId),
                (b, node) =>
                {
                    var envTypes = b.Get(clientModel, x => x.EnvironmentTypes);
                    var envTypeId = b.Get(node, x => x.EnvironmentTypeId);
                    var envType = b.Get(envTypes, envTypeId, (envTypes, envTypeId) => envTypes.SingleOrDefault(y => y.Id == envTypeId));
                    var envTypeLabel = b.If(
                        b.HasObject(envType),
                        b => b.Get(envType, x => x.Name),
                        b => b.Const("(not selected)"));

                    return b.T(envTypeLabel);
                });

            return
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("drop-shadow p-4 rounded bg-white");
                    },
                b.DataGrid(
                    nodesGridBuilder,
                    b.Get(clientModel, x => x.InfrastructureNodes),
                    nameof(InfrastructureNode.NodeName),
                    nameof(InfrastructureNode.MachineIp),
                    nameof(InfrastructureNode.UiPort),
                    nameof(InfrastructureNode.EnvironmentTypeId)));

            //var addUrl = b.Const("Not implemented");


            //var rows = b.Get(clientModel, x => x.InfrastructureNodes.ToList());

            //var rc = b.RenderCell((LayoutBuilder b, Var<InfrastructureNode> node, Var<DataTable.Column> col) =>
            //{
            //    var envTypes = b.Get(clientModel, x => x.EnvironmentTypes);
            //    var nodeNameRef = b.Ref(b.Get(node, x => x.NodeName));

            //    b.If(b.IsEmpty(b.GetRef(nodeNameRef)), b => b.SetRef(nodeNameRef, b.Const("(not set)")));

            //    var columnName = b.Get(col, x => x.Name);

            //    var machineIp = b.Get(node, x => x.MachineIp);
            //    var uiPort = b.Get(node, x => x.UiPort);
            //    var uiUrl = b.Concat(b.Const("http://"), machineIp, b.Const(":"), uiPort.As<string>());
            //    var envTypeId = b.Get(node, x => x.EnvironmentTypeId);
            //    var envType = b.Get(envTypes, envTypeId, (envTypes, envTypeId) => envTypes.SingleOrDefault(y => y.Id == envTypeId));

            //    var envTypeLabel = b.If(b.HasObject(envType),
            //        b => b.Get(envType, x => x.Name),
            //        b => b.Const("(not selected)"));

            //    var nameLink = b.Link(b.GetRef(nodeNameRef), b.Url<Routes.Node.Edit, Guid>(b.Get(node, x => x.Id)));

            //    return b.VPadded4(b.Switch(
            //        columnName, b => b.Text("Not supported"),
            //        (nameof(InfrastructureNode.MachineIp), b => b.Text(machineIp)),
            //        (nameof(InfrastructureNode.NodeName), b => nameLink),
            //        (nameof(InfrastructureNode.UiPort), b => b.Text(uiUrl)),
            //        (nameof(InfrastructureNode.EnvironmentTypeId), b => b.Text(envTypeLabel))));
            //});

            //var dg = b.DataGrid<InfrastructureNode>(
            //    new()
            //    {
            //        b=> b.AddClass(b.NavigateButton(b=>
            //        {
            //            b.Set(x=>x.Label, "Add node");
            //            b.Set(x => x.Href, addUrl);
            //        }), "text-white")
            //    },
            //    b =>
            //    {
            //        b.AddColumn(nameof(InfrastructureNode.NodeName), "Name");
            //        b.AddColumn(nameof(InfrastructureNode.MachineIp), "IP");
            //        b.AddColumn(nameof(InfrastructureNode.UiPort), "Controller UI");
            //        b.AddColumn(nameof(InfrastructureNode.EnvironmentTypeId), "Type");
            //        b.SetRows(rows);
            //        b.SetRenderCell(rc);
            //    });
            //b.AddClass(dg, "drop-shadow");
            //return dg;
        }


        public static Var<IVNode> RenderEditNodePage(
            this LayoutBuilder b,
            Var<M.EditPage> clientModel)
        {
            var node = b.Get(clientModel, x => x.InfrastructureNode);
            return b.StyledDiv(
                "flex flex-col w-full",
                b.ValidationPanel(clientModel),
                b.RenderEditNodeForm(clientModel));
        }

        public static Var<IVNode> RenderEditNodeForm(
            this LayoutBuilder b,
            Var<M.EditPage> clientModel)
        {
            var node = b.Get(clientModel, x => x.InfrastructureNode);
            var envId = b.Get(node, x => x.EnvironmentTypeId);
            var envTypes = b.Get(clientModel, m => m.EnvironmentTypes.ToList());
            var saveUrl = b.Const("Not implemented");
            var toolbar = b.Toolbar(b => { },
                b => b.SubmitButton<InfrastructureNode>(b =>
                {
                    b.Set(x => x.Label, "Save");
                    b.Set(x => x.Href, saveUrl);
                    b.Set(x => x.Payload, node);
                    b.Set(x => x.ButtonClass, "rounded text-white py-2 px-4 shadow bg-sky-500");
                }));

            var form = b.Form(
                b =>
                {
                    b.AddClass("p-4 bg-white rounded drop-shadow");
                },
                toolbar,
                ("Node name", b.MdsInputText(b => b.BindTo(clientModel, x => x.InfrastructureNode, x => x.NodeName))),
                ("Machine address", b.MdsInputText(b => b.BindTo(clientModel, x => x.InfrastructureNode, x => x.MachineIp))),
                ("Node UI port", b.MdsInputText(b => b.BindTo(clientModel, x => x.InfrastructureNode, x => x.UiPort))));

            // TODO: Node env type?!

            //var osChoicesDd = b.DropDown(b.MapChoices(envTypes, x => x.Id, x => x.Name, b.Get(node, x => x.EnvironmentTypeId)));
            //b.SetOnChoiceSelected<M.EditPage, Guid>(osChoicesDd, (b, state, value) =>
            //{
            //    var node = b.Get(clientModel, x => x.InfrastructureNode);
            //    b.Set(node, x => x.EnvironmentTypeId, value);
            //});

            return form;
        }

        public static Var<IVNode> Form(
            this LayoutBuilder b,
            Action<PropsBuilder<HtmlDiv>> setProps, Var<IVNode> toolbar, Var<System.Collections.Generic.List<IVNode>> formFieldControls)
        {
            var nodes = b.NewCollection<IVNode>();
            b.Push(
                nodes,
                b.StyledDiv(
                    "col-span-2 flex flex-row justify-end items-center",
                    toolbar));
            b.PushRange(nodes, formFieldControls);

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("grid grid-cols-2 gap-4 items-center p-4");
                    setProps(b);
                },
                nodes);
        }

        public static Var<IVNode> Form(this LayoutBuilder b, Action<PropsBuilder<HtmlDiv>> setProps, Var<IVNode> toolbar, params (string, Var<IVNode>)[] forms)
        {
            var formFieldControls = b.NewCollection<IVNode>();

            foreach (var formField in forms)
            {
                b.Push(formFieldControls, b.TextSpan(formField.Item1));
                b.Push(formFieldControls, formField.Item2);
            }

            return b.Form(setProps, toolbar, formFieldControls);
        }

        public static void AddFormField(this LayoutBuilder b, Var<System.Collections.Generic.List<IVNode>> nodes, string label, Var<IVNode> fieldControl)
        {
            b.Push(nodes, b.TextSpan(label));
            b.Push(nodes, fieldControl);
        }

        //public static (Var<IVNode>, Var<IVNode>) FormField(this LayoutBuilder b, string label, Var<IVNode> fieldControl)
        //{
        //    return (b.TextSpan(label), fieldControl);
        //}

        public static Var<string> WithDefault(this SyntaxBuilder b, Var<string> value)
        {
            return b.If(b.HasValue(value), b => value, b => b.Const("(not set)"));
        }
    }
}
