using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Routing.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MdsCommon.Controls
{
    public class DataTableBuilder<TRow>
    {
        public Action<PropsBuilder<HtmlTable>> SetTableProps { get; set; } = b => { };
        public Action<PropsBuilder<HtmlThead>> SetTheadProps { get; set; } = b => { };
        public Action<PropsBuilder<HtmlTh>, Var<string>> SetThProps { get; set; } = (b, column) => { };
        public Action<PropsBuilder<HtmlTbody>> SetTbodyProps { get; set; } = b => { };
        public Action<PropsBuilder<HtmlTr>, Var<TRow>> SetTrProps { get; set; } = (b, row) => { };
        public Action<PropsBuilder<HtmlTd>, Var<TRow>, Var<string>> SetTdProps { get; set; } = (b, row, column) => { };

        public Func<LayoutBuilder, Var<string>, Var<IVNode>> CreateHeaderCell { get; set; } = (b, column) => b.HtmlSpanText(b => { }, column);
        public Func<LayoutBuilder, Var<TRow>, Var<string>, Var<IVNode>> CreateDataCell { get; set; } = (b, row, column) => b.HtmlSpanText(b => { }, b.GetProperty<string>(row, column));

        public DataTableBuilder<TRow> Clone()
        {
            return new DataTableBuilder<TRow>()
            {
                CreateDataCell = CreateDataCell,
                CreateHeaderCell = CreateHeaderCell,
                SetTableProps = SetTableProps,
                SetTbodyProps = SetTbodyProps,
                SetTdProps = SetTdProps,
                SetTheadProps = SetTheadProps,
                SetThProps = SetThProps,
                SetTrProps = SetTrProps
            };
        }
    }

    public static class DataTableExtensions
    {
        public static void OverrideHeaderCell<TRow>(this DataTableBuilder<TRow> tableBuilder, string columnName, Func<LayoutBuilder, Var<IVNode>> cellBuilder)
        {
            var prevBuilder = tableBuilder.CreateHeaderCell;
            tableBuilder.CreateHeaderCell = (LayoutBuilder b, Var<string> column) =>
            {
                return b.If(
                    b.AreEqual(column, b.Const(columnName)),
                    b =>
                    {
                        return b.Call(cellBuilder);
                    },
                    b => b.Call(prevBuilder, column));
            };
        }

        public static void OverrideDataCell<TRow>(this DataTableBuilder<TRow> tableBuilder, string columnName, Func<LayoutBuilder, Var<TRow>, Var<IVNode>> cellBuilder)
        {
            var prevBuilder = tableBuilder.CreateDataCell;
            tableBuilder.CreateDataCell = (LayoutBuilder b, Var<TRow> row, Var<string> column) =>
            {
                return b.If(
                    b.AreEqual(column, b.Const(columnName)),
                    b =>
                    {
                        return b.Call(cellBuilder, row);
                    },
                    b => b.Call(prevBuilder, row, column));
            };
        }

        public static void AddTrProps<TRow>(this DataTableBuilder<TRow> tableBuilder, Action<PropsBuilder<HtmlTr>, Var<TRow>> setProps)
        {
            var prevProps = tableBuilder.SetTrProps;
            tableBuilder.SetTrProps = (b, row) =>
            {
                prevProps(b, row);
                setProps(b, row);
            };
        }

        public static void AddTdProps<TRow>(this DataTableBuilder<TRow> tableBuilder, Action<PropsBuilder<HtmlTd>, Var<TRow>, Var<string>> setProps)
        {
            var prevProps = tableBuilder.SetTdProps;
            tableBuilder.SetTdProps = (b, row, column) =>
            {
                prevProps(b, row, column);
                setProps(b, row, column);
            };
        }
    }

    public class DataTable
    {
        public static List<string> GetColumns<TRow>()
        {
            var properties = typeof(TRow).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return properties.Select(x => x.Name).ToList();
        }
    }

    public static class MdsDefaultBuilder
    {
        public static DataTableBuilder<TRow> DataTable<TRow>()
        {
            return new DataTableBuilder<TRow>()
            {
                SetTableProps = b =>
                {
                    b.SetClass("bg-white border-collapse w-full overflow-hidden");
                },
                SetTheadProps = b =>
                {
                    b.SetClass("text-left text-sm text-gray-500 bg-white drop-shadow-sm");
                },
                SetThProps = (b, column) => b.SetClass("py-4 border-b border-gray-300 bg-white"),
                SetTdProps = (b, row, column) => b.SetClass("py-4 border-b border-gray-300")
            };
        }

        public static DataGridBuilder<TRow> DataGrid<TRow>()
        {
            return new DataGridBuilder<TRow>()
            {
                DataTableBuilder = MdsDefaultBuilder.DataTable<TRow>(),
                SetContainerProps = b =>
                {
                    b.SetClass("flex flex-col w-full bg-white gap-8");
                }
            };
        }
    }

    public static class DataTableControl
    {
        public static Var<IVNode> DataTable<TRow>(this LayoutBuilder b, DataTableBuilder<TRow> tableBuilder, Var<List<TRow>> rows, Var<List<string>> columns = null)
        {
            if (columns == null)
                columns = b.Const(MdsCommon.Controls.DataTable.GetColumns<TRow>());

            return b.HtmlTable(
                tableBuilder.SetTableProps,
                b.HtmlThead(
                    tableBuilder.SetTheadProps,
                    b.Map(
                        columns,
                        (b, column) => b.HtmlTh(
                            b => tableBuilder.SetThProps(b, column),
                            b.Call(tableBuilder.CreateHeaderCell, column)))),
                b.HtmlTbody(
                    tableBuilder.SetTbodyProps,
                    b.Map(
                        rows, (b, row) =>
                        b.HtmlTr(
                            b => tableBuilder.SetTrProps(b, row),
                            b.Map(
                                columns, (b, column) =>
                                b.HtmlTd(
                                    b => tableBuilder.SetTdProps(b, row, column),
                                    b.Call(tableBuilder.CreateDataCell, row, column)))))));
        }

        public static Var<IVNode> DataTable<TRow>(this LayoutBuilder b, DataTableBuilder<TRow> tableBuilder, Var<List<TRow>> rows, params string[] columns)
        {
            return b.DataTable(tableBuilder, rows, b.Const(columns.ToList()));
        }
    }

    //public static class DataTable
    //{
    //    public class Column
    //    {
    //        public string Name { get; set; }
    //        public string Caption { get; set; }
    //        public string Class { get; set; }
    //    }

    //    public class Style
    //    {
    //        public string Container { get; set; } = "bg-white p-4 rounded";
    //        public string Thead { get; set; } = "text-left text-sm text-gray-500 bg-white drop-shadow-sm";
    //        public string Th { get; set; } = "py-4 border-b border-gray-300 bg-white";
    //        public string Td { get; set; } = "border-b border-gray-300";
    //    }

    //    public class Props<TRow>
    //    {
    //        public List<Column> Columns { get; set; } = new();
    //        public List<TRow> Rows { get; set; } = new();
    //        public System.Func<TRow, Column, IVNode> CreateCell { get; set; }
    //        public Action<HtmlTr, TRow> CreateRow { get; set; } = (b, r) => { };
    //        public Style Style { get; set; } = new Style();
    //    }

    //    internal static Var<IVNode> Render<TRow>(LayoutBuilder b, Var<Props<TRow>> props, Action<PropsBuilder<HtmlDiv>> buildProps)
    //    {
    //        var style = b.Get(props, x => x.Style);

    //        //Function<HyperNode, object, Column> cellBuilder = b.Get<Function<HyperNode, object, Column>>(props, nameof(Props.CreateCell));
    //        var createRow = b.Get(props, x => x.CreateRow);

    //        var columns = b.Get(props, x => x.Columns);
    //        var rows = b.Get(props, x => x.Rows);

    //        var table = b.HtmlTable(
    //            b =>
    //            {
    //                b.SetClass("border-collapse w-full overflow-hidden");
    //            },
    //            b.HtmlThead(
    //                b =>
    //                {
    //                    b.SetClass(b.Get(style, x => x.Thead));
    //                },
    //                b.Map(
    //                    columns,
    //                    (b, col) =>
    //                    {
    //                        var caption = b.Get(col, x => x.Caption);
    //                        var captionRef = b.NewObj<Reference<string>>(x => x.Set(x => x.Value, caption));

    //                        b.If(b.IsEmpty(caption), b => b.Set(captionRef, x => x.Value, b.Get(col, x => x.Name)));

    //                        return b.HtmlTh(b =>
    //                        {
    //                            b.AddClass(b.Get(style, x => x.Th));
    //                            b.AddClass(b.Get(col, x => x.Class));
    //                        },
    //                        b.TextSpan(b.Get(captionRef, x => x.Value)));
    //                    })),
    //            b.HtmlTbody(
    //                b => { },
    //                b.Map(
    //                    rows,
    //                    (b, record) =>
    //                    {
    //                        return b.HtmlTr(b =>
    //                        {
    //                            b.Call(createRow, b.Props, record);
    //                            b.AddClass("group");
    //                        },
    //                        b.Map(
    //                            columns,
    //                            (b, column) =>
    //                            {
    //                                return b.HtmlTd(b =>
    //                                {
    //                                    b.AddClass(b.Get(style, x => x.Td));
    //                                },
    //                                b.Call(b.Get(props, x => x.CreateCell), record, column));
    //                            }));
    //                    })));

    //        return b.HtmlDiv(b =>
    //        {
    //            b.SetClass("w-full");
    //            b.AddClass(b.Get(style, x => x.Container));
    //            buildProps(b);
    //        },
    //        table);
    //    }
    //}

    //public static partial class Controls
    //{
    //    public static Var<IVNode> DataTable<TRow>(
    //        this LayoutBuilder b,
    //        Var<DataTable.Props<TRow>> props,
    //        Action<PropsBuilder<HtmlDiv>> buildProps)
    //    {
    //        return MdsCommon.Controls.DataTable.Render(b, props, buildProps);
    //    }

    //    public static Var<IVNode> VPadded4(this LayoutBuilder b, Var<IVNode> content)
    //    {
    //        return b.HtmlDiv(
    //            b =>
    //            {
    //                b.SetClass("py-4");
    //            },
    //            content);
    //    }


    //    public static void AddColumn<TRow>(
    //        this Modifier<DataTable.Props<TRow>> props,
    //        string name,
    //        System.Action<Modifier<DataTable.Column>> optional = null)
    //    {
    //        props.Update(b => b.Columns, b =>
    //        {
    //            b.Add(b =>
    //            {
    //                b.Set(x => x.Name, name);
    //                b.Update(optional);
    //            });
    //        });
    //    }

    //    public static void AddColumn<TRow>(
    //       this Modifier<DataTable.Props<TRow>> props,
    //       string name,
    //       string caption,
    //       System.Action<Modifier<DataTable.Column>> optional = null)
    //    {
    //        AddColumn(props, name, b =>
    //        {
    //            b.Set(x => x.Caption, caption);
    //            b.Update(optional);
    //        });
    //    }

    //    public static void SetRows<TRow>(
    //        this Modifier<DataTable.Props<TRow>> props,
    //        Var<List<TRow>> rows)
    //    {
    //        props.Set(x => x.Rows, rows);
    //    }

    //    public static void SetRenderCell<TRow>(
    //        this Modifier<DataTable.Props<TRow>> props,
    //        Var<System.Func<TRow, DataTable.Column, IVNode>> cellBuilder)
    //    {
    //        props.Set(x => x.CreateCell, cellBuilder);
    //    }

    //    public static Var<System.Func<TRow, DataTable.Column, IVNode>> RenderCell<TRow>(this LayoutBuilder b, System.Func<LayoutBuilder, Var<TRow>, Var<DataTable.Column>, Var<IVNode>> renderer)
    //    {
    //        return b.Def(renderer);
    //    }
    //}
}

