using MdsCommon;
using Metapsi;

namespace MdsLocal
{
    //public partial class MdsLocalApplication
    //{
    //    public static void SetupHealth(
    //        ApplicationSetup applicationSetup,
    //        ImplementationGroup implementationGroup,
    //        MdsCommon.InfrastructureNodeSettings configuration,
    //        MdsLocalApplication.State localApp,
    //        string nodeName)
    //    {
    //        var healthTimer = applicationSetup.AddBusinessState(new Timer.State());
    //        var healthNotifier = applicationSetup.AddBusinessState(new RedisNotifier.State());

    //        const string HealthPing = "HealthPing";

    //        applicationSetup.MapEvent<MdsLocalApplication.Event.GlobalControllerReached>(
    //            e =>
    //            {
    //                if (!string.IsNullOrEmpty(e.EventData.InfrastructureConfiguration.HealthStatusOutputChannel))
    //                {

    //                    e.Using(healthTimer, implementationGroup).EnqueueCommand(
    //                        Metapsi.Timer.SetTimer,
    //                        new Metapsi.Timer.Command.Set()
    //                        {
    //                            Name = HealthPing,
    //                            IntervalMilliseconds = 30000
    //                        });
    //                }
    //            });

    //        applicationSetup.MapEvent<ApplicationIsShuttingDown>(
    //            e => e.Using(healthTimer, implementationGroup).EnqueueCommand(Metapsi.Timer.RemoveTimer, new Metapsi.Timer.Command.Remove() { Name = HealthPing }));

    //        applicationSetup.MapEventIf<Metapsi.Timer.Event.Tick>(
    //            e => e.Name == HealthPing,
    //            e =>
    //            {
    //                e.Using(localApp, implementationGroup).EnqueueCommand(MdsLocalApplication.CheckMachineStatus);
    //            });

    //        // Forward status to global controller
    //        // Use redis notification so we don't get exceptions while 
    //        // infrastructure controller is restarted
    //        applicationSetup.MapEvent<MdsLocalApplication.Event.HealthPing>(
    //            e =>
    //            {
    //                var hs = e.EventData.HealthStatus;

    //                var serializable = MdsCommon.NodeStatus.ToSerializable(hs);

    //                if (serializable != null && !string.IsNullOrEmpty(serializable.MachineName))
    //                {
    //                    e.Using(healthNotifier, implementationGroup).EnqueueCommand(
    //                        RedisNotifier.NotifyChannel,
    //                        new RedisChannelMessage(
    //                            configuration.HealthStatusOutputChannel,
    //                            nameof(MdsCommon.NodeStatus),
    //                             Metapsi.Serialize.ToJson(serializable)));
    //                    e.Using(healthNotifier, implementationGroup).EnqueueCommand(async (cc, state) =>
    //                    {
    //                        cc.NotifyGlobal(hs);
    //                    });
    //                }
    //            });
    //    }
    //}
}
