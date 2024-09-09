using System.Threading.Tasks;
using Metapsi;
using System;
using System.Linq;
using System.Collections.Generic;
using MdsCommon;

namespace MdsInfrastructure
{
    public static partial class MdsInfrastructureApplication
    {
        //public static Request<Db.SchemaValidationResult> ValidateSchema { get; set; } = new Request<Db.SchemaValidationResult>(nameof(ValidateSchema));
        ////public static Request<InfrastructureSummary> GetInfrastructureSummary { get; set; } = new(nameof(GetInfrastructureSummary));
        //public static Request<InfrastructureStatus> LoadInfraStatus { get; set; } = new (nameof(LoadInfraStatus));
        //public static Request<Db.ConfigurationHeadersList> LoadAllConfigurationHeaders { get; set; } = new Request<Db.ConfigurationHeadersList>(nameof(LoadAllConfigurationHeaders));
        //public static Request<InfrastructureConfiguration, Guid> LoadConfiguration { get; set; } = new Request<InfrastructureConfiguration, Guid>(nameof(LoadConfiguration));
        //public static Command<InfrastructureConfiguration> SaveConfiguration { get; set; } = new Command<InfrastructureConfiguration>(nameof(SaveConfiguration));
        //public static Command<InfrastructureNode> SaveNode { get; set; } = new Command<InfrastructureNode>(nameof(SaveNode));
        //public static Command<Guid> DeleteConfigurationById { get; set; } = new Command<Guid>(nameof(DeleteConfigurationById));
        //public static Request<Deployment> LoadCurrentDeployment { get; set; } = new Request<Deployment>(nameof(LoadCurrentDeployment));
        //public static Request<InfrastructureConfiguration> LoadCurrentConfiguration { get; set; } = new Request<InfrastructureConfiguration>(nameof(LoadCurrentConfiguration));
        //public static Request<Deployment, Guid> LoadDeploymentById { get; set; } = new Request<Deployment, Guid>(nameof(LoadDeploymentById));
        //public static Request<List<Deployment>> LoadDeploymentsHistory { get; set; } = new(nameof(LoadDeploymentsHistory));
        //public static Request<List<MdsCommon.Project>> LoadAllProjects { get; set; } = new Request<List<MdsCommon.Project>>(nameof(LoadAllProjects));
        //public static Request<List<InfrastructureNode>> LoadAllNodes { get; set; } = new (nameof(LoadAllNodes));
        //public static Request<List<InfrastructureService>> LoadAllServices { get; set; } = new(nameof(LoadAllServices));
        //public static Command<List<MdsCommon.ServiceConfigurationSnapshot>, InfrastructureConfiguration> ConfirmDeployment { get; set; } = new(nameof(ConfirmDeployment));
        //public static Request<List<ParameterType>> GetAllParameterTypes { get; set; } = new Request<List<ParameterType>>(nameof(GetAllParameterTypes));
        ////public static Command<InfrastructureConfiguration> SaveConfigurationSummary { get; set; } = new Command<InfrastructureConfiguration>(nameof(SaveConfigurationSummary));
        //public static Request<MdsCommon.ServiceConfigurationSnapshot, string> LoadServiceSnapshotByHash { get; set; } = new Request<MdsCommon.ServiceConfigurationSnapshot, string>(nameof(LoadServiceSnapshotByHash));
        ////public static Request<AllNodes> LoadAllNodes { get; set; } = new Request<AllNodes>(nameof(LoadAllNodes));
        //public static Request<List<EnvironmentType>> LoadEnvironmentTypes { get; set; } = new(nameof(LoadEnvironmentTypes));
        //public static Request<List<MdsCommon.MachineStatus>> LoadHealthStatus { get; set; } = new(nameof(LoadHealthStatus));
        //public static Request<List<MdsCommon.AlgorithmInfo>> GetRemoteBuilds { get; set; } = new Request<List<MdsCommon.AlgorithmInfo>>(nameof(GetRemoteBuilds));
        //public static Request<List<MdsCommon.AlgorithmInfo>, List<MdsCommon.AlgorithmInfo>> RegisterNewBinaries { get; set; } = new(nameof(RegisterNewBinaries));
        //public static Request<Deployment, Guid> LoadLastConfigurationDeployment { get; set; } = new Request<Deployment, Guid>(nameof(LoadLastConfigurationDeployment));
        //public static Command<string> RestartService { get; set; } = new(nameof(RestartService));
        //public static Command<MdsCommon.ProjectVersion> SaveVersionEnabled { get; set; } = new Command<MdsCommon.ProjectVersion>(nameof(SaveVersionEnabled));

        //public static Request<string> GetInfrastructureName { get; set; } = new Request<string>(nameof(GetInfrastructureName));

        //public static Request<List<NoteType>> GetAllNoteTypes { get; set; } = new Request<List<NoteType>>(nameof(GetAllNoteTypes));

        //public static Command<MachineStatus> StoreHealthStatus = new(nameof(StoreHealthStatus));

        public class State
        {
            
        }

        public static partial class Event
        {

        }

        public class InputArguments
        {
            public int UiPort { get; set; }
            public int InfrastructureApiPort { get; set; }
            public string DbPath { get; set; }
            public string LogFilePath { get; set; }

            public string BuildManagerRedisUrl { get; set; }
            public string InfrastructureName { get; set; }
            public string BuildManagerUrl { get; set; }
            public string BuildManagerNodeUrl { get; set; } // In case the node needs a different url than the infra. closed ports...

            public string SmtpHost { get; set; }
            public string From { get; set; }
            public string Password { get; set; }
            public string ErrorEmails { get; set; }
            public string CertificateThumbprint { get; set; }

            public string InfrastructureEventsInputChannel { get; set; }
            public string BinariesAvailableInputChannel { get; set; }
            public string HealthStatusInputChannel { get; set; }

            public string BroadcastDeploymentOutputChannel { get; set; }
            public string NodeCommandOutputChannel { get; set; }

            public string AdminUserName { get; set; }
            public string AdminPassword { get; set; }
        }


        public class MdsInfraReferences
        {
            public ApplicationSetup ApplicationSetup { get; set; }
            public ImplementationGroup ImplementationGroup { get; set; }
            public HttpServer.State HttpGateway { get; set; }
            //public RedisReader.State RedisReader { get; set; }
            //public RedisWriter.State RedisWriter { get; set; }
            //public RedisListener.State RedisListener { get; set; }
            //public RedisNotifier.State RedisNotifier { get; set; }
        }


        //public static HttpServer.DemandResultItem.HttpResponse ToJsonResponse(object response)
        //{
        //    return new HttpGateway.HttpListener.DemandResultItem.HttpResponse()
        //    {
        //        ContentType = "application/json",
        //        ResponseCode = 200,
        //        ResponseContent = Metapsi.Serialize.ToTypedJson(response)
        //    };
        //}

        public static async Task GatewayIsUp(this MdsInfraReferences references)
        {
            while (true)
            {
                if (references.HttpGateway.WebHostTask == null)
                {
                    await Task.Delay(500);
                }
                else break;
            }
        }
    }
}
