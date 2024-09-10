using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System;
using System.Collections.Generic;
using MdsCommon;
using Metapsi;
using Metapsi.Sqlite;
using System.Transactions;
using System.Data.Common;

namespace MdsInfrastructure
{
    public static class Db
    {
        public static async Task<InfrastructureStatusData> LoadInfrastructureStatus(
            SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(async c =>
            {
                var activeDeployment = await c.LoadActiveDeployment();
                var currentConfiguration = 
                activeDeployment.ConfigurationHeaderId == Guid.Empty ?
                new InfrastructureConfiguration() : await c.LoadSpecificConfiguration(activeDeployment.ConfigurationHeaderId);

                return new InfrastructureStatusData()
                {
                    Deployment = activeDeployment,
                    HealthStatus = await c.LoadFullInfrastructureHealthStatus(),
                    InfrastructureEvents = await c.LoadAllInfrastructureEvents(),
                    InfrastructureConfiguration = currentConfiguration,
                    InfrastructureNodes = (await c.LoadRecords<InfrastructureNode>()).ToList()
                };
            });
        }

        public static async Task<List<Deployment>> LoadDeploymentHistory(
            SqliteQueue sqliteQueue)
        {
            var records = await sqliteQueue.WithRollback(async t => await t.LoadRecords<Deployment>());
            return records.OrderByDescending(x => x.Timestamp).ToList();
        }

        public static async Task<List<Project>> LoadAllProjects(
            SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(async t => await t.LoadStructures(Project.Data));
        }

        public static async Task<List<InfrastructureNode>> LoadAllNodes(this SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(async x =>
            {
                return (await x.LoadRecords<InfrastructureNode>()).ToList();
            });
        }

        public static async Task<List<InfrastructureService>> LoadAllServices(
            SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(async x =>
            {
                return (await x.LoadRecords<InfrastructureService>()).ToList();
            });
        }

        /// <summary>
        /// Stores & returns the algorithm build results for binaries that are not already registered
        /// </summary>
        /// <param name="fullDbPath"></param>
        /// <param name="remoteBinaries"></param>
        /// <returns></returns>
        public static async Task<List<AlgorithmInfo>> RefreshBinaries(SqliteQueue sqliteQueue, List<MdsCommon.AlgorithmInfo> remoteBinaries)
        {
            List<AlgorithmInfo> newBinaries = new List<AlgorithmInfo>();

            await sqliteQueue.WithCommit(async c =>
            {
                var allProjects = await c.LoadStructures(Project.Data);

                // Delete old entries
                foreach (var project in allProjects)
                {
                    foreach (var version in new List<ProjectVersion>(project.Versions))
                    {
                        foreach (var binaries in new List<ProjectVersionBinaries>(version.Binaries))
                        {
                            var stillValid = remoteBinaries.FirstOrDefault(x => x.Name == project.Name && x.Version == version.VersionTag && x.Target == binaries.Target);
                            if (stillValid == null)
                            {
                                await c.DeleteRecord(binaries);
                                version.Binaries.Remove(binaries);
                            }
                        }
                        if (!version.Binaries.Any())
                        {
                            await c.DeleteRecord(version);
                            project.Versions.Remove(version);
                        }
                    }

                    if (!project.Versions.Any())
                    {
                        await c.DeleteRecord(project);
                    }
                }

                // Add new entries

                foreach (AlgorithmInfo algorithmInfo in remoteBinaries)
                {
                    var project = await c.LoadRecord((Project x) => x.Name, algorithmInfo.Name);

                    if (project == null)
                    {
                        project = new MdsCommon.Project()
                        {
                            Enabled = true,
                            Name = algorithmInfo.Name
                        };

                        await c.InsertRecord(project);
                    }
                    var versions = await c.LoadRecords((ProjectVersion x) => x.VersionTag, algorithmInfo.Version);
                    var version = versions.SingleOrDefault(x => x.ProjectId == project.Id);
                    if (version == null)
                    {
                        version = new MdsCommon.ProjectVersion()
                        {
                            ProjectId = project.Id,
                            Enabled = true,
                            VersionTag = algorithmInfo.Version,
                        };

                        await c.InsertRecord(version);
                    }

                    version = await c.LoadStructure(ProjectVersion.Data, version.Id);

                    var build = version.Binaries.SingleOrDefault(x => x.ProjectVersionId == version.Id && x.Target == algorithmInfo.Target);

                    if (build == null)
                    {
                        build = new MdsCommon.ProjectVersionBinaries()
                        {
                            BuildNumber = algorithmInfo.BuildNumber,
                            ProjectVersionId = version.Id,
                            Target = algorithmInfo.Target
                        };
                        await c.InsertRecord(build);
                        newBinaries.Add(algorithmInfo);
                    }
                }
            });

            return newBinaries;
        }

