using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsLocal
{
    public static class SyncHistory
    {
        public class DataModel
        {
            public List<SyncResult> SyncHistory { get; set; } = new List<SyncResult>();
        }

        public static async Task<IResponse> List(CommandContext commandContext, HttpContext requestData)
        {
            var dataModel = new DataModel()
            {
                SyncHistory = (await commandContext.Do(MdsLocalApplication.GetSyncHistory)).ToList()
            };
            return Page.Response(dataModel, (b, clientModel) => b.Layout(b.LocalMenu(nameof(SyncHistory)), b.Render(b.Const(new Header.Props()
            {
                Main = new Header.Title() { Operation = "Sync history" },
                User = requestData.User(),
            })), b.Render(dataModel)));
        }

        public static Var<HyperNode> Render(this BlockBuilder b, DataModel dataModel)
        {
            var view = b.Div("flex flex-col");

            var clientRows = b.Const(dataModel.SyncHistory.OrderByDescending(x => x.Timestamp).ToList());

            var renderCell = b.Def((BlockBuilder b, Var<SyncResult> serviceSnapshot, Var<DataTable.Column> col) =>
            {
                Var<string> columnName = b.Get(col, x => x.Name);

                var cell = b.Switch<HyperNode, string>(columnName,
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

