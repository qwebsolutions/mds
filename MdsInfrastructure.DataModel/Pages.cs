﻿using System;
using System.Collections.Generic;
using MdsCommon;

namespace MdsInfrastructure
{
    // TODO: Remove explicit API support
    public class EditConfigurationPage
    {
        public string CurrentConfigurationSimplifiedJson { get; set; } = string.Empty;

        public SaveConfigurationResponse SaveConfigurationResponse { get; set; }= new ();
        public MergeConfigurationResponse MergeConfigurationResponse { get; set; } = new();

        public User User { get; set; }

        public string ServicesFilter { get; set; } = string.Empty;
        public string ApplicationsFilter { get; set; } = string.Empty;
        public string VariablesFilter { get; set; } = string.Empty;
        public string ParametersFilter { get; set; } = string.Empty;

        public InfrastructureConfiguration Configuration { get; set; }
        public List<MdsCommon.Project> AllProjects { get; set; }
        public List<InfrastructureNode> InfrastructureNodes { get; set; }
        //public Page Page { get; set; } = new Page();
        public Deployment LastConfigurationDeployment { get; set; }
        public Deployment LastInfrastructureDeployment { get; set; }
        public string InitialConfiguration { get; set; }
        public List<EnvironmentType> EnvironmentTypes { get; set; }
        public List<ParameterType> ParameterTypes { get; set; }
        public List<NoteType> NoteTypes { get; set; }
        public string LastDeployed { get; set; }


        // Edited entities
        public Guid EditVariableId { get; set; }
        public Guid EditServiceId { get; set; }
        public Guid EditParameterId { get; set; }
        public Guid EditApplicationId { get; set; }
        public Guid EditServiceNoteId { get; set; }

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

        public string ValidationMessage { get; set; } = string.Empty;
    }
}
