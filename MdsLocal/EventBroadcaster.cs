using Metapsi;
using System.Threading.Tasks;

namespace MdsLocal
{
    public static class EventBroadcaster
    {
        public static Command<MdsCommon.InfrastructureEvent> BroadcastEvent { get; set; } = new Command<MdsCommon.InfrastructureEvent>(nameof(BroadcastEvent));

        public class State
        {

        }
        
        public static async Task Broadcast(CommandContext commandContext, State state, MdsCommon.InfrastructureEvent infrastructureEvent)
        {
            await commandContext.Do(BroadcastEvent, infrastructureEvent);
        }
    }
}
