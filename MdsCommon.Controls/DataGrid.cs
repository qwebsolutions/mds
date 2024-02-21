using MdsCommon.HtmlControls;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;

namespace MdsCommon.Controls
{
    public class DataGridData<TRow>
    {
        public TableData<TRow> TableData { get; set; } = new();
        public ToolbarData ToolbarData { get; set; } = new();
    }

    public class DataGridDefinition<TRow> : IControlDefinition<DataGridData<TRow>>
    {
        public ControlDefinition<DataGridData<TRow>> Container { get; set; }
        public TableDefinition<TRow> Table { get; set; }
        public ToolbarDefinition Toolbar { get; set; }
        public ControlDefinition<TRow> ToolbarRowAction { get; set; }

        public Func<LayoutBuilder, Var<DataGridData<TRow>>, Var<IVNode>> GetRenderer()
        {
            return Container.GetRenderer();
        }
    }

    public static class DataGridExtensions
    {
        public static DataGridDefinition<TRow> DefaultDataGrid<TRow>()
        {
            var dataGrid = new DataGridDefinition<TRow>();
            dataGrid.Container = ControlDefinition.New<DataGridData<TRow>>(
                "div",
                (b, data, props) =>
                {
                    b.SetClass(props, b.Const("flex flex-col gap-2"));
                },
                (b, data) => b.Render(dataGrid.Toolbar, b.Get(data, x => x.ToolbarData)),
                (b, data) => b.Render(dataGrid.Table, b.Get(data, x => x.TableData)));

            dataGrid.Table = MdsCommon.HtmlControls.Control.DefaultTable<TRow>();
            dataGrid.Toolbar = MdsCommon.Controls.Control.DefaultToolbar();
            dataGrid.ToolbarRowAction = ControlDefinition.New<TRow>(
                "div",
                (b, data, props) =>
                {
                    b.SetClass(props, "flex flex-row gap-2");
                });

            dataGrid.Table.TableRow.EditProps((b, props) =>
            {
                b.AddClass(props, "group");
            });

            dataGrid.Table.TableRow.AddChild(
                (b, data) =>
                {
                    return b.H(
                        "td",
                        (b, props) =>
                        {
                            b.AddClass(props, "relative");
                        },
                        b.H(
                            "div",
                            (b, props) =>
                            {
                                b.SetClass(props, "hidden absolute group-hover:flex flex-row items-center justify-center right-1 top-0 bottom-0");
                            },
                            b.Render(dataGrid.ToolbarRowAction, b.Get(data, x => x.Row))));
                });

            return dataGrid;
        }

        public static Var<IVNode> DataGrid<TRow>(
            this LayoutBuilder b,
            Action<ControlBuilder<DataGridDefinition<TRow>, DataGridData<TRow>>> custom)
            //where TRow: new()
        {
            return b.FromDefinition(DefaultDataGrid<TRow>, custom);
        }

        public static void OnTable<TRow>(
            this ControlBuilder<DataGridDefinition<TRow>, DataGridData<TRow>> b,
            Action<ControlBuilder<TableDefinition<TRow>, TableData<TRow>>> action)
        {
            b.OnChildControl(x => x.Table, x => x.TableData, action);
        }

        //public static void OnToolbarLeft<TRow>(
        //    this ControlBuilder<DataGridDefinition<TRow>, DataGridData<TRow>> b,
        //    Action<ControlBuilder<ControlDefinition<ToolbarData>, ToolbarData>> action)
        //{
        //    b.OnChildControl(x => x.Toolbar.Left, x => x.ToolbarData, action);
        //}

        public static void AddToolbarChild<TRow>(
            this ControlBuilder<DataGridDefinition<TRow>, DataGridData<TRow>> b,
            Func<LayoutBuilder, Var<IVNode>> buildChild,
            HorizontalPlacement placement = HorizontalPlacement.Left)
        {
            b.OnChildControl(
                x => x.Toolbar,
                x => x.ToolbarData,
                b => b.AddToolbarChild(buildChild, placement));
        }
    }


    public static partial class DataGrid
    {
        public class Props
        {
            //public HyperNode Toolbar { get; set; }
            //public HyperNode DataTable { get; set; }// TODO: The data table needs to receive filtering data
            //public bool ShowSearch { get; set; }
        }

