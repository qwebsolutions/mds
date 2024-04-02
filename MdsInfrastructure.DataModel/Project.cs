using MdsCommon;
using Metapsi.Ui;
using System.Collections.Generic;

namespace MdsInfrastructure
{
    public class ListProjectsPage : IHasLoadingPanel, IHasValidationPanel
    {
        public List<Project> ProjectsList { get; set; }
        public List<InfrastructureConfiguration> AllConfigurationHeaders { get; set; }
        public List<InfrastructureService> InfrastructureServices { get; set; }
        public bool ShowSidePanel { get; set; }
        public Project SelectedProject { get; set; }
        public bool IsLoading { get; set; }
        public string ValidationMessage { get; set; }

        public Metapsi.Ui.User User { get; set; }
    }
}
