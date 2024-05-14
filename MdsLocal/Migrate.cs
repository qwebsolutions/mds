using Dapper;
using MdsCommon;
using Metapsi.Sqlite;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MdsLocal
{
    public static class Migrate
    {
        public static async Task All(string fullDbPath)
        {
            await Metapsi.Sqlite.Db.WithCommit(
                fullDbPath,
                async t =>
                {
                    await AddSnapshotEnabledField(t);
                    await ReplaceOldDateTimeFormat(t);
                });
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
                    await t.Connection.ExecuteAsync($"ALTER TABLE {Ddl.QuoteIdentifier(nameof(ServiceConfigurationSnapshot))} ADD {columnDefinition}");
                }
            });
        }

        public static async Task ReplaceOldDateTimeFormat(OpenTransaction t)
        {
            var replaceSql = async (
                OpenTransaction t,
                string tableName,
                string dateTimeField) =>
            {
                var allRecords = await t.Connection.QueryAsync<(string Id, string Timestamp)>($"select Id, {dateTimeField} from {tableName}", transaction: t.Transaction);
                foreach (var record in allRecords)
                {
                    // Do not update if format is already correct
                    if (record.Timestamp.Contains(" "))
                    {
                        var snapshotTimestamp = DateTime.Parse(record.Timestamp, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
                        snapshotTimestamp = DateTime.SpecifyKind(snapshotTimestamp, DateTimeKind.Utc);
                        await t.Connection.ExecuteAsync(
                            $"update {tableName} set {dateTimeField}=@timestamp where Id=@Id",
                            new
                            {
                                Id = record.Id,
                                timestamp = snapshotTimestamp.ToString("O", System.Globalization.CultureInfo.InvariantCulture)
                            },
                            transaction: t.Transaction);
                    }
                }
            };


            await replaceSql(t, nameof(InfrastructureEvent), nameof(InfrastructureEvent.Timestamp));
            await replaceSql(t, nameof(ServiceConfigurationSnapshot), nameof(ServiceConfigurationSnapshot.SnapshotTimestamp));
            await replaceSql(t, nameof(SyncResult), nameof(SyncResult.Timestamp));
        }
    }
}
