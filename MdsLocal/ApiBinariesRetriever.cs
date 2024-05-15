using Metapsi;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.IO;

namespace MdsLocal
{
    public static partial class ApiBinariesRetriever
    {
        private static TimeSpan KeepInMemory = TimeSpan.FromMinutes(5);

        private static HttpClient httpClient = new System.Net.Http.HttpClient();

        public class InMemoryBinaries
        {
            public string ProjectName { get; set; }
            public string ProjectVersion { get; set; }
            public System.IO.Stream Stream { get; set; }
            public DateTime RetrievalTimestamp { get; set; } = DateTime.UtcNow;
        }

        public class State
        {
            public string BinariesApiUrl { get; set; }
            public string ProjectArchivesBasePath { get; set; }
            public string BuildTarget { get; set; }
            public List<InMemoryBinaries> CachedBinaries { get; set; } = new List<InMemoryBinaries>();


            public long totalRetrieveBinariesMs = 0;
            public int totalRetrieveBinariesCount = 0;
        }

        public static async Task SetUrl(CommandContext commandContext, State state, string binariesApi)
        {
            state.BinariesApiUrl = binariesApi;
        }

        public static async Task CleanupBinaries(CommandContext commandContext, State state)
        {
            state.CachedBinaries.RemoveAll(x => (DateTime.UtcNow - x.RetrievalTimestamp) > KeepInMemory);
        }

        public static async Task<MdsLocal.ProjectBinary> RetrieveBinaries(
            CommandContext commandContext,
            State state,
            string projectName,
            string projectVersion,
            string intoBinariesPath)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var cachedBinaries = state.CachedBinaries.SingleOrDefault(x => x.ProjectName == projectName && x.ProjectVersion == projectVersion);

            if (cachedBinaries == null)
            {
                string relativeUrl = $"GetBinaries/{state.BuildTarget}/{projectName}/{projectVersion}";

                var fullUri = new System.Uri(new System.Uri(state.BinariesApiUrl), relativeUrl);

                var response = await httpClient.GetAsync(fullUri, System.Net.Http.HttpCompletionOption.ResponseHeadersRead);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var alternativeTarget = string.Empty;
                    if (state.BuildTarget == "win10-x64")
                    {
                        alternativeTarget = "win-x64";
                    }

                    if (state.BuildTarget == "win-x64")
                    {
                        alternativeTarget = "win10-x64";
                    }

                    var alternativeUri = new System.Uri(
                        new System.Uri(state.BinariesApiUrl),
                        $"GetBinaries/{alternativeTarget}/{projectName}/{projectVersion}");

                    response = await httpClient.GetAsync(alternativeUri, System.Net.Http.HttpCompletionOption.ResponseHeadersRead);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    string errorMessage = $"Could not retrieve binaries from {fullUri.AbsoluteUri}";

                    throw new System.Exception(errorMessage);
                }

                // Throws HttpRequestException, which is handled
                response.EnsureSuccessStatusCode();

                using (var zipStream = await response.Content.ReadAsStreamAsync())
                {
                    MemoryStream memoryStream = new MemoryStream();
                    await zipStream.CopyToAsync(memoryStream);

                    cachedBinaries = new InMemoryBinaries()
                    {
                        ProjectName = projectName,
                        ProjectVersion = projectVersion,
                        Stream = memoryStream
                    };

                    state.CachedBinaries.Add(cachedBinaries);
                }
            }

            var binariesStream = cachedBinaries.Stream;
            binariesStream.Position = 0;

            var tempFile = System.IO.Path.GetTempFileName();

            using (var fileStream = System.IO.File.Create(tempFile))
            {
                await binariesStream.CopyToAsync(fileStream);
            }

            System.IO.Compression.ZipFile.ExtractToDirectory(tempFile, intoBinariesPath, true);
            string exePath = System.IO.Path.Combine(intoBinariesPath, projectName);
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                exePath += ".exe";
            }

            System.IO.File.Delete(tempFile);

            state.totalRetrieveBinariesMs += sw.ElapsedMilliseconds;
            state.totalRetrieveBinariesCount++;
            System.Diagnostics.Debug.WriteLine($"Retrieve binaries {sw.ElapsedMilliseconds} ms");
            System.Diagnostics.Debug.WriteLine($"Total retrieve binaries {state.totalRetrieveBinariesMs} ms");
            System.Diagnostics.Debug.WriteLine($"Total retrieve binaries count {state.totalRetrieveBinariesCount}");

            return new MdsLocal.ProjectBinary()
            {
                FullExePath = exePath
            };
        }
    }
}
