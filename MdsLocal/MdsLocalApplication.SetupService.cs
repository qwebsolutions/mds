using Metapsi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdsCommon;

namespace MdsLocal
{
    public static partial class MdsLocalApplication
    {
        /// <summary>
        ///  True if parameters changed
        /// </summary>
        /// <param name="state"></param>
        /// <param name="configuration"></param>
        /// <param name="localService"></param>
        /// <returns></returns>
        public static async Task<bool> CreateAlgorithmParametersFile(
            State state,
            MdsCommon.ServiceConfigurationSnapshot serviceConfiguration)
        {
            string parametersFullPath = GetServiceParametersPath(state.ServicesBasePath, serviceConfiguration);
            string parameters = GetAlgorithmParametersJson(serviceConfiguration);

            var createFile = false;

            if (!System.IO.File.Exists(parametersFullPath))
            {
                createFile = true;
            }
            else
            {
                var previousParameters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(System.IO.File.ReadAllText(parametersFullPath));

                var configParameters = serviceConfiguration.ServiceConfigurationSnapshotParameters;

                if (previousParameters.Count() != configParameters.Count())
                {
                    createFile = true;
                }
                else
                {
                    foreach (var p in configParameters)
                    {
                        if (!previousParameters.ContainsKey(p.ParameterName))
                        {
                            createFile = true;
                            break;
                        }
                        else
                        if (previousParameters[p.ParameterName] != p.DeployedValue)
                        {
                            createFile = true;
                            break;
                        }
                    }
                }
            }

            if (createFile)
            {
                System.IO.File.WriteAllText(parametersFullPath, parameters);
            }

            return createFile;

        }

        public static async Task CreateMdsParametersFile(
            State state,
            MdsCommon.ServiceConfigurationSnapshot localService)
        {
            Metapsi.Mds.InstallationData serviceVersion = new Metapsi.Mds.InstallationData()
            {
                ConfigurationId = localService.Id.ToString(),
                NodeName = localService.NodeName,
                ProjectName = localService.ProjectName,
                ServiceName = localService.ServiceName,
                Version = localService.ProjectVersionTag,
                InfrastructureName = state.InfrastructureConfiguration.InfrastructureName,
                InstalledOn = DateTime.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
                DataFolder = System.IO.Path.Combine(state.BaseDataFolder, localService.ServiceName)
            };

            string json = System.Text.Json.JsonSerializer.Serialize(serviceVersion, options: new System.Text.Json.JsonSerializerOptions()
            {
                WriteIndented = true
            });

            string supervisorParametersPath = GetMdsParametersPath(state.ServicesBasePath, localService.ServiceName);
            System.IO.File.WriteAllText(supervisorParametersPath, json);
        }

        public static Metapsi.Mds.InstallationData GetServiceVersionData(
            string servicesBasePath,
            string serviceName)
        {
            return Metapsi.Serialize.FromJson<Metapsi.Mds.InstallationData>(System.IO.File.ReadAllText(GetMdsParametersPath(servicesBasePath, serviceName)));
        }

        public static async Task<bool> PrepareServiceBinaries(
            CommandContext commandContext,
            State state,
            MdsCommon.ServiceConfigurationSnapshot localService)
        {
            string serviceFolderPath = GetServiceFolderPath(state.ServicesBasePath, localService.ServiceName);
            System.IO.Directory.CreateDirectory(serviceFolderPath); // Works even it it already exists

            string serviceExePath = GetServiceExePath(state.ServicesBasePath, state.NodeName, localService);

            if (System.IO.File.Exists(serviceExePath))
            {
                ServiceAlreadyAtVersion serviceAlreadyAtVersion = new ServiceAlreadyAtVersion();
                serviceAlreadyAtVersion.ServiceConfigurationSnapshot.Add(localService);
                serviceAlreadyAtVersion.ServiceBinary.Add(new ServiceBinary()
                {
                    ServiceFullExePath = serviceExePath
                });

                commandContext.PostEvent(serviceAlreadyAtVersion);
                return true;
            }

            MdsLocal.ProjectBinary projectBinaries = await commandContext.Do(
                GetProjectBinaries, 
                localService.ProjectName, 
                localService.ProjectVersionTag,
                GetServiceFolderPath(state.ServicesBasePath, localService));

            System.IO.File.Move(projectBinaries.FullExePath, serviceExePath);
            string exeConfigPath = projectBinaries.FullExePath + ".config";
            if (System.IO.File.Exists(exeConfigPath))
            {
                System.IO.File.Move(exeConfigPath, serviceExePath + ".config");
            }

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                int executePermission = S_IRUSR | S_IXUSR | S_IWUSR;
                int result = chmod(serviceExePath, executePermission);
            }

