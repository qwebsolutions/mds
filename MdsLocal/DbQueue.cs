using Metapsi;
using System.Threading.Tasks;

namespace MdsLocal;

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
}
