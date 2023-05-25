using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MdsBuildManager
{
    public class Processor
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly CancellationToken _cancellationToken;

        public Processor(
            IBackgroundTaskQueue taskQueue,
            IHostApplicationLifetime applicationLifetime)
        {
            _taskQueue = taskQueue;
            _cancellationToken = applicationLifetime.ApplicationStopping;
        }

        public async ValueTask AddProcess(Func<Task> work)
        {
            await _taskQueue.QueueBackgroundWorkItemAsync(cancelToken => {
                if (!cancelToken.IsCancellationRequested)
                {
                    return new ValueTask(work());
                }
                    
                return new ValueTask(Task.FromCanceled(cancelToken));
            });
        }
    }
}
