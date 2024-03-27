//using MdsCommon.Controls;
//using Metapsi;
//using Metapsi.Hyperapp;
//using Metapsi.Syntax;
//using Metapsi.Ui;
//using System;
//using System.Collections.Generic;

//namespace MdsCommon.HtmlControls
//{
//    public static class DataTableExtensions
//    {
//        public static Var<IVNode> AutoFormat<T>(this LayoutBuilder b, Var<T> value, Var<string> type)
//        {
//            return b.Switch(type,
//                b => b.T(b.AsString(value)),
//                ("List`1", (LayoutBuilder b) => b.T(b.AsString(b.CollectionLength(value.As<List<object>>())))),
//                ("DateTime", (LayoutBuilder b) => b.T(b.FormatDatetime(value.As<DateTime>()))),
//                ("Boolean", (LayoutBuilder b) =>
//                b.H(
//                    "input",
//                    (b, props) =>
//                    {
//                        b.SetDynamic(props, Html.type, b.Const("checkbox"));
//                        b.SetDynamic(props, Html.@checked, value.As<bool>());
//                        b.SetDynamic(props, Html.disabled, b.Const(true));
//                    })));
//        }

       

//        public static void GuessColumns<TRow>(
//            this ControlBuilder<TableDefinition<TRow>, TableData<TRow>> b, 
//            List<string> except = null)
//        {
//            if (except == null) except = new List<string>();
            
//            var properties = typeof(TRow).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
//            foreach (var property in properties)
//            {
//                if (!except.Contains(property.Name))
//                {
//                    b.SetData((b, data) =>
//                    {
//                        var columnData = b.NewObj<ColumnData>();
//                        b.Set(columnData, x => x.Name, property.Name);
//                        b.Set(columnData, x => x.TypeName, property.PropertyType.Name);
//                        b.Push(b.Get(data, x => x.Columns), columnData);
//                    });
//                }
//            }

//            b.Control.HeaderCell.SetChild((b, column) =>
//            {
//                var split = b.SplitOnUppercase(b.Get(column, x=>x.Name));
//                var columnName = b.JoinStrings(b.Const(" "), split);

//                return b.H("th", b.EmptyProps(), b.T(columnName));
//            });

//            b.Control.CellContent.SetChild((b, cellData) =>
//            {
//                return b.Call(
//                    AutoFormat, 
//                    b.GetProperty<object>(b.Get(cellData, x => x.Row), b.Get(cellData, x => x.Column.Name)),
//                    b.Get(cellData, x => x.Column.TypeName));
//            });
//        }

//        public static void FillFrom<TRow>(
//            this ControlBuilder<TableDefinition<TRow>, TableData<TRow>> b,
//            Var<List<TRow>> rows,
//            List<string> exceptColumns = null)
//        {
//            b.GuessColumns(exceptColumns);
//            b.SetData((b, data) => b.Set(data, x => x.Rows, rows));
//        }

//        public static void OverrideColumnCell<TRow>(
//            this ControlBuilder<TableDefinition<TRow>, TableData<TRow>> b,
//            string columName,
//            Func<LayoutBuilder, Var<CellData<TRow>>, Var<IVNode>> render)
//        {
//            List<Func<LayoutBuilder, Var<CellData<TRow>>, Var<List<IVNode>>>> originalBuilders = new(b.Control.TableCell.ChildrenBuilders);

//            b.Control.TableCell.ChildrenBuilders.Clear();
//            b.Control.TableCell.ChildrenBuilders.Add((LayoutBuilder b, Var<CellData<TRow>> data) =>
//            {
//                var allChildren = b.NewCollection<IVNode>();

//                b.If(
//                    b.AreEqual(b.Get(data, x => x.Column.Name), b.Const(columName)),
//                    b =>
//                    {
//                        b.Push(allChildren, b.Call(render, data));
//                    },
//                    b =>
//                    {
//                        foreach (var original in originalBuilders)
//                        {
//                            var remainingChildren = b.Call(original, data);
//                            b.Foreach(remainingChildren, (b, child) =>
//                            {
//                                b.Push(allChildren, child);
//                            });
//                        }
//                    });

//                return allChildren;
//            });
//        }

