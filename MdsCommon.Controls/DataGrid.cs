﻿using MdsCommon.HtmlControls;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;

namespace MdsCommon.Controls
{
    public class DataGridData
    {

    }

    public class DataGridBuilder<TRow> : CompoundBuilder<DataGridData>
        where TRow : new()
    {
        public TableBuilder<TRow> Table { get; set; }
        public ControlBuilder Container { get; set; }
        public ControlBuilder Toolbar { get; set; }

        protected override void Setup(BlockBuilder b)
        {
            this.Table = new TableBuilder<TRow>();
            this.Table.Init(b);
            this.Container = ControlBuilder.New("div", b =>
            {
                b.SetClass("flex flex-col w-full");
            },
            b => Toolbar.GetRoot(b),
            b => Table.GetRoot(b));

            this.Toolbar = ControlBuilder.New("div",
                b =>
                {
                    b.SetClass("flex flex-row gap-2");
                });
        }

        public override Var<IVNode> GetRoot(BlockBuilder b)
        {
            return this.Container.GetRoot(b);
        }
    }

    public static partial class DataGridExtensions
    {
        public static Var<IVNode> DataGrid<TRow>(this BlockBuilder b, Action<BlockBuilder, DataGridBuilder<TRow>> customize)
            where TRow : new()
        {
            return b.BuildControl<DataGridBuilder<TRow>, DataGridData>(customize);
        }
    }

    public static partial class DataGrid
    {
        public class Props
        {
            //public HyperNode Toolbar { get; set; }
            //public HyperNode DataTable { get; set; }// TODO: The data table needs to receive filtering data
            //public bool ShowSearch { get; set; }
        }

        internal static Var<HyperNode> Render<TRow>(BlockBuilder b,
            List<Func<BlockBuilder, Var<HyperNode>>> buildToolbar,
            Action<Modifier<DataTable.Props<TRow>>> buildDataTable,
            Action<BlockBuilder, Var<ActionBar.Props<TRow>>, Var<TRow>> buildActions = null)
        {
            string flexClass = "flex flex-col w-full bg-white rounded";

            if (buildActions != null)
            {
                var dataTableProps = b.NewObj<DataTable.Props<TRow>>(buildDataTable);
                var originalRenderer = b.Get(dataTableProps, x => x.CreateCell);
                var renderCell = b.Def<TRow, DataTable.Column, HyperNode>((b, row, col) =>
                {
                    var currentRenderer = b.Get(dataTableProps, x => x.CreateCell);
                    return b.If<HyperNode>(
                        b.AreEqual(b.Get(col, x => x.Name), b.Const("__action__")),
                        b =>
                        {
                            var props = b.NewObj(new ActionBar.Props<TRow>());
                            buildActions(b, props, row);

                            var hoverDiv = b.Div("invisible group-hover:visible");
                            b.Add(hoverDiv, b.ActionBar(props, row));
                            return hoverDiv;
                        },
                        b =>
                        {
                            return b.Call(originalRenderer, row, col);
                        });
                });


                b.Modify(dataTableProps, b =>
                {
                    b.AddColumn<TRow>("__action__", " ");
                    b.SetRenderCell<TRow>(renderCell);
                });

                return b.Div(flexClass,
                    b => b.Toolbar(buildToolbar.ToArray()),
                    b => b.DataTable(dataTableProps));
            }
            else
            {
                return b.Div(flexClass,
                    b => b.Toolbar(buildToolbar.ToArray()),
                    b => b.FromDefault(DataTable.Render, buildDataTable));
            }
        }
    }

    public static partial class Controls
    {
        //internal static Var<HyperNode> DataGrid(
        //    this BlockBuilder b,
        //    Var<HyperNode> toolbar,
        //    Var<HyperNode> dataTable,
        //    Var<bool> showSearch)
        //{
        //    var props = b.NewObj<DataGrid.Props>();
        //    b.Set(props, x => x.Toolbar, toolbar);
        //    b.Set(props, x => x.ShowSearch, showSearch);
        //    b.Set(props, x => x.DataTable, dataTable);

        //    return b.Call(Hyperapp.DataGrid.Render, props);
        //}

        public static Var<HyperNode> DataGrid<TRow>(
            this BlockBuilder b,
            List<Func<BlockBuilder, Var<HyperNode>>> buildToolbar,
            Action<Modifier<DataTable.Props<TRow>>> buildDataTable,
            System.Action<BlockBuilder, Var<ActionBar.Props<TRow>>, Var<TRow>> buildActions = null)
        {
            return MdsCommon.Controls.DataGrid.Render<TRow>(b, buildToolbar, buildDataTable, buildActions);
            ////var toolbarProps = b.NewObj(buildToolbar);
            //var dataTableProps = b.NewObj(buildDataTable);

            //if (buildActions != null)
            //{
            //    b.Modify(dataTableProps, b =>
            //    {
            //        b.AddColumn<TRow>("__action__", " ");

            //        var originalRenderer = b.Get(x => x.CreateCell);
            //        b.SetRenderCell<TRow>((b, row, col) =>
            //        {
            //            var currentRenderer = b.Get(dataTableProps, x => x.CreateCell);
            //            return b.If<HyperNode>(
            //                b.AreEqual(b.Get(col, x => x.Name), b.Const("__action__")),
            //                b =>
            //                {
            //                    var props = b.NewObj(new ActionBar.Props<TRow>());
            //                    buildActions(b, props, row);

            //                    var hoverDiv = b.Div("invisible group-hover:visible");
            //                    b.Add(hoverDiv, b.ActionBar(props, row));
            //                    return hoverDiv;
            //                },
            //                b =>
            //                {
            //                    return b.Call(originalRenderer, row, col);
            //                });
            //        });
            //    });
            //}

            //var toolBar = b.Toolbar(toolbarProps);
            //var dataTable = b.DataTable(dataTableProps);

            //return b.DataGrid(toolBar, dataTable, b.Const(false));
        }
    }
}

