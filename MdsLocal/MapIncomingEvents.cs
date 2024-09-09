using MdsCommon;
using Metapsi;
using Metapsi.Sqlite;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdsLocal;

public static partial class MdsLocalApplication
{
    public static void MapIncomingEvents(
        this IEndpointRouteBuilder endpoint,
        string nodeName,
        SqliteQueue dbQueue)
    {
        endpoint.OnMessage<NodeConfigurationUpdate>(async (commandContext, newConfiguration) =>
        {
            await LocalDb.SetNewConfiguration(dbQueue, newConfiguration.Snapshots.Where(snapshot => snapshot.NodeName == nodeName).ToList());
            commandContext.PostEvent(new ConfigurationChanged()
            {
                BinariesApiUrl = newConfiguration.BinariesApiUrl,
                DeploymentId = newConfiguration.DeploymentId,
                InfrastructureName = newConfiguration.InfrastructureName
            });
        });

        endpoint.OnMessage<CleanupInfrastructureEvents>(async (commandContext, cleanup) =>
        {
            var removed = await dbQueue.CleanupInfrastructureEvents(cleanup.KeepMaxCount, cleanup.KeepMaxDays);

            if (removed > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (removed == 1)
                {
                    stringBuilder.AppendLine($"{removed} infrastructure event deleted");
                }
                else if (removed > 1)
                {
                    stringBuilder.AppendLine($"{removed} infrastructure events deleted");
                }

                var cleanupCompleteEvent = new InfrastructureEvent()
                {
                    ShortDescription = "Cleanup complete",
                    Source = "Cleanup job",
                    FullDescription = stringBuilder.ToString()
                };

                await dbQueue.SaveInfrastructureEvent(cleanupCompleteEvent);
            }
        });
    }
}
