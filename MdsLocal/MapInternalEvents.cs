using MdsCommon;
using Metapsi;
using Metapsi.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace MdsLocal;

//public class ConfigurationChanged : IData
//{
//    public Guid DeploymentId { get; set; }
//    public string InfrastructureName { get; set; }
//    public string BinariesApiUrl { get; set; }
//}

//public class ProcessExited : IData
//{
//    public string ServiceName { get; set; }
//    public int Pid { get; set; }
//    public int ExitCode { get; set; }
//    public string FullExePath { get; set; }
//}

public class SyncResultBuilder: TaskQueue<SyncResult>
{
    public SyncResultBuilder(): base(new SyncResult())
    {

    }
}

public class LocalControllerSettings
{
    public string NodeName { get; set; }
    public string FullDbPath { get; set; }
    public string ServicesBasePath { get; set; }
    public string BaseDataFolder { get; set; }
    public string BuildTarget { get; set; }
    public string InfrastructureApiUrl { get; set; }
}

public static partial class MdsLocalApplication
{
    public class OsProcessTracker
    {
        public class PendingStop
        {
            public Guid DeploymentId { get; set; }
            public int Pid { get; set; }
        }

        public TaskQueue TaskQueue { get; set; } = new TaskQueue();
        public List<PendingStop> PendingStops { get; set; } = new();
    }

    public static async Task HandleProcessStop(
        OsProcessTracker osProcessTracker,
        SqliteQueue sqliteQueue,
        GlobalNotifier globalNotifier,
        System.Diagnostics.Process osProcess,
        ServiceProcess serviceProcess,
        LocalControllerSettings localControllerSettings)
    {
        var pendingStop = osProcessTracker.PendingStops.SingleOrDefault(x => x.Pid == osProcess.Id);

        if (pendingStop != null)
        {

            // Process killed by controller
            osProcessTracker.PendingStops.Remove(pendingStop);
            return;
        }

        await sqliteQueue.SaveInfrastructureEvent(new InfrastructureEvent()
        {
            Criticality = InfrastructureEventCriticality.Fatal,
            Source = serviceProcess.ServiceName,
            FullDescription = $"Service {serviceProcess.ServiceName} crashed. ({serviceProcess.FullExePath})",
            ShortDescription = "Service crash",
            Type = InfrastructureEventType.ProcessExit
        });

        await globalNotifier.NotifyGlobal(new ServiceCrash()
        {
            ExitCode = osProcess.ExitCode,
            NodeName = localControllerSettings.NodeName,
            ServiceName = serviceProcess.ServiceName,
            ServicePath = serviceProcess.FullExePath
        });
        await Task.Delay(10000);
        await ServiceProcessExtensions.StartServiceProcess(
            sqliteQueue,
            globalNotifier,
            osProcessTracker,
            serviceProcess.ServiceName,
            localControllerSettings,
            Guid.Empty);

        await sqliteQueue.SaveInfrastructureEvent(new InfrastructureEvent()
        {
            Criticality = InfrastructureEventCriticality.Info,
            Source = serviceProcess.ServiceName,
            FullDescription = $"Service {serviceProcess.ServiceName} started. ({serviceProcess.FullExePath})",
            ShortDescription = "Service recovery",
            Type = InfrastructureEventType.ProcessStart
        });

        await globalNotifier.NotifyGlobal(new ServiceRecovered()
        {
            NodeName = localControllerSettings.NodeName,
            ServiceName = serviceProcess.ServiceName,
            ServicePath = serviceProcess.FullExePath
        });
    }

