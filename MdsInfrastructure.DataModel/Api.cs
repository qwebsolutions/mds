using MdsCommon;
using Metapsi;
using System.Collections.Generic;
using System;

namespace MdsInfrastructure
{
    public static class Api
    {
        public static class Event
        {
            public class BroadcastDeployment : IData { }

            public class BinariesSynchronized : IData
            {
                public string BinariesDescription { get; set; }
            }
        }

        public static Request<string> ValidateSchema { get; set; } = new(nameof(ValidateSchema));
        public static Request<InfrastructureStatus> LoadInfraStatus { get; set; } = new(nameof(LoadInfraStatus));
        public static Request<ConfigurationHeadersList> LoadAllConfigurationHeaders { get; set; } = new(nameof(LoadAllConfigurationHeaders));
        public static Request<InfrastructureConfiguration, Guid> LoadConfiguration { get; set; } = new(nameof(LoadConfiguration));
        public static Command<InfrastructureConfiguration> SaveConfiguration { get; set; } = new(nameof(SaveConfiguration));
        public static Command<InfrastructureNode> SaveNode { get; set; } = new(nameof(SaveNode));
        public static Command<Guid> DeleteConfigurationById { get; set; } = new(nameof(DeleteConfigurationById));
        public static Request<Deployment> LoadCurrentDeployment { get; set; } = new(nameof(LoadCurrentDeployment));
        public static Request<InfrastructureConfiguration> LoadCurrentConfiguration { get; set; } = new(nameof(LoadCurrentConfiguration));
        public static Request<Deployment, Guid> LoadDeploymentById { get; set; } = new(nameof(LoadDeploymentById));
        public static Request<List<Deployment>> LoadDeploymentsHistory { get; set; } = new(nameof(LoadDeploymentsHistory));
        public static Request<List<MdsCommon.Project>> LoadAllProjects { get; set; } = new(nameof(LoadAllProjects));
        public static Request<List<InfrastructureNode>> LoadAllNodes { get; set; } = new(nameof(LoadAllNodes));
        public static Request<List<InfrastructureService>> LoadAllServices { get; set; } = new(nameof(LoadAllServices));
        public static Command<ConfirmDeploymentInput> ConfirmDeployment { get; set; } = new(nameof(ConfirmDeployment));
        public static Request<List<ParameterType>> GetAllParameterTypes { get; set; } = new Request<List<ParameterType>>(nameof(GetAllParameterTypes));
        public static Request<MdsCommon.ServiceConfigurationSnapshot, string> LoadServiceSnapshotByHash { get; set; } = new(nameof(LoadServiceSnapshotByHash));
        public static Request<List<EnvironmentType>> LoadEnvironmentTypes { get; set; } = new(nameof(LoadEnvironmentTypes));
        public static Request<List<MdsCommon.MachineStatus>> LoadHealthStatus { get; set; } = new(nameof(LoadHealthStatus));
        public static Request<List<MdsCommon.AlgorithmInfo>> GetRemoteBuilds { get; set; } = new(nameof(GetRemoteBuilds));
        public static Request<List<MdsCommon.AlgorithmInfo>, List<MdsCommon.AlgorithmInfo>> RegisterNewBinaries { get; set; } = new(nameof(RegisterNewBinaries));
        public static Request<Deployment, Guid> LoadLastConfigurationDeployment { get; set; } = new(nameof(LoadLastConfigurationDeployment));
        public static Command<string> RestartService { get; set; } = new(nameof(RestartService));
        public static Command<MdsCommon.ProjectVersion> SaveVersionEnabled { get; set; } = new(nameof(SaveVersionEnabled));
        public static Request<string> GetInfrastructureName { get; set; } = new(nameof(GetInfrastructureName));
        public static Request<List<NoteType>> GetAllNoteTypes { get; set; } = new(nameof(GetAllNoteTypes));
        public static Command<MachineStatus> StoreHealthStatus { get; set; } = new(nameof(StoreHealthStatus));
    }

    public class ConfirmDeploymentInput
    {
        public List<MdsCommon.ServiceConfigurationSnapshot> Snapshots { get; set; }
        public InfrastructureConfiguration Configuration { get; set; }
    }
}
