using Metapsi;
using System.Linq;

namespace MdsLocal
{
    public partial class MdsLocalApplication
    {
        public static void SetupDeployment(
            ApplicationSetup applicationSetup,
            ImplementationGroup implementationGroup,
            MdsCommon.InfrastructureNodeSettings infrastructureConfiguration,
            MdsLocalApplication.State localApp,
            EventBroadcaster.State eventBroadcaster,
            string binariesRepositoryFolder,
            string buildTarget)
        {
            var deploymentListener = applicationSetup.AddBusinessState(new RedisListener.State());
            var binariesRetriever = applicationSetup.AddBusinessState(new ApiBinariesRetriever.State()
            {
                ProjectArchivesBasePath = binariesRepositoryFolder,
                BuildTarget = buildTarget
            });

            applicationSetup.MapEvent<MdsLocalApplication.Event.GlobalControllerReached>(
                e =>
                {
                    e.Using(binariesRetriever, implementationGroup).EnqueueCommand(ApiBinariesRetriever.SetUrl, infrastructureConfiguration.BinariesApiUrl);

                    e.Logger.LogDebug($"Start listening for deployments on {infrastructureConfiguration.BroadcastDeploymentInputChannel}");

                    // Listen for deployment on infrastructure channel
                    e.Using(deploymentListener, implementationGroup).EnqueueCommand(
                        RedisListener.StartListening,
                        new Metapsi.RedisChannel(infrastructureConfiguration.BroadcastDeploymentInputChannel));
                });

            applicationSetup.MapEventIf<RedisListener.Event.NotificationReceived>(
                e => e.ChannelName == new RedisChannel(infrastructureConfiguration.BroadcastDeploymentInputChannel).ChannelName,
                e => e.Using(localApp, implementationGroup).EnqueueCommand(
                    MdsLocalApplication.SynchronizeConfiguration, 
                    "Infrastructure notification"));

            implementationGroup.MapRequest(MdsLocalApplication.GetProjectBinaries, async (rc, projectName, projectVersion, intoPath) =>
            {
                return await rc
                .Using(binariesRetriever, implementationGroup)
                .EnqueueRequest(ApiBinariesRetriever.RetrieveBinaries, projectName, projectVersion, intoPath);
            });
        }
    }
}