    public static async Task NotifyStartStatus(
        SqliteQueue sqliteQueue, 
        LocalControllerSettings localControllerSettings,
        GlobalNotifier globalNotifier,
        OsProcessTracker osProcessTracker)
    {
        var nodeStartedEvent = new NodeEvent.Started()
        {
            NodeName = localControllerSettings.NodeName
        };

        foreach (var serviceName in await ServiceProcessExtensions.GetInstalledServices(localControllerSettings.ServicesBasePath))
        {
            nodeStartedEvent.InstalledServices.Add(serviceName);
            try
            {
                var serviceProcess = ServiceProcessExtensions.GetServiceProcess(localControllerSettings.NodeName, serviceName);
                if (serviceProcess == null)
                {
                    nodeStartedEvent.NotRunningServices.Add(serviceName);
                }
                else
                {
                    nodeStartedEvent.RunningServices.Add(serviceName);
                    ServiceProcessExtensions.AttachExitHandler(serviceProcess, localControllerSettings.NodeName, localControllerSettings.ServicesBasePath, async sp =>
                    {
                        await osProcessTracker.TaskQueue.Enqueue(async () =>
                        {
                            await HandleProcessStop(
                                osProcessTracker, 
                                sqliteQueue, 
                                globalNotifier, 
                                serviceProcess, 
                                sp, 
                                localControllerSettings);
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                nodeStartedEvent.Errors.Add(ex.Message);
            }
        }
        nodeStartedEvent.NodeStatus = await GetNodeStatus(localControllerSettings.NodeName);
        await globalNotifier.NotifyGlobal(nodeStartedEvent);

        await sqliteQueue.SaveInfrastructureEvent(new InfrastructureEvent()
        {
            Criticality = nodeStartedEvent.Errors.Any() ? InfrastructureEventCriticality.Warning : InfrastructureEventCriticality.Info,
            ShortDescription = $"Node started",
            Source = nodeStartedEvent.NodeName,
            Type = InfrastructureEventType.MdsLocalRestart,
            FullDescription = nodeStartedEvent.GetFullDescription()
        });
    }

    //public static void MapInternalEventsRemoveThis(
    //    this ApplicationSetup applicationSetup,
    //    ImplementationGroup ig,
    //    MdsLocalApplication.State appState,
    //    SqliteQueue sqliteQueue,
    //    string buildTarget)
    //{

    //    OsProcessTracker pendingStopTracker = new OsProcessTracker();

    //    applicationSetup.MapEvent<ApplicationRevived>(e =>
    //    {
    //        e.Using(appState, ig).EnqueueCommand(async (CommandContext commandContext, MdsLocalApplication.State state) =>
    //        {
               
    //        });
    //    });

    //    applicationSetup.MapEvent<ConfigurationChanged>(e =>
    //    {
    //        e.Using(appState, ig).EnqueueCommand(async (CommandContext commandContext, MdsLocalApplication.State state) =>
    //        {
    //            await ServiceProcessExtensions.SyncServices(
    //                commandContext,
    //                appState.NodeName,
    //                sqliteQueue,
    //                appState.ServicesBasePath,
    //                appState.BaseDataFolder,
    //                buildTarget,
    //                e.EventData.BinariesApiUrl,
    //                e.EventData.InfrastructureName,
    //                pendingStopTracker,
    //                e.EventData.DeploymentId);

    //            var healthStatus = await GetNodeStatus(commandContext, state.NodeName);
    //            commandContext.NotifyGlobal(healthStatus);
    //        });
    //    });

    //    applicationSetup.MapEvent<ProcessExited>(e =>
    //    {
    //        var _notAwaited = pendingStopTracker.TaskQueue.Enqueue(async () =>
    //        {
    //            var pendingStop = pendingStopTracker.PendingStops.SingleOrDefault(x => x.Pid == e.EventData.Pid);

    //            if (pendingStop == null)
    //            {
    //                // Fatal crash, unrelated to a deployment
    //                e.Using(appState, ig).EnqueueCommand(async (cc, state) =>
    //                {

    //                });
    //            }
    //            else
    //            {
    //                // Process killed by controller
    //                pendingStopTracker.PendingStops.Remove(pendingStop);

    //                // Global notification was already handled on the spot by the function that triggered the stop
    //            }
    //        });
    //    });
    //}
}
