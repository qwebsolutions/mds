using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Data.SQLite;
using Metapsi;
using Metapsi.Sqlite;
using Dapper;
using System;

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

        public static async Task<Metapsi.Sqlite.Validate.FieldsDiff> ValidateSchema(string fullDbPath)
        {
            return await Metapsi.Sqlite.Validate.ValidateSqliteSchema(fullDbPath, SchemaRecords);
        }

        public static async Task<List<MdsCommon.ServiceConfigurationSnapshot>> LoadKnownConfiguration(string fullDbPath)
        {
            return await Db.WithRollback(fullDbPath, async c =>
            {
                return await c.Transaction.LoadStructures(MdsCommon.ServiceConfigurationSnapshot.Data);
            });
        }

        //public static async Task<MdsCommon.ServiceConfigurationSnapshot> LoadServiceConfiguration(string fullDbPath, string serviceName)
        //{
        //    MdsCommon.ServiceConfigurationSnapshot serviceConfiguration = new MdsCommon.ServiceConfigurationSnapshot();

        //    using (SQLiteConnection conn = new SQLiteConnection($"Data Source = {fullDbPath}"))
        //    {
        //        conn.Open();
        //        var transaction = conn.BeginTransaction();
        //        var s = await transaction.LoadRecords<MdsCommon.ServiceConfigurationSnapshot, string>(x => x.ServiceName, serviceName);

        //        if (s.Any())
        //        {
        //            throw new NotImplementedException();
        //            //serviceConfiguration = await transaction.LoadStructure<MdsCommon.ServiceSnapshot>(s.Single().Id);
        //        }

        //        await transaction.RollbackAsync();
        //    }

        //    return serviceConfiguration;
        //}

        public static async Task<IEnumerable<SyncResult>> LoadSyncHistory(string fullDbPath)
        {
            var syncHistory = await Db.WithRollback(fullDbPath, c => c.Transaction.LoadRecords<SyncResult>());
            return syncHistory.OrderByDescending(x => x.Timestamp);
        }

        public static async Task SetNewConfiguration(
            string fullDbPath,
            List<MdsCommon.ServiceConfigurationSnapshot> localControllerConfiguration)
        {
            await Db.WithCommit(fullDbPath, async c =>
            {
                await c.Connection.ExecuteAsync($"delete from {nameof(MdsCommon.ServiceConfigurationSnapshotParameter)}", transaction: c.Transaction);
                await c.Connection.ExecuteAsync($"delete from {nameof(MdsCommon.ServiceConfigurationSnapshot)}", transaction: c.Transaction);

                await c.Transaction.InsertRecords(typeof(MdsCommon.ServiceConfigurationSnapshot), localControllerConfiguration);
                await c.Transaction.InsertRecords(typeof(MdsCommon.ServiceConfigurationSnapshotParameter), localControllerConfiguration.SelectMany(x => x.ServiceConfigurationSnapshotParameters));
            });
        }

        public static LocalSettings GetLocalSettings(string nodeName, string fullDbPath, string infrastructureApiUrl)
        {
            return new LocalSettings()
            {
                FullDbPath = fullDbPath,
                InfrastructureApiUrl = infrastructureApiUrl,
                NodeName = nodeName
            };
        }

        public static async Task RegisterSyncResult(string fullDbPath, SyncResult syncResult)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source = {fullDbPath}"))
            {
                conn.Open();
                var transaction = conn.BeginTransaction();

                Metapsi.Sqlite.DbAccess.Save(syncResult, transaction);
                await transaction.CommitAsync();
            }
        }

        //public static async Task<RecordCollection<RunningServiceProcess>> LoadAllServiceProcessesFromDbPath(string fullDbPath, string nodeName)
        //{
        //    using (SQLiteConnection conn = new SQLiteConnection($"Data Source = {fullDbPath}"))
        //    {
        //        conn.Open();
        //        var transaction = conn.BeginTransaction();

        //        var allServiceProcesses = await LoadAllServiceProcesses(transaction, nodeName);
        //        await transaction.CommitAsync();
        //        return allServiceProcesses;
        //    }
        //}

        ///// <summary>
        ///// Loads from LocalService + actual running processes
        ///// </summary>
        ///// <param name="transaction"></param>
        ///// <returns></returns>
        //public static async Task<RecordCollection<RunningServiceProcess>> LoadAllServiceProcesses(SQLiteTransaction transaction, string nodeName)
        //{

        //}

        public static async Task<FullLocalStatus> LoadFullLocalStatus(string fullDbPath, string nodeName)
        {
            return await Db.WithRollback(fullDbPath,
                async c =>
                {
                    var transaction = c.Transaction;
                    FullLocalStatus localStatus = new();
                    localStatus.LocalServiceSnapshots.AddRange(await transaction.LoadRecords<MdsCommon.ServiceConfigurationSnapshot>());
                    localStatus.SyncResults.AddRange(await transaction.LoadRecords<SyncResult>());
                    return localStatus;
                });
        }
    }
}