        internal static Var<HyperNode> Render<TRow>(LayoutBuilder b,
            List<Func<LayoutBuilder, Var<HyperNode>>> buildToolbar,
            Action<Modifier<DataTable.Props<TRow>>> buildDataTable,
            Action<LayoutBuilder, Var<ActionBar.Props<TRow>>, Var<TRow>> buildActions = null)
        {
            string flexClass = "flex flex-col w-full bg-white rounded";

            if (buildActions != null)
            {
                var dataTableProps = b.NewObj<DataTable.Props<TRow>>(buildDataTable);
                var originalRenderer = b.Get(dataTableProps, x => x.CreateCell);
                var renderCell = b.Def<LayoutBuilder, TRow, DataTable.Column, HyperNode>((b, row, col) =>
                {
                    var currentRenderer = b.Get(dataTableProps, x => x.CreateCell);
                    return b.If(
                        b.AreEqual(b.Get(col, x => x.Name), b.Const("__action__")),
                        b =>
                        {
                            var props = b.NewObj(new ActionBar.Props<TRow>());
                            buildActions(b, props, row);

                            var hoverDiv = b.Div("invisible group-hover:visible");
                            b.Add(hoverDiv, b.ActionBar(props, row));
                            return hoverDiv;
                        },
                        b =>
                        {
                            return b.Call(originalRenderer, row, col);
                        });
                });


                b.Modify(dataTableProps, b =>
                {
                    b.AddColumn<TRow>("__action__", " ");
                    b.SetRenderCell<TRow>(renderCell);
                });

                return b.Div(flexClass,
                    b => b.Toolbar(buildToolbar.ToArray()),
                    b => b.DataTable(dataTableProps));
            }
            else
            {
                return b.Div(flexClass,
                    b => b.Toolbar(buildToolbar.ToArray()),
                    b => b.FromDefault(DataTable.Render, buildDataTable));
            }
        }
    }

    public static partial class Controls
    {
        //internal static Var<HyperNode> DataGrid(
        //    this BlockBuilder b,
        //    Var<HyperNode> toolbar,
        //    Var<HyperNode> dataTable,
        //    Var<bool> showSearch)
        //{
        //    var props = b.NewObj<DataGrid.Props>();
        //    b.Set(props, x => x.Toolbar, toolbar);
        //    b.Set(props, x => x.ShowSearch, showSearch);
        //    b.Set(props, x => x.DataTable, dataTable);

        //    return b.Call(Hyperapp.DataGrid.Render, props);
        //}

        public static Var<HyperNode> DataGrid<TRow>(
            this LayoutBuilder b,
            List<Func<LayoutBuilder, Var<HyperNode>>> buildToolbar,
            Action<Modifier<DataTable.Props<TRow>>> buildDataTable,
            System.Action<LayoutBuilder, Var<ActionBar.Props<TRow>>, Var<TRow>> buildActions = null)
        {
            return MdsCommon.Controls.DataGrid.Render<TRow>(b, buildToolbar, buildDataTable, buildActions);
            ////var toolbarProps = b.NewObj(buildToolbar);
            //var dataTableProps = b.NewObj(buildDataTable);

            //if (buildActions != null)
            //{
            //    b.Modify(dataTableProps, b =>
            //    {
            //        b.AddColumn<TRow>("__action__", " ");

            //        var originalRenderer = b.Get(x => x.CreateCell);
            //        b.SetRenderCell<TRow>((b, row, col) =>
            //        {
            //            var currentRenderer = b.Get(dataTableProps, x => x.CreateCell);
            //            return b.If<HyperNode>(
            //                b.AreEqual(b.Get(col, x => x.Name), b.Const("__action__")),
            //                b =>
            //                {
            //                    var props = b.NewObj(new ActionBar.Props<TRow>());
            //                    buildActions(b, props, row);

            //                    var hoverDiv = b.Div("invisible group-hover:visible");
            //                    b.Add(hoverDiv, b.ActionBar(props, row));
            //                    return hoverDiv;
            //                },
            //                b =>
            //                {
            //                    return b.Call(originalRenderer, row, col);
            //                });
            //        });
            //    });
            //}

            //var toolBar = b.Toolbar(toolbarProps);
            //var dataTable = b.DataTable(dataTableProps);

            //return b.DataGrid(toolBar, dataTable, b.Const(false));
        }
    }
}

