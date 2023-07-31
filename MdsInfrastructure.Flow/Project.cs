using Metapsi;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MdsInfrastructure.Flow;

public static class Project
{
    public class List : Metapsi.Http.Get<Routes.Project.List>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
        {
            var allConfigurations = await commandContext.Do(Api.LoadAllConfigurationHeaders);

            ListProjectsPage page = new ListProjectsPage()
            {
                ProjectsList = await commandContext.Do(Api.LoadAllProjects),
                AllConfigurationHeaders = allConfigurations.ConfigurationHeaders,
                InfrastructureServices = allConfigurations.Services,
                User = httpContext.User()
            };

            return Page.Result(page);
        }
    }
}
