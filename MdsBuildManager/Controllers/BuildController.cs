﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
//using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Payloads;
using System.Security.Cryptography;
using System.IO.Compression;
using MdsCommon;
using MdsBuildManager;
using Metapsi;
using Microsoft.AspNetCore.Http;

namespace Algorithm
{
    [Route("")]
    public partial class BuildController : Controller
    {
        private readonly Processor processor;
        private InputArguments InputArguments = null;
        private readonly HashHelper hashHelper;
        private readonly CommandContext commandContext;

        public BuildController(
            Processor processor,
            InputArguments inputArguments,
            HashHelper hashHelper,
            CommandContext commandContext)
        {
            this.processor = processor;
            this.InputArguments = inputArguments;
            this.hashHelper = hashHelper;
            this.commandContext = commandContext;
        }

        [HttpPost]
        [Route("PostBuildNotification")]
        public async Task<IActionResult> PostBuildNotification([FromBody] BuildCompletedPayload buildNotification)
        {
            Func<Task> work = async () =>
            {
                string jsonBuildNotification = System.Text.Json.JsonSerializer.Serialize(buildNotification);

                var buildNumber = buildNotification.Resource.BuildNumber;
                var buildInfo = buildNotification.Resource.SourceGetVersion.Split(":");
                var tag = buildInfo[1].Split("/").Last();
                var version = tag.Split("-").Last();
                var commitsha = buildInfo[2];
                var connection = new VssConnection(
                    new Uri(InputArguments.AzureDevopsOrganisation),
                    new VssBasicCredential(
                        userName: string.Empty,
                        password: InputArguments.AzureDevopsToken));

                // project id
                var azureDevopsProjectId = Guid.Parse(buildNotification.ResourceContainers.Project.Id);

                // pipeline id
                var azureDevopsPipelineId = buildNotification.Resource.Definition.Id;

                // build id
                var buildId = buildNotification.Resource.Id;

                // Create the client for interacting with Azure DevOps Pipelines
                using var buildHttpClient = connection.GetClient<BuildHttpClient>();

                //var bi = await buildHttpClient.GetBuildAsync(azureDevopsProjectId, buildId);

                try
                {
                    var artifact = await buildHttpClient.GetArtifactContentZipAsync(
                                        project: azureDevopsProjectId,
                                        buildId: buildId,
                                        artifactName: InputArguments.ArtifactsFolder);

                    var knownHashes = this.hashHelper.GetBinariesData();

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
                                await StoreBuild(
                                    this.commandContext,
                                    this.InputArguments,
                                    this.hashHelper,
                                    projectName,
                                    osTarget,
                                    memoryStream,
                                    buildNumber,
                                    commitsha,
                                    tag,
                                    version,
                                    buildId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            };
            await processor.AddProcess(work);

            return Ok();
        }

        public class BinariesUploadModel
        {
            public IFormFile Binaries { get; set; }
        }

        [DisableRequestSizeLimit]
        [HttpPost("UploadBinaries")]
        //public ActionResult UploadBinaries([FromForm] BinariesUploadModel model)
        public async Task<IActionResult> UploadBinaries()
        {
            try
            {
                var knownBuilds = await hashHelper.GetBuildData();
                var knownBinaries = await hashHelper.GetBinariesData();

                var projectName = Request.Form["project"];
                var version = Request.Form["version"];
                var revision = Request.Form["revision"];
                var target = Request.Form["target"];

                var today = DateTime.UtcNow.ToString("yyyyMMdd");

                var todayBuilds = knownBuilds.Where(x => x.BuildNumber.StartsWith(today));
                var todayBuildIndex = 1;
                if (todayBuilds.Any())
                {
                    todayBuildIndex = todayBuilds.Select(x => int.Parse(x.BuildNumber.Replace(today, string.Empty).Replace(".", string.Empty))).Max() + 1;
                }

                var buildNumber = $"{today}.{todayBuildIndex}";

                // Getting Image
                var binariesFile = Request.Form.Files[0];


                MemoryStream memoryStream = new MemoryStream();
                await binariesFile.CopyToAsync(memoryStream);

                if (string.IsNullOrWhiteSpace(target))
                {
                    target = "linux-x64";

                    var archiveStream = new MemoryStream();
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await memoryStream.CopyToAsync(archiveStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    using (var archive = new ZipArchive(archiveStream))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (entry.Name.EndsWith($"{projectName}.exe"))
                            {
                                target = "win10-x64";
                                break;
                            }
                        }
                    }
                }

                if (knownBuilds.Any(x => x.ProjectName == projectName && x.Version == version && x.Target == target))
                {
                    throw new Exception("\nERROR! Project version already built for this target");
                }
                    
                await StoreBuild(
                    this.commandContext, 
                    this.InputArguments, 
                    this.hashHelper, 
                    projectName,
                    target,
                    memoryStream, 
                    buildNumber, 
                    revision, 
                    revision, 
                    version, 
                    0);

                commandContext.PostEvent(new UploadComplete());

                return Ok("\nUpload complete");
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet("ListBinaries")]
        public async Task<IActionResult> ListBinaries()
        {
            var algorithms = new List<AlgorithmInfo>();
            var allBinaries = await this.hashHelper.GetBinariesData();
            var allBuildData = await this.hashHelper.GetBuildData();

            foreach (var binary in allBinaries)
            {
                if (System.IO.File.Exists(binary.BinaryPath))
                {
                    var versionsWithThisBinaries = allBuildData.Where(x => x.Base64Hash == binary.Base64Hash).OrderByDescending(x => x.Timestamp);

                    var buildData = versionsWithThisBinaries.FirstOrDefault();

                    if (buildData != null)
                    {

                        algorithms.Add(new AlgorithmInfo()
                        {
                            GitHash = buildData.CommitSha,
                            Name = buildData.ProjectName,
                            Version = buildData.Version,
                            BuildNumber = buildData.BuildNumber,
                            Target = buildData.Target
                        });
                    }
                }
            }

            return Ok(algorithms.OrderBy(a => a.Name));
        }

        private static string ProjectName(string path)
        {
            return System.IO.Path.GetFileName(path).Split(".").First();
        }

        private static string Target(string path)
        {
            string directoryName = System.IO.Path.GetDirectoryName(path);
            return new DirectoryInfo(directoryName).Name;
        }

        [HttpGet("GetBinaries/{target}/{algorithmName}/{algorithmVersion}")]
        public async Task<IActionResult> GetAlgorithmFile(string target, string algorithmName, string algorithmVersion)
        {
            var builds = await this.hashHelper.GetBuildData();
            var binaries = await this.hashHelper.GetBinariesData();

            var knownProject = builds.SingleOrDefault(x => x.ProjectName == algorithmName && x.Version == algorithmVersion && x.Target == target);

            if (knownProject == null)
                return NotFound();

            var knownBinary = binaries.SingleOrDefault(x => x.Base64Hash == knownProject.Base64Hash);

            if (knownBinary != null)
            {
                using var stream = System.IO.File.OpenRead(knownBinary.BinaryPath);
                var fileinfo = Path.GetFileName(knownBinary.BinaryPath);
                using var memStream = new MemoryStream();
                await stream.CopyToAsync(memStream);

                return File(memStream.ToArray(), "application/zip", $"{fileinfo}");
            }

            return NotFound();
        }

        [HttpGet("Health")]
        public async Task<IActionResult> Health()
        {
            return Ok("All is good!");
        }


        public static async Task StoreBuild(
            CommandContext commandContext,
            InputArguments inputArguments,
            HashHelper hashHelper,
            string projectName,
            string osTarget,
            MemoryStream projectArchiveStream,
            string buildNumber,
            string commitsha,
            string tag,
            string version,
            int buildId)
        {
            await hashHelper.CreateDbSchema();


            // If build is new, register it
            // If binaries are duplicate of previous, notify & do not save

            var knownBuilds = await hashHelper.GetBuildData();
            var knownBinaries = await hashHelper.GetBinariesData();

            string hash = hashHelper.GetProjectFilesHash(projectArchiveStream);
                        
            Binaries sameBinaries = knownBinaries.FirstOrDefault(x => x.Base64Hash == hash);

            if (sameBinaries != null)
            {
                // Binaries already available, new build is a duplicate of some previous version, do not store it
                // The project version itself could be new though

                var exactlySameBuild = knownBuilds.FirstOrDefault(x => x.CommitSha == commitsha && x.BuildId == buildId && x.BuildNumber == buildNumber && x.Version == version && x.Tag == tag && x.ProjectName == projectName && x.Target == osTarget);

                if (exactlySameBuild != null)
                {
                    // Same build is checked twice, ignore any result
                }
                else
                {
                    // Add just the version, the binaries are already available

                    using (var connection = await hashHelper.OpenNewConnectionAsync())
                    {
                        var transaction = connection.BeginTransaction();

                        await hashHelper.AddNewVersion(transaction, tag, buildNumber, commitsha, projectName, version, buildId, hash, osTarget);

                        transaction.Commit();
                    }

                    if (commandContext != null && inputArguments.NotifyDuplicateBinaries)
                    {
                        var duplicateOf = knownBuilds.OrderByDescending(x => x.Timestamp).FirstOrDefault(x => x.Base64Hash == hash);

                        if (duplicateOf != null)
                        {
                            commandContext.PostEvent(new DuplicateBinaries()
                            {
                                Base64Hash = hash,
                                BinaryPath = sameBinaries.BinaryPath,
                                NewCommitSha = commitsha,
                                NewVersion = version,
                                PreviousCommitSha = duplicateOf.CommitSha,
                                PreviousVersion = duplicateOf.Version
                            });
                        }
                    }
                }
            }
            else
            {
                // New binaries, must be stored, alongside version
                var fileExtension = "zip";
                var buildTargetFolderPath = System.IO.Path.Combine(inputArguments.BinariesFolder, osTarget);
                Directory.CreateDirectory(buildTargetFolderPath);
                var path = System.IO.Path.Combine(buildTargetFolderPath, $"{projectName}.{commitsha}.{fileExtension}");
                await using (Stream file = System.IO.File.Create(path))
                {
                    projectArchiveStream.Seek(0, SeekOrigin.Begin);
                    await projectArchiveStream.CopyToAsync(file);
                }

                using (var connection = await hashHelper.OpenNewConnectionAsync())
                {
                    var transaction = connection.BeginTransaction();

                    await hashHelper.AddNewBinaries(transaction, hash, path);
                    await hashHelper.AddNewVersion(transaction, tag, buildNumber, commitsha, projectName, version, buildId, hash, osTarget);

                    transaction.Commit();
                }

                if (commandContext != null && inputArguments.NotifyNewBinaries)
                {
                    commandContext.PostEvent(new NewBinaries()
                    {
                        NewCommitSha = commitsha,
                        NewVersion = version,
                        BinaryPath = path
                    });
                }
                return;
            }
        }
    }
}
