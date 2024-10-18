using MdsCommon;
using Metapsi;
using Metapsi.Sqlite;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MdsCommon
{
    public static class EventsLogHandler
    {
        public static async Task<IResult> Get(HttpContext httpContext, SqliteQueue sqliteQueue)
        {
            var allEvents = new MdsCommon.ListInfrastructureEventsPage()
            {
                InfrastructureEvents = await MdsCommon.Db.LoadAllInfrastructureEvents(sqliteQueue),
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

