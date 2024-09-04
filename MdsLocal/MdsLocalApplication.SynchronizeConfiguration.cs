using MdsCommon;
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
            bool needsDeploy = false;

            SyncResult syncResult = new SyncResult()
            {
                ResultCode = SyncStatusCodes.UpToDate,
                Timestamp = DateTime.UtcNow,
                Trigger = trigger
            };

            GetUpToDateConfigurationResponse upToDateConfigurationResult = null;

            try
            {
                // Previous configuration is compared with up to date configuration to identify upgrade status (changed/unchanged)
                // Up to date configuration overwrites previous one, if changed.

                syncResult.AddInfo("Loading local configuration ...");
                var nodeConfiguration = await commandContext.Do(GetLocalKnownConfiguration);
                syncResult.AddInfo("Local configuration loaded");
                syncResult.AddInfo("Retrieving updated configuration ...");
                upToDateConfigurationResult = await commandContext.Do(Api.GetUpToDateConfiguration);
                var upToDateConfiguration = upToDateConfigurationResult.ServiceSnapshots;
                syncResult.AddInfo("Updated configuration retrieved");
                LocalServicesConfigurationDiff localServicesDiff = null;

                var newCurrentConfiguration = new List<MdsCommon.ServiceConfigurationSnapshot>();

                switch (nodeConfiguration, upToDateConfiguration)
                {
                    case (null, null):
                        throw new Exception("No configuration!");
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
                        needsDeploy = true;
                        commandContext.NotifyGlobal(new DeploymentEvent.Started()
                        {
                            DeploymentId = upToDateConfigurationResult.CurrentDeploymentId
                        });

                        foreach (var serviceDiff in localServicesDiff.AddedServices)
                        {
                            syncResult.AddInfo($"New service detected: {serviceDiff.ServiceName}");
                        }

                        foreach (var serviceDiff in localServicesDiff.ChangedServices)
                        {
                            syncResult.AddInfo($"Service change detected: {serviceDiff.Previous.ServiceName}");
                        }

                        foreach (var serviceDiff in localServicesDiff.RemovedServices)
                        {
                            syncResult.AddInfo($"Service removal detected: {serviceDiff.ServiceName}");
                        }

                        syncResult.AddInfo("Saving new configuration ...");

                        // Save configuration
                        await commandContext.Do(OverwriteLocalConfiguration, upToDateConfiguration);


                        syncResult.AddInfo("Configuration saved");

                        // Configuration updated, so dropped services get another chance
                        state.ServiceCrashEvents.Clear();

                        foreach (var droppedService in state.DroppedServices)
                        {
                            syncResult.AddInfo($"Dropped service {droppedService} reset, will attempt restart");
                        }

                        state.DroppedServices.Clear();

                        // Change sync result from default 'UpToDate' to 'Changed'
                        syncResult.ResultCode = SyncStatusCodes.Changed;

                        //ConfigurationChanged configurationChanged = new ConfigurationChanged()
                        //{
                        //    LocalServicesConfigurationDiff = localServicesDiff
                        //};
                        //commandContext.PostEvent(configurationChanged);
                    }
                    else
                    {
                        syncResult.AddInfo($"No changes detected");
                    }
                }

                if (upToDateConfiguration != null)
                {
                    // Save sync result (either default 'UpToDate', 'Failed' or 'Changed')
                    //await commandContext.Do(StoreSyncResult, syncResult);

                    commandContext.PostEvent(new Event.ConfigurationSynchronized()
                    {
                        ResultCode = syncResult.ResultCode
                    });

                    await SynchronizeRunningProcesses(commandContext, state, localServicesDiff, syncResult, upToDateConfigurationResult != null ? upToDateConfigurationResult.CurrentDeploymentId : Guid.Empty);
                }
            }
            catch (Exception ex)
            {
                commandContext.Logger.LogException(ex, "SynchronizeConfiguration");
                syncResult.AddError(ex.Message);
            }
            finally
            {
                await commandContext.Do(StoreSyncResult, syncResult);
                if (needsDeploy)
                {
                    await CheckMachineStatus(commandContext, state);
                    if (upToDateConfigurationResult != null)
                    {
                        {
                            commandContext.NotifyGlobal(new DeploymentEvent.Done()
                            {
                                DeploymentId = upToDateConfigurationResult.CurrentDeploymentId
                            });
                        }
                    }
                }
            }
        }

        private static void DeleteServiceFolder(CommandContext commandContext, string servicesBasePath, string serviceName)
        {
            try
            {
                string serviceFullPath = System.IO.Path.Combine(servicesBasePath, serviceName);
                System.IO.Directory.Delete(serviceFullPath, true);
                Console.WriteLine($"DeleteServiceFolder {serviceFullPath}");
            }
            catch (Exception ex)
            {
                commandContext.Logger.LogException(ex, "DeleteServiceFolder");
            }
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
