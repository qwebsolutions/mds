using Metapsi;
using Metapsi.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsCommon;

public class DbQueue : TaskQueue<string>
{
    public DbQueue(string fullDbPath) : base(fullDbPath)
    {
    }
}

public static class DbQueueExtensions
{
    public static async Task SaveInfrastructureEvent(this DbQueue queue, MdsCommon.InfrastructureEvent infrastructureEvent)
    {
        await queue.Enqueue(async (dbPath) => await MdsCommon.Db.SaveInfrastructureEvent(dbPath, infrastructureEvent));
    }

    public static async Task<List<InfrastructureEvent>> GetAllInfrastructureEvents(this DbQueue queue)
    {
        return await queue.Enqueue(Db.LoadAllInfrastructureEvents);
    }

    public static async Task DeleteInfrastructureEvent(this DbQueue queue, Guid id)
    {
        await queue.Enqueue(
            async (path) => await Metapsi.Sqlite.Db.WithCommit(
                path, 
                async c => await c.Transaction.DeleteRecord<InfrastructureEvent>(id)));
    }

    public static async Task<int> CleanupInfrastructureEvents(this DbQueue dbQueue, int keepMaxCount, int keepMaxDays)
    {
        var allInfrastructureEvents = await dbQueue.GetAllInfrastructureEvents();

        var toRemoveIds = new List<Guid>();
        if (keepMaxCount >= 0)
        {
            toRemoveIds.AddRange(allInfrastructureEvents.Skip(keepMaxCount).Select(x => x.Id));
        }

        if (keepMaxDays > 0)
        {
            toRemoveIds.AddRange(allInfrastructureEvents.Where(x => x.Timestamp < DateTime.Now.AddDays(-1 * keepMaxDays)).Select(x => x.Id));
        }

        toRemoveIds = toRemoveIds.Distinct().ToList();

        foreach (var eventId in toRemoveIds)
        {
            await dbQueue.DeleteInfrastructureEvent(eventId);
        }

        return toRemoveIds.Count;
    }
}
