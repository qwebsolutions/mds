using Metapsi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MdsCommon;

public static class DebugTo
{
    private static TaskQueue FileCreatorQueue = new TaskQueue();
    private static Dictionary<string, TaskQueue> FileWriterQueues = new Dictionary<string, TaskQueue>();

    public static async Task File(string fileName, string text)
    {
#if DEBUG
        await FileCreatorQueue.Enqueue(async () =>
        {
            if (!FileWriterQueues.ContainsKey(fileName))
            {
                FileWriterQueues.Add(fileName, new TaskQueue());
                string dir = System.IO.Path.GetDirectoryName(fileName);
                System.IO.Directory.CreateDirectory(dir);
                await System.IO.File.WriteAllTextAsync(fileName, string.Empty);
            }
        });

        Console.WriteLine($"FileWriterQueues {FileWriterQueues.Count}");

        await FileWriterQueues[fileName].Enqueue(async () =>
        {
            // Use builder to preserve os specific newline
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(text);
            await System.IO.File.AppendAllTextAsync(fileName, builder.ToString());
        });
#endif
    }
}