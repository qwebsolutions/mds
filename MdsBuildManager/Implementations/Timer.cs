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

            using var connection = new VssConnection(
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
                    using var artifact = await buildHttpClient.GetArtifactContentZipAsync(
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

                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    using (var entryStream = entry.Open())
                                    {
                                        await entryStream.CopyToAsync(memoryStream);
                                        entryStream.Close();
                                    }

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
}