            commandContext.PostEvent(new ServiceBinaryCreated()
            {
                FullExePath = serviceExePath,
                ServiceName = localService.ServiceName
            });
            return true;
        }

        public static Dictionary<string, string> GetInstalledParameters(
            State state,
            MdsCommon.ServiceConfigurationSnapshot serviceConfiguration)
        {
            string parametersFullPath = GetServiceParametersPath(state.ServicesBasePath, serviceConfiguration);

            if (!System.IO.File.Exists(parametersFullPath))
                return new Dictionary<string, string>();

            var previousParameters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(System.IO.File.ReadAllText(parametersFullPath));
            return previousParameters;
        }

        private static string GetAlgorithmParametersJson(MdsCommon.ServiceConfigurationSnapshot serviceConfiguration)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            foreach (var parameter in serviceConfiguration.ServiceConfigurationSnapshotParameters)
            {
                parameters[parameter.ParameterName] = parameter.DeployedValue;
            }

            string jsonParameters = System.Text.Json.JsonSerializer.Serialize(parameters, options: new System.Text.Json.JsonSerializerOptions()
            {
                WriteIndented = true
            });
            return jsonParameters;
        }


        /// <summary>
        /// BasePath/ServiceName/ProjectName/VersionTag/_Mds.ServiceName[.exe]
        /// </summary>
        /// <param name="baseServicesFolder"></param>
        /// <param name="localService"></param>
        /// <returns></returns>
        public static string GetServiceExePath(string baseServicesFolder, string nodeName, MdsCommon.ServiceConfigurationSnapshot localService)
        {
            string fullExePath = System.IO.Path.Combine(baseServicesFolder, localService.ServiceName, GetServiceExeName(nodeName, localService.ServiceName));
            return fullExePath;
        }

        public static string GetServiceParametersPath(string baseServicesFolder, MdsCommon.ServiceConfigurationSnapshot localService)
        {
            string parametersPath = System.IO.Path.Combine(baseServicesFolder, localService.ServiceName, "parameters.json");
            return parametersPath;
        }

        public static string GetMdsParametersPath(string baseServicesFolder, string serviceName)
        {
            string supervisorConfigPath = System.IO.Path.Combine(baseServicesFolder, serviceName, "mds.json");
            return supervisorConfigPath;
        }

        public static string GetSupervisorParametersPath(string baseServicesFolder, MdsCommon.ServiceConfigurationSnapshot localService)
        {
            string supervisorConfigPath = System.IO.Path.Combine(baseServicesFolder, localService.ServiceName, "supervisor.json");
            return supervisorConfigPath;
        }


        //public static string GetMesParametersPath(string baseServicesFolder, MdsCommon.LocalService localService)
        //{
        //    string fullExePath = System.IO.Path.Combine(baseServicesFolder, localService.ServiceName, localService.ProjectName, localService.VersionTag, "mes.json");
        //    return fullExePath;
        //}


        public static string GetServiceFolderPath(string baseServicesFolder, MdsCommon.ServiceConfigurationSnapshot localService)
        {
            string serviceFolderPath = System.IO.Path.Combine(baseServicesFolder, localService.ServiceName);
            return serviceFolderPath;
        }

        public class MesParameters
        {
            public string InstanceId { get; set; }
            public string RedisListenerUrl { get; set; }
            public string RedisListenerChannel { get; set; }
            public string ConfirmationListenerChannel { get; set; }
        }

        /// <summary>
        /// BasePath/ServiceName
        /// </summary>
        /// <param name="servicesBasePath"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private static string GetServiceFolderPath(string servicesBasePath, string serviceName)
        {
            string folderPath = System.IO.Path.Combine(servicesBasePath, serviceName);
            return folderPath;
        }

        public enum Os
        {
            Windows,
            Linux
        }

        private static Os GetOs()
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                return Os.Windows;
            return Os.Linux;
        }

        public static string GetServiceExeName(string nodeName, string serviceName)
        {
            bool hasExeExtension = GetOs() == Os.Windows;
            string extensionPlaceholder = string.Empty;
            if (hasExeExtension)
                extensionPlaceholder = ".exe";

            string serviceExeName = $"{ExePrefix(nodeName)}{serviceName}{extensionPlaceholder}";
            return serviceExeName;
        }
    }
}
