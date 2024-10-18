using MdsCommon;
using Metapsi;
using Metapsi.Sqlite;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MdsLocal;

public class GlobalNotifier
{
    HttpClient httpClient = new HttpClient();

    private string infrastructureEventsUrl = string.Empty;

    public GlobalNotifier(string infrastructureApiBaseUrl)
    {
        this.infrastructureEventsUrl = infrastructureApiBaseUrl.Trim('/') + "/event";
    }

    public async Task NotifyGlobal(object message)
    {
        var response = await httpClient.PostMessage(this.infrastructureEventsUrl, message);
        response.EnsureSuccessStatusCode();
    }
}

public static partial class MdsLocalApplication
{
    public static void MapIncomingEvents(
        this IEndpointRouteBuilder endpoint,
        LocalControllerSettings localControllerSettings,
        SqliteQueue dbQueue,
        OsProcessTracker osProcessTracker,
        GlobalNotifier notifier)
    {
        endpoint.OnMessage<NodeConfigurationUpdate>(async (newConfiguration) =>
        {
            System.Diagnostics.Debug.WriteLine($"NodeConfiguration update: {Metapsi.Serialize.ToJson(newConfiguration)}");
            await LocalDb.SetNewConfiguration(dbQueue, newConfiguration.Snapshots.Where(snapshot => snapshot.NodeName == localControllerSettings.NodeName).ToList());
            await ServiceProcessExtensions.SyncServices(
                notifier,
                dbQueue,
                localControllerSettings,
                localControllerSettings.BuildTarget,
                newConfiguration.BinariesApiUrl,
                newConfiguration.InfrastructureName,
                osProcessTracker,
                newConfiguration.DeploymentId);

            var healthStatus = await GetNodeStatus(localControllerSettings.NodeName);
            await notifier.NotifyGlobal(healthStatus);
        });

        endpoint.OnMessage<CleanupInfrastructureEvents>(async (cleanup) =>
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
