using MdsCommon;
using Metapsi;
using Metapsi.SignalR;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;

namespace MdsInfrastructure
{
    public static partial class MdsInfrastructureApplication
    {
        public static void MapIncomingEvents(this IEndpointRouteBuilder endpoint)
        {
            endpoint.OnMessage<DeploymentEvent.DeploymentComplete>(async (cc, message) =>
            {
                await cc.Do(Backend.SaveDeploymentEvent, new DbDeploymentEvent()
                {
                    DeploymentId = message.DeploymentId,
                    EventType = nameof(DeploymentEvent.DeploymentComplete),
                });
                await DefaultMetapsiSignalRHub.HubContext.Clients.All.RaiseEvent(message);
            });

            endpoint.OnMessage<DeploymentEvent.ServiceStop>(async (cc, message) =>
            {
                await cc.Do(Backend.SaveDeploymentEvent, new DbDeploymentEvent()
                {
                    DeploymentId = message.DeploymentId,
                    EventType = nameof(DeploymentEvent.ServiceStop),
                    ServiceName = message.ServiceName
                });
                await DefaultMetapsiSignalRHub.HubContext.Clients.All.RaiseEvent(new RefreshDeploymentReviewModel());
            });

            endpoint.OnMessage<DeploymentEvent.ServiceStart>(async (cc, message) =>
            {
                await cc.Do(Backend.SaveDeploymentEvent, new DbDeploymentEvent()
                {
                    DeploymentId = message.DeploymentId,
                    EventType = nameof(DeploymentEvent.ServiceStart),
                    ServiceName = message.ServiceName
                });

                await DefaultMetapsiSignalRHub.HubContext.Clients.All.RaiseEvent(new RefreshDeploymentReviewModel());
            });

            endpoint.OnMessage<DeploymentEvent.ServiceInstall>(async (cc, message) =>
            {
                await cc.Do(Backend.SaveDeploymentEvent, new DbDeploymentEvent()
                {
                    DeploymentId = message.DeploymentId,
                    EventType = nameof(DeploymentEvent.ServiceInstall),
                    ServiceName = message.ServiceName
                });

                await DefaultMetapsiSignalRHub.HubContext.Clients.All.RaiseEvent(new RefreshDeploymentReviewModel());
            });

            endpoint.OnMessage<DeploymentEvent.ServiceUninstall>(async (cc, message) =>
            {
                await cc.Do(Backend.SaveDeploymentEvent, new DbDeploymentEvent()
                {
                    DeploymentId = message.DeploymentId,
                    EventType = nameof(DeploymentEvent.ServiceUninstall),
                    ServiceName = message.ServiceName
                });

                await DefaultMetapsiSignalRHub.HubContext.Clients.All.RaiseEvent(new RefreshDeploymentReviewModel());
            });

            endpoint.OnMessage<DeploymentEvent.ServiceSynchronized>(async (cc, message) =>
            {
                await cc.Do(Backend.SaveDeploymentEvent, new DbDeploymentEvent()
                {
                    DeploymentId = message.DeploymentId,
                    EventType = nameof(DeploymentEvent.ServiceSynchronized),
                    ServiceName = message.ServiceName,
                    Error = message.Error
                });

                var deployment = await cc.Do(Backend.LoadDeploymentById, message.DeploymentId);
                var deploymentEvents = await cc.Do(Backend.LoadDeploymentEvents, message.DeploymentId);
                var changes = ChangesReport.Get(deployment.Transitions.Select(x => x.FromSnapshot).Where(x => x != null).ToList(), deployment.GetDeployedServices().ToList());

                bool complete = false;
                if (changes.ServiceChanges.Count == deploymentEvents.Where(x => x.EventType == nameof(DeploymentEvent.ServiceSynchronized)).Count())
                {
                    await cc.Do(Backend.SaveDeploymentEvent, new DbDeploymentEvent()
                    {
                        DeploymentId = message.DeploymentId,
                        EventType = nameof(DeploymentEvent.DeploymentComplete)
                    });
                    complete = true;
                }

                await DefaultMetapsiSignalRHub.HubContext.Clients.All.RaiseEvent(new RefreshDeploymentReviewModel());
                if (complete)
                {
                    var hasErrors = deploymentEvents.Any(x => !string.IsNullOrEmpty(x.Error));
                    await DefaultMetapsiSignalRHub.HubContext.Clients.All.RaiseEvent(new DeploymentEvent.DeploymentComplete()
                    {
                        DeploymentId = message.DeploymentId,
                        HasErrors = hasErrors
                    });
                }
            });

            endpoint.OnMessage<MachineStatus>(async (cc, message) =>
            {
                await cc.Do(Backend.StoreHealthStatus, message);
                await DefaultMetapsiSignalRHub.HubContext.Clients.All.RaiseEvent(new RefreshInfrastructureStatusModel());
            });

            endpoint.OnMessage<InfrastructureEvent>(async (cc, message) =>
            {
                await cc.Do(MdsCommon.Api.SaveInfrastructureEvent, message);
            });

            endpoint.OnMessage<ServiceCrash>(async (cc, message) =>
            {
                await cc.Do(MdsCommon.Api.SaveInfrastructureEvent, new InfrastructureEvent()
                {
                    Criticality = InfrastructureEventCriticality.Fatal,
                    Source = message.ServiceName,
                    Timestamp = MetapsiDateTime.FromRoundTrip(message.TimestampIso),
                    FullDescription = $"Service {message.ServiceName} crashed. ({message.NodeName} {message.ServicePath})",
                    ShortDescription = "Service crash",
                    Type = InfrastructureEventType.ProcessExit
                });
            });

            endpoint.OnMessage<ServiceRecovered>(async (cc, message) =>
            {
                await cc.Do(MdsCommon.Api.SaveInfrastructureEvent, new InfrastructureEvent()
                {
                    Criticality = InfrastructureEventCriticality.Info,
                    Source = message.ServiceName,
                    Timestamp = MetapsiDateTime.FromRoundTrip(message.TimestampIso),
                    FullDescription = $"Service {message.ServiceName} started. ({message.NodeName} {message.ServicePath})",
                    ShortDescription = "Service recovery",
                    Type = InfrastructureEventType.ProcessStart
                });
            });
        }
    }
}
