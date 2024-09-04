using MdsCommon;
using System.Collections.Generic;

namespace MdsInfrastructure
{
    public class ListProjectsPage : IHasLoadingPanel, IHasValidationPanel
    {
        public List<Project> ProjectsList { get; set; }
        public List<InfrastructureConfiguration> AllConfigurationHeaders { get; set; }
        public List<InfrastructureService> InfrastructureServices { get; set; }
        public List<BinariesRepositoryEntry> Binaries { get; set; } = new();
        // A different list, because the popup might give an error.
        // In that case we need to keep track of the previous list
        // to figure out which one was removed successfuly and which one not
        public List<BinariesRepositoryEntry> ToDeleteBinaries { get; set; } = new();
        public bool ShowSidePanel { get; set; }
        public Project SelectedProject { get; set; }
        public bool IsLoading { get; set; }
        public string ValidationMessage { get; set; }

        public User User { get; set; }

        public string SearchKeyword { get; set; }

        public string DeleteError { get; set; } = string.Empty;
    }

    public class BinariesRepositoryEntry
    {
        public bool Selected { get; set; }
        public string ProjectName { get; set; }
        public string ProjectVersion { get; set; }
        public string Target { get; set; }
        public string BuildNumber { get; set; }
        public bool Removed { get; set; }
        public bool IsInUse { get; set; }
    }
}
