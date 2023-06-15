using Metapsi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdsLocal
{
    static partial class MdsLocalApplication
    {

        public static async Task SynchronizeConfiguration(CommandContext commandContext, State state, string trigger)
        {

            // Previous configuration is compared with up to date configuration to identify upgrade status (changed/unchanged)
            // Up to date configuration overwrites previous one, if changed.

            SyncResult syncResult = new SyncResult()
            {
                ResultCode = SyncStatusCodes.UpToDate,
                Timestamp = DateTime.UtcNow,
                Trigger = trigger
            };

            var nodeConfiguration = await commandContext.Do(GetLocalKnownConfiguration);
            var upToDateConfiguration = await commandContext.Do(Api.GetUpToDateConfiguration);
            LocalServicesConfigurationDiff localServicesDiff = null;

            var newCurrentConfiguration = new List<MdsCommon.ServiceConfigurationSnapshot>();

            switch (nodeConfiguration, upToDateConfiguration)
            {
                case (null, null):
                    throw new Exception("Very bad!");
                case (null, List<MdsCommon.ServiceConfigurationSnapshot> upToDateConfig):
                    {
                        newCurrentConfiguration = upToDateConfig;

                        // Everything is changed, so the diff is from empty configuration to first actual one
                        localServicesDiff = DiffServices(new List<MdsCommon.ServiceConfigurationSnapshot>(), upToDateConfig);
                    }
                    break;
                case (List<MdsCommon.ServiceConfigurationSnapshot> persistedConfig, null):
                    {
                        newCurrentConfiguration = persistedConfig;
                    }
                    break;
                case (List<MdsCommon.ServiceConfigurationSnapshot> persistedConfig, List<MdsCommon.ServiceConfigurationSnapshot> upToDateConfig):
                    {
                        localServicesDiff = DiffServices(persistedConfig, upToDateConfig);

                        if (localServicesDiff.HasAnyUpdate())
                        {
                            syncResult.ResultCode = SyncStatusCodes.Changed;
                            newCurrentConfiguration = upToDateConfig;
                        }

                        break;
                    }
            }

            if (localServicesDiff != null)
            {
                if (localServicesDiff.HasAnyUpdate())
                {
                    // Save configuration
                    await commandContext.Do(OverwriteLocalConfiguration, upToDateConfiguration);

                    // Configuration updated, so dropped services get another chance
                    state.ServiceCrashEvents.Clear();
                    state.DroppedServices.Clear();

                    // Change sync result from default 'UpToDate' to 'Changed'
                    syncResult.ResultCode = SyncStatusCodes.Changed;

                    ConfigurationChanged configurationChanged = new ConfigurationChanged()
                    {
                        LocalServicesConfigurationDiff = localServicesDiff
                    };
                    commandContext.PostEvent(configurationChanged);
                }
            }

            if (upToDateConfiguration != null)
            {
                // Save sync result (either default 'UpToDate', 'Failed' or 'Changed')
                await commandContext.Do(StoreSyncResult, syncResult);

                commandContext.PostEvent(new Event.ConfigurationSynchronized()
                {
                    ResultCode = syncResult.ResultCode
                });

                await SynchronizeRunningProcesses(commandContext, state);
            }
        }

        private static void DeleteServiceFolder(string servicesBasePath, string serviceName)
        {
            string serviceFullPath = System.IO.Path.Combine(servicesBasePath, serviceName);
            System.IO.Directory.Delete(serviceFullPath, true);
            Console.WriteLine($"DeleteServiceFolder {serviceFullPath}");
        }

        private static bool SameVersionIsInstalled(string servicesBasePath, MdsCommon.ServiceConfigurationSnapshot serviceConfiguration)
        {
            string serviceFullPath = System.IO.Path.Combine(servicesBasePath, serviceConfiguration.ServiceName);
            if (!System.IO.Directory.Exists(serviceFullPath))
                return false;

            try
            {
                var serviceVersion = GetServiceVersionData(servicesBasePath, serviceConfiguration.ServiceName);
                return serviceVersion.Version == serviceConfiguration.ProjectVersionTag;
            }
            catch (Exception ex)
            {
                // We couldn't read file & deserialize, probably the file structure changed, so better reinstall
                return false;
            }
        }

        private static async Task StartService(CommandContext commandContext, State state, string serviceName)
        {
            // Do not attempt to start service that was already dropped (possibly for a different reason than restart loop)
            if (state.DroppedServices.Contains(serviceName))
            {
                Console.WriteLine($"StartService dropped {serviceName}");
                return;
            }

            state.ServiceCrashEvents.RemoveAll(x => (DateTime.UtcNow - x.CrashTimestamp).TotalHours > 24);

            string serviceExeFullPath = System.IO.Path.Combine(state.ServicesBasePath, serviceName, MdsLocalApplication.GetServiceExeName(state.NodeName, serviceName));
            string inFolder = System.IO.Path.GetDirectoryName(serviceExeFullPath);
            await StartProcess(commandContext, state, serviceExeFullPath, false, inFolder);
            Console.WriteLine($"StartService {serviceExeFullPath}");
        }
    }

    public class UnstableServiceDropped : IData
    {
        public string ServiceName { get; set; }
        public int RestartCount { get; set; }
        public int InSeconds { get; set; }
    }

    //public class MisconfiguredServiceDropped: MetapsiRuntime.IData
    //{
    //    public string ServiceName { get; set; }
    //    public string ErrorMessage { get; set; }
    //}
}
