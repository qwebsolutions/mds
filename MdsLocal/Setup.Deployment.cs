using Metapsi;
using System;
using System.Linq;
using System.Net.Http;

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
            const string BinariesCacheTimerName = "CheckBinariesCache";

            var binariesCacheTimer = applicationSetup.AddBusinessState(new Timer.State());

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

                    e.Using(binariesCacheTimer, implementationGroup).EnqueueCommand(
                        Metapsi.Timer.SetTimer,
                        new Metapsi.Timer.Command.Set()
                        {
                            Name = BinariesCacheTimerName,
                            IntervalMilliseconds = Convert.ToInt32(TimeSpan.FromMinutes(1).TotalMilliseconds)
                        });
                });

            applicationSetup.MapEventIf<Metapsi.Timer.Event.Tick>(
                e => e.Name == BinariesCacheTimerName,
                e =>
                {
                    e.Using(binariesRetriever, implementationGroup).EnqueueCommand(ApiBinariesRetriever.CleanupBinaries);
                });

            applicationSetup.MapEvent<ApplicationIsShuttingDown>(
                e => e.Using(binariesCacheTimer, implementationGroup).EnqueueCommand(Metapsi.Timer.Shutdown));


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