        public static async Task SaveVersionEnabled(SqliteQueue sqliteQueue, ProjectVersion projectVersion)
        {
            await sqliteQueue.WithCommit(async t =>
            {
                await t.Connection.ExecuteAsync("update ProjectVersion set Enabled = @Enabled where Id = @Id", projectVersion, t);
            });
        }

        public static async Task SaveConfiguration(
            SqliteQueue sqliteQueue,
            InfrastructureConfiguration singleConfiguration)
        {
            await sqliteQueue.WithCommit(async c =>
                {
                    await c.SaveStructure(InfrastructureConfiguration.Data, singleConfiguration);
                });
        }

        public static async Task SaveNode(
            SqliteQueue sqliteQueue,
            InfrastructureNode node)
        {
            await sqliteQueue.WithCommit(async c =>
                {
                    await c.SaveRecord(node);
                });
        }

        public static async Task SaveRecord(
            this System.Data.Common.DbTransaction transaction,
            IRecord record)
        {
            string tableName = record.GetType().Name;

            await transaction.Connection.ExecuteAsync($"delete from {tableName} where Id = @Id", record, transaction);
            string insertStatement = DbAccess.GetInsertStatement(record);
            await transaction.Connection.ExecuteAsync(insertStatement, record, transaction);
        }

        public static async Task<bool> DeleteConfiguration(
            SqliteQueue sqliteQueue,
            Guid configurationId)
        {
            throw new NotImplementedException();
            //using (SQLiteConnection conn = new SQLiteConnection($"Data Source = {fullDbPath}"))
            //{
            //    conn.Open();
            //    var transaction = conn.BeginTransaction();

            //    SingleConfiguration singleConfiguration = await Load.LoadDataStructure<SingleConfiguration>(transaction, InfrastructureConfiguration.Relation.Main(), configurationId);
            //    SingleConfiguration empty = new SingleConfiguration();

            //    var diff = empty.DiffToPrevious(singleConfiguration);
            //    Metapsi.DbAccess.DbSave.SaveChanges(diff, transaction);

            //    await transaction.CommitAsync();
            //}
            //return true;
        }

