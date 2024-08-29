using Metapsi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MdsCommon;

namespace MdsLocal
{
    public static partial class MdsLocalApplication
    {
        public static partial class Event
        {
            public class ProcessesSynchronized : IData
            {
                public List<string> Stopped { get; set; } = new List<string>();
                public List<string> Started { get; set; } = new List<string>();
            }
        }

        public static async Task SynchronizeRunningProcesses(CommandContext commandContext, State state)
        {
            var localKnownConfiguration = await commandContext.Do(GetLocalKnownConfiguration);
            var fakeDiff = DiffServices(localKnownConfiguration, localKnownConfiguration);

            await SynchronizeRunningProcesses(commandContext, state, fakeDiff, new SyncResult());
        }

        public static async Task SynchronizeRunningProcesses(CommandContext commandContext, State state, LocalServicesConfigurationDiff configurationDiff, SyncResult syncResult)
        {
            Event.ProcessesSynchronized processesSynchronized = new Event.ProcessesSynchronized();
            //syncResult.AddInfo("Reloading local configuration after update ...");
            var localKnownConfiguration = await commandContext.Do(GetLocalKnownConfiguration);
            //syncResult.AddInfo("Reloaded");
            var serviceProcesses = await commandContext.Do(GetRunningProcesses);

            syncResult.AddInfo($"{serviceProcesses.Count} running processes identified for node {state.NodeName}");

            // Stop services that are running, but should not be, because they are removed or have different configuration

            foreach (var serviceProcess in serviceProcesses)
            {
                string configurationId = string.Empty;

                var serviceConfiguration = localKnownConfiguration.SingleOrDefault(x => x.ServiceName == serviceProcess.ServiceName);

                // Completely removed, stop
                if (serviceConfiguration == null)
                {
                    syncResult.AddInfo($"{serviceProcess.ServiceName} not included in the new configuration. Attempting stop ...");
                    processesSynchronized.Stopped.Add(serviceProcess.ServiceName);
                    await commandContext.Do(StopProcess, serviceProcess);
                    syncResult.AddInfo($"Service {serviceProcess.FullExePath} (PID {serviceProcess.Pid}) stopped");
                    commandContext.PostEvent(new DeploymentEvent.ServiceStopped()
                    {
                        Id = serviceConfiguration.Id
                    });
                    commandContext.Logger.LogDebug($"SynchronizeRunningProcesses: {serviceProcess.ServiceName} stopped");
                }
                else
                {
                    var changedService = configurationDiff.ChangedServices.SingleOrDefault(x => x.Next.ServiceName == serviceProcess.ServiceName);

                    if (changedService != null)
                    {
                        if (!changedService.Next.Enabled)
                        {
                            syncResult.AddInfo($"Service {serviceProcess.ServiceName} disabled, attempting stop...");
                        }
                        else
                        {
                            syncResult.AddInfo($"Service {serviceProcess.ServiceName} configuration changed, attempting stop...");
                        }
                        // Configuration changed, should be reinstalled
                        processesSynchronized.Stopped.Add(serviceProcess.ServiceName);
                        await commandContext.Do(StopProcess, serviceProcess);
                        syncResult.AddInfo($"Service {serviceProcess.ServiceName} stopped");
                        commandContext.PostEvent(new DeploymentEvent.ServiceStopped()
                        {
                            Id = serviceConfiguration.Id
                        });
                        commandContext.Logger.LogDebug($"SynchronizeRunningProcesses: {serviceProcess.ServiceName} stopped");
                    }
                }
            }

            // On first installation directory is not even created
            if (System.IO.Directory.Exists(state.ServicesBasePath))
            {
                // Remove directory if either service is not needed anymore or configuration is changed
                foreach (string serviceDirectory in System.IO.Directory.GetDirectories(state.ServicesBasePath))
                {
                    string serviceName = System.IO.Path.GetFileName(serviceDirectory);

                    var serviceConfiguration = localKnownConfiguration.SingleOrDefault(x => x.ServiceName == serviceName);

                    string serviceFullPath = System.IO.Path.Combine(state.ServicesBasePath, serviceName);

                    // Deleted, so remove directory
                    if (serviceConfiguration == null)
                    {
                        syncResult.AddInfo($"Attempting to remove directory {serviceFullPath} ...");
                        DeleteServiceFolder(commandContext, state.ServicesBasePath, serviceName);
                        syncResult.AddInfo($"Removed");
                    }
                    else
                    {
                        // If service folder is not valid (partially removed or misconfigured by hand)
                        if (!System.IO.File.Exists(GetMdsParametersPath(state.ServicesBasePath, serviceName)))
                        {
                            syncResult.AddWarning($"directory {serviceFullPath} seems to be misconfigured, configuration files are missing");
                            syncResult.AddInfo($"Attempting to remove directory {serviceFullPath} ...");
                            DeleteServiceFolder(commandContext, state.ServicesBasePath, serviceName);
                            syncResult.AddInfo($"Removed");
                        }
                        else
                        {
                            var changedService = configurationDiff.ChangedServices.SingleOrDefault(x => x.Next.ServiceName == serviceName);
                            if (changedService != null)
                            {
                                if (changedService.Previous.ProjectVersionTag != changedService.Next.ProjectVersionTag)
                                {
                                    syncResult.AddInfo($"Binaries changed for {serviceName}");
                                    syncResult.AddInfo($"Previous version {changedService.Previous.ProjectVersionTag}");
                                    syncResult.AddInfo($"Next version {changedService.Next.ProjectVersionTag}");
                                    syncResult.AddInfo($"Attempting to remove directory {serviceFullPath} ...");
                                    DeleteServiceFolder(commandContext, state.ServicesBasePath, serviceName);
                                    syncResult.AddInfo($"Removed");
                                }
                            }
                        }
                    }
                }
            }

            // Take OS processes again, some of them were just stopped for upgrade
            serviceProcesses = await commandContext.Do(GetRunningProcesses);

            commandContext.Logger.LogDebug($"SynchronizeRunningProcesses serviceProcesses owned {Metapsi.Serialize.ToJson(serviceProcesses)}");
            commandContext.Logger.LogDebug($"SynchronizeRunningProcesses {localKnownConfiguration.Count()}");

            foreach (var serviceConfiguration in localKnownConfiguration)
            {
                commandContext.Logger.LogDebug($"SynchronizeRunningProcesses {Metapsi.Serialize.ToJson(serviceConfiguration)}");

                if (!SameVersionIsInstalled(state.ServicesBasePath, serviceConfiguration))
                {
                    string serviceFullPath = System.IO.Path.Combine(state.ServicesBasePath, serviceConfiguration.ServiceName);
                    syncResult.AddInfo($"Installing binaries in {serviceFullPath}...");
                    commandContext.Logger.LogDebug($"PrepareServiceBinaries {serviceConfiguration.ServiceName}");
                    await PrepareServiceBinaries(commandContext, state, serviceConfiguration);
                    syncResult.AddInfo($"Done");
                    await CreateMdsParametersFile(state, serviceConfiguration);
                    syncResult.AddInfo($"Creating infrastructure files for service {serviceConfiguration.ServiceName}...");
                    Metapsi.Mds.CreateServiceCommandDbFile(Metapsi.Mds.GetServiceCommandDbFile(state.BaseDataFolder, serviceConfiguration.ServiceName));
                    Metapsi.Mds.ClearServiceCommands(Metapsi.Mds.GetServiceCommandDbFile(state.BaseDataFolder, serviceConfiguration.ServiceName));
                    syncResult.AddInfo($"Done");
                }

                if (!configurationDiff.IdenticalServices.Any(x => x.ServiceName == serviceConfiguration.ServiceName))
                {
                    // For sure create if service is added
                    var createParametersFile = configurationDiff.AddedServices.Any(x=>x.ServiceName == serviceConfiguration.ServiceName);
                    if (!createParametersFile)
                    {
                        // Compare with installed parameters, not previous configuration parameters
                        // to avoid stale parameters in case some previous deployment failed
                        var installedParameters = GetInstalledParameters(state, serviceConfiguration);
                        var installedParametersData = installedParameters.Select(x => new ServiceParameterData()
                        {
                            ParameterName = x.Key,
                            DeployedValue = x.Value
                        });

                        var newConfigurationParameters = serviceConfiguration.ServiceConfigurationSnapshotParameters.Select(x => x.GetServiceParameterData());

                        var parametersDiff = Diff.CollectionsByKey(installedParametersData, newConfigurationParameters, x => x.ParameterName);
                        if (parametersDiff.Any())
                        {
                            // Create if parameters are actually different
                            createParametersFile = true;
                        }
                    }

                    if (createParametersFile)
                    {
                        syncResult.AddInfo($"Creating parameters file {GetServiceParametersPath(state.ServicesBasePath, serviceConfiguration)}");
                        // Overwrite parameters file anyway, is simpler & probably faster than checking if it is changed
                        await CreateAlgorithmParametersFile(state, serviceConfiguration);
                        commandContext.PostEvent(new Event.ServiceSetupComplete()
                        {
                            ServiceSnapshot = serviceConfiguration
                        });

                        syncResult.AddInfo($"Service {serviceConfiguration.ServiceName} configured");
                    }
                }

                // If not running (was previously stopped if configuration is changed)
                if (!serviceProcesses.Any(x => x.ServiceName == serviceConfiguration.ServiceName))
                {
                    if (serviceConfiguration.Enabled)
                    {
                        processesSynchronized.Started.Add(serviceConfiguration.ServiceName);
                        syncResult.AddInfo($"Starting service {serviceConfiguration.ServiceName} ...");
                        await StartService(commandContext, state, serviceConfiguration.ServiceName);
                        syncResult.AddInfo($"Service {serviceConfiguration.ServiceName} started");
                    }
                }
            }

            commandContext.PostEvent(processesSynchronized);
        }
    }
}