//        private const string HeroiconsCog6Tooth = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"w-6 h-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.324.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 011.37.49l1.296 2.247a1.125 1.125 0 01-.26 1.431l-1.003.827c-.293.24-.438.613-.431.992a6.759 6.759 0 010 .255c-.007.378.138.75.43.99l1.005.828c.424.35.534.954.26 1.43l-1.298 2.247a1.125 1.125 0 01-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.57 6.57 0 01-.22.128c-.331.183-.581.495-.644.869l-.213 1.28c-.09.543-.56.941-1.11.941h-2.594c-.55 0-1.02-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 01-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 01-1.369-.49l-1.297-2.247a1.125 1.125 0 01.26-1.431l1.004-.827c.292-.24.437-.613.43-.992a6.932 6.932 0 010-.255c.007-.378-.138-.75-.43-.99l-1.004-.828a1.125 1.125 0 01-.26-1.43l1.297-2.247a1.125 1.125 0 011.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.087.22-.128.332-.183.582-.495.644-.869l.214-1.281z\" />\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M15 12a3 3 0 11-6 0 3 3 0 016 0z\" />\r\n</svg>\r\n";

//        public static void AddHoverRowAction<TState, TRow>(
//            this ControlBuilder<DataGridDefinition<TRow>, DataGridData<TRow>> b,
//            Func<SyntaxBuilder, Var<TState>, Var<TRow>, Var<TState>> onClick,
//            string iconSvg = HeroiconsCog6Tooth,
//            Action<PropsBuilder, Var<TRow>, Var<DynamicObject>> extraProps = null,
//            Func<SyntaxBuilder, Var<TRow>, Var<bool>> visible = null)
//        {
//            if (visible == null)
//            {
//                visible = (b, row) => b.Const(true);
//            }

//            b.Control.ToolbarRowAction.AddChild((b, row) =>
//            {
//                return b.H(
//                    "div",
//                    (b, props) =>
//                    {
//                        b.If(
//                            b.Call(visible, row),
//                            b =>
//                            {
//                                b.SetClass(props, "flex rounded bg-gray-200 p-1 cursor-pointer justify-center items-center opacity-50 hover:opacity-100");
//                                b.SetDynamic(props, Html.innerHTML, b.Const(iconSvg));
//                                b.OnClickAction(props, b.MakeAction((SyntaxBuilder b, Var<TState> state) =>
//                                {
//                                    return b.Call(onClick, state, row);
//                                }));

//                                if (extraProps != null)
//                                {
//                                    extraProps(b, row, props);
//                                }
//                                else
//                                {
//                                    b.AddClass(props, "w-10 h-10");
//                                }
//                            });
//                    });
//            });
//        }

//        public static void AddHoverRowAction<TState, TRow>(
//            this ControlBuilder<DataGridDefinition<TRow>, DataGridData<TRow>> b,
//            Func<SyntaxBuilder, Var<HyperType.Action<TState, TRow>>> onClick,
//            string iconSvg = HeroiconsCog6Tooth,
//            Action<PropsBuilder, Var<TRow>, Var<DynamicObject>> extraProps = null)
//        {
//            b.Control.ToolbarRowAction.AddChild((b, row) =>
//            {
//                b.AddModuleStylesheet();

//                return b.H(
//                    "div",
//                    (b, props) =>
//                    {
//                        b.SetClass(props, "flex rounded bg-gray-200 w-10 h-10 p-1 cursor-pointer justify-center items-center opacity-50 hover:opacity-100");
//                        b.SetDynamic(props, Html.innerHTML, b.Const(iconSvg));
//                        //b.OnClickAction<TState>(props, b.Call(onClick));

//                        b.OnClickAction<TState>(props, b.MakeActionDescriptor(b.Call(onClick), row));

//                        if (extraProps != null)
//                        {
//                            extraProps(b, row, props);
//                        }
//                    });
//            });
//        }

//        public static void SetCommonStyle<TRow>(this ControlBuilder<TableDefinition<TRow>, TableData<TRow>> b)
//            where TRow : new()
//        {
//            var builder = b.Control;

//            builder.Table.EditProps((b, props) =>
//            {
//                b.AddClass(props, b.Const("border-collapse w-full overflow-hidden"));
//            });

//            builder.TableCell.EditProps((b, props) =>
//            {
//                b.AddClass(props, b.Const("border-b border-gray-300 py-4"));
//            });

//            builder.HeaderRow.EditProps((b, props) =>
//            {
//                b.AddClass(props, b.Const("text-left text-sm text-gray-500 bg-white drop-shadow-sm"));
//            });

//            builder.HeaderCell.EditProps((b, props) =>
//            {
//                b.AddClass(props, b.Const("py-4 border-b border-gray-300 bg-white"));
//            });
//        }


//        public static Var<IVNode> Table(this LayoutBuilder b, Action<PropsBuilder, Var<DynamicObject>> buildProps)
//        {
//            return b.H("table", buildProps);
//        }
//    }
//}

