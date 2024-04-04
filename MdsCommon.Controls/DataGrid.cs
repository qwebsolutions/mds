using MdsCommon.HtmlControls;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsCommon.Controls
{
    public class DataGridBuilder<TRow>
    {
        public DataTableBuilder<TRow> DataTableBuilder { get; set; }

        public Action<PropsBuilder<HtmlDiv>> SetContainerProps { get; set; } = b => { b.SetClass(b.Const("flex flex-col gap-2")); };

        public Func<LayoutBuilder, Var<IVNode>> CreateToolbarActions { get; set; } = b => b.VoidNode();

        public Func<LayoutBuilder, Var<TRow>, Var<List<IVNode>>> CreateRowActions { get; set; } = (b, row) => b.NewCollection<IVNode>();
    }

    public static class DataGridControl
    {
        private const string GarbageBinIcon = "<svg\r\n id=\"Layer_1\"\r\n data-name=\"Layer 1\"\r\n xmlns=\"http://www.w3.org/2000/svg\"\r\n        viewBox=\"0 0 105.16 122.88\" width=\"20\" height=\"20\"\r\n        ><defs\r\n            ><style>\r\n                .cls-1 {\r\n                    fill-rule: evenodd;\r\n                }\r\n            </style></defs\r\n        ><path\r\n            fill=\"currentColor\"\r\n            class=\"cls-1\"\r\n            d=\"M11.17,37.16H94.65a8.4,8.4,0,0,1,2,.16,5.93,5.93,0,0,1,2.88,1.56,5.43,5.43,0,0,1,1.64,3.34,7.65,7.65,0,0,1-.06,1.44L94,117.31v0l0,.13,0,.28v0a7.06,7.06,0,0,1-.2.9v0l0,.06v0a5.89,5.89,0,0,1-5.47,4.07H17.32a6.17,6.17,0,0,1-1.25-.19,6.17,6.17,0,0,1-1.16-.48h0a6.18,6.18,0,0,1-3.08-4.88l-7-73.49a7.69,7.69,0,0,1-.06-1.66,5.37,5.37,0,0,1,1.63-3.29,6,6,0,0,1,3-1.58,8.94,8.94,0,0,1,1.79-.13ZM5.65,8.8H37.12V6h0a2.44,2.44,0,0,1,0-.27,6,6,0,0,1,1.76-4h0A6,6,0,0,1,43.09,0H62.46l.3,0a6,6,0,0,1,5.7,6V6h0V8.8h32l.39,0a4.7,4.7,0,0,1,4.31,4.43c0,.18,0,.32,0,.5v9.86a2.59,2.59,0,0,1-2.59,2.59H2.59A2.59,2.59,0,0,1,0,23.62V13.53H0a1.56,1.56,0,0,1,0-.31v0A4.72,4.72,0,0,1,3.88,8.88,10.4,10.4,0,0,1,5.65,8.8Zm42.1,52.7a4.77,4.77,0,0,1,9.49,0v37a4.77,4.77,0,0,1-9.49,0v-37Zm23.73-.2a4.58,4.58,0,0,1,5-4.06,4.47,4.47,0,0,1,4.51,4.46l-2,37a4.57,4.57,0,0,1-5,4.06,4.47,4.47,0,0,1-4.51-4.46l2-37ZM25,61.7a4.46,4.46,0,0,1,4.5-4.46,4.58,4.58,0,0,1,5,4.06l2,37a4.47,4.47,0,0,1-4.51,4.46,4.57,4.57,0,0,1-5-4.06l-2-37Z\"\r\n        />\r\n    </svg>\r\n\r\n";

        private const string ActionColumnName = "__action__";

        public static Var<IVNode> DataGrid<TRow>(
            this LayoutBuilder b,
            DataGridBuilder<TRow> dataGridBuilder,
            Var<List<TRow>> rows,
            Var<List<string>> columns)
        {
            var dataTableBuilder = dataGridBuilder.DataTableBuilder.Clone();

            var withActionsColumn = b.NewCollection<string>();
            b.PushRange(withActionsColumn, columns);
            b.Push(withActionsColumn, b.Const(ActionColumnName));

            dataTableBuilder.CreateHeaderCell = (b, column) =>
            {
                return b.If(
                    b.AreEqual(column, b.Const(ActionColumnName)),
                    b =>
                    {
                        return b.VoidNode();
                    },
                    b =>
                    {
                        return dataGridBuilder.DataTableBuilder.CreateHeaderCell(b, column);
                    });
            };

            dataTableBuilder.AddTrProps((b, row) =>
            {
                b.AddClass("group");
            });

            dataTableBuilder.AddTdProps(
                (b, row, column) =>
                b.If(
                    b.AreEqual(column, b.Const(ActionColumnName)),
                    b => b.AddClass("relative")));

            dataTableBuilder.OverrideDataCell(ActionColumnName,
                (b, row) =>
                {
                    return b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("hidden absolute group-hover:flex flex-row items-center justify-center right-1 top-0 bottom-0");
                        },
                        dataGridBuilder.CreateRowActions(b, row));
                });

            //dataTableBuilder.CreateDataCell = (b, row, column) =>
            //{
            //    return b.If(
            //        b.AreEqual(column, b.Const(ActionColumnName)),
            //        b =>
            //        {
            //            dataGridBuilder.CreateRowActions()

            //        },
            //        b =>
            //        {
            //            return dataGridBuilder.DataTableBuilder.CreateDataCell(b, row, column);
            //        });
            //};

            return b.HtmlDiv(
                dataGridBuilder.SetContainerProps,
                b.Call(dataGridBuilder.CreateToolbarActions),
                b.DataTable(dataTableBuilder, rows, withActionsColumn));
        }

        public static Var<IVNode> DataGrid<TRow>(
            this LayoutBuilder b,
            DataGridBuilder<TRow> dataGridBuilder,
            Var<List<TRow>> rows)
        {
            return b.DataGrid(dataGridBuilder, rows, b.Const(DataTable.GetColumns<TRow>()));
        }

        public static Var<IVNode> DataGrid<TRow>(
            this LayoutBuilder b,
            DataGridBuilder<TRow> dataGridBuilder,
            Var<List<TRow>> rows,
            params string[] columns)
        {
            return b.DataGrid(dataGridBuilder, rows, b.Const(columns.ToList()));
        }

        public static void AddRowAction<TRow>(
            this DataGridBuilder<TRow> dataGridBuilder,
            Func<LayoutBuilder, Var<TRow>, Var<IVNode>> create)
        {
            var prevBuilder = dataGridBuilder.CreateRowActions;

            dataGridBuilder.CreateRowActions = (b, row) =>
            {
                var prevActions = prevBuilder(b, row);
                var outActions = b.NewCollection<IVNode>();
                b.PushRange(outActions, prevActions);
                b.Push(outActions, create(b, row));
                return outActions;
            };
        }

        public static Var<IVNode> RowIconAction(this LayoutBuilder b, Action<PropsBuilder<HtmlButton>> buildContainer, Var<IVNode> child)
        {
            return b.HtmlButton(
                b =>
                {
                    b.SetClass("flex rounded bg-gray-200 w-10 h-10 p-1 cursor-pointer justify-center items-center opacity-50 hover:opacity-100");
                    buildContainer(b);
                },
                child);
        }

        public static Var<IVNode> DeleteRowIconAction(this LayoutBuilder b, Action<PropsBuilder<HtmlButton>> buildContainer)
        {
            return b.RowIconAction(b =>
            {
                b.AddClass("text-red-500");
                buildContainer(b);
            },
            b.Svg(GarbageBinIcon));
        }
    }

    //public class DataGridData<TRow>
    //{
    //    public TableData<TRow> TableData { get; set; } = new();
    //    public ToolbarData ToolbarData { get; set; } = new();
    //}

    //public class DataGridDefinition<TRow> : IControlDefinition<DataGridData<TRow>>
    //{
    //    public ControlDefinition<DataGridData<TRow>> Container { get; set; }
    //    public TableDefinition<TRow> Table { get; set; }
    //    public ToolbarDefinition Toolbar { get; set; }
    //    public ControlDefinition<TRow> ToolbarRowAction { get; set; }

    //    public Func<LayoutBuilder, Var<DataGridData<TRow>>, Var<IVNode>> GetRenderer()
    //    {
    //        return Container.GetRenderer();
    //    }
    //}

    //public static class DataGridExtensions
    //{
    //    public static DataGridDefinition<TRow> DefaultDataGrid<TRow>()
    //    {
    //        var dataGrid = new DataGridDefinition<TRow>();
    //        dataGrid.Container = ControlDefinition.New<DataGridData<TRow>>(
    //            "div",
    //            (b, data, props) =>
    //            {
    //                b.SetClass(props, b.Const("flex flex-col gap-2"));
    //            },
    //            (b, data) => b.Render(dataGrid.Toolbar, b.Get(data, x => x.ToolbarData)),
    //            (b, data) => b.Render(dataGrid.Table, b.Get(data, x => x.TableData)));

    //        dataGrid.Table = MdsCommon.HtmlControls.Control.DefaultTable<TRow>();
    //        dataGrid.Toolbar = MdsCommon.Controls.Control.DefaultToolbar();
    //        dataGrid.ToolbarRowAction = ControlDefinition.New<TRow>(
    //            "div",
    //            (b, data, props) =>
    //            {
    //                b.SetClass(props, "flex flex-row gap-2");
    //            });

    //        dataGrid.Table.TableRow.EditProps((b, props) =>
    //        {
    //            b.AddClass(props, "group");
    //        });

    //        dataGrid.Table.TableRow.AddChild(
    //            (b, data) =>
    //            {
    //                return b.H(
    //                    "td",
    //                    (b, props) =>
    //                    {
    //                        b.AddClass(props, "relative");
    //                    },
    //                    b.H(
    //                        "div",
    //                        (b, props) =>
    //                        {
    //                            b.SetClass(props, "hidden absolute group-hover:flex flex-row items-center justify-center right-1 top-0 bottom-0");
    //                        },
    //                        b.Render(dataGrid.ToolbarRowAction, b.Get(data, x => x.Row))));
    //            });

    //        return dataGrid;
    //    }

    //    public static Var<IVNode> DataGrid<TRow>(
    //        this LayoutBuilder b,
    //        Action<ControlBuilder<DataGridDefinition<TRow>, DataGridData<TRow>>> custom)
    //        //where TRow: new()
    //    {
    //        return b.FromDefinition(DefaultDataGrid<TRow>, custom);
    //    }

    //    public static void OnTable<TRow>(
    //        this ControlBuilder<DataGridDefinition<TRow>, DataGridData<TRow>> b,
    //        Action<ControlBuilder<TableDefinition<TRow>, TableData<TRow>>> action)
    //    {
    //        b.OnChildControl(x => x.Table, x => x.TableData, action);
    //    }

    //    public static void AddToolbarChild<TRow>(
    //        this ControlBuilder<DataGridDefinition<TRow>, DataGridData<TRow>> b,
    //        Func<LayoutBuilder, Var<IVNode>> buildChild,
    //        HorizontalPlacement placement = HorizontalPlacement.Left)
    //    {
    //        b.OnChildControl(
    //            x => x.Toolbar,
    //            x => x.ToolbarData,
    //            b => b.AddToolbarChild(buildChild, placement));
    //    }
    //}


    //public static partial class DataGrid
    //{
    //    public class Props
    //    {
    //    }

    //    internal static Var<IVNode> Render<TRow>(LayoutBuilder b,
    //        List<Func<LayoutBuilder, Var<IVNode>>> buildToolbar,
    //        Action<Modifier<DataTable.Props<TRow>>> buildDataTable,
    //        Action<LayoutBuilder, Var<ActionBar.Props<TRow>>, Var<TRow>> buildActions = null)
    //    {
    //        string flexClass = "flex flex-col w-full bg-white rounded";

    //        if (buildActions != null)
    //        {
    //            var dataTableProps = b.NewObj<DataTable.Props<TRow>>(buildDataTable);
    //            var originalRenderer = b.Get(dataTableProps, x => x.CreateCell);
    //            var renderCell = b.Def<LayoutBuilder, TRow, DataTable.Column, IVNode>((b, row, col) =>
    //            {
    //                var currentRenderer = b.Get(dataTableProps, x => x.CreateCell);
    //                return b.If(
    //                    b.AreEqual(b.Get(col, x => x.Name), b.Const("__action__")),
    //                    b =>
    //                    {
    //                        var props = b.NewObj(new ActionBar.Props<TRow>());
    //                        buildActions(b, props, row);

    //                        return b.HtmlDiv(
    //                            b =>
    //                            {
    //                                b.SetClass("invisible group-hover:visible");
    //                            },
    //                            b.ActionBar(props, row));
    //                    },
    //                    b =>
    //                    {
    //                        return b.Call(originalRenderer, row, col);
    //                    });
    //            });


    //            b.Modify(dataTableProps, b =>
    //            {
    //                b.AddColumn<TRow>("__action__", " ");
    //                b.SetRenderCell<TRow>(renderCell);
    //            });

    //            return b.HtmlDiv(
    //                b =>
    //                {
    //                    b.SetClass(flexClass);
    //                },
    //                b.Toolbar(buildToolbar.ToArray()),
    //                b.DataTable(dataTableProps, b => { }));
    //        }
    //        else
    //        {
    //            return b.HtmlDiv(
    //                b =>
    //                {
    //                    b.SetClass(flexClass);
    //                },
    //                b.Toolbar(buildToolbar.ToArray()),
    //                b.FromDefault((LayoutBuilder b, Var<DataTable.Props<TRow>> props) =>
    //                {
    //                    return DataTable.Render(b, props, b => { });
    //                },
    //                buildDataTable));
    //        }
    //    }
    //}

    //public static partial class Controls
    //{
    //    public static Var<IVNode> DataGrid<TRow>(
    //        this LayoutBuilder b,
    //        List<Func<LayoutBuilder, Var<IVNode>>> buildToolbar,
    //        Action<Modifier<DataTable.Props<TRow>>> buildDataTable,
    //        System.Action<LayoutBuilder, Var<ActionBar.Props<TRow>>, Var<TRow>> buildActions = null)
    //    {
    //        return MdsCommon.Controls.DataGrid.Render<TRow>(b, buildToolbar, buildDataTable, buildActions);
    //    }
    //}
}

