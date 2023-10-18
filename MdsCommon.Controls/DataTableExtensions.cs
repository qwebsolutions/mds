using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
using System;
using System.Collections.Generic;

namespace MdsCommon.HtmlControls
{
    //public class TableBuilder<TRow> : CompoundBuilder<TableData<TRow>>
    //    where TRow: new()
    //{

    //    public override Var<IVNode> GetRoot(LayoutBuilder b, Var<TableData<TRow>> data)
    //    {
    //        return Table.GetRoot(b);
    //    }

    //    protected override void Setup(LayoutBuilder b)
    //    {
    //    }
    //}


    public static class DataTableExtensions
    {

        //public static Var<IVNode> DataTable<TRow>(this LayoutBuilder b, Action<TableBuilder<TRow>, Var<TableData<TRow>>> customize = null)
        //    where TRow : new()
        //{
        //    return b.BuildControl<TableBuilder<TRow>, TableData<TRow>>(customize);
        //}

        //public static void GuessColumns<TRow>(this TableBuilder<TRow> tableBuilder, List<string> except = null)
        //    where TRow : new()
        //{
        //    if (except == null) except = new List<string>();

        //    tableBuilder.FillData((b, data) =>
        //    {
        //        var properties = typeof(TRow).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        //        foreach (var property in properties)
        //        {
        //            if (!except.Contains(property.Name))
        //            {
        //                b.Push(b.Get(data, x => x.Columns), b.Const(property.Name));
        //            }
        //        }
        //    });

        //    tableBuilder.HeaderCell.BuildControl = (b, column, props) =>
        //    {
        //        return b.H("th", props, b.T(column));
        //    };
        //    tableBuilder.TableCell.BuildControl = (b, cellData, props) =>
        //    {
        //        return b.H("td", props, b.T(b.AsString(b.GetProperty<object>(b.Get(cellData, x => x.Row), b.Get(cellData, x => x.Column)))));
        //    };
        //}

        public static void WrapBuildControl<TData>(
            this HtmlControlBuilder<TData> builder,
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
            //builder.Table.SetProps(b =>
            //{
            //    b.AddClass(b.Const("border-collapse w-full overflow-hidden"));
            //});

            //builder.TableCell.SetProps(b =>
            //{
            //    b.AddClass(b.Const("border-b border-gray-300 py-4"));
            //});

            //builder.HeaderRow.SetProps(b =>
            //{
            //    b.AddClass(b.Const("text-left text-sm text-gray-500 bg-white drop-shadow-sm"));
            //});

            //builder.HeaderCell.SetProps(b =>
            //{
            //    b.AddClass(b.Const("py-4 border-b border-gray-300 bg-white"));
            //});
        }


        public static Var<IVNode> Table(this LayoutBuilder b, Action<PropsBuilder, Var<DynamicObject>> buildProps)
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

