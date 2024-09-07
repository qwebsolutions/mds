using MdsCommon;
using Metapsi;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
        DbQueue dbQueue,
        string buildTarget)
    {

        PendingStopTracker pendingStopTracker = new PendingStopTracker();

        applicationSetup.MapEvent<ApplicationRevived>(e =>
        {
            e.Using(appState, ig).EnqueueCommand(async (CommandContext commandContext, MdsLocalApplication.State state) =>
            {
                var nodeStartedEvent = new NodeEvent.Started()
                {
                    NodeName = state.NodeName
                };

                foreach (var serviceName in await ServiceProcessExtensions.GetInstalledServices(state.ServicesBasePath))
                {
                    nodeStartedEvent.InstalledServices.Add(serviceName);
                    try
                    {
                        var serviceProcess = ServiceProcessExtensions.GetServiceProcess(state.NodeName, serviceName);
                        if (serviceProcess == null)
                        {
                            nodeStartedEvent.NotRunningServices.Add(serviceName);
                        }
                        else
                        {
                            nodeStartedEvent.RunningServices.Add(serviceName);
                            ServiceProcessExtensions.AttachExitHandler(serviceProcess, state.NodeName, state.ServicesBasePath, sp =>
                            {
                                commandContext.PostEvent(new ProcessExited()
                                {
                                    ExitCode = serviceProcess.ExitCode,
                                    ServiceName = serviceName,
                                    FullExePath = sp.FullExePath,
                                    Pid = sp.Pid
                                });
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        nodeStartedEvent.Errors.Add(ex.Message);
                    }
                }
                nodeStartedEvent.NodeStatus = await GetNodeStatus(commandContext, state.NodeName);
                commandContext.NotifyGlobal(nodeStartedEvent);
            });
        });

        applicationSetup.MapEvent<ConfigurationChanged>(e =>
        {
            e.Using(appState, ig).EnqueueCommand(async (CommandContext commandContext, MdsLocalApplication.State state) =>
            {
                await ServiceProcessExtensions.SyncServices(
                    commandContext,
                    appState.NodeName,
                    dbQueue,
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
                        await dbQueue.SaveInfrastructureEvent(new InfrastructureEvent()
                        {
                            Criticality = InfrastructureEventCriticality.Fatal,
                            Source = e.EventData.ServiceName,
                            FullDescription = $"Service {e.EventData.ServiceName} crashed. ({e.EventData.FullExePath})",
                            ShortDescription = "Service crash",
                            Type = InfrastructureEventType.ProcessExit
                        });

                        cc.NotifyGlobal(new ServiceCrash()
                        {
                            ExitCode = e.EventData.ExitCode,
                            NodeName = appState.NodeName,
                            ServiceName = e.EventData.ServiceName,
                            ServicePath = e.EventData.FullExePath
                        });

                        await Task.Delay(10000);

                        await ServiceProcessExtensions.StartServiceProcess(cc, e.EventData.ServiceName, appState.NodeName, appState.ServicesBasePath, Guid.Empty);

                        await dbQueue.SaveInfrastructureEvent(new InfrastructureEvent()
                        {
                            Criticality = InfrastructureEventCriticality.Info,
                            Source = e.EventData.ServiceName,
                            FullDescription = $"Service {e.EventData.ServiceName} started. ({e.EventData.FullExePath})",
                            ShortDescription = "Service recovery",
                            Type = InfrastructureEventType.ProcessStart
                        });

                        cc.NotifyGlobal(new ServiceRecovered()
                        {
                            NodeName = appState.NodeName,
                            ServiceName = e.EventData.ServiceName,
                            ServicePath = e.EventData.FullExePath,
                        });
                    });
                }
                else
                {
                    // Process killed by controller
                    pendingStopTracker.PendingStops.Remove(pendingStop);

                    // Global notification was already handled on the spot by the function that triggered the stop
                }
            });
        });
    }
}
