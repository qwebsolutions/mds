using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Ui;
using Metapsi.Hyperapp;
using System.Text;
using System;
using Metapsi;
using MdsCommon.Controls;
using static MdsLocal.SyncHistory;

namespace MdsLocal
{
    public class RenderSyncHistory : HyperPage<SyncHistory.DataModel>
    {
        public override Var<IVNode> OnRender(LayoutBuilder b, Var<SyncHistory.DataModel> dataModel)
        {
            b.AddModuleStylesheet();
            var headerProps = b.NewObj<Header.Props>();
            b.Set(headerProps, x => x.Main, b.Const(new Header.Title() { Operation = "Sync history" }));
            b.Set(headerProps, x => x.User, b.Get(dataModel, x => x.User));
            b.Set(headerProps, x => x.UseSignIn, b.Const(false));

            return b.Layout(
                b.LocalMenu(nameof(SyncHistory)),
                b.Render(headerProps), Render2(b, dataModel)).As<IVNode>();
        }

        public static Var<HyperNode> Render2(LayoutBuilder b, Var<SyncHistory.DataModel> dataModel)
        {
            var view = b.Div("flex flex-col");

            var clientRows = b.Get(dataModel, x => x.SyncHistory);

            var renderCell = b.Def((LayoutBuilder b, Var<SyncResult> serviceSnapshot, Var<DataTable.Column> col) =>
            {
                Var<string> columnName = b.Get(col, x => x.Name);

                var cell = b.Switch(columnName,
                    b => b.Text(b.GetProperty<string>(serviceSnapshot, columnName)),
                    (nameof(SyncResult.Timestamp), b => b.Text(b.ItalianFormat(b.Get(serviceSnapshot, x => x.Timestamp)))));

                return b.VPadded4(cell);
            });

            var props = b.NewObj<DataTable.Props<SyncResult>>(b =>
            {
                b.AddColumn(nameof(SyncResult.Timestamp));
                b.AddColumn(nameof(SyncResult.Trigger), "Sync trigger");
                b.AddColumn(nameof(SyncResult.ResultCode), "Result");
                b.SetRows(clientRows);
                b.SetRenderCell(renderCell);
            });

            b.Add(view, b.DataTable(props));

            return view;
        }
    }
}
