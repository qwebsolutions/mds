using Metapsi;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MdsLocal
{
    public static partial class MdsLocalApplication
    {
        public static async Task RegisterCrash(CommandContext commandContext, State state, ServiceCrashEvent serviceCrash)
        {
            string serviceName = serviceCrash.ServiceName;
            state.ServiceCrashEvents.Add(serviceCrash);

            var crashEventsOfThisServiceInLastMinute = state.ServiceCrashEvents.Where(x => x.ServiceName == serviceName && x.CrashTimestamp > DateTime.UtcNow.AddMinutes(-1));

            if (crashEventsOfThisServiceInLastMinute.Count() >= state.DropAtCrashesPerMinute)
            {
                var firstEvent = crashEventsOfThisServiceInLastMinute.OrderBy(x => x.CrashTimestamp).First();

                commandContext.PostEvent(new UnstableServiceDropped()
                {
                    ServiceName = serviceName,
                    InSeconds = Convert.ToInt32((DateTime.UtcNow - firstEvent.CrashTimestamp).TotalSeconds),
                    RestartCount = crashEventsOfThisServiceInLastMinute.Count()
                });

                state.DroppedServices.Add(serviceCrash.ServiceName);
                commandContext.Logger.LogDebug($"Register crash: {serviceName}, crash events: {crashEventsOfThisServiceInLastMinute.Count()}");
            }
            else
            {
                await CheckServiceDbLogMessages(commandContext, state, serviceName);
            }
        }
    }
}
