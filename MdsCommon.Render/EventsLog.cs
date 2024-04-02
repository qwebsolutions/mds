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
                b => b.EventPanel(b.Get(clientModel, x => x.SelectedEvent)));

            var eventsTableBuilder = MdsDefaultBuilder.DataTable<InfrastructureEvent>();

            eventsTableBuilder.OverrideHeaderCell(nameof(InfrastructureEvent.ShortDescription), b => b.T("Description"));

            eventsTableBuilder.OverrideDataCell(
                nameof(InfrastructureEvent.Timestamp),
                (b, row) =>
                {
                    var date = b.Get(row, x => x.Timestamp);
                    var dateStringLocale = b.ItalianFormat(date);

                    return b.Link(
                        dateStringLocale,
                        b.MakeAction<ListInfrastructureEventsPage>(
                        (b, clientModel) =>
                        {
                            b.Set(clientModel, x => x.SelectedEvent, row);
                            b.ShowSidePanel();
                            return b.Clone(clientModel);
                        }));
                });
            eventsTableBuilder.OverrideDataCell(
                nameof(InfrastructureEvent.Criticality),
                (b, row) =>
                {
                    var criticality = b.Get(row, x => x.Criticality);

                    return b.HtmlSpan(
                        b =>
                        {
                        },
                        b.TextSpan(criticality),
                        b.AlertBadge(criticality));
                });


            return b.HtmlDiv(
                b =>
                {

                },
                sidePanel,
                b.MdsMainPanel(
                    b =>
                    {

                    },
                    b.DataTable(
                        eventsTableBuilder,
                        b.Get(clientModel, x => x.InfrastructureEvents),
                        nameof(InfrastructureEvent.Timestamp),
                        nameof(InfrastructureEvent.Criticality),
                        nameof(InfrastructureEvent.Source),
                        nameof(InfrastructureEvent.ShortDescription)
                        )));

            //var renderCell = b.Def((LayoutBuilder b, Var<InfrastructureEvent> row, Var<DataTable.Column> col) =>
            //{
            //    var date = b.Get(row, x => x.Timestamp);
            //    var dateStringLocale = b.ItalianFormat(date);

            //    return b.VPadded4(b.Switch<LayoutBuilder, IVNode, string>(
            //        b.Get(col, x => x.Name),
            //        b => b.Link(dateStringLocale, b.MakeAction<ListInfrastructureEventsPage>(
            //            (b, clientModel) =>
            //            {
            //                b.Set(clientModel, x => x.SelectedEvent, row);
            //                return b.ShowSidePanel(clientModel);
            //            })),

            //        (nameof(InfrastructureEvent.Criticality), b =>
            //        {
            //            var criticality = b.Get(row, x => x.Criticality);

            //            return b.HtmlSpan(
            //                b =>
            //                {

            //                },
            //                b.TextSpan(criticality),
            //                b.AlertBadge(criticality));
            //        }),
            //        (nameof(InfrastructureEvent.Source), b => b.TextSpan(b.Get(row, x => x.Source))),
            //        (nameof(InfrastructureEvent.ShortDescription), b => b.TextSpan(b.Get(row, x => x.ShortDescription)))));
            //});

            //var rows = b.Get(clientModel, x => x.InfrastructureEvents);

            //var props = b.NewObj<DataTable.Props<InfrastructureEvent>>(b =>
            //{
            //    b.AddColumn(nameof(InfrastructureEvent.Timestamp), "Timestamp");
            //    b.AddColumn(nameof(InfrastructureEvent.Criticality));
            //    b.AddColumn(nameof(InfrastructureEvent.Source));
            //    b.AddColumn(nameof(InfrastructureEvent.ShortDescription), "Description");
            //    b.SetRows(rows);
            //    b.SetRenderCell<InfrastructureEvent>(renderCell);
            //});
            //var dt = b.DataTable(props, b=>
            //{
            //    b.AddClass("drop-shadow");
            //});
            //return b.HtmlDiv(
            //    b =>
            //    {

            //    },
            //    sidePanel,
            //    dt);
        }

        public static Var<IVNode> EventPanel(this LayoutBuilder b, Var<MdsCommon.InfrastructureEvent> e)
        {
            var gridLayout = b.HtmlDiv(
                b =>
                {
                    b.SetClass("grid grid-cols-2 gap-4");
                },
                b.TextSpan("Event timestamp"),
                b.TextSpan(b.ItalianFormat(b.Get(e, e => e.Timestamp))),
                b.TextSpan("Event description"),
                b.TextSpan(b.Get(e, e => e.ShortDescription)),
                b.TextSpan("Event source"),
                b.TextSpan(b.Get(e, e => e.Source)),
                b.TextSpan("Details"),
                b.TextSpan(""));

            var details = b.HtmlDiv(
                b =>
                {
                    b.SetClass("overflow-auto");// Doesn't work
                },
                b.HtmlPre(b => { },
                    b.TextSpan(b.Get(e, e => e.FullDescription))));

            return b.HtmlDiv(b => { },
                gridLayout,
                details);
        }
    }
}