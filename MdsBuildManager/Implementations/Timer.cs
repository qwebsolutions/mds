using MdsBuildManager;
using Microsoft.Extensions.Hosting;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Metapsi;
using Metapsi.Sqlite;

namespace MdsBuildManager
{
    public static class AzureBuilds
    {
        public class State
        {

        }

        public static async Task CheckForever(CommandContext commandContext, InputArguments inputArguments, HashHelper hashHelper, SqliteQueue sqliteQueue)
        {
            if (inputArguments.AzurePoolingIntervalSeconds > 0)
            {
                var notAwaited = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            await CheckOnce(commandContext, inputArguments, hashHelper, sqliteQueue);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        await Task.Delay(TimeSpan.FromSeconds(inputArguments.AzurePoolingIntervalSeconds));
                    }
                });
            }
        }

        public static async Task CheckOnce(CommandContext commandContext, InputArguments inputArguments, HashHelper hashHelper, SqliteQueue sqliteQueue)
        {
            if (string.IsNullOrEmpty(inputArguments.AzureDevopsOrganisation))
                return;

            var connection = new VssConnection(
                        new Uri(inputArguments.AzureDevopsOrganisation),
                        new VssBasicCredential(
                            userName: string.Empty,
                            password: inputArguments.AzureDevopsToken));

            using var buildHttpClient = connection.GetClient<BuildHttpClient>();

            var builds = await buildHttpClient.GetBuildsAsync2(
                project: inputArguments.AzureProject,
                definitions: inputArguments.AzurePipeDefinitions,
                resultFilter: BuildResult.Succeeded);
            //var knownHashes = await this.hashHelper.GetBinariesData();
            var knownBuilds = await hashHelper.GetBuildData(sqliteQueue);
            //var newBuilds = builds
            //                .Where(build => !knownHashes.Any(h => h.BuildId == build.Id));

            var buildsFound = false;

            foreach (var build in builds)
            {
                var buildNumber = build.BuildNumber;
                var buildInfo = build.SourceBranch.Split("/");
                var tag = buildInfo.Last();
                var version = tag.Split("-").Last();
                var commitsha = build.SourceVersion;

                // A build contains more projects at once
                bool buildAlreadyChecked = HashHelper.BuildAlreadyChecked(knownBuilds, build.Id, version, commitsha);

                // If build was already checked, there is no notification at all
                if (!buildAlreadyChecked)
                {
                    var artifact = await buildHttpClient.GetArtifactContentZipAsync(
                                            project: inputArguments.AzureProject,
                                            buildId: build.Id,
                                            artifactName: inputArguments.ArtifactsFolder);

                    using (var archive = new ZipArchive(artifact))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (entry.FullName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                            {
                                var projectName = Path.GetFileNameWithoutExtension(entry.FullName);
                                string osTarget = entry.FullName.Split("/")[1];

                                MemoryStream memoryStream = new MemoryStream();
                                await entry.Open().CopyToAsync(memoryStream);
                                await Algorithm.BuildController.StoreBuild(
                                    commandContext,
                                    inputArguments,
                                    hashHelper,
                                    projectName,
                                    osTarget,
                                    memoryStream,
                                    buildNumber,
                                    commitsha,
                                    tag,
                                    version,
                                    build.Id,
                                    sqliteQueue);
                                buildsFound = true;
                            }
                        }
                    }
                }
            }

            if (buildsFound)
            {
                if (commandContext != null)
                {
                    commandContext.PostEvent(new PollingComplete());
                }
            }
        }
    }

    //public class TimerService : IHostedService, IAsyncDisposable
    //{
    //    private readonly Task completedTask = Task.CompletedTask;
    //    private readonly Processor processor;
    //    private readonly InputArguments inputArguments;
    //    private readonly HashHelper hashHelper;
    //    private readonly CommandContext commandContext;
    //    private System.Threading.Timer? timer;


    //    public TimerService(Processor processor, InputArguments inputArguments, HashHelper hashHelper, CommandContext commandContext)
    //    {
    //        this.processor = processor;
    //        this.inputArguments = inputArguments;
    //        this.hashHelper = hashHelper;
    //        this.commandContext = commandContext;
    //    }

    //    public Task StartAsync(CancellationToken stoppingToken)
    //    {
    //        timer = new System.Threading.Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(this.inputArguments.AzurePoolingIntervalSeconds));

    //        return completedTask;
    //    }

    //    private void DoWork(object? state)
    //    {
    //        Func<Task> work = async () =>
    //        {
    //            try
    //            {
    //                if (string.IsNullOrEmpty(inputArguments.AzureDevopsOrganisation))
    //                    return;

    //                var connection = new VssConnection(
    //                            new Uri(inputArguments.AzureDevopsOrganisation),
    //                            new VssBasicCredential(
    //                                userName: string.Empty,
    //                                password: inputArguments.AzureDevopsToken));

    //                using var buildHttpClient = connection.GetClient<BuildHttpClient>();

    //                var builds = await buildHttpClient.GetBuildsAsync2(
    //                    project: inputArguments.AzureProject,
    //                    definitions: inputArguments.AzurePipeDefinitions,
    //                    resultFilter: BuildResult.Succeeded);
    //                //var knownHashes = await this.hashHelper.GetBinariesData();
    //                var knownBuilds = await this.hashHelper.GetBuildData();
    //                //var newBuilds = builds
    //                //                .Where(build => !knownHashes.Any(h => h.BuildId == build.Id));

    //                foreach (var build in builds)
    //                {
    //                    var buildNumber = build.BuildNumber;
    //                    var buildInfo = build.SourceBranch.Split("/");
    //                    var tag = buildInfo.Last();
    //                    var version = tag.Split("-").Last();
    //                    var commitsha = build.SourceVersion;

    //                    // A build contains more projects at once
    //                    bool buildAlreadyChecked = HashHelper.BuildAlreadyChecked(knownBuilds, build.Id, version, commitsha);

    //                    // If build was already checked, there is no notification at all
    //                    if (!buildAlreadyChecked)
    //                    {
    //                        var artifact = await buildHttpClient.GetArtifactContentZipAsync(
    //                                                project: inputArguments.AzureProject,
    //                                                buildId: build.Id,
    //                                                artifactName: inputArguments.ArtifactsFolder);

    //                        using (var archive = new ZipArchive(artifact))
    //                        {
    //                            foreach (var entry in archive.Entries)
    //                            {
    //                                if (entry.FullName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
    //                                {
    //                                    var projectName = Path.GetFileNameWithoutExtension(entry.FullName);
    //                                    string osTarget = entry.FullName.Split("/")[1];

    //                                    MemoryStream memoryStream = new MemoryStream();
    //                                    await entry.Open().CopyToAsync(memoryStream);
    //                                    await Algorithm.BuildController.StoreBuild(
    //                                        commandContext,
    //                                        this.inputArguments,
    //                                        this.hashHelper,
    //                                        projectName,
    //                                        osTarget,
    //                                        memoryStream,
    //                                        buildNumber,
    //                                        commitsha,
    //                                        tag,
    //                                        version,
    //                                        build.Id);
    //                                }
    //                            }
    //                        }
    //                    }
    //                }

    //                if (commandContext != null)
    //                {
    //                    commandContext.PostEvent(new PollingComplete());
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                Console.WriteLine(ex.Message);
    //            }
    //            finally
    //            {
    //                GC.Collect();
    //            }
    //        };
    //        this.processor.AddProcess(work).ConfigureAwait(false);
    //    }

    //    public Task StopAsync(CancellationToken stoppingToken)
    //    {
    //        timer?.Change(Timeout.Infinite, 0);

    //        return completedTask;
    //    }

    //    public async ValueTask DisposeAsync()
    //    {
    //        if (timer is IAsyncDisposable time)
    //        {
    //            await time.DisposeAsync();
    //        }

    //        timer = null;
    //    }
    //}
}
