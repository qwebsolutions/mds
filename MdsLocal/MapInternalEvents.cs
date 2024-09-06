using MdsCommon;
using Metapsi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsLocal;

public class ConfigurationChanged : IData
{
    public Guid DeploymentId { get; set; }
    public string InfrastructureName { get; set; }
    public string BinariesApiUrl { get; set; }
}

public class ProcessExited : IData
{
    public string ServiceName { get; set; }
    public int Pid { get; set; }
    public int ExitCode { get; set; }
    public string FullExePath { get; set; }
}

public static partial class MdsLocalApplication
{
    public class PendingStopTracker
    {
        public class PendingStop
        {
            public Guid DeploymentId { get; set; }
            public int Pid { get; set; }
        }

        public TaskQueue TaskQueue { get; set; } = new TaskQueue();
        public List<PendingStop> PendingStops { get; set; } = new();
    }

    public static void MapInternalEvents(
        this ApplicationSetup applicationSetup,
        ImplementationGroup ig,
        MdsLocalApplication.State appState,
        string fullDbPath,
        string buildTarget)
    {

        PendingStopTracker pendingStopTracker = new PendingStopTracker();

        applicationSetup.MapEvent<ConfigurationChanged>(e =>
        {
            e.Using(appState, ig).EnqueueCommand(async (CommandContext commandContext, MdsLocalApplication.State state) =>
            {
                await ServiceProcessExtensions.SyncServices(
                    commandContext,
                    appState.NodeName,
                    fullDbPath,
                    appState.ServicesBasePath,
                    appState.BaseDataFolder,
                    buildTarget,
                    e.EventData.BinariesApiUrl,
                    e.EventData.InfrastructureName,
                    pendingStopTracker,
                    e.EventData.DeploymentId);

                var healthStatus = await GetNodeStatus(commandContext, state.NodeName);
                commandContext.NotifyGlobal(healthStatus);
            });
        });

        applicationSetup.MapEvent<ProcessExited>(e =>
        {
            var _notAwaited = pendingStopTracker.TaskQueue.Enqueue(async () =>
            {
                var pendingStop = pendingStopTracker.PendingStops.SingleOrDefault(x => x.Pid == e.EventData.Pid);

                if (pendingStop == null)
                {
                    // Fatal crash, unrelated to a deployment
                    e.Using(appState, ig).EnqueueCommand(async (cc, state) =>
                    {
                        cc.NotifyGlobal(new ServiceCrash()
                        {
                            ExitCode = e.EventData.ExitCode,
                            NodeName = appState.NodeName,
                            ServiceName = e.EventData.ServiceName,
                            ServicePath = e.EventData.FullExePath
                        });

                        await Task.Delay(10000);

                        await ServiceProcessExtensions.StartServiceProcess(cc, e.EventData.ServiceName, appState.NodeName, appState.ServicesBasePath, Guid.Empty);
                    });
                }
                else
                {
                    // Process killed by controller
                    pendingStopTracker.PendingStops.Remove(pendingStop);

                    e.Using(appState, ig).EnqueueCommand(async (cc, state) =>
                    {
                        cc.NotifyGlobal(new DeploymentEvent.ServiceStop()
                        {
                            DeploymentId = pendingStop.DeploymentId,
                            NodeName = appState.NodeName,
                            ServiceName = e.EventData.ServiceName
                        });
                    });
                }
            });
        });
    }
}
