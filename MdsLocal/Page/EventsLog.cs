using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MdsLocal
{
    public static partial class EventsLog
    {
        public static async Task<IResponse> List(CommandContext commandContext, HttpContext requestData)
        {
            var allEvents = new MdsCommon.ListInfrastructureEventsPage()
            {
                InfrastructureEvents = await commandContext.Do(MdsCommon.Api.GetAllInfrastructureEvents)
            };

            return Page.Response(
                allEvents, 
                (b, clientModel) => b.Call(
                    Common.Layout, 
                    b.LocalMenu(nameof(EventsLog)),
                    b.Render(b.Const(new Header.Props()
                    {
                        Main = new Header.Title() { Operation = "Infrastructure events" },
                        User = requestData.User(),
                    })), 
                    b.RenderListInfrastructureEventsPage(clientModel)));
        }
    }
}

