using Metapsi;
using Metapsi.Hyperapp;
using System;

namespace MdsInfrastructure
{
    public static class Frontend
    {
        public class SaveConfigurationResponse : ApiResponse { }
        public static Request<SaveConfigurationResponse, InfrastructureConfiguration> SaveConfiguration { get; set; } = new(nameof(SaveConfiguration));

        public class ConfirmDeploymentResponse : ApiResponse { }
        public static Request<ConfirmDeploymentResponse, Guid> ConfirmDeployment { get; set; } = new(nameof(ConfirmDeployment));
    }
}
