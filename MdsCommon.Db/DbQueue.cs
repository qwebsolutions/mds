using Metapsi;
using Metapsi.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsCommon;

//public class DbQueue : TaskQueue<string>
//{
//    public long TotalMilliseconds { get; set; } = 0;
//    public int TotalCalls { get; set; } = 0;

//    public DbQueue(string fullDbPath) : base(fullDbPath)
//    {
//    }

//    public override async Task Enqueue(Func<string, Task> task)
//    {
//        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
//        await base.Enqueue(task);
//        TotalMilliseconds += sw.ElapsedMilliseconds;
//        TotalCalls += 1;
//    }

//    public override async Task<TResult> Enqueue<TResult>(Func<string, Task<TResult>> task)
//    {
//        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
//        var result = await base.Enqueue(task);
//        TotalMilliseconds += sw.ElapsedMilliseconds;
//        TotalCalls += 1;
//        var total = TimeSpan.FromMilliseconds(TotalMilliseconds);
//        return result;
//    }
//}

public static class DbQueueExtensions
{
    public static async Task<List<InfrastructureEvent>> GetAllInfrastructureEvents(this SqliteQueue queue)
    {
        return await Db.LoadAllInfrastructureEvents(queue);
    }

    public static async Task DeleteInfrastructureEvent(this SqliteQueue queue, Guid id)
    {
        await queue.WithCommit(
                async c => await c.DeleteRecord<InfrastructureEvent>(id));
    }

    public static async Task<int> CleanupInfrastructureEvents(this SqliteQueue dbQueue, int keepMaxCount, int keepMaxDays)
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
