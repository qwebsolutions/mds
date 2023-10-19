﻿using MdsCommon.Controls;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MdsCommon.HtmlControls
{
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

            b.Control.HeaderCell.SetChild((b, column) =>
            {
                return b.H("th", b.EmptyProps(), b.T(column));
            });

            b.Control.CellContent.SetChild((b, cellData) =>
            {
                return b.T(b.AsString(b.GetProperty<object>(b.Get(cellData, x => x.Row), b.Get(cellData, x => x.Column))));
            });
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
            Func<LayoutBuilder, Var<CellData<TRow>>, Var<IVNode>> render)
        {
            var cellContentRenderer = b.Control.CellContent.GetRenderer();
            b.Control.TableCell.SetChild((b, data) =>
            {
                return b.If(
                    b.AreEqual(b.Get(data, x => x.Column), columName),
                    b =>
                    {
                        return b.Call(render, data);
                    },
                    b => b.Call(cellContentRenderer, data));
            });
        }

        public static void OverrideColumnCell<TRow>(
            this ControlBuilder<TableDefinition<TRow>, TableData<TRow>> b,
            string columName,
            Func<LayoutBuilder, Var<CellData<TRow>>, Var<IVNode>> render)
        {
            b.OverrideColumnCell(b.Const(columName), render);
        }

        private const string HeroiconsCog6Tooth = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"w-6 h-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.324.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 011.37.49l1.296 2.247a1.125 1.125 0 01-.26 1.431l-1.003.827c-.293.24-.438.613-.431.992a6.759 6.759 0 010 .255c-.007.378.138.75.43.99l1.005.828c.424.35.534.954.26 1.43l-1.298 2.247a1.125 1.125 0 01-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.57 6.57 0 01-.22.128c-.331.183-.581.495-.644.869l-.213 1.28c-.09.543-.56.941-1.11.941h-2.594c-.55 0-1.02-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 01-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 01-1.369-.49l-1.297-2.247a1.125 1.125 0 01.26-1.431l1.004-.827c.292-.24.437-.613.43-.992a6.932 6.932 0 010-.255c.007-.378-.138-.75-.43-.99l-1.004-.828a1.125 1.125 0 01-.26-1.43l1.297-2.247a1.125 1.125 0 011.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.087.22-.128.332-.183.582-.495.644-.869l.214-1.281z\" />\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M15 12a3 3 0 11-6 0 3 3 0 016 0z\" />\r\n</svg>\r\n";

        public static void AddHoverRowAction<TState, TRow>(
            this ControlBuilder<DataGridDefinition<TRow>, DataGridData<TRow>> b,
            Func<BlockBuilder, Var<TState>, Var<TRow>, Var<TState>> onClick,
            string iconSvg = HeroiconsCog6Tooth,
            Action<PropsBuilder, Var<TRow>, Var<DynamicObject>> extraProps = null)
        {
            b.Control.ToolbarRowAction.AddChild((b, row) =>
            {
                return b.H(
                    "div",
                    (b, props) =>
                    {
                        b.SetClass(props, "flex rounded bg-gray-200 w-10 h-10 p-1 cursor-pointer justify-center items-center opacity-50 hover:opacity-100");
                        b.SetDynamic(props, Html.innerHTML, b.Const(iconSvg));
                        b.OnClickAction(props, b.MakeAction((BlockBuilder b, Var<TState> state) =>
                        {
                            return b.Call(onClick, state, row);
                        }));

                        if (extraProps != null)
                        {
                            extraProps(b, row, props);
                        }
                    });
            });
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

