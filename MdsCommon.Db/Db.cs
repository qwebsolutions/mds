using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Metapsi.Sqlite;

namespace MdsCommon
{
    public static class Db
    {
        public static async Task<List<InfrastructureEvent>> LoadAllInfrastructureEvents(SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(
                async t =>
                {
                    return await t.LoadAllInfrastructureEvents();
                });
        }

        public static async Task<List<InfrastructureEvent>> LoadAllInfrastructureEvents(
            this System.Data.Common.DbTransaction transaction)
        {
            var allEvents = await transaction.LoadRecords<MdsCommon.InfrastructureEvent>();
            return allEvents.OrderByDescending(x => x.Timestamp).ToList();
        }

        public static async Task<MdsCommon.InfrastructureEvent> SaveInfrastructureEvent(
            this SqliteQueue sqliteQueue,
            MdsCommon.InfrastructureEvent infrastructureEvent)
        {
            await sqliteQueue.WithCommit(async t => DbAccess.Save(infrastructureEvent, t));
            return infrastructureEvent;
        }

        public static async Task<MdsCommon.InfrastructureEvent> LoadMostRecentInfrastructureEvent(
            SqliteQueue sqliteQueue,
            string serviceName)
        {
            return await sqliteQueue.WithRollback(async t =>
            {
                MdsCommon.InfrastructureEvent lastEvent = await t.Connection.QuerySingleOrDefaultAsync<MdsCommon.InfrastructureEvent>($"select * from {nameof(MdsCommon.InfrastructureEvent)} where ServiceName = @serviceName order by datetime(Timestamp) desc limit 1", new { serviceName }, t);
                return lastEvent;
            });
        }

        /// <summary>
        /// Added May 2024
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static async Task SetWalMode(SQLiteConnection connection)
        {
            await connection.ExecuteAsync("PRAGMA journal_mode=WAL;");
        }

        /// <summary>
        /// Added May 2024
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static async Task AddSnapshotEnabledField(OpenTransaction t)
        {
            await t.Transaction.Migrate<ServiceConfigurationSnapshot>(async (transaction, diff) =>
            {
                if (diff.ExpectedFieldIsMissing(nameof(ServiceConfigurationSnapshot.Enabled)))
                {
                    var expectedEnabled = diff.ExpectedField(nameof(ServiceConfigurationSnapshot.Enabled));
                    expectedEnabled.dflt_value = "1";
                    var columnDefinition = expectedEnabled.ColumnDefinition();
                    await t.Connection.ExecuteAsync(
                        $"ALTER TABLE {Ddl.QuoteIdentifier(nameof(ServiceConfigurationSnapshot))} ADD {columnDefinition}",
                        transaction: transaction);
                }
            });
        }

        /// <summary>
        /// Added May 2024
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static async Task DropSnapshotHashField(OpenTransaction t)
        {
            await t.Transaction.Migrate<ServiceConfigurationSnapshot>(async (transaction, diff) =>
            {
                if (diff.UnusedFieldIsPresent("Hash"))
                {
                    await t.Connection.ExecuteAsync(
                        $"ALTER TABLE {Ddl.QuoteIdentifier(nameof(ServiceConfigurationSnapshot))} DROP 'Hash'",
                        transaction: transaction);
                }
            });
        }
    }
}