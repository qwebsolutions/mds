using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MdsLocal
{
    public class EventsLogHandler : Http.Get<MdsCommon.EventsLog.List>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
        {
            var allEvents = new MdsCommon.ListInfrastructureEventsPage()
            {
                InfrastructureEvents = await commandContext.Do(MdsCommon.Api.GetAllInfrastructureEvents)
            };

            return Page.Result(allEvents);
        }
    }
}

