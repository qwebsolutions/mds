using MdsCommon.Controls;
using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsCommon
{

    public static partial class Render
    {
        public static Var<IVNode> RenderListInfrastructureEventsPage(this LayoutBuilder b, Var<ListInfrastructureEventsPage> clientModel)
        {
            b.AddModuleStylesheet();

            var sidePanel = b.SidePanel(
            clientModel,
            b => b.EventPanel(
                b.Get(clientModel, x => x.SelectedEvent)));

            b.Comment("After side panel");

            var renderCell = b.Def((LayoutBuilder b, Var<InfrastructureEvent> row, Var<DataTable.Column> col) =>
            {
                var date = b.Get(row, x => x.Timestamp);
                var dateStringLocale = b.ItalianFormat(date);

                return b.VPadded4(b.Switch<LayoutBuilder, IVNode, string>(
                    b.Get(col, x => x.Name),
                    b => b.Link(dateStringLocale, b.MakeAction<ListInfrastructureEventsPage>(
                        (b, clientModel) =>
                        {
                            b.Set(clientModel, x => x.SelectedEvent, row);
                            return b.ShowSidePanel(clientModel);
                        })),

                    (nameof(InfrastructureEvent.Criticality), b =>
                    {
                        var criticality = b.Get(row, x => x.Criticality);

                        return b.HtmlSpan(
                            b =>
                            {

                            },
                            b.T(criticality),
                            b.AlertBadge(criticality));
                    }),
                    (nameof(InfrastructureEvent.Source), b => b.T(b.Get(row, x => x.Source))),
                    (nameof(InfrastructureEvent.ShortDescription), b => b.T(b.Get(row, x => x.ShortDescription)))));
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
            var dt = b.DataTable(props, b=>
            {
                b.AddClass("drop-shadow");
            });
            return b.HtmlDiv(
                b =>
                {

                },
                sidePanel,
                dt);
        }

        public static Var<IVNode> EventPanel(this LayoutBuilder b, Var<MdsCommon.InfrastructureEvent> e)
        {
            var gridLayout = b.HtmlDiv(
                b =>
                {
                    b.SetClass("grid grid-cols-2 gap-4");
                },
                b.T("Event timestamp"),
                b.T(b.ItalianFormat(b.Get(e, e => e.Timestamp))),
                b.T("Event description"),
                b.T(b.Get(e, e => e.ShortDescription)),
                b.T("Event source"),
                b.T(b.Get(e, e => e.Source)),
                b.T("Details"),
                b.T(""));

            var details = b.HtmlDiv(
                b =>
                {
                    b.SetClass("overflow-auto");// Doesn't work
                },
                b.HtmlPre(b => { },
                    b.T(b.Get(e, e => e.FullDescription))));

            return b.HtmlDiv(b => { },
                gridLayout,
                details);
        }
    }
}