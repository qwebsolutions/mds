using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MdsCommon
{
    public class EventsLogHandler : Http.Get<MdsCommon.Routes.EventsLog.List>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
        {
            var allEvents = new MdsCommon.ListInfrastructureEventsPage()
            {
                InfrastructureEvents = await commandContext.Do(MdsCommon.Api.GetAllInfrastructureEvents),
                User = httpContext.User()
            };

            return Page.Result(allEvents);
        }
    }
}