        public static async Task<Guid> ConfirmDeployment(
            SqliteQueue sqliteQueue,
            List<ServiceConfigurationSnapshot> infraSnapshot,
            InfrastructureConfiguration infrastructureConfiguration)
        {
            var previousDeployment = await LoadActiveDeployment(sqliteQueue);
            var previousServiceConfigurations = previousDeployment.GetDeployedServices();

            Guid deploymentGuid = Guid.NewGuid();

            await sqliteQueue.WithCommit(async c =>
            {
                Deployment deployment = new Deployment()
                {
                    Id = deploymentGuid,
                    ConfigurationHeaderId = infrastructureConfiguration.Id,
                    ConfigurationName = infrastructureConfiguration.Name,

                    // Deployment is always a new one and needs the timestamp of the actual deployment moment, which is the point where it gets saved here
                    Timestamp = DateTime.UtcNow
                };

                await c.InsertRecord(deployment);

                var allServiceNames = previousServiceConfigurations.Select(x => x.ServiceName).Union(infraSnapshot.Select(x => x.ServiceName));

                foreach (string serviceName in allServiceNames)
                {
                    var beforeSnapshot = previousServiceConfigurations.SingleOrDefault(x => x.ServiceName == serviceName);
                    var afterSnapshot = infraSnapshot.SingleOrDefault(x => x.ServiceName == serviceName);

                    switch (beforeSnapshot, afterSnapshot)
                    {
                        case (null, null):
                            throw new ArgumentException();
                        case (ServiceConfigurationSnapshot removed, null):
                            {
                                await c.InsertRecord(new DeploymentServiceTransition()
                                {
                                    DeploymentId = deployment.Id,
                                    FromServiceConfigurationSnapshotId = removed.Id,
                                    ToServiceConfigurationSnapshotId = Guid.Empty,
                                });
                                // There's no snapshot to save when it is removed
                            }
                            break;
                        case (null, ServiceConfigurationSnapshot added):
                            {
                                await c.InsertRecord(new DeploymentServiceTransition()
                                {
                                    DeploymentId = deployment.Id,
                                    FromServiceConfigurationSnapshotId = Guid.Empty,
                                    ToServiceConfigurationSnapshotId = added.Id
                                });

                                // If snapshot is not yet saved, save it
                                var savedSnapshot = await c.LoadRecord<ServiceConfigurationSnapshot>(afterSnapshot.Id);
                                if (savedSnapshot == null)
                                {
                                    await c.SaveStructure(ServiceConfigurationSnapshot.Data, afterSnapshot);
                                }
                            }
                            break;
                        default:
                            {
                                await c.InsertRecord(new DeploymentServiceTransition()
                                {
                                    DeploymentId = deployment.Id,
                                    FromServiceConfigurationSnapshotId = beforeSnapshot.Id,
                                    ToServiceConfigurationSnapshotId = afterSnapshot.Id
                                });

                                // If snapshot is not yet saved, save it
                                var savedSnapshot = await c.LoadRecord<ServiceConfigurationSnapshot>(afterSnapshot.Id);
                                if (savedSnapshot == null)
                                {
                                    await c.SaveStructure(ServiceConfigurationSnapshot.Data, afterSnapshot);
                                }
                            }
                            break;
                    }
                }
            });

            return deploymentGuid;
        }

        public static async Task<List<NoteType>> LoadAllNoteTypes(
            SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(async t =>
            {
                var allNoteTypes = await t.LoadRecords<NoteType>();
                return allNoteTypes.ToList();
            });
        }


        public static async Task<Deployment> LoadActiveDeployment(
            this System.Data.Common.DbTransaction transaction)
        {
            var deployment = await transaction.Connection.QueryFirstOrDefaultAsync<Deployment>("select * from Deployment order by datetime(timestamp) desc limit 1 ", transaction: transaction);

            if (deployment == null)
            {
                return new Deployment();
            }
            else
            {
                return await transaction.LoadSpecificDeployment(deployment.Id);
            }
        }

        public static async Task<Deployment> LoadActiveDeployment(SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(async c =>
            {
                return await c.LoadActiveDeployment();
            });
        }

        public static async Task SaveDeploymentEvent(SqliteQueue sqliteQueue, DbDeploymentEvent dbDeploymentEvent)
        {
            await sqliteQueue.WithCommit(async c =>
            {
                await c.SaveRecord(dbDeploymentEvent);
            });
        }

        public static async Task<List<DbDeploymentEvent>> GetDeploymentEvents(SqliteQueue sqliteQueue, Guid deploymentId)
        {
            return await sqliteQueue.WithRollback(async c =>
            {
                var deploymentEvents = await c.LoadRecords<DbDeploymentEvent, Guid>(x => x.DeploymentId, deploymentId);
                return deploymentEvents.ToList();
            });
        }

