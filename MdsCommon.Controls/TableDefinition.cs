using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Data;

namespace MdsCommon.HtmlControls
{
    public class TableData<TRow>
    {
        public List<TRow> Rows { get; set; } = new();
        public List<string> Columns { get; set; } = new();
    }

    public class TableRowData<TRow>
    {
        public TRow Row { get; set; }
        public List<string> Columns { get; set; }
    }

    public class CellData<TRow>
    {
        public TRow Row { get; set; }
        public string Column { get; set; }
    }

    public class TableDefinition<TRow> : IControlDefinition<TableData<TRow>>
    {
        public ControlDefinition<TableData<TRow>> Table { get; set; }
        public ControlDefinition<List<string>> HeaderRow { get; set; }
        public ControlDefinition<string> HeaderCell { get; set; }
        public ControlDefinition<TableRowData<TRow>> TableRow { get; set; }
        public ControlDefinition<CellData<TRow>> TableCell { get; set; }
        public ControlDefinition<CellData<TRow>> CellContent { get; set; }

        public Func<LayoutBuilder, Var<TableData<TRow>>, Var<IVNode>> GetRenderer()
        {
            return Table.GetRenderer();
        }
    }

    public static partial class Control
    {
        public static TableDefinition<TRow> DefaultTable<TRow>()
        {
            TableDefinition<TRow> builder = new();
            builder.Table = ControlDefinition.New<TableData<TRow>>(
                "table",
                (b, data, props) => { },
                (b, data) =>
                {
                    var rows = b.NewCollection<IVNode>();
                    b.Push(rows, b.Render(builder.HeaderRow, b.Get(data, x => x.Columns)));

                    b.Foreach(
                        b.Get(data, x => x.Rows),
                        (b, row) =>
                        {
                            var tableRowData = b.NewObj<TableRowData<TRow>>();
                            b.Set(tableRowData, x => x.Row, row);
                            b.Set(tableRowData, x => x.Columns, b.Get(data, x => x.Columns));
                            b.Push(rows, b.Render(builder.TableRow, tableRowData));
                        });

                    return rows;
                });

            builder.HeaderRow = ControlDefinition.New<List<string>>(
                "tr",
                (b, data, props) => { },
                (b, data) => b.Map(
                    data,
                    (b, column) => b.Render(builder.HeaderCell, column)));

            builder.HeaderCell = ControlDefinition.New<string>(
                "td",
                (b, data, props) => { },
                (b, column) => b.T(column));

            builder.TableRow = ControlDefinition.New<TableRowData<TRow>>(
                "tr",
                (b, data, props) => { },
                (b, data) => b.Map(
                    b.Get(data, x => x.Columns),
                    (b, column) =>
                    {
                        var cellData = b.NewObj<CellData<TRow>>();
                        b.Set(cellData, x => x.Row, b.Get(data, x => x.Row));
                        b.Set(cellData, x => x.Column, column);
                        return b.Render(builder.TableCell, cellData);
                    }));

            builder.TableCell = ControlDefinition.New<CellData<TRow>>(
                "td",
                (b, data, props) => { },
                (b, cellData) => b.Render(builder.CellContent, cellData));

            builder.CellContent = ControlDefinition.New<CellData<TRow>>(
                "span",
                (b, data, props) => { },
                (b, data) => b.T("-"));

            return builder;
        }

        public static Var<IVNode> Table<TRow>(
            this LayoutBuilder b,
            Action<ControlBuilder<TableDefinition<TRow>, TableData<TRow>>, Var<TableData<TRow>>> customize)
        {
            return b.FromDefinition(DefaultTable<TRow>, customize);
        }

    }
}

