using Metapsi;
using System.Threading.Tasks;
using System.Linq;

namespace MdsLocal
{
    public static partial class ApiBinariesRetriever
    {
        public class State
        {
            public string BinariesApiUrl { get; set; }
            public string ProjectArchivesBasePath { get; set; }
            public string BuildTarget { get; set; }
        }

        public static async Task SetUrl(CommandContext commandContext, State state, string binariesApi)
        {
            state.BinariesApiUrl = binariesApi;
        }

        public static async Task<MdsLocal.ProjectBinary> RetrieveBinaries(
            CommandContext commandContext, 
            State state, 
            string projectName, 
            string projectVersion,
            string intoBinariesPath)
        {
            System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
            //var projectName = serviceConfigurationSnapshot.ProjectName;
            //var versionTag = serviceConfigurationSnapshot.ProjectVersionTag;
            string relativeUrl = $"GetBinaries/{state.BuildTarget}/{projectName}/{projectVersion}";

            var fullUri = new System.Uri(new System.Uri(state.BinariesApiUrl), relativeUrl);

            var response = await httpClient.GetAsync(fullUri, System.Net.Http.HttpCompletionOption.ResponseHeadersRead);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                string errorMessage = $"Could not retrieve binaries from {fullUri.AbsoluteUri}";

                throw new System.Exception(errorMessage);

                //return new SupplyResult()
                //{
                //    Error = errorMessage
                //};
            }

            // Throws HttpRequestException, which is handled
            response.EnsureSuccessStatusCode();

            //var binariesStream = await httpClient.GetStreamAsync(new System.Uri(new System.Uri(state.BinariesApiUrl), relativeUrl));

            using (var binariesStream = await response.Content.ReadAsStreamAsync())
            {
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

                return new MdsLocal.ProjectBinary()
                {
                    FullExePath = exePath
                };
            }
        }
    }
}
