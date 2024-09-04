using Dapper;
using MdsCommon;
using Metapsi.Sqlite;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure
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
                });

            await Metapsi.Sqlite.Db.WithCommit(fullDbPath, async c =>
            {
                await c.Transaction.CreateTableIfNotExists<DbDeploymentEvent>();
            });
        }
    }
}
