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
            var allAlgorithmsFromRepository = await commandContext.Do(Backend.GetRemoteBuilds);
            var binaries = allAlgorithmsFromRepository.Select(x => new BinariesRepositoryEntry()
            {
                ProjectName = x.Name,
                ProjectVersion = x.Version,
                Target = x.Target,
                BuildNumber = x.BuildNumber,
            });

            ListProjectsPage page = new ListProjectsPage()
            {
                ProjectsList = await commandContext.Do(Backend.LoadAllProjects),
                Binaries = binaries.ToList(),
                AllConfigurationHeaders = allConfigurations.ConfigurationHeaders,
                InfrastructureServices = allConfigurations.ConfigurationHeaders.SelectMany(x=>x.InfrastructureServices).ToList(),
                User = httpContext.User()
            };

            return Page.Result(page);
        }
    }
}