        public static async Task<MdsCommon.ServiceConfigurationSnapshot> LoadIdenticalSnapshot(SqliteQueue sqliteQueue, MdsCommon.ServiceConfigurationSnapshot expectedSnapshot)
        {
            return await sqliteQueue.WithRollback(
                async c =>
                {

                    var fieldNames = Ddl.FieldNames(expectedSnapshot).Except(new List<string>() { nameof(ServiceConfigurationSnapshot.Id), nameof(ServiceConfigurationSnapshot.SnapshotTimestamp) });

                    var whereFields = string.Join(" and ", fieldNames.Select(x => $"{Ddl.QuoteIdentifier(x)}=@{x}"));

                    var headQuery = $"select * from {nameof(ServiceConfigurationSnapshot)} where {whereFields}";

                    var multipleSnapshotsWithSameHeaderData = await c.Connection.QueryAsync<MdsCommon.ServiceConfigurationSnapshot>(headQuery, expectedSnapshot, c);

                    // If there is no snapshot with same header data, for sure parameters are not relevant anymore
                    if (!multipleSnapshotsWithSameHeaderData.Any())
                    {
                        return null;
                    }

                    foreach (var savedSnapshotHeader in multipleSnapshotsWithSameHeaderData)
                    {
                        var savedFullSnapshot = await c.LoadStructure(ServiceConfigurationSnapshot.Data, savedSnapshotHeader.Id);

                        if (MatchesAllParameters(savedFullSnapshot, expectedSnapshot))
                        {
                            return savedFullSnapshot;
                        }
                    }

                    return null;
                });
        }

        private static bool MatchesAllParameters(ServiceConfigurationSnapshot saved, ServiceConfigurationSnapshot expected)
        {
            var parametersDiff = Diff.CollectionsByKey(saved.ServiceConfigurationSnapshotParameters, expected.ServiceConfigurationSnapshotParameters, x => x.ParameterName);

            if (parametersDiff.JustInFirst.Any())
                return false;

            if (parametersDiff.JustInSecond.Any())
                return false;

            // parametersDiff.Common will not return anything, as their IDs are different

            foreach (var snapshotParameter in parametersDiff.Different)
            {
                // snapshotParameter.ParameterTypeId is not relevant for deployment
                // snapshotParameter.ConfiguredValue is not relevant for deployment

                if (snapshotParameter.InFirst.DeployedValue != snapshotParameter.InSecond.DeployedValue)
                    return false;
            }

            return true;
        }

        private class LastDeployedChange
        {
            public DateTime DeploymentTimestamp { get; set; }
            public Guid SnapshotId { get; set; }
        }

        public static async Task<Deployment> LoadSpecificDeployment(
            this System.Data.Common.DbTransaction transaction,
            Guid deploymentId)
        {
            var deployment = await transaction.LoadStructure(Deployment.Data, deploymentId);

            if (deployment == null)
                return new Deployment();

            var toSnapshotIds = deployment.Transitions.Where(x => x.ToServiceConfigurationSnapshotId != Guid.Empty).Select(x => x.ToServiceConfigurationSnapshotId).ToList();
            var allSnapshotIds = new List<Guid>(toSnapshotIds);
            allSnapshotIds.AddRange(deployment.Transitions.Where(x => x.FromServiceConfigurationSnapshotId != Guid.Empty).Select(x=>x.FromServiceConfigurationSnapshotId));
            allSnapshotIds = allSnapshotIds.Distinct().ToList();

            var allSnapshots = await transaction.LoadStructures(ServiceConfigurationSnapshot.Data, allSnapshotIds);

            foreach (var transition in deployment.Transitions)
            {
                if (transition.FromServiceConfigurationSnapshotId != Guid.Empty)
                {
                    transition.FromSnapshot = allSnapshots.SingleOrDefault(x => x.Id == transition.FromServiceConfigurationSnapshotId);
                }
                if (transition.ToServiceConfigurationSnapshotId != Guid.Empty)
                {
                    transition.ToSnapshot = allSnapshots.SingleOrDefault(x => x.Id == transition.ToServiceConfigurationSnapshotId);
                }
            }

            var stringIds = toSnapshotIds.Select(x => x.ToString()).ToList();

            // I think this attempted to check for each service when was the last actual change (in any deployment)

            //var remainingChanges = await transaction.Connection.QueryAsync<LastDeployedChange>(
            //    $@"select dep.Timestamp as {nameof(LastDeployedChange.DeploymentTimestamp)}, t.{nameof(DeploymentServiceTransition.ToServiceConfigurationSnapshotId)} as {nameof(LastDeployedChange.SnapshotId)} from Deployment dep 
            //                inner join {nameof(DeploymentServiceTransition)} t on t.DeploymentId = dep.id 
            //                where {nameof(DeploymentServiceTransition.FromServiceConfigurationSnapshotId)} != {nameof(DeploymentServiceTransition.ToServiceConfigurationSnapshotId)} and 
            //                {nameof(DeploymentServiceTransition.ToServiceConfigurationSnapshotId)} in @stringIds", new { stringIds }, transaction);

            foreach(var t in deployment.Transitions.Where(x=>x.ToServiceConfigurationSnapshotId != Guid.Empty))
            {
                if (t.ToSnapshot != null)
                {
                    // Just use the snapshot timestamp and hope for the best

                    deployment.LastConfigurationChanges.Add(new ConfigurationTime()
                    {
                        ServiceName = t.ToSnapshot.ServiceName,
                        LastConfigurationChangeTimestamp = t.ToSnapshot.SnapshotTimestamp
                    });
                }

                // So then was attempting to match the actual deployment timestamp to the transition

                //deployment.LastConfigurationChanges.Add(new ConfigurationTime()
                //{
                //    ServiceName = t.ToSnapshot.ServiceName,
                //    LastConfigurationChangeTimestamp = remainingChanges.Where(x=>x.SnapshotId == t.ToServiceConfigurationSnapshotId).OrderByDescending(x=>x.DeploymentTimestamp).First().DeploymentTimestamp
                //});
            }

            return deployment;
        }

        public static async Task<Deployment> LoadSpecificDeployment(SqliteQueue sqliteQueue, Guid deploymentId)
        {
            return await sqliteQueue.WithRollback(async c =>
            {
                return await c.LoadSpecificDeployment(deploymentId);
            });
        }


        public static async Task<List<MdsCommon.ServiceConfigurationSnapshot>> LoadNodeConfiguration(
            SqliteQueue sqliteQueue,
            string nodeName)
        {
            var lastDeployment = await LoadActiveDeployment(sqliteQueue);

            if (!lastDeployment.GetDeployedServices().Any(x => x.NodeName == nodeName))
                return new();

            return lastDeployment.GetDeployedServices().Where(x => x.NodeName == nodeName).ToList();
        }

        private static long healthStatusTotalMs = 0;
        private static int healthStatusCalls = 0;

        public static async Task StoreHealthStatus(
            SqliteQueue sqliteQueue,
            MdsCommon.MachineStatus healthStatus)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            await sqliteQueue.WithCommit(async c =>
            {
                var nodeName = new List<string>() { healthStatus.NodeName };
                var previous = await c.LoadStructures<MachineStatus, string>(MachineStatus.Data, x => x.NodeName, nodeName);
                Console.WriteLine(Metapsi.Serialize.ToJson(previous));
                if (previous.SingleOrDefault() != null)
                {
                    await c.DeleteStructure(previous.Single(), MachineStatus.Data.ChildrenNodes);
                }

                await c.InsertStructure(healthStatus, MachineStatus.Data.ChildrenNodes);
            });

            System.Diagnostics.Debug.WriteLine($"Store health status: {sw.ElapsedMilliseconds} ms");
            healthStatusTotalMs += sw.ElapsedMilliseconds;
            healthStatusCalls++;
            System.Diagnostics.Debug.WriteLine($"Store health status total: {healthStatusTotalMs} ms");
            System.Diagnostics.Debug.WriteLine($"Store health status calls: {healthStatusCalls}");
        }

