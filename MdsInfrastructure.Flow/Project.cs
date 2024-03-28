using Metapsi;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure.Flow;

public static class Project
{
    public class List : Metapsi.Http.Get<Routes.Project.List>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
        {
            var allConfigurations = await commandContext.Do(Backend.LoadAllConfigurationHeaders);

            ListProjectsPage page = new ListProjectsPage()
            {
                ProjectsList = await commandContext.Do(Backend.LoadAllProjects),
                AllConfigurationHeaders = allConfigurations.ConfigurationHeaders,
                InfrastructureServices = allConfigurations.ConfigurationHeaders.SelectMany(x=>x.InfrastructureServices).ToList(),
                User = httpContext.User()
            };

            return Page.Result(page);
        }
    }
}
