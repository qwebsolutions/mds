using Metapsi;
using Metapsi.Syntax;
using System.Threading.Tasks;
using Metapsi.Hyperapp;
using MdsCommon;
using MdsInfrastructure;
using Microsoft.AspNetCore.Http;

namespace MdsInfrastructure
{
    public static partial class EventsLog
    {
        public static async Task<IResponse> List(CommandContext commandContext, HttpContext requestData)
        {
            var allEvents = await commandContext.Do(MdsCommon.Api.GetAllInfrastructureEvents);
            MdsCommon.ListInfrastructureEventsPage listInfrastructureEventsPage = new ListInfrastructureEventsPage()
            {
                InfrastructureEvents = allEvents
            };

            return Page.Response(
                listInfrastructureEventsPage, 
                (b, clientModel) => b.Call(
                    Common.Layout, 
                    b.InfraMenu(nameof(EventsLog), requestData.User().IsSignedIn()),
                    b.Render(
                        b.Const(new Header.Props(){
                            Main = new Header.Title() { Operation = "Infrastructure events" },
                            User = requestData.User()
                        })), 
                    b.RenderListInfrastructureEventsPage(clientModel)));
        }
    }
}
