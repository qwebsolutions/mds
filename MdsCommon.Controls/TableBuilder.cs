using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Collections.Generic;

namespace MdsCommon.HtmlControls
{
    public class TableData<TRow>
    {
        public List<TRow> Rows { get; set; } = new();
        public List<string> Columns { get; set; } = new();
    }

    public class TableRowData<TRow>
        where TRow : new()
    {
        public TRow Row { get; set; } = new();
        public string Column { get; set; }
    }

    public class CellData<TRow>
    {
        public TRow Row { get; set; }
        public string Column { get; set; }
    }

    public class TableBuilder<TRow>
    {
        public HtmlControlBuilder<TableData<TRow>> Table { get; set; }
        public HtmlControlBuilder<List<string>> HeaderRow { get; set; }
        public HtmlControlBuilder<string> HeaderCell { get; set; }
        public HtmlControlBuilder<TRow> TableRow { get; set; }
        public HtmlControlBuilder<CellData<TRow>> TableCell { get; set; }
    }

    public static class TableBuilderExtensions
    {
        //public static TableBuilder<TRow> SetupTable<TRow>(TableBuilder<TRow> builder)
        //{
        //    builder.Table = HtmlControlBuilder.New<TableData<TRow>>(
        //        "table",
        //        b => { },
        //        (b, data) =>
        //        {
        //            var rows = b.NewCollection<IVNode>();
        //            b.Push(rows, builder.HeaderRow.GetRoot(b));

        //            b.Foreach(
        //                b.Get(data, x => x.Rows),
        //                (b, row) =>
        //                {
        //                    b.Push(rows, TableRow.GetRoot(b));
        //                });

        //            return rows;
        //        });

        //    this.HeaderRow = HtmlControlBuilder.New<List<string>>(
        //        "tr",
        //        b => { },
        //        (b, data) => b.Map(
        //            data,
        //            (b, column) => this.HeaderCell.GetRoot(b, column)));

        //    this.HeaderCell = HtmlControlBuilder.New<string>(
        //        "td",
        //        b => { },
        //        (b, column) => b.T(column));

        //    this.TableRow = HtmlControlBuilder.New<TRow>(
        //        "tr",
        //        b => { },
        //        (b, row) => b.Map(
        //            b.Get(row, x => x.Columns),
        //            (b, column) =>
        //            {
        //                var cellData = b.NewObj<CellData<TRow>>();
        //                b.Set(cellData, x => x.Row, row);
        //                b.Set(cellData, x => x.Column, column);
        //                this.TableCell
        //                return this.TableCell.GetRoot(b, cellData);
        //            }));

        //    this.TableCell = HtmlControlBuilder.New<CellData<TRow>>(
        //        "td",
        //        b => { },
        //        (b, cellData) =>
        //        {
        //            return b.T("-");
        //        });
        //}
    }
}

