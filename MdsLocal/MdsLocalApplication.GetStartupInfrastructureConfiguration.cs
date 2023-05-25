using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Metapsi;

namespace MdsLocal
{
    public static partial class MdsLocalApplication
    {
        [System.Runtime.InteropServices.DllImport("libc", SetLastError = true)]
        private static extern int chmod(string pathname, int mode);

        const int S_IRUSR = 0x100;
        const int S_IWUSR = 0x80;
        const int S_IXUSR = 0x40;

        // group permission
        const int S_IRGRP = 0x20;
        const int S_IWGRP = 0x10;
        const int S_IXGRP = 0x8;

        // other permissions
        const int S_IROTH = 0x4;
        const int S_IWOTH = 0x2;
        const int S_IXOTH = 0x1;



        public class ServiceCrashEvent
        {
            public string ServiceName { get; set; }
            public DateTime CrashTimestamp { get; set; }
            public string Error { get; set; } = string.Empty;
        }



        public static async Task ValidateSchema(CommandContext commandContext, State state)
        {
            state.StartupWarnings = await commandContext.Do(PerformStartupValidations);
        }

        public static async Task GetStartupInfrastructureConfiguration(CommandContext commandContext, State state)
        {

            var infraConfig = await commandContext.Do(Api.GetInfrastructureNodeSettings);

            commandContext.Logger.LogDebug($"infra config: {Metapsi.Serialize.ToJson(infraConfig)}");

            // Initial reference must be kept, it is shared! This moves data from response to shared InfrastructureConfiguration
            state.InfrastructureConfiguration.BinariesApiUrl = infraConfig.BinariesApiUrl;
            state.InfrastructureConfiguration.BroadcastDeploymentInputChannel = infraConfig.BroadcastDeploymentInputChannel;
            state.InfrastructureConfiguration.HealthStatusOutputChannel = infraConfig.HealthStatusOutputChannel;
            state.InfrastructureConfiguration.InfrastructureEventsOutputChannel = infraConfig.InfrastructureEventsOutputChannel;
            state.InfrastructureConfiguration.InfrastructureName = infraConfig.InfrastructureName;
            state.InfrastructureConfiguration.NodeCommandInputChannel = infraConfig.NodeCommandInputChannel;
            state.InfrastructureConfiguration.NodeUiPort = infraConfig.NodeUiPort;

            commandContext.Logger.LogInfo($"state config: {Metapsi.Serialize.ToJson(state)}");

            commandContext.PostEvent(new Event.GlobalControllerReached()
            {
                InfrastructureConfiguration = infraConfig
            });
        }

        //public static async Task SaveLocalEvent(CommandContext commandContext, State state, MdsCommon.InfrastructureEvent localEvent)
        //{
        //    await commandContext.Do(MdsCommon.MdsCommonFunctions.SaveInfrastructureEvent, localEvent);
        //}

        public static string GuessServiceName(string nodeName, string fullExePath)
        {
            string processName = System.IO.Path.GetFileName(fullExePath);
            string serviceName = processName.Replace(ExePrefix(nodeName), string.Empty).Replace(".exe", string.Empty);
            return serviceName;
        }

        public static string ExePrefix(string nodeName)
        {
            return $"_{nodeName}.";
        }

        public static List<System.Diagnostics.Process> IdentifyOwnedProcesses(string nodeName)
        {
            List<System.Diagnostics.Process> ownedProcesses = new List<System.Diagnostics.Process>();
            System.Diagnostics.Process[] allProcesses = System.Diagnostics.Process.GetProcesses();
            foreach (var process in allProcesses)
            {
                if (process.ProcessName.StartsWith(ExePrefix(nodeName)))
                    ownedProcesses.Add(process);
            }

            return ownedProcesses;
        }

        public static System.Diagnostics.Process GetRunningProcessForService(string nodeName, MdsCommon.ServiceConfigurationSnapshot localService)
        {
            string processName = GetProcessName(GetServiceExeName(nodeName, localService.ServiceName));
            //MdsCommon.Debug.Log("SEARCHINGPROCESSNAME", processName);
            System.Diagnostics.Process[] matchingProcesses = System.Diagnostics.Process.GetProcessesByName(processName);
            //var allProcesses = System.Diagnostics.Process.GetProcesses();
            //foreach (var process in allProcesses)
            //{
            //    MdsCommon.Debug.Log($"PROCESSNAME: {process.ProcessName}");
            //}
            //MdsCommon.Debug.Log("MATCHEDPROCESSES", matchingProcesses.Length);
            if (matchingProcesses.Count() > 1)
            {
                throw new Exception("Multiple running services!");
            }

            if (matchingProcesses.Count() == 1)
            {
                return matchingProcesses.First();
            }
            return null;
        }

        public static string GetProcessName(string serviceExeName)
        {
            return serviceExeName.Replace(".exe", string.Empty);
        }

        public static string ServiceName(string processPath)
        {
            return System.IO.Path.GetFileNameWithoutExtension(processPath);
        }

        public static string GetVersionTag(string processPath)
        {
            string[] segments = processPath.Split(System.IO.Path.DirectorySeparatorChar);
            return segments.Reverse().ToList()[1];
        }

        public static string GetProjectName(string processPath)
        {
            string[] segments = processPath.Split(System.IO.Path.DirectorySeparatorChar);
            return segments.Reverse().ToList()[2];
        }
    }

    public class MemoryMetrics
    {
        public double Total;
        public double Used;
        public double Free;
    }


    //public class LocalEvent: IData
    //{
    //    public MdsCommon.InfrastructureEvent InfrastructureEvent { get; set; }
    //}
}
