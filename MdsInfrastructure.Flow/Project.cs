using Metapsi;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MdsInfrastructure.Flow;

public static class Project
{
    public class List : Metapsi.Http.Get<Routes.Project.List>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
        {
            var buildsList = await commandContext.Do(Backend.GetRemoteBuilds);
            var page = await LoadAll(commandContext, httpContext, buildsList);
            return Page.Result(page);
        }

        public static async Task<ListProjectsPage> LoadAll(
            CommandContext commandContext, 
            HttpContext httpContext,
            System.Collections.Generic.List<MdsCommon.AlgorithmInfo> allAlgorithmsFromRepository)
        {
            var allConfigurations = await commandContext.Do(Backend.LoadAllConfigurationHeaders);
            var allProjects = await commandContext.Do(Backend.LoadAllProjects);
            var allServices = allConfigurations.ConfigurationHeaders.SelectMany(x => x.InfrastructureServices).ToList();

            var binaries = allAlgorithmsFromRepository.Select(x => new BinariesRepositoryEntry()
            {
                ProjectName = x.Name,
                ProjectVersion = x.Version,
                Target = x.Target,
                BuildNumber = x.BuildNumber,
                IsInUse = IsInUse(x.Name, x.Version, allProjects, allServices)
            });

            ListProjectsPage page = new ListProjectsPage()
            {
                ProjectsList = allProjects,
                Binaries = binaries.ToList(),
                AllConfigurationHeaders = allConfigurations.ConfigurationHeaders,
                InfrastructureServices = allServices,
                User = httpContext.User(),
            };

            return page;
        }

        public static bool IsInUse(
            string projectName,
            string versionName,
            System.Collections.Generic.List<MdsCommon.Project> allProjects,
            System.Collections.Generic.List<InfrastructureService> allServices)
        {
            var project = allProjects.SingleOrDefault(x => x.Name == projectName);
            if (project == null)
                return false;

            var version = project.Versions.SingleOrDefault(x => x.VersionTag == versionName);
            if (version == null)
                return false;

            return allServices.Any(x => x.ProjectVersionId == version.Id);
        }
    }
}
