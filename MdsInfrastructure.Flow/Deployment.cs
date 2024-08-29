using MdsCommon;
using Metapsi;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure.Flow
{
    public static class Deployment
    {
        public class List : Metapsi.Http.Get<Routes.Deployment.List>
        {
            public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
            {
                return Page.Result(new DeploymentHistory()
                {
                    Deployments = await commandContext.Do(Backend.LoadDeploymentsHistory),
                    User = httpContext.User()
                });
            }
        }

        public class Review : Metapsi.Http.Get<Routes.Deployment.Review, Guid>
        {
            public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext, Guid deploymentId)
            {
                var deploymentReview = await Load(commandContext, deploymentId);
                deploymentReview.User = httpContext.User();
                return Page.Result(deploymentReview);
            }

            public static async Task<DeploymentReview> Load(CommandContext commandContext, Guid deploymentId)
            {
                var deployment = await commandContext.Do(Backend.LoadDeploymentById, deploymentId);

                var fromSnapshotIds = deployment.Transitions.Select(x => x.FromServiceConfigurationSnapshotId);
                var toSnapshotIds = deployment.Transitions.Select(x => x.ToServiceConfigurationSnapshotId);

                var changes = ChangesReport.Get(deployment.Transitions.Select(x => x.FromSnapshot).Where(x => x != null).ToList(), deployment.GetDeployedServices().ToList());

                return new DeploymentReview()
                {
                    ChangesReport = changes,
                    Deployment = deployment,
                };
            }
        }

        public class Preview : Metapsi.Http.Get<Routes.Deployment.ConfigurationPreview, Guid>
        {
            public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext, Guid configurationId)
            {
                var savedConfiguration = await commandContext.Do(Backend.LoadConfiguration, configurationId);
                var serverModel = await MdsInfrastructureFunctions.InitializeEditConfiguration(commandContext, savedConfiguration);

                var currentDeployment = await commandContext.Do(Backend.LoadCurrentDeployment);
                if (currentDeployment == null)
                    currentDeployment = new MdsInfrastructure.Deployment();

                var snapshot = await MdsInfrastructureFunctions.TakeConfigurationSnapshot(
                    commandContext,
                    savedConfiguration,
                    serverModel.AllProjects,
                    serverModel.InfrastructureNodes);

                var changesReport = MdsInfrastructure.ChangesReport.Get(currentDeployment.GetDeployedServices().ToList(), snapshot);
                return Page.Result(new DeploymentPreview()
                {
                    SavedConfiguration = savedConfiguration,
                    ChangesReport = changesReport,
                    Deployment = currentDeployment,
                    User = httpContext.User()
                });
            }
        }
    }
}
