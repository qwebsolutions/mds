﻿using Dapper;
using MdsCommon;
using Metapsi;
using Metapsi.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MdsInfrastructure;

public static class Cleanup
{
    private class TimerTick : IData
    {
        public int Hour { get; set; }
        public int Minute { get; set; }
    }

    private class StartCleanup : IData
    {
        public int KeepDeploymentsMaxCount { get; set; } = -1;
        public int KeepDeploymentsMaxDays { get; set; } = -1;
        public int KeepEventsMaxCount { get; set; } = -1;
        public int KeepEventsMaxDays { get; set; } = -1;
    }

    public class State
    {
        public System.Timers.Timer timer = new System.Timers.Timer(System.TimeSpan.FromSeconds(1));
        public DateTime LastCleanup { get; set; } = DateTime.MinValue;
    }

    public static void AddCleanup(
        this ApplicationSetup applicationSetup,
        ImplementationGroup implementationGroup,
        MdsInfrastructureApplication.State state,
        ServiceDoc.DbQueue dbQueue)
    {
        var cleanupState = applicationSetup.AddBusinessState(new Cleanup.State());

        applicationSetup.MapEvent<ApplicationRevived>(e =>
        {
            e.Using(cleanupState, implementationGroup).EnqueueCommand(async (cc, state) =>
            {
                cleanupState.timer.AutoReset = true;
                cleanupState.timer.Start();
                cleanupState.timer.Elapsed += (s, e) =>
                {
                    var now = DateTime.Now;
                    cleanupState.timer.Interval = 60000 - (now.Second * 1000 + now.Millisecond);
                    // In case it triggers at :59 will tick twice
                    if (now.Second == 0)
                    {
                        cc.PostEvent(new TimerTick()
                        {
                            Hour = now.Hour,
                            Minute = now.Minute
                        });
                        Console.WriteLine($"Timer tick {DateTime.Now}");
                    }
                };
            });
        });

        applicationSetup.MapEvent<TimerTick>(e =>
        {
            e.Using(cleanupState, implementationGroup).EnqueueCommand(async (cc, state) =>
            {
                var runDaily = await dbQueue.GetDocument<ConfigKey>(ConfigKey.CleanupRunDailyAt);
                if (runDaily == null)
                    return;

                if (string.IsNullOrWhiteSpace(runDaily.Value))
                    return;

                var segments = runDaily.Value.Split(":");
                if (segments.Length != 2)
                    return;

                var hour = Int32.Parse(segments.First());
                var minute = Int32.Parse(segments.Last());

                if (e.EventData.Hour == hour && e.EventData.Minute == minute)
                {
                    var keepDeploymentsMaxCount = await dbQueue.GetDocument<ConfigKey>(ConfigKey.CleanupDeploymentsKeepMaxCount);
                    var keepDeploymentsMaxDays = await dbQueue.GetDocument<ConfigKey>(ConfigKey.CleanupDeploymentsKeepMaxDays);
                    var keepEventsMaxCount = await dbQueue.GetDocument<ConfigKey>(ConfigKey.CleanupEventsKeepMaxCount);
                    var keepEventsMaxDays = await dbQueue.GetDocument<ConfigKey>(ConfigKey.CleanupEventsKeepMaxDays);

                    if (keepDeploymentsMaxDays != null || keepDeploymentsMaxCount != null || keepEventsMaxCount != null || keepEventsMaxDays != null)
                    {
                        // Cleanup has at least one configuration

                        StartCleanup startCleanup = new StartCleanup();

                        if (!string.IsNullOrEmpty(keepDeploymentsMaxDays.Value))
                        {
                            startCleanup.KeepDeploymentsMaxDays = Int32.Parse(keepDeploymentsMaxDays.Value);
                        }

                        if (!string.IsNullOrEmpty(keepDeploymentsMaxCount.Value))
                        {
                            startCleanup.KeepDeploymentsMaxCount = Int32.Parse(keepDeploymentsMaxCount.Value);
                        }

                        if (!string.IsNullOrWhiteSpace(keepEventsMaxCount.Value))
                        {
                            startCleanup.KeepEventsMaxCount = Int32.Parse(keepEventsMaxCount.Value);
                        }

                        if (!string.IsNullOrWhiteSpace(keepEventsMaxDays.Value))
                        {
                            startCleanup.KeepEventsMaxDays = Int32.Parse(keepEventsMaxDays.Value);
                        }

                        if (startCleanup.KeepDeploymentsMaxCount != -1 || startCleanup.KeepDeploymentsMaxDays != -1 || startCleanup.KeepEventsMaxCount != -1 || startCleanup.KeepEventsMaxDays != -1)
                        {
                            cc.PostEvent(startCleanup);
                        }
                    }
                }
            });
        });

        applicationSetup.MapEvent<StartCleanup>(e =>
        {
            e.Using(state, implementationGroup).EnqueueCommand(async (commandContext, state) =>
            {
                try
                {
                    var allDeploymentHeaders = new List<Deployment>(await dbQueue.SqliteQueue.WithRollback(async t => await t.LoadRecords<Deployment>()));
                    allDeploymentHeaders = allDeploymentHeaders.OrderByDescending(x => x.Timestamp).ToList();

                    var removedDeploymentsCount = 0;

                    // Most recent is active, never delete
                    if (allDeploymentHeaders.Any())
                    {
                        var activeDeploymentId = allDeploymentHeaders.First().Id;

                        allDeploymentHeaders = allDeploymentHeaders.Skip(1).ToList();

                        List<Guid> toRemoveIds = new();

                        if (e.EventData.KeepDeploymentsMaxCount >= 0)
                        {
                            toRemoveIds.AddRange(allDeploymentHeaders.Skip(e.EventData.KeepDeploymentsMaxCount).Select(x => x.Id));
                        }

                        if (e.EventData.KeepDeploymentsMaxDays > 0)
                        {
                            toRemoveIds.AddRange(allDeploymentHeaders.Where(x => x.Timestamp < DateTime.Now.AddDays(-1 * e.EventData.KeepDeploymentsMaxDays)).Select(x => x.Id));
                        }

                        toRemoveIds = toRemoveIds.Distinct().ToList();

                        // Now it gets tricky. We need to remove snapshots only if they are not used by any deployment transition anymore

                        await dbQueue.SqliteQueue.WithCommit(
                            async c =>
                            {
                                foreach (var deploymentId in toRemoveIds)
                                {
                                    if (deploymentId != activeDeploymentId)
                                    {
                                        await c.DeleteRecords<DbDeploymentEvent, Guid>(x => x.DeploymentId, new List<Guid>() { deploymentId });
                                        await c.DeleteRecords<DeploymentServiceTransition, Guid>(x => x.DeploymentId, new List<Guid>() { deploymentId });
                                        await c.DeleteRecord<Deployment>(deploymentId);
                                    }
                                }

                                var allRemainingTransitions = await c.LoadRecords<DeploymentServiceTransition>();
                                var allSnapshotIds = await c.Connection.QueryAsync<Guid>($"select Id from {nameof(ServiceConfigurationSnapshot)}", transaction: c);
                                var stillUsedSnapshotIds = new List<Guid>();
                                stillUsedSnapshotIds.AddRange(allRemainingTransitions.Select(x => x.FromServiceConfigurationSnapshotId));
                                stillUsedSnapshotIds.AddRange(allRemainingTransitions.Select(x => x.ToServiceConfigurationSnapshotId));
                                stillUsedSnapshotIds = stillUsedSnapshotIds.Where(x => x != Guid.Empty).Distinct().ToList();

                                var notUsedSnapshotIds = allSnapshotIds.Except(stillUsedSnapshotIds);

                                foreach (var notUsedSnapshotId in notUsedSnapshotIds)
                                {
                                    await c.DeleteRecord<ServiceConfigurationSnapshot>(notUsedSnapshotId);
                                    await c.DeleteRecords<ServiceConfigurationSnapshotParameter, Guid>(x => x.ServiceConfigurationSnapshotId, new List<Guid>() { notUsedSnapshotId });
                                }
                            });

                        removedDeploymentsCount = toRemoveIds.Count;
                    }

                    var removedEventsCount = await dbQueue.SqliteQueue.CleanupInfrastructureEvents(e.EventData.KeepEventsMaxCount, e.EventData.KeepEventsMaxDays);

                    if (e.EventData.KeepEventsMaxCount >= 0 || e.EventData.KeepEventsMaxDays >= 0)
                    {
                        var allNodes = await Db.LoadAllNodes(dbQueue.SqliteQueue);
                        foreach (var node in allNodes)
                        {
                            commandContext.NotifyNode(node.NodeName, new CleanupInfrastructureEvents()
                            {
                                KeepMaxCount = e.EventData.KeepEventsMaxCount,
                                KeepMaxDays = e.EventData.KeepEventsMaxDays
                            });
                        }
                    }

                    if (removedDeploymentsCount > 0 || removedEventsCount > 0)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        if (removedDeploymentsCount == 1)
                        {
                            stringBuilder.AppendLine($"{removedDeploymentsCount} deployment deleted");
                        }
                        else if (removedDeploymentsCount > 1)
                        {
                            stringBuilder.AppendLine($"{removedDeploymentsCount} deployments deleted");
                        }

                        if (removedEventsCount == 1)
                        {
                            stringBuilder.AppendLine($"{removedEventsCount} infrastructure event deleted");
                        }
                        else if (removedEventsCount > 1)
                        {
                            stringBuilder.AppendLine($"{removedEventsCount} infrastructure events deleted");
                        }

                        var cleanupCompleteEvent = new InfrastructureEvent()
                        {
                            ShortDescription = "Cleanup complete",
                            Source = "Cleanup job",
                            FullDescription = stringBuilder.ToString()
                        };

                        await dbQueue.SqliteQueue.SaveInfrastructureEvent(cleanupCompleteEvent);
                    }
                }
                catch (Exception ex)
                {
                    await dbQueue.SqliteQueue.SaveInfrastructureEvent(new InfrastructureEvent()
                    {
                        ShortDescription = "Deployments cleanup error",
                        FullDescription = ex.Message,
                        Source = "Cleanup job"
                    });
                }
            });
        });
    }

    private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        throw new System.NotImplementedException();
    }
}
