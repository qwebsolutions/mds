using Metapsi;

namespace MdsLocal
{
    //public static partial class MdsLocalApplication
    //{
    //    public static EventBroadcaster.State SetupEventBroadcaster(
    //        ApplicationSetup applicationSetup,
    //        ImplementationGroup implementationGroup,
    //        string fullDbPath,
    //        RedisNotifier.State infrastructureEventNotifier,
    //        MdsCommon.InfrastructureNodeSettings infrastructureConfiguration)
    //    {
    //        var eventsQueue = applicationSetup.AddRedisSender(implementationGroup);

    //        var eventBroadcaster = applicationSetup.AddBusinessState(new EventBroadcaster.State()
    //        {

    //        });

    //        implementationGroup.MapCommand(EventBroadcaster.BroadcastEvent,
    //            async (rc, infraEvent) =>
    //            {
    //                await MdsCommon.Db.SaveInfrastructureEvent(fullDbPath, infraEvent);

    //                if (!string.IsNullOrEmpty(infrastructureConfiguration.InfrastructureEventsOutputChannel))
    //                {
    //                    await rc.Using(infrastructureEventNotifier, implementationGroup).EnqueueCommand(
    //                        RedisNotifier.NotifyChannel,
    //                        new RedisChannelMessage(
    //                            infrastructureConfiguration.InfrastructureEventsOutputChannel,
    //                            nameof(MdsCommon.InfrastructureEvent),
    //                            Metapsi.Serialize.ToJson(infraEvent)));
    //                }
    //            });

    //        return eventBroadcaster;
    //    }
    //}
}
