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
        public static async Task<List<InfrastructureEvent>> LoadAllInfrastructureEvents(string fullDbPath)
        {
            return await Metapsi.Sqlite.Db.WithRollback(
                fullDbPath,
                async c =>
                {
                    return await c.Transaction.LoadAllInfrastructureEvents();
                });
        }

        public static async Task<List<InfrastructureEvent>> LoadAllInfrastructureEvents(
            this System.Data.Common.DbTransaction transaction)
        {
            var allEvents = await transaction.LoadRecords<MdsCommon.InfrastructureEvent>();
            return allEvents.OrderByDescending(x => x.Timestamp).ToList();
        }

        public static async Task<MdsCommon.InfrastructureEvent> SaveInfrastructureEvent(
            string fullDbPath,
            MdsCommon.InfrastructureEvent infrastructureEvent)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source = {fullDbPath}"))
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                DbAccess.Save(infrastructureEvent, transaction);
                await LimitEvents(transaction, 500);
                await transaction.CommitAsync();

                return infrastructureEvent;
            }
        }

        public static async Task<MdsCommon.InfrastructureEvent> LoadMostRecentInfrastructureEvent(
            string fullDbPath,
            string serviceName)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source = {fullDbPath}"))
            {
                conn.Open();

                MdsCommon.InfrastructureEvent lastEvent = await conn.QuerySingleOrDefaultAsync<MdsCommon.InfrastructureEvent>($"select * from {nameof(MdsCommon.InfrastructureEvent)} where ServiceName = @serviceName order by datetime(Timestamp) desc limit 1", new { serviceName });
                return lastEvent;
            }
        }

        public static async Task LimitEvents(
            SQLiteTransaction transaction,
            int maxCount)
        {
            string lastAcceptedTimestampQuery =
                @"select min(timestamp) from
                (select timestamp as timestamp from InfrastructureEvent order by Timestamp desc
                limit @maxCount)";

            string deleteOlderThanAcceptedStatement = "delete from InfrastructureEvent where timestamp < @lastAcceptedTimestamp";

            string lastAcceptedTimestamp = await transaction.Connection.ExecuteScalarAsync<string>(lastAcceptedTimestampQuery, new { maxCount }, transaction);

            if (!string.IsNullOrEmpty(lastAcceptedTimestamp))
            {
                await transaction.Connection.ExecuteAsync(deleteOlderThanAcceptedStatement, new { lastAcceptedTimestamp = lastAcceptedTimestamp }, transaction);
            }
        }
    }
}