using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System;
using System.Collections.Generic;
using MdsCommon;
using Metapsi;
using Metapsi.Sqlite;

namespace MdsInfrastructure
{
    public static class Db
    {
        public static async Task<string> ValidateSchema(
            string fullDbPath)
        {
            var fieldsDiff = await Validate.ValidateSqliteSchema(fullDbPath, new System.Collections.Generic.List<Type>()
            {
                typeof(Application),
                typeof(InfrastructureConfiguration),
                //typeof(ConfigurationNote),
                typeof(Project),
                typeof(ProjectVersion),
                typeof(ProjectVersionBinaries),
                typeof(Deployment),
                typeof(DeploymentServiceTransition),
                typeof(EnvironmentType),
                typeof(MdsCommon.InfrastructureEvent),
                typeof(InfrastructureNode),
                typeof(InfrastructureService),
                typeof(InfrastructureServiceParameterBinding),
                typeof(InfrastructureServiceParameterDeclaration),
                typeof(InfrastructureServiceParameterValue),
                typeof(InfrastructureVariable),
                typeof(MdsCommon.MachineStatus),
                typeof(MdsCommon.ServiceConfigurationSnapshot),
                typeof(MdsCommon.ServiceConfigurationSnapshotParameter),
                typeof(MdsCommon.ServiceStatus),
                typeof(ParameterType),
                typeof(NoteType),
                typeof(InfrastructureServiceNote)
            });

            if (fieldsDiff.SameFields == false)
            {
                return $"Db schema mismatch: extra fields {Metapsi.Serialize.ToJson(fieldsDiff.ExtraFields)}, missing fields {Metapsi.Serialize.ToJson(fieldsDiff.MissingFields)}, db path {fullDbPath}";
            }
            return string.Empty;
        }

        public static async Task<InfrastructureStatus> LoadInfrastructureStatus(
            string fullDbPath)
        {
            return await Metapsi.Sqlite.Db.WithRollback(fullDbPath, async c =>
            {
                var activeDeployment = await c.Transaction.LoadActiveDeployment();
                var currentConfiguration = 
                activeDeployment.ConfigurationHeaderId == Guid.Empty ?
                new InfrastructureConfiguration() : await c.Transaction.LoadSpecificConfiguration(activeDeployment.ConfigurationHeaderId);

                return new InfrastructureStatus()
                {
                    Deployment = activeDeployment,
                    HealthStatus = await c.Transaction.LoadFullInfrastructureHealthStatus(),
                    InfrastructureEvents = await c.Transaction.LoadAllInfrastructureEvents(),
                    InfrastructureConfiguration = currentConfiguration,
                    InfrastructureNodes = (await c.Transaction.LoadRecords<InfrastructureNode>()).ToList()
                };
            });
        }

        public static async Task<List<Deployment>> LoadDeploymentHistory(
            string fullDbPath)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source = {fullDbPath}"))
            {
                conn.Open();
                var transaction = conn.BeginTransaction();

                return (await transaction.LoadRecords<Deployment>()).OrderByDescending(x => x.Timestamp).ToList();
            }
        }

        public static async Task<List<Project>> LoadAllProjects(
            string fullDbPath)
        {
            return await Metapsi.Sqlite.Db.WithRollback(fullDbPath, x =>
            {
                return x.Transaction.LoadStructures(Project.Data);
            });
        }

        public static async Task<List<InfrastructureNode>> LoadAllNodes(
            string fullDbPath)
        {
            return await Metapsi.Sqlite.Db.WithRollback(fullDbPath, async x =>
            {
                return (await x.Transaction.LoadRecords<InfrastructureNode>()).ToList();
            });
        }

        public static async Task<List<InfrastructureService>> LoadAllServices(
            string fullDbPath)
        {
            return await Metapsi.Sqlite.Db.WithRollback(fullDbPath, async x =>
            {
                return (await x.Transaction.LoadRecords<InfrastructureService>()).ToList();
            });
        }

