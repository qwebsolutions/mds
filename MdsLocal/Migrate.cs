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
            using (var connection = Metapsi.Sqlite.Db.ToConnection(fullDbPath))
            {
                await MdsCommon.Db.SetWalMode(connection);
            }

            await Metapsi.Sqlite.Db.WithCommit(
                fullDbPath,
                async t =>
                {
                    await MdsCommon.Db.AddSnapshotEnabledField(t);
                    await MdsCommon.Db.DropSnapshotHashField(t);
                    await ReplaceOldDateTimeFormat(t);
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
