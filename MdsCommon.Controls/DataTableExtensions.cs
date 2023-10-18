using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
using System;
using System.Collections.Generic;

namespace MdsCommon.HtmlControls
{
    public class TableData<TRow>
    {
        public List<TRow> Rows { get; set; } = new();
        public List<string> Columns { get; set; } = new();
    }

    public class CellData<TRow>
    {
        public TRow Row { get; set; }
        public string Column { get; set; }
    }

    public class TableBuilder<TRow> : CompoundBuilder<TableData<TRow>>
        where TRow: new()
    {
        public ControlBuilder<TableData<TRow>> Table { get; set; }
        public ControlBuilder<List<string>> HeaderRow { get; set; }
        public ControlBuilder<string> HeaderCell { get; set; }
        public ControlBuilder<TRow> TableRow { get; set; }
        public ControlBuilder<CellData<TRow>> TableCell { get; set; }

        public override Var<IVNode> GetRoot(LayoutBuilder b)
        {
            return Table.GetRoot(b);
        }

        protected override void Setup(LayoutBuilder b)
        {
            this.Table = ControlBuilder.New<TableData<TRow>>(
                "table", 
                b => { },
                (b, data)=>
                {
                    var rows = b.NewCollection<IVNode>();
                    b.Push(rows, HeaderRow.GetRoot(b));

                    b.Foreach(
                        b.Get(data, x => x.Rows),
                        (b, row) =>
                        {
                            b.Push(rows, TableRow.Build(b, row));
                        });

                    return rows;
                });

            this.HeaderRow = ControlBuilder.New(
                "tr",
                b => { },
                (b, data) => b.Map(
                    b.Get(data, x => x.Columns),
                    (b, column) => this.HeaderCell.Build(b, column)));

            this.HeaderCell = ControlBuilder.New<string>(
                "td",
                b => { },
                (b, column) => b.T(column));

            this.TableRow = ControlBuilder.New<TRow>(
                "tr",
                b => { },
                (b, row) => b.Map(
                    b.Get(Data, x => x.Columns),
                    (b, column) =>
                    {
                        var cellData = b.NewObj<CellData<TRow>>();
                        b.Set(cellData, x => x.Row, row);
                        b.Set(cellData, x => x.Column, column);
                        return this.TableCell.Build(b, cellData);
                    }));

            this.TableCell = ControlBuilder.New<CellData<TRow>>(
                "td",
                b => { },
                (b, cellData) =>
                {
                    return b.T("-");
                });
        }
    }


    public static class DataTableExtensions
    {

        public static Var<IVNode> DataTable<TRow>(this LayoutBuilder b, Action<TableBuilder<TRow>, Var<TableData<TRow>>> customize = null)
            where TRow : new()
        {
            return b.BuildControl<TableBuilder<TRow>, TableData<TRow>>(customize);
        }

        public static void GuessColumns<TRow>(this TableBuilder<TRow> tableBuilder, List<string> except = null)
            where TRow : new()
        {
            if (except == null) except = new List<string>();

            tableBuilder.FillData = (b, data) =>
            {
                var properties = typeof(TRow).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var property in properties)
                {
                    if (!except.Contains(property.Name))
                    {
                        b.Push(b.Get(data, x => x.Columns), b.Const(property.Name));
                    }
                }
            };

            tableBuilder.HeaderCell.BuildControl = (b, column, props) =>
            {
                return b.H("th", props, b.T(column));
            };
            tableBuilder.TableCell.BuildControl = (b, cellData, props) =>
            {
                return b.H("td", props, b.T(b.AsString(b.GetProperty<object>(b.Get(cellData, x => x.Row), b.Get(cellData, x => x.Column)))));
            };
        }

        public static void WrapBuildControl<TData>(
            this ControlBuilder<TData> builder,
            Func<LayoutBuilder, Var<TData>, Var<DynamicObject>, Func<LayoutBuilder, Var<TData>, Var<DynamicObject>, Var<IVNode>>, Var<IVNode>> wrapper)
            where TData : new()
        {
            var baseBuilder = builder.BuildControl;
            builder.BuildControl = (LayoutBuilder b, Var<TData> data, Var<DynamicObject> props) =>
            {
                return wrapper(b, data, props, baseBuilder);
            };
        }

        public static void SetCommonStyle<TRow>(this BlockBuilder b, TableBuilder<TRow> builder)
            where TRow : new()
        {
            builder.Table.SetProps(b =>
            {
                b.AddClass(b.Const("border-collapse w-full overflow-hidden"));
            });

            builder.TableCell.SetProps(b =>
            {
                b.AddClass(b.Const("border-b border-gray-300 py-4"));
            });

            builder.HeaderRow.SetProps(b =>
            {
                b.AddClass(b.Const("text-left text-sm text-gray-500 bg-white drop-shadow-sm"));
            });

            builder.HeaderCell.SetProps(b =>
            {
                b.AddClass(b.Const("py-4 border-b border-gray-300 bg-white"));
            });
        }

        public static void SetClass(this PropsBuilder b, Var<string> @class)
        {
            b.SetDynamic(b.Props, Html.@class, @class);
        }

        public static void SetClass(this PropsBuilder b, string @class)
        {
            b.SetClass(b.Const(@class));
        }

        public static void AddClass(this PropsBuilder b, Var<string> @class)
        {
            var currentClass = b.GetDynamic(b.Props, Html.@class);
            b.SetDynamic(b.Props, Html.@class, b.Concat(currentClass, b.Const(" "), @class));
        }

        public static void AddClass(this PropsBuilder b, string @class)
        {
            b.AddClass(b.Const(@class));
        }

        public static Var<IVNode> Table(this LayoutBuilder b, Action<PropsBuilder> buildProps)
        {
            return b.H("table", buildProps);
        }

        //private static PropsBuilder CreatePropsBuilder(this BlockBuilder b)
        //{
        //    return new PropsBuilder(b.ModuleBuilder, b.Block);
        //}

        //private static void BuildProps(this BlockBuilder b, Action<PropsBuilder> buildProps)
        //{
        //    buildProps(b.CreatePropsBuilder());
        //}
    }
}