        public static async Task<MdsCommon.ServiceConfigurationSnapshot> LoadServiceConfiguration(
            SqliteQueue sqliteQueue,
            string serviceName)
        {
            var currentDeployment = await LoadActiveDeployment(sqliteQueue);

            var toSnapshots = currentDeployment.Transitions.Where(x => x.ToSnapshot != null).Select(x => x.ToSnapshot);
            var config = toSnapshots.SingleOrDefault(x => x.ServiceName == serviceName);
            return config;
        }

        public static async Task<Deployment> LoadLastDeployment(SQLiteTransaction transaction)
        {
            Deployment lastDeployment = await transaction.Connection.QueryFirstOrDefaultAsync<Deployment>("select * from Deployment order by datetime(timestamp) desc limit 1");
            return lastDeployment;
        }

        public static async Task<Deployment> LoadLastDeploymentOfConfiguration(SqliteQueue sqliteQueue, Guid configurationId)
        {
            return await sqliteQueue.WithRollback(async t =>
            {
                Deployment lastDeployment = await t.Connection.QueryFirstOrDefaultAsync<Deployment>("select * from Deployment where ConfigurationHeaderId = @configurationId order by datetime(timestamp) desc limit 1", new { configurationId }, t);
                return lastDeployment;
            });
        }

        public static async Task<List<EnvironmentType>> LoadEnvironmentTypes(SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(async c =>
            {
                return (await c.LoadRecords<EnvironmentType>()).ToList();
            });
        }

        public static async Task<System.Collections.Generic.List<ParameterType>> LoadParameterTypes(SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(async t =>
            {
                System.Collections.Generic.List<ParameterType> parameterTypes = new System.Collections.Generic.List<ParameterType>();

                parameterTypes.AddRange(await t.LoadRecords<ParameterType>());

                return parameterTypes;
            });
        }

        public static async Task<ConfigurationHeadersList> LoadConfigurationHeaders(SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(
                async c =>
                {
                    var configRows = (await c.LoadRecords<InfrastructureConfiguration>()).ToList();
                    var serviceRows = await c.LoadRecords<InfrastructureService>();

                    foreach (var config in configRows)
                    {
                        config.InfrastructureServices.AddRange(serviceRows.Where(x => x.ConfigurationHeaderId == config.Id));
                    }

                    return new ConfigurationHeadersList()
                    {
                        ConfigurationHeaders = configRows
                    };
                });
        }

        public static async Task<InfrastructureConfiguration> LoadSpecificConfiguration(SqliteQueue sqliteQueue, Guid id)
        {
            return await sqliteQueue.WithRollback(
                async c =>
                {
                    return await c.LoadSpecificConfiguration(id);
                });
        }

        public static async Task<InfrastructureConfiguration> LoadSpecificConfiguration(
            this System.Data.Common.DbTransaction transaction,
            Guid id)
        {
            return await transaction.LoadStructure(InfrastructureConfiguration.Data, id);
        }

        public static async Task<InfrastructureConfiguration> LoadCurrentConfiguration(SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(
                async c =>
                {
                    return await c.LoadCurrentConfiguration();
                });
        }

        public static async Task<InfrastructureConfiguration> LoadCurrentConfiguration(this System.Data.Common.DbTransaction transaction)
        {
            var activeDeployment = await transaction.LoadActiveDeployment();
            if (activeDeployment.ConfigurationHeaderId == Guid.Empty)
                return new InfrastructureConfiguration();

            return await transaction.LoadStructure(InfrastructureConfiguration.Data, activeDeployment.ConfigurationHeaderId);
        }

        public static async Task<List<MdsCommon.MachineStatus>> LoadFullInfrastructureHealthStatus(SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(
                async c =>
                {
                    return await c.LoadFullInfrastructureHealthStatus();
                });
        }

        private static async Task<List<MdsCommon.MachineStatus>> LoadFullInfrastructureHealthStatus(
            this System.Data.Common.DbTransaction transaction)
        {
            return await transaction.LoadStructures(MdsCommon.MachineStatus.Data);
        }
    }
}