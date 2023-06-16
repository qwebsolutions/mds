using Metapsi;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure.Flow;

public static class Status
{
    public class Infra : Metapsi.Http.Get<Routes.Status.Infra>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
        {
            var statusPage = await Load.Status(commandContext);
            statusPage.User = httpContext.User();
            return Page.Result(statusPage);
        }
    }

    public class Application : Metapsi.Http.Get<Routes.Status.Application, string>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext, string applicationName)
        {
            var pageData = await Load.Status(commandContext);
            pageData.User = httpContext.User();
            Guid selectedApplicationId = pageData.InfrastructureConfiguration.Applications.Single(x => x.Name == applicationName).Id;

            return Page.Result<ApplicationStatus>(new ApplicationStatus()
            {
                ApplicationName = applicationName,
                InfrastructureStatus = pageData
            });
        }
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
