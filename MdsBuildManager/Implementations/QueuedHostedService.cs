//using Microsoft.Extensions.Hosting;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace MdsBuildManager
//{
//    //public sealed class QueuedHostedService : BackgroundService
//    //{
//    //    private readonly IBackgroundTaskQueue _taskQueue;

//    //    public QueuedHostedService(IBackgroundTaskQueue taskQueue) => _taskQueue = taskQueue;

//    //    protected override Task ExecuteAsync(CancellationToken stoppingToken)
//    //    {
//    //        return ProcessTaskQueueAsync(stoppingToken);
//    //    }

//    //    private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
//    //    {
//    //        while (!stoppingToken.IsCancellationRequested)
//    //        {
//    //            try
//    //            {
//    //                Func<CancellationToken, ValueTask>? workItem =
//    //                    await _taskQueue.DequeueAsync(stoppingToken);

//    //                await workItem(stoppingToken);
//    //            }
//    //            catch (OperationCanceledException)
//    //            {
//    //                // Prevent throwing if stoppingToken was signaled
//    //            }
//    //            catch (Exception ex)
//    //            {
//    //            }
//    //        }
//    //    }

//    //    public override async Task StopAsync(CancellationToken stoppingToken)
//    //    {
//    //        await base.StopAsync(stoppingToken);
//    //    }
//    //}
//}
