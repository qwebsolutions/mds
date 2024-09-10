using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Data.SQLite;
using Metapsi;
using Metapsi.Sqlite;
using Dapper;
using System;
using System.Transactions;

namespace MdsLocal
{

    public static class LocalDb
    {
        //public partial class SyncHistory : IDataStructure
        //{
        //    public RecordCollection<MdsLocal.SyncResult> SyncResults { get; set; } = new RecordCollection<MdsLocal.SyncResult>();
        //    public RecordCollection<MdsLocal.SyncUpdatedConfiguration> UpdatedConfigurations { get; set; } = new RecordCollection<MdsLocal.SyncUpdatedConfiguration>();
        //}

        public static List<System.Type> SchemaRecords = new List<System.Type>()
        {
            typeof(MdsCommon.ServiceConfigurationSnapshot),
            typeof(MdsCommon.ServiceConfigurationSnapshotParameter),
            typeof(MdsLocal.SyncResult),
            typeof(MdsCommon.InfrastructureEvent)
        };

        public static async Task<List<MdsCommon.ServiceConfigurationSnapshot>> LoadKnownConfiguration(SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(async c =>
            {
                return await c.LoadStructures(MdsCommon.ServiceConfigurationSnapshot.Data);
            });
        }

        public static async Task<IEnumerable<SyncResult>> LoadSyncHistory(SqliteQueue sqliteQueue)
        {
            var syncHistory = await sqliteQueue.WithRollback(
                async c =>
                {
                    var syncResults = await c.LoadRecords<SyncResult>();
                    foreach (var syncResult in syncResults)
                    {
                        var errorsQuery = $"select * from [{nameof(SyncResultLog)}] where {nameof(SyncResultLog.SyncResultId)} = @Id and \"type\" = '{SyncResultLogType.Error}'";
                        var errors = await c.Connection.QueryAsync<SyncResultLog>(errorsQuery, new { syncResult.Id }, c);
                        syncResult.Log.AddRange(errors);
                    }
                    return syncResults;
                });

            return syncHistory.OrderByDescending(x => x.Timestamp);
        }

        public static async Task<SyncResult> LoadFullSyncResult(SqliteQueue sqliteQueue, Guid id)
        {
            var fullSyncResult = await sqliteQueue.WithRollback(async c =>
            {
                var syncResult = await c.LoadRecord<SyncResult>(id);

                var log = await c.Connection.QueryAsync<SyncResultLog>($"select * from [{nameof(SyncResultLog)}] where {nameof(SyncResultLog.SyncResultId)} = @id", new { id }, c);
                syncResult.Log.AddRange(log.OrderBy(x => x.Index));

                return syncResult;
            });

            return fullSyncResult;
        }

        public static async Task SetNewConfiguration(
            SqliteQueue sqliteQueue,
            List<MdsCommon.ServiceConfigurationSnapshot> localControllerConfiguration)
        {
            await sqliteQueue.WithCommit(async c =>
            {
                await c.Connection.ExecuteAsync($"delete from {nameof(MdsCommon.ServiceConfigurationSnapshotParameter)}", transaction: c);
                await c.Connection.ExecuteAsync($"delete from {nameof(MdsCommon.ServiceConfigurationSnapshot)}", transaction: c);

                await c.InsertRecords(typeof(MdsCommon.ServiceConfigurationSnapshot), localControllerConfiguration);
                await c.InsertRecords(typeof(MdsCommon.ServiceConfigurationSnapshotParameter), localControllerConfiguration.SelectMany(x => x.ServiceConfigurationSnapshotParameters));
            });
        }

        //public static LocalSettings GetLocalSettings(string nodeName, string fullDbPath, string infrastructureApiUrl)
        //{
        //    return new LocalSettings()
        //    {
        //        FullDbPath = fullDbPath,
        //        InfrastructureApiUrl = infrastructureApiUrl,
        //        NodeName = nodeName
        //    };
        //}

        public static async Task RegisterSyncResult(SqliteQueue sqliteQueue, SyncResult syncResult)
        {
            await sqliteQueue.WithCommit(async t =>
            {
                Metapsi.Sqlite.DbAccess.Save(syncResult, t);

                await t.CreateTableIfNotExists<SyncResultLog>(f =>
                {
                    if (f.FieldName == nameof(SyncResultLog.Id))
                    {
                        f.Definition = "ID STRING PRIMARY KEY";
                    }
                });
                Metapsi.Sqlite.DbAccess.SaveCollection(nameof(SyncResultLog.SyncResultId), syncResult.Log, t);
            });
        }


        public static async Task<FullLocalStatus> LoadFullLocalStatus(SqliteQueue sqliteQueue, string nodeName)
        {
            return await sqliteQueue.WithRollback(
                async c =>
                {
                    FullLocalStatus localStatus = new();
                    localStatus.LocalServiceSnapshots.AddRange(await c.LoadRecords<MdsCommon.ServiceConfigurationSnapshot>());
                    localStatus.SyncResults.AddRange(await c.LoadRecords<SyncResult>());
                    return localStatus;
                });
        }
    }
}
