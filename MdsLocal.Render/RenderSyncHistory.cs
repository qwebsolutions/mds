using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Ui;
using Metapsi.Hyperapp;
using System.Text;
using System;
using Metapsi;
using MdsCommon.Controls;
using static MdsLocal.SyncHistory;
using Metapsi.Html;
using System.Collections.Specialized;

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
                b.Render(headerProps), Render(b, dataModel));
        }

        private static Var<IVNode> Render(LayoutBuilder b, Var<SyncHistory.DataModel> dataModel)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col");
                },
                SyncHistoryTable(b, dataModel));
        }

        private static Var<IVNode> SyncHistoryTable(LayoutBuilder b, Var<SyncHistory.DataModel> model)
        {
            var tableBuilder = MdsDefaultBuilder.DataTable<SyncResult>();
            tableBuilder.OverrideHeaderCell(nameof(SyncResult.Trigger), b => b.T("Sync trigger"));
            tableBuilder.OverrideHeaderCell(nameof(SyncResult.ResultCode), b => b.T("Result"));
            tableBuilder.OverrideDataCell(nameof(SyncResult.Timestamp), (b, syncResult) => b.T(b.ItalianFormat(b.Get(syncResult, x => x.Timestamp))));

            return b.DataTable(
                tableBuilder,
                b.Get(model, x => x.SyncHistory),
                nameof(SyncResult.Timestamp),
                nameof(SyncResult.Trigger),
                nameof(SyncResult.ResultCode));
        }
    }
}
