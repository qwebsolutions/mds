using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Ui;
using System;
using System.Collections.Generic;

namespace MdsInfrastructure
{
    public class SaveConfigurationInput
    {
        public InfrastructureConfiguration InfrastructureConfiguration { get; set; }
        public string OriginalJson { get; set; } = string.Empty;
    }

    public class SaveConfigurationResponse : ApiResponse
    {
        public List<string> ConflictMessages { get; set; } = new List<string>();
    }

    public class MergeConfigurationInput
    {
        public string SourceConfigurationJson { get; set; }
        public InfrastructureConfiguration EditedConfiguration { get; set; }
    }

    public class MergeConfigurationResponse : ApiResponse
    {
        public List<string> ConflictMessages { get; set; } = new();
        public List<string> SuccessMessages { get; set; } = new();
        public string ConfigurationJson { get; set; }
        public InfrastructureConfiguration Configuration { get; set; }
        public InfrastructureConfiguration SourceConfiguration { get; set; }
        //public string SourceConfigurationJson { get; set; }
    }

    public class GetConfigurationJsonResponse : ApiResponse
    {
        public string Json { get; set; }
    }

    public static class Frontend
    {
        public static Request<SaveConfigurationResponse, SaveConfigurationInput> SaveConfiguration { get; set; } = new(nameof(SaveConfiguration));
        public static Request<MergeConfigurationResponse, MergeConfigurationInput> MergeConfiguration { get; set; } = new(nameof(MergeConfiguration));
        public static Request<GetConfigurationJsonResponse, InfrastructureConfiguration> GetConfigurationJson { get; set; } = new(nameof(GetConfigurationJson));

        public class ConfirmDeploymentResponse : ApiResponse { }
        public static Request<ConfirmDeploymentResponse, Guid> ConfirmDeployment { get; set; } = new(nameof(ConfirmDeployment));

    }
}
