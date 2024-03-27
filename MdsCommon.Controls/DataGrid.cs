using MdsCommon.HtmlControls;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;

namespace MdsCommon.Controls
{
    public class DataGridBuilder<TRow>
    {
        public DataTableBuilder<TRow> DataTableBuilder { get; set; }

        public Action<PropsBuilder<HtmlDiv>> SetContainerProps { get; set; } = b => { b.SetClass(b.Const("flex flex-col gap-2")); };

        public Func<LayoutBuilder, Var<IVNode>> CreateToolbarActions { get; set; } = b => b.VoidNode();

        public Func<LayoutBuilder, Var<TRow>, Var<IVNode>> CreateRowActions { get; set; } = null;
    }

    public static class DataGridControl
    {
        private const string ActionColumnName = "__action__";

        public static Var<IVNode> DataGrid<TRow>(
            this LayoutBuilder b,
            DataGridBuilder<TRow> dataGridBuilder,
            Var<List<TRow>> rows,
            Var<List<string>> columns = null)
        {
            if(columns == null)
                columns = b.Const(DataTable.GetColumns<TRow>());

            var dataTableBuilder = new DataTableBuilder<TRow>()
            {
                CreateDataCell = dataGridBuilder.DataTableBuilder.CreateDataCell,
                CreateHeaderCell = dataGridBuilder.DataTableBuilder.CreateHeaderCell,
                SetTableProps = dataGridBuilder.DataTableBuilder.SetTableProps,
                SetTbodyProps = dataGridBuilder.DataTableBuilder.SetTbodyProps,
                SetTdProps = dataGridBuilder.DataTableBuilder.SetTdProps,
                SetTheadProps = dataGridBuilder.DataTableBuilder.SetTheadProps,
                SetThProps = dataGridBuilder.DataTableBuilder.SetThProps,
                SetTrProps = dataGridBuilder.DataTableBuilder.SetTrProps
            };

            var withActionsColumn = b.NewCollection<string>();
            b.PushRange(withActionsColumn, columns);
            if (dataGridBuilder.CreateRowActions != null)
            {
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

                dataTableBuilder.CreateDataCell = (b, row, column) =>
                {
                    return b.If(
                        b.AreEqual(column, b.Const(ActionColumnName)),
                        b =>
                        {
                            return b.VoidNode();
                        },
                        b =>
                        {
                            return dataGridBuilder.DataTableBuilder.CreateDataCell(b, row, column);
                        });
                };
            }

            return b.HtmlDiv(
                dataGridBuilder.SetContainerProps,
                b.Call(dataGridBuilder.CreateToolbarActions),
                b.DataTable(dataTableBuilder, rows, withActionsColumn));
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

