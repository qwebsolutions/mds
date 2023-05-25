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
            Event.ProcessesSynchronized processesSynchronized = new Event.ProcessesSynchronized();

            var localKnownConfiguration = await commandContext.Do(GetLocalKnownConfiguration);
            var serviceProcesses = await commandContext.Do(GetRunningProcesses);

            // Stop services that are running, but should not be, because they are removed or have different configuration

            foreach (var serviceProcess in serviceProcesses)
            {
                string configurationId = string.Empty;

                var serviceConfiguration = localKnownConfiguration.SingleOrDefault(x => x.ServiceName == serviceProcess.ServiceName);

                // Completely removed, stop
                if (serviceConfiguration == null)
                {
                    processesSynchronized.Stopped.Add(serviceProcess.ServiceName);
                    await commandContext.Do(StopProcess, serviceProcess);
                    commandContext.Logger.LogDebug($"SynchronizeRunningProcesses: {serviceProcess.ServiceName} stopped");
                }
                else
                {
                    var serviceVersion = GetServiceVersionData(state.ServicesBasePath, serviceConfiguration.ServiceName);
                    if (serviceVersion.ConfigurationId != serviceConfiguration.Id.ToString())
                    {
                        // Configuration changed, should be reinstalled
                        processesSynchronized.Stopped.Add(serviceProcess.ServiceName);
                        await commandContext.Do(StopProcess, serviceProcess);
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

                    // Deleted, so remove directory
                    if (serviceConfiguration == null)
                    {
                        DeleteServiceFolder(state.ServicesBasePath, serviceName);
                    }
                    else
                    {
                        // If service folder is not valid (partially removed or misconfigured by hand)
                        if (!System.IO.File.Exists(GetMdsParametersPath(state.ServicesBasePath, serviceName)))
                        {
                            DeleteServiceFolder(state.ServicesBasePath, serviceName);
                        }
                        else
                        {
                            var serviceVersion = GetServiceVersionData(state.ServicesBasePath, serviceName);

                            // Configuration changed, so remove directory, will be recreated
                            if (serviceVersion.ConfigurationId != serviceConfiguration.Id.ToString())
                            {
                                DeleteServiceFolder(state.ServicesBasePath, serviceName);
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
                    commandContext.Logger.LogDebug($"PrepareServiceBinaries {serviceConfiguration.ServiceName}");
                    await PrepareServiceBinaries(commandContext, state, serviceConfiguration);
                    await CreateMdsParametersFile(state, serviceConfiguration);
                    Metapsi.Mds.CreateServiceCommandDbFile(Metapsi.Mds.GetServiceCommandDbFile(state.BaseDataFolder, serviceConfiguration.ServiceName));
                    Metapsi.Mds.ClearServiceCommands(Metapsi.Mds.GetServiceCommandDbFile(state.BaseDataFolder, serviceConfiguration.ServiceName));
                }

                // Overwrite parameters file anyway, is simpler & probably faster than checking if it is changed
                await CreateAlgorithmParametersFile(state, serviceConfiguration);
                commandContext.PostEvent(new Event.ServiceSetupComplete()
                {
                    ServiceSnapshot= serviceConfiguration
                });

                // If not running (was previously stopped if configuration is changed)
                if (!serviceProcesses.Any(x => x.ServiceName == serviceConfiguration.ServiceName))
                {
                    processesSynchronized.Started.Add(serviceConfiguration.ServiceName);
                    await StartService(commandContext, state, serviceConfiguration.ServiceName);
                }
            }

            commandContext.PostEvent(processesSynchronized);
        }
    }
}
