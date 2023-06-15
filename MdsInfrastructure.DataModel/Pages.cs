using Metapsi;
using System;
using System.Collections.Generic;

namespace MdsInfrastructure
{
    public class EditConfigurationPage //: IEditPage<EditConfigurationPage>, IHasTabs, IHasValidationPanel
    {
        public Guid EntityId { get; set; }
        //public PageStack.Stack<EditConfigurationPage> EditStack { get; set; } = new();

        public InfrastructureConfiguration Configuration { get; set; }
        public List<MdsCommon.Project> AllProjects { get; set; }
        public List<InfrastructureNode> InfrastructureNodes { get; set; }
        //public Page Page { get; set; } = new Page();
        public Deployment LastConfigurationDeployment { get; set; }
        public Deployment LastInfrastructureDeployment { get; set; }
        public InfrastructureConfiguration InitialConfiguration { get; set; }
        public List<EnvironmentType> EnvironmentTypes { get; set; }
        public List<ParameterType> ParameterTypes { get; set; }
        public List<NoteType> NoteTypes { get; set; }
        public string LastDeployed { get; set; }

        public bool IsActiveConfiguration
        {
            get
            {
                if (Metapsi.Record.IsEmpty(LastConfigurationDeployment))
                {
                    return false;
                }

                if (Metapsi.Record.IsEmpty(LastInfrastructureDeployment))
                {
                    return false;
                }

                return LastConfigurationDeployment.Id == LastInfrastructureDeployment.Id;
            }
        }

        public string FirstLevelSelectedTab { get; set; } = string.Empty;
        public string SecondLevelSelectedTab { get; set; } = string.Empty;
        public string ValidationMessage { get; set; } = string.Empty;
    }
}
