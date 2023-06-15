using Metapsi;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MdsInfrastructure.Flow;

public class Status : Metapsi.Http.Get<Routes.Status.Infra>
{
    public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
    {
        var statusPage = await Load.Status(commandContext);
        statusPage.User = httpContext.User();
        return Page.Result(statusPage);
    }
}

internal static partial class Load
{
    public static async Task<InfrastructureStatus> Status(CommandContext commandContext)
    {
        string validation = await commandContext.Do(Api.ValidateSchema);

        if (!string.IsNullOrEmpty(validation))
        {
            return new InfrastructureStatus()
            {
                SchemaValidationMessage = validation
            };
        }

        return await commandContext.Do(Api.LoadInfraStatus);
    }
}
