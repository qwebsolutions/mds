using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public static void GuessColumns<TRow>(
            this ControlBuilder<TableDefinition<TRow>, TableData<TRow>> b, 
            List<string> except = null)
        {
            if (except == null) except = new List<string>();
            
            var properties = typeof(TRow).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (!except.Contains(property.Name))
                {
                    b.Push(b.Get(b.Data, x => x.Columns), b.Const(property.Name));
                }
            }

            b.Control.HeaderCell.BuildControl = (b, column, props) =>
            {
                return b.H("th", props, b.T(column));
            };
            b.Control.CellContent.BuildControl = (b, cellData, props) =>
            {
                return b.T(b.AsString(b.GetProperty<object>(b.Get(cellData, x => x.Row), b.Get(cellData, x => x.Column))));
            };
        }

        public static void FillFrom<TRow>(
            this ControlBuilder<TableDefinition<TRow>, TableData<TRow>> b,
            Var<List<TRow>> rows,
            List<string> exceptColumns = null)
        {
            b.GuessColumns(exceptColumns);
            b.Set(b.Data, x => x.Rows, rows);
        }

        public static void OverrideColumnCell<TRow>(
            this ControlBuilder<TableDefinition<TRow>, TableData<TRow>> b,
            Var<string> columName,
            Func<LayoutBuilder, Var<CellData<TRow>>, Var<DynamicObject>, Var<IVNode>> render)
        {
            b.Control.CellContent.WrapBuildControl((b, cellData, props, baseBuilder) =>
            {
                b.Log("column name", b.Get(cellData, x => x.Column));

                return b.If(
                    b.AreEqual(b.Get(cellData, x => x.Column), columName),
                    b =>
                    {
                        return b.Call(render, cellData, props);
                    },
                    b => baseBuilder(b, cellData, props));
            });
        }

        public static void OverrideColumnCell<TRow>(
            this ControlBuilder<TableDefinition<TRow>, TableData<TRow>> b,
            string columName,
            Func<LayoutBuilder, Var<CellData<TRow>>, Var<DynamicObject>, Var<IVNode>> render)
        {
            b.OverrideColumnCell(b.Const(columName), render);
        }

        public static void WrapBuildControl<TData>(
            this ControlDefinition<TData> builder,
            Func<LayoutBuilder, Var<TData>, Var<DynamicObject>, Func<LayoutBuilder, Var<TData>, Var<DynamicObject>, Var<IVNode>>, Var<IVNode>> wrapper)
            where TData : new()
        {
            var baseBuilder = builder.BuildControl;
            builder.BuildControl = (LayoutBuilder b, Var<TData> data, Var<DynamicObject> props) =>
            {
                return wrapper(b, data, props, baseBuilder);
            };
        }

        public static void SetCommonStyle<TRow>(this ControlBuilder<TableDefinition<TRow>, TableData<TRow>> b)
            where TRow : new()
        {
            var builder = b.Control;

            builder.Table.EditProps((b, props) =>
            {
                b.AddClass(props, b.Const("border-collapse w-full overflow-hidden"));
            });

            builder.TableCell.EditProps((b, props) =>
            {
                b.AddClass(props, b.Const("border-b border-gray-300 py-4"));
            });

            builder.HeaderRow.EditProps((b, props) =>
            {
                b.AddClass(props, b.Const("text-left text-sm text-gray-500 bg-white drop-shadow-sm"));
            });

            builder.HeaderCell.EditProps((b, props) =>
            {
                b.AddClass(props, b.Const("py-4 border-b border-gray-300 bg-white"));
            });
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