        /// <summary>
        /// Stores & returns the algorithm build results for binaries that are not already registered
        /// </summary>
        /// <param name="fullDbPath"></param>
        /// <param name="allBinaries"></param>
        /// <returns></returns>
        public static async Task<List<AlgorithmInfo>> SaveNewBinaries(string fullDbPath, List<MdsCommon.AlgorithmInfo> allBinaries)
        {
            List<AlgorithmInfo> newBinaries = new List<AlgorithmInfo>();

            await Metapsi.Sqlite.Db.WithCommit(fullDbPath, async c =>
            {
                foreach (AlgorithmInfo algorithmInfo in allBinaries)
                {
                    var project = await c.Transaction.LoadRecord((Project x) => x.Name, algorithmInfo.Name);

                    if (project == null)
                    {
                        project = new MdsCommon.Project()
                        {
                            Enabled = true,
                            Name = algorithmInfo.Name
                        };

                        await c.Transaction.InsertRecord(project);
                    }
                    var versions = await c.Transaction.LoadRecords((ProjectVersion x) => x.VersionTag, algorithmInfo.Version);
                    var version = versions.SingleOrDefault(x => x.ProjectId == project.Id);
                    if (version == null)
                    {
                        version = new MdsCommon.ProjectVersion()
                        {
                            ProjectId = project.Id,
                            Enabled = true,
                            VersionTag = algorithmInfo.Version,
                        };

                        await c.Transaction.InsertRecord(version);
                    }

                    version = await c.Transaction.LoadStructure(ProjectVersion.Data, version.Id);

                    var build = version.Binaries.SingleOrDefault(x => x.ProjectVersionId == version.Id && x.Target == algorithmInfo.Target);

                    if (build == null)
                    {
                        build = new MdsCommon.ProjectVersionBinaries()
                        {
                            BuildNumber = algorithmInfo.BuildNumber,
                            ProjectVersionId = version.Id,
                            Target = algorithmInfo.Target
                        };
                        await c.Transaction.InsertRecord(build);
                        newBinaries.Add(algorithmInfo);
                    }
                }
            });

            return newBinaries;
        }

