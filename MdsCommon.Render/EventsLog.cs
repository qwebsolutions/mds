using MdsCommon.Controls;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsCommon
{

    public static partial class Render
    {
        public static Var<HyperNode> RenderListInfrastructureEventsPage(this BlockBuilder b, Var<ListInfrastructureEventsPage> clientModel)
        {
            b.AddStylesheet("MdsCommon.css");

            var container = b.Div();
            b.Comment("Created container");
            b.Comment("Client model consted");
            b.Add(container, b.SidePanel(
                clientModel,
                b => b.EventPanel(
                    b.Get(clientModel, x => x.SelectedEvent))));

            b.Comment("After side panel");

            var renderCell = b.Def((BlockBuilder b, Var<InfrastructureEvent> row, Var<DataTable.Column> col) =>
            {
                var date = b.Get(row, x => x.Timestamp);
                var dateStringLocale = b.ItalianFormat(date);

                return b.VPadded4(b.Switch(
                    b.Get(col, x => x.Name),
                    b => b.Link(dateStringLocale, b.MakeAction<ListInfrastructureEventsPage>(
                        (b, clientModel) =>
                        {
                            b.Log("Show side panel");
                            b.Log(clientModel);
                            b.Set(clientModel, x => x.SelectedEvent, row);
                            return b.ShowSidePanel(clientModel);
                        })),
                    (nameof(InfrastructureEvent.Criticality), b =>
                    {
                        var criticality = b.Get(row, x => x.Criticality);
                        var container = b.Span();
                        b.Add(container, b.Text(criticality));
                        b.AddAlertBadge(container, criticality);
                        return container;
                    }
                ),
                    (nameof(InfrastructureEvent.Source), b => b.Text(b.Get(row, x => x.Source))),
                    (nameof(InfrastructureEvent.ShortDescription), b => b.Text(b.Get(row, x => x.ShortDescription)))));
            });

            var rows = b.Get(clientModel, x => x.InfrastructureEvents);

            var props = b.NewObj<DataTable.Props<InfrastructureEvent>>(b =>
            {
                b.AddColumn(nameof(InfrastructureEvent.Timestamp), "Timestamp");
                b.AddColumn(nameof(InfrastructureEvent.Criticality));
                b.AddColumn(nameof(InfrastructureEvent.Source));
                b.AddColumn(nameof(InfrastructureEvent.ShortDescription), "Description");
                b.SetRows(rows);
                b.SetRenderCell<InfrastructureEvent>(renderCell);
            });
            var dt = b.Add(container, b.DataTable(props));
            b.AddClass(dt, "drop-shadow");
            return container;
        }

        public static Var<HyperNode> EventPanel(this BlockBuilder b, Var<MdsCommon.InfrastructureEvent> e)
        {
            var panel = b.Div();
            var gridLayout = b.Add(panel, b.Div("grid grid-cols-2 gap-4"));
            b.Add(gridLayout, b.Text("Event timestamp"));
            b.Add(gridLayout, b.Text(b.ItalianFormat(b.Get(e, e => e.Timestamp))));

            b.Add(gridLayout, b.Text("Event description"));
            b.Add(gridLayout, b.Text(b.Get(e, e => e.ShortDescription)));

            b.Add(gridLayout, b.Text("Event source"));
            b.Add(gridLayout, b.Text(b.Get(e, e => e.Source)));

            b.Add(gridLayout, b.Text("Details"));
            b.Add(gridLayout, b.Text(""));

            var details = b.Add(panel, b.Div("overflow-auto"));// Doesn't work

            var pre = b.Add(details, b.Node("pre"));
            b.Add(pre, b.TextNode(b.Get(e, e => e.FullDescription)));

            return panel;
        }
    }
}