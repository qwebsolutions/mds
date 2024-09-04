using MdsCommon;
using Metapsi;
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

    public static class UserExtensions
    {
        public static MdsCommon.User User(this HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return new User();
            }

            return new MdsCommon.User
            {
                AuthType = "OIDC",
                Name = userIdClaim.Value
            };
        }
    }
}