        public static async Task SaveVersionEnabled(string fullDbPath, ProjectVersion projectVersion)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source = {fullDbPath}"))
            {
                conn.Open();
                await conn.ExecuteAsync("update ProjectVersion set Enabled = @Enabled where Id = @Id", projectVersion);
            }
        }

        public static async Task SaveConfiguration(
            string fullDbPath,
            InfrastructureConfiguration singleConfiguration)
        {
            await Metapsi.Sqlite.Db.WithCommit(fullDbPath,
                async c =>
                {
                    await c.Transaction.SaveStructure(InfrastructureConfiguration.Data, singleConfiguration);
                });
        }

        public static async Task SaveNode(
            string fullDbPath,
            InfrastructureNode node)
        {
            await Metapsi.Sqlite.Db.WithCommit(fullDbPath,
                async c =>
                {
                    await c.Transaction.SaveRecord(node);
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
            string fullDbPath,
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

        public static async Task ConfirmDeployment(
            string fullDbPath,
            List<ServiceConfigurationSnapshot> infraSnapshot,
            InfrastructureConfiguration infrastructureConfiguration)
        {
            var previousDeployment = await LoadActiveDeployment(fullDbPath);
            var previousServiceConfigurations = previousDeployment.GetDeployedServices();

            await Metapsi.Sqlite.Db.WithCommit(fullDbPath, async c =>
            {
                Deployment deployment = new Deployment()
                {
                    ConfigurationHeaderId = infrastructureConfiguration.Id,
                    ConfigurationName = infrastructureConfiguration.Name,

                    // Deployment is always a new one and needs the timestamp of the actual deployment moment, which is the point where it gets saved here
                    Timestamp = DateTime.UtcNow
                };

                await c.Transaction.InsertRecord(deployment);

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
                                await c.Transaction.InsertRecord(new DeploymentServiceTransition()
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
                                await c.Transaction.InsertRecord(new DeploymentServiceTransition()
                                {
                                    DeploymentId = deployment.Id,
                                    FromServiceConfigurationSnapshotId = Guid.Empty,
                                    ToServiceConfigurationSnapshotId = added.Id
                                });

                                // If snapshot is not yet saved, save it
                                var savedSnapshot = await c.Transaction.LoadRecord<ServiceConfigurationSnapshot>(afterSnapshot.Id);
                                if (savedSnapshot == null)
                                {
                                    await c.Transaction.SaveStructure(ServiceConfigurationSnapshot.Data, afterSnapshot);
                                }
                            }
                            break;
                        default:
                            {
                                await c.Transaction.InsertRecord(new DeploymentServiceTransition()
                                {
                                    DeploymentId = deployment.Id,
                                    FromServiceConfigurationSnapshotId = beforeSnapshot.Id,
                                    ToServiceConfigurationSnapshotId = afterSnapshot.Id
                                });

                                // If snapshot is not yet saved, save it
                                var savedSnapshot = await c.Transaction.LoadRecord<ServiceConfigurationSnapshot>(afterSnapshot.Id);
                                if (savedSnapshot == null)
                                {
                                    await c.Transaction.SaveStructure(ServiceConfigurationSnapshot.Data, afterSnapshot);
                                }
                            }
                            break;
                    }
                }
            });
        }

        public static async Task<List<NoteType>> LoadAllNoteTypes(
            string fullDbPath)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source = {fullDbPath}"))
            {
                conn.Open();
                var transaction = conn.BeginTransaction();

                var allNoteTypes = await transaction.LoadRecords<NoteType>();
                await transaction.RollbackAsync();
                return allNoteTypes.ToList();
            }
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

        public static async Task<Deployment> LoadActiveDeployment(string fullDbPath)
        {
            return await Metapsi.Sqlite.Db.WithRollback(fullDbPath, async c =>
            {
                return await c.Transaction.LoadActiveDeployment();
            });
        }

        public static async Task<MdsCommon.ServiceConfigurationSnapshot> LoadServiceSnapshotByHash(string fullDbPath, string hash)
        {
            return await Metapsi.Sqlite.Db.WithRollback(fullDbPath,
                async c =>
                {
                    try
                    {
                        var snapshotRecordWithSameHash = await c.Transaction.LoadRecords(
                            (ServiceConfigurationSnapshot x) => x.Hash,
                            hash);

                        if (snapshotRecordWithSameHash.Any())
                        {
                            // Take First(), not Single(), to compensate for the many bugs this hash thing had and maybe still has
                            var first = snapshotRecordWithSameHash.OrderByDescending(x => x.SnapshotTimestamp).First();

                            return await c.Transaction.LoadStructure(ServiceConfigurationSnapshot.Data, first.Id);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("LoadServiceSnapshotByHash exception hash = " + hash);
                        throw;
                    }
                });
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

            var remainingChanges = await transaction.Connection.QueryAsync<LastDeployedChange>(
                $@"select dep.Timestamp as {nameof(LastDeployedChange.DeploymentTimestamp)}, t.{nameof(DeploymentServiceTransition.ToServiceConfigurationSnapshotId)} as {nameof(LastDeployedChange.SnapshotId)} from Deployment dep 
                            inner join {nameof(DeploymentServiceTransition)} t on t.DeploymentId = dep.id 
                            where {nameof(DeploymentServiceTransition.FromServiceConfigurationSnapshotId)} != {nameof(DeploymentServiceTransition.ToServiceConfigurationSnapshotId)} and 
                            {nameof(DeploymentServiceTransition.ToServiceConfigurationSnapshotId)} in @stringIds", new { stringIds }, transaction);

            foreach(var t in deployment.Transitions.Where(x=>x.ToServiceConfigurationSnapshotId != Guid.Empty))
            {
                deployment.LastConfigurationChanges.Add(new ConfigurationTime()
                {
                    ServiceName = t.ToSnapshot.ServiceName,
                    LastConfigurationChangeTimestamp = remainingChanges.Where(x=>x.SnapshotId == t.ToServiceConfigurationSnapshotId).OrderByDescending(x=>x.DeploymentTimestamp).First().DeploymentTimestamp
                });
            }

            return deployment;
        }

        public static async Task<Deployment> LoadSpecificDeployment(string fullDbPath, Guid deploymentId)
        {
            return await Metapsi.Sqlite.Db.WithRollback(fullDbPath, async c =>
            {
                return await c.Transaction.LoadSpecificDeployment(deploymentId);
            });
        }


        public static async Task<List<MdsCommon.ServiceConfigurationSnapshot>> LoadNodeConfiguration(
            string fullDbPath,
            string nodeName)
        {
            var lastDeployment = await LoadActiveDeployment(fullDbPath);

            if (!lastDeployment.GetDeployedServices().Any(x => x.NodeName == nodeName))
                return new();

            return lastDeployment.GetDeployedServices().Where(x => x.NodeName == nodeName).ToList();
        }

        public static async Task StoreHealthStatus(
            string fullDbPath,
            MdsCommon.MachineStatus healthStatus)
        {
            await Metapsi.Sqlite.Db.WithCommit(fullDbPath, async c =>
            {
                var nodeName = new List<string>() { healthStatus.NodeName };
                var previous = await c.Transaction.LoadStructures<MachineStatus, string>(MachineStatus.Data, x => x.NodeName, nodeName);
                Console.WriteLine(Metapsi.Serialize.ToJson(previous));
                if (previous.SingleOrDefault() != null)
                {
                    await c.Transaction.DeleteStructure(previous.Single(), MachineStatus.Data.ChildrenNodes);
                }

                await c.Transaction.InsertStructure(healthStatus, MachineStatus.Data.ChildrenNodes);
            });

            //using (SQLiteConnection conn = new SQLiteConnection($"Data Source = {fullDbPath}"))
            //{
            //    conn.Open();
            //    var transaction = conn.BeginTransaction();

            //    string nodeName = healthStatus.MachineStatus.First().NodeName;

            //    var prevStatuses = await transaction.LoadRecords<MachineStatus, string>(x => x.NodeName, nodeName);
            //    await transaction.DeleteRecords<MachineStatus, Guid>(x => x.Id, prevStatuses.Select(x => x.Id));
            //    await transaction.DeleteRecords<ServiceStatus, Guid>(x => x.MachineStatusId, prevStatuses.Select(x => x.Id));

            //    var diff = Diff.Structures(new NodeStatus(), healthStatus);
            //    transaction.SaveChanges(diff);
            //    await transaction.CommitAsync();
            //    Console.WriteLine("StoreHealthStatus.CommitAsync()");
            //}

            //return healthStatus;
        }

        public static async Task<MdsCommon.ServiceConfigurationSnapshot> LoadServiceConfiguration(
            string fullDbPath,
            string serviceName)
        {
            var currentDeployment = await LoadActiveDeployment(fullDbPath);

            var toSnapshots = currentDeployment.Transitions.Where(x => x.ToSnapshot != null).Select(x => x.ToSnapshot);
            var config = toSnapshots.SingleOrDefault(x => x.ServiceName == serviceName);
            return config;
        }

        public static async Task<Deployment> LoadLastDeployment(SQLiteTransaction transaction)
        {
            Deployment lastDeployment = await transaction.Connection.QueryFirstOrDefaultAsync<Deployment>("select * from Deployment order by datetime(timestamp) desc limit 1");
            return lastDeployment;
        }

        public static async Task<Deployment> LoadLastDeploymentOfConfiguration(string fullDbPath, Guid configurationId)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source = {fullDbPath}"))
            {
                conn.Open();
                Deployment lastDeployment = await conn.QueryFirstOrDefaultAsync<Deployment>("select * from Deployment where ConfigurationHeaderId = @configurationId order by datetime(timestamp) desc limit 1", new { configurationId });
                return lastDeployment;
            }
        }

        public static async Task<List<EnvironmentType>> LoadEnvironmentTypes(string fullDbPath)
        {
            return await Metapsi.Sqlite.Db.WithRollback(fullDbPath, async c =>
            {
                return (await c.Transaction.LoadRecords<EnvironmentType>()).ToList();
            });
        }

        public static async Task<System.Collections.Generic.List<ParameterType>> LoadParameterTypes(string fullDbPath)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source = {fullDbPath}"))
            {
                conn.Open();
                var transaction = conn.BeginTransaction();

                System.Collections.Generic.List<ParameterType> parameterTypes = new System.Collections.Generic.List<ParameterType>();

                parameterTypes.AddRange(await transaction.LoadRecords<ParameterType>());

                transaction.Commit();

                return parameterTypes;
            }
        }

        public static async Task<ConfigurationHeadersList> LoadConfigurationHeaders(string fullDbPath)
        {
            return await Metapsi.Sqlite.Db.WithRollback(
                fullDbPath,
                async c =>
                {
                    var configRows = (await c.Transaction.LoadRecords<InfrastructureConfiguration>()).ToList();
                    var serviceRows = await c.Transaction.LoadRecords<InfrastructureService>();

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

        public static async Task<InfrastructureConfiguration> LoadSpecificConfiguration(string fullDbPath, Guid id)
        {
            return await Metapsi.Sqlite.Db.WithRollback(
                fullDbPath,
                async c =>
                {
                    return await c.Transaction.LoadSpecificConfiguration(id);
                });
        }

        public static async Task<InfrastructureConfiguration> LoadSpecificConfiguration(
            this System.Data.Common.DbTransaction transaction,
            Guid id)
        {
            return await transaction.LoadStructure(InfrastructureConfiguration.Data, id);
        }

        public static async Task<InfrastructureConfiguration> LoadCurrentConfiguration(string fullDbPath)
        {
            return await Metapsi.Sqlite.Db.WithRollback(
                fullDbPath,
                async c =>
                {
                    return await c.Transaction.LoadCurrentConfiguration();
                });
        }

        public static async Task<InfrastructureConfiguration> LoadCurrentConfiguration(this System.Data.Common.DbTransaction transaction)
        {
            var activeDeployment = await transaction.LoadActiveDeployment();
            if (activeDeployment.ConfigurationHeaderId == Guid.Empty)
                return new InfrastructureConfiguration();

            return await transaction.LoadStructure(InfrastructureConfiguration.Data, activeDeployment.ConfigurationHeaderId);
        }

        public static async Task<List<MdsCommon.MachineStatus>> LoadFullInfrastructureHealthStatus(string fullDbPath)
        {
            return await Metapsi.Sqlite.Db.WithRollback(
                fullDbPath,
                async c =>
                {
                    return await c.Transaction.LoadFullInfrastructureHealthStatus();
                });
        }

        private static async Task<List<MdsCommon.MachineStatus>> LoadFullInfrastructureHealthStatus(
            this System.Data.Common.DbTransaction transaction)
        {
            return await transaction.LoadStructures(MdsCommon.MachineStatus.Data);
        }
    }
}