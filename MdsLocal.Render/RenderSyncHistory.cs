using Metapsi.Syntax;
using MdsCommon;
using Metapsi.Hyperapp;
using Metapsi;
using MdsCommon.Controls;
using static MdsLocal.SyncHistory;
using Metapsi.Html;
using System.Linq;

namespace MdsLocal
{
    public static class RenderSyncHistory
    {
        public static Var<IVNode> Render(LayoutBuilder b, Var<SyncHistory.DataModel> dataModel)
        {
            b.AddModuleStylesheet();
            var headerProps = b.NewObj<Header.Props>();
            b.Set(headerProps, x => x.Main, b.Const(new Header.Title() { Operation = "Sync history" }));
            b.Set(headerProps, x => x.User, b.Get(dataModel, x => x.User));
            b.Set(headerProps, x => x.UseSignIn, b.Const(false));

            return b.Layout(
                b.LocalMenu(nameof(SyncHistory)),
                b.Render(headerProps), RenderContent(b, dataModel));
        }

        private static Var<IVNode> RenderContent(LayoutBuilder b, Var<SyncHistory.DataModel> dataModel)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col");
                },
                b.SidePanel((LayoutBuilder b) =>
                {
                    var selectedResult = b.Get(dataModel, x => x.SelectedResult);
                    return b.Optional(
                        b.HasObject(selectedResult),
                        b =>
                        {
                            return b.MdsMainPanel(b => { }, SyncLogTable(b, selectedResult));
                        });
                }),
                b.MdsMainPanel(b => { }, SyncHistoryTable(b, dataModel)));
        }

        private static Var<IVNode> SyncLogTable(LayoutBuilder b, Var<SyncResult> syncResult)
        {
            const string info = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"w-6 h-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"m11.25 11.25.041-.02a.75.75 0 0 1 1.063.852l-.708 2.836a.75.75 0 0 0 1.063.853l.041-.021M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Zm-9-3.75h.008v.008H12V8.25Z\" />\r\n</svg>\r\n";
            const string error = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"w-6 h-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126ZM12 15.75h.007v.008H12v-.008Z\" />\r\n</svg>\r\n";

            var tableBuilder = MdsDefaultBuilder.DataTable<SyncResultLog>();
            tableBuilder.OverrideDataCell(nameof(SyncResultLog.Type), (LayoutBuilder b, Var<SyncResultLog> log) =>
            {
                return b.Switch(
                    b.Get(log, x => x.Type),
                    b => b.Text(b.Get(log, x => x.Type)),
                    (SyncResultLogType.Info, b => b.Svg(info, "text-sky-500")),
                    (SyncResultLogType.Warning, b => b.Svg(error, "text-amber-500")),
                    (SyncResultLogType.Error, b => b.Svg(error, "text-red-500")));
            });

            tableBuilder.OverrideHeaderCell(nameof(SyncResultLog.Type), b => b.HtmlDiv());

            tableBuilder.AddTdProps(
                (b, log, column) =>
                {
                    b.If(
                        b.AreEqual(column, b.Const(nameof(SyncResultLog.Message))),
                        b =>
                        {
                            b.AddClass("text-sm");
                        });
                });

            tableBuilder.SetTbodyProps = b =>
            {
                b.AddClass("p-4");
            };

            return b.HtmlDiv(
                b.DataTable(
                tableBuilder,
                b.Get(syncResult, x => x.Log),
                nameof(SyncResultLog.Type),
                nameof(SyncResultLog.Message)));
        }

        private static Var<IVNode> SyncHistoryTable(LayoutBuilder b, Var<SyncHistory.DataModel> model)
        {
            var tableBuilder = MdsDefaultBuilder.DataTable<SyncResult>();
            tableBuilder.OverrideHeaderCell(nameof(SyncResult.Trigger), b => b.Text("Sync trigger"));
            tableBuilder.OverrideHeaderCell(nameof(SyncResult.ResultCode), b => b.Text("Result"));
            tableBuilder.OverrideDataCell(nameof(SyncResult.Timestamp), (b, syncResult) =>
            {
                var loadLogButton = b.HtmlButton(
                    b =>
                    {
                        b.SetClass("text-sky-500 underline");
                        b.OnClickAction((SyntaxBuilder b, Var<DataModel> model) =>
                        {
                            return b.MakeStateWithEffects(
                                b.ShowLoading(model),
                                b.GetJson(
                                    b.GetApiUrl(Frontend.LoadFullSyncResult, b.Get(syncResult, x => x.Id).As<string>()),
                                    b.MakeAction((SyntaxBuilder b, Var<DataModel> page, Var<FullSyncResultResponse> response) =>
                                    {
                                        b.Set(page, x => x.SelectedResult, b.Get(response, x => x.SyncResult));
                                        b.Log(page);
                                        b.HideLoading(page);
                                        b.ShowSidePanel();
                                        return b.Clone(page);
                                    }),
                                    b.MakeAction((SyntaxBuilder b, Var<DataModel> page, Var<ClientSideException> ex) =>
                                    {
                                        b.HideLoading(page);
                                        b.Alert(ex);
                                        return b.Clone(page);
                                    })));
                        });
                    },
                    b.Text(b.ItalianFormat(b.Get(syncResult, x => x.Timestamp))));

                return b.HtmlDiv(
                    b =>
                    {
                        b.AddClass("flex flex-row gap-1");
                    },
                    loadLogButton,
                    b.Optional(
                        b.Get(syncResult, x => x.Log.Any(x => x.Type == SyncResultLogType.Error)),
                        b => b.Badge(b.Const("error"), b.Const("bg-red-600"))));
            });

            return b.DataTable(
                tableBuilder,
                b.Get(model, x => x.SyncHistory),
                nameof(SyncResult.Timestamp),
                nameof(SyncResult.Trigger),
                nameof(SyncResult.ResultCode));
        }
    }
}
