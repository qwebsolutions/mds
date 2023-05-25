using Metapsi;
using System.Threading.Tasks;
using System.Linq;

namespace MdsLocal
{
    public static partial class ZippedProjectRetriever
    {
        public class State
        {
            public string ProjectArchivesBasePath { get; set; }
        }

        public static async Task<ProjectBinary> GetProjectBinary(
            CommandContext commandContext,
            State state,
            MdsCommon.ServiceConfigurationSnapshot serviceConfigurationSnapshot,
            string intoPath)
        {
            var exePath = UnzipToPath(
                state.ProjectArchivesBasePath,
                serviceConfigurationSnapshot.ProjectName,
                serviceConfigurationSnapshot.ProjectVersionTag,
                intoPath);

            return new MdsLocal.ProjectBinary()
            {
                FullExePath = exePath
            };
        }

        //public static MetapsiRuntime.ApplicationSnapshot AddZippedProjectRetriever(
        //    this MetapsiRuntime.ApplicationSnapshot applicationSnapshot,
        //    string name,
        //    string zipSourcePath)
        //{
        //    applicationSnapshot.AddComponent(name, ZippedProjectRetriever.Revive, No.Command, SupplyResults);
        //    applicationSnapshot.AddReviveParameter(name, nameof(State.ProjectArchivesBasePath), zipSourcePath);

        //    return applicationSnapshot;
        //}

        public static string UnzipToPath(
            string projectArchivesBasePath,
            string projectName,
            string projectVersionTag,
            string intoBinariesPath)
        {
            string projectBinaryArchivePath = GetProjectBinaryArchivePath(
                projectArchivesBasePath,
                projectName,
                projectVersionTag);

            //string projectBinariesFolder = System.IO.Path.Combine(state.UnzippedProjectsBasePath, retrievedProjectPath.ForLocalService.ProjectName);
            System.IO.Compression.ZipFile.ExtractToDirectory(projectBinaryArchivePath, intoBinariesPath, true);
            string exePath = System.IO.Path.Combine(intoBinariesPath, projectName);
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                exePath += ".exe";
            }

            return exePath;
        }

        public static string GetProjectBinaryArchivePath(string projectBinariesBasePath, string projectName, string versionTag)
        {
            string folderPath = System.IO.Path.Combine(projectBinariesBasePath, $"{projectName}.{versionTag}.zip");
            return folderPath;
        }
    }
}
