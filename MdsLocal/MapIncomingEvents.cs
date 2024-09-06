using MdsCommon;
using Metapsi;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;

namespace MdsLocal;

public static partial class MdsLocalApplication
{
    public static void MapIncomingEvents(
        this IEndpointRouteBuilder endpoint,
        string nodeName,
        TaskQueue dbQueue,
        string fullDbPath)
    {
        endpoint.OnMessage<NodeConfigurationUpdate>(async (commandContext, newConfiguration) =>
        {
            await dbQueue.Enqueue(async () => await LocalDb.SetNewConfiguration(fullDbPath, newConfiguration.Snapshots.Where(snapshot => snapshot.NodeName == nodeName).ToList()));
            commandContext.PostEvent(new ConfigurationChanged()
            {
                BinariesApiUrl = newConfiguration.BinariesApiUrl,
                DeploymentId = newConfiguration.DeploymentId,
                InfrastructureName = newConfiguration.InfrastructureName
            });
        });
    }
}
