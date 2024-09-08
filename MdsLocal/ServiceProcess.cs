using MdsCommon;
using Metapsi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static MdsLocal.ApiBinariesRetriever;

namespace MdsLocal;

public class ServiceProcess
{
    public System.String ServiceName { get; set; } = System.String.Empty;
    public System.Int32 Pid { get; set; }
    public System.DateTime StartTimestampUtc { get; set; }
    public System.String FullExePath { get; set; } = System.String.Empty;
    public System.Int32 UsedRamMB { get; set; }
}

public enum Os
{
    Windows,
    Linux
}

public class TemporaryBinaries
{
    public string ProjectName { get; set; }
    public string ProjectVersion { get; set; }
    public string TemporaryZipPath { get; set; }
}

public static class ServiceProcessExtensions
{
    public static HttpClient httpClient = new HttpClient();

    record ProjectVersion(string projectName, string versionName);

    static TaskQueue dbQueue = new TaskQueue();

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

    public static async Task SyncServices(
        this CommandContext commandContext,
        string nodeName,
        DbQueue dbQueue,
        string servicesBasePath,
        string servicesDataPath,
        string buildTarget,
        string binariesApiUrl,
        string infrastructureName,
        MdsLocalApplication.PendingStopTracker pendingStopTracker,
        Guid deploymentId)
    {
        SyncResultBuilder syncResultBuilder = new SyncResultBuilder();
        await syncResultBuilder.Enqueue(async (state) => state.Trigger = "Deployment");

        var installedServices = await GetInstalledServices(servicesBasePath);
        var currentConfiguration = await dbQueue.Enqueue(LocalDb.LoadKnownConfiguration);

        var union = installedServices.Union(currentConfiguration.Select(x => x.ServiceName)).Distinct().ToList();

        HashSet<ProjectVersion> neededVersions = new HashSet<ProjectVersion>();

        foreach (var snapshot in currentConfiguration)
        {
            var installData = GetServiceInstallData(servicesBasePath, snapshot.ServiceName);
            if (installData == null)
            {
                neededVersions.Add(new ProjectVersion(snapshot.ProjectName, snapshot.ProjectVersionTag));
            }
            else
            {
                if (installData.ProjectName != snapshot.ProjectName || installData.Version != snapshot.ProjectVersionTag)
                {
                    neededVersions.Add(new ProjectVersion(snapshot.ProjectName, snapshot.ProjectVersionTag));
                }
            }
        }

        List<TemporaryBinaries> temporaryBinaries = new List<TemporaryBinaries>();

        foreach (var neededBinaries in neededVersions)
        {
            var binariesContent = await GetBinariesZipContent(httpClient, neededBinaries.projectName, neededBinaries.versionName, buildTarget, binariesApiUrl);

            var tempFile = System.IO.Path.GetTempFileName();
            await syncResultBuilder.Enqueue(async (r) => r.AddInfo($"Downloading {neededBinaries.projectName} {neededBinaries.versionName}"));
            using (var fileStream = System.IO.File.Create(tempFile))
            {
                await fileStream.WriteAsync(binariesContent);
                await fileStream.FlushAsync();
            }

            temporaryBinaries.Add(new TemporaryBinaries()
            {
                ProjectName = neededBinaries.projectName,
                ProjectVersion = neededBinaries.versionName,
                TemporaryZipPath = tempFile
            });
        }

        List<Task> serviceProcessSyncTasks = new List<Task>();

        foreach (var serviceName in union)
        {
            var snapshot = currentConfiguration.SingleOrDefault(x => x.ServiceName == serviceName);
            serviceProcessSyncTasks.Add(
                PatchService(
                    commandContext,
                    serviceName,
                    snapshot,
                    nodeName,
                    servicesBasePath,
                    servicesDataPath,
                    temporaryBinaries,
                    infrastructureName,
                    pendingStopTracker,
                    deploymentId,
                    syncResultBuilder));
        }

        await Task.WhenAll(serviceProcessSyncTasks);

        foreach (var tempBinaries in temporaryBinaries)
        {
            System.IO.File.Delete(tempBinaries.TemporaryZipPath);
        }

        await syncResultBuilder.Enqueue(async (r) =>
        {
            if (!r.Log.Any())
            {
                r.ResultCode = SyncStatusCodes.UpToDate;
            }
            else
            {
                if (r.Log.Any(x => x.Type == SyncResultLogType.Error))
                {
                    r.ResultCode = SyncStatusCodes.Failed;
                }
                else
                {
                    r.ResultCode = SyncStatusCodes.Changed;
                }
            }
        });

        await dbQueue.Enqueue(async (dbPath) => await MdsLocal.LocalDb.RegisterSyncResult(dbPath, await syncResultBuilder.GetState()));
    }

    // null snapshot = uninstall
    public static async Task PatchService(
        this CommandContext commandContext,
        string serviceName,
        ServiceConfigurationSnapshot snapshot,
        string nodeName,
        string servicesBasePath,
        string servicesBaseDataPath,
        List<TemporaryBinaries> temporaryBinaries,
        string infrastructureName,
        MdsLocalApplication.PendingStopTracker pendingStopTracker,
        Guid deploymentId,
        SyncResultBuilder syncResultBuilder)
    {
        try
        {
            if (snapshot == null)
            {
                // Service is not valid anymore, it needs to be uninstalled
                await StopServiceProcess(commandContext, nodeName, serviceName, servicesBaseDataPath, pendingStopTracker, deploymentId);
                await syncResultBuilder.Enqueue(async (b) => b.AddInfo($"Service {serviceName} stopped"));
                await UninstallService(commandContext, servicesBasePath, serviceName, nodeName, deploymentId);
                await syncResultBuilder.Enqueue(async (b) => b.AddInfo($"Service {serviceName} uninstalled"));
            }
            else
            {
                var installedParameters = GetServiceInstallParameters(servicesBasePath, serviceName);
                var installData = GetServiceInstallData(servicesBasePath, serviceName);

                var serviceInstalled = installedParameters != null && installData != null;

                if (!serviceInstalled)
                {
                    // Service was not previously installed, install now
                    await InstallServiceBinaries(commandContext, serviceName, snapshot.ProjectName, snapshot.ProjectVersionTag, nodeName, servicesBasePath, temporaryBinaries, deploymentId);
                    await syncResultBuilder.Enqueue(async (b) => b.AddInfo($"Service {serviceName} installed ({snapshot.ProjectName} {snapshot.ProjectVersionTag})"));
                    await CreateServiceParametersFile(commandContext, snapshot, servicesBasePath, deploymentId);
                    await CreateServiceInstallFile(snapshot, infrastructureName, servicesBaseDataPath, servicesBasePath);

                    if (snapshot.Enabled)
                    {
                        await StartServiceProcess(commandContext, serviceName, nodeName, servicesBasePath, deploymentId);
                        await syncResultBuilder.Enqueue(async (b) => b.AddInfo($"Service {serviceName} started"));
                    }
                }
                else
                {
                    // Service was previously installed, binaries changed => complete reinstall
                    if (installData.ProjectName != snapshot.ProjectName || installData.Version != snapshot.ProjectVersionTag)
                    {
                        // Project or version is different, update
                        await StopServiceProcess(commandContext, nodeName, serviceName, servicesBasePath, pendingStopTracker, deploymentId);
                        await syncResultBuilder.Enqueue(async (b) => b.AddInfo($"Service {serviceName} stopped"));
                        await UninstallService(commandContext, servicesBasePath, serviceName, nodeName, deploymentId);
                        await syncResultBuilder.Enqueue(async (b) => b.AddInfo($"Service {serviceName} uninstalled ({installData.ProjectName} {installData.Version})"));
                        await InstallServiceBinaries(commandContext, serviceName, snapshot.ProjectName, snapshot.ProjectVersionTag, nodeName, servicesBasePath, temporaryBinaries, deploymentId);
                        await syncResultBuilder.Enqueue(async (b) => b.AddInfo($"Service {serviceName} installed ({snapshot.ProjectName} {snapshot.ProjectVersionTag})"));
                        await CreateServiceInstallFile(snapshot, infrastructureName, servicesBaseDataPath, servicesBasePath);
                        await CreateServiceParametersFile(commandContext, snapshot, servicesBasePath, deploymentId);
                    }
                    else
                    {
                        // Same binaries, just different parameters
                        if (!ServiceParametersAreIdentical(snapshot, installedParameters))
                        {
                            await StopServiceProcess(commandContext, nodeName, serviceName, servicesBasePath, pendingStopTracker, deploymentId);
                            await syncResultBuilder.Enqueue(async (b) => b.AddInfo($"Service {serviceName} stopped"));
                            await CreateServiceInstallFile(snapshot, infrastructureName, servicesBaseDataPath, servicesBasePath);
                            await CreateServiceParametersFile(commandContext, snapshot, servicesBasePath, deploymentId);
                        }
                    }

                    if (snapshot.Enabled)
                    {
                        // If there was no change, we still attempt to force the service to start if it's not running
                        var alreadyRunning = GetServiceProcess(nodeName, serviceName);
                        if (alreadyRunning == null)
                        {
                            await StartServiceProcess(commandContext, serviceName, nodeName, servicesBasePath, deploymentId);
                            await syncResultBuilder.Enqueue(async (b) => b.AddInfo($"Service {serviceName} started"));
                        }
                    }
                    else
                    {
                        await StopServiceProcess(commandContext, nodeName, serviceName, servicesBaseDataPath, pendingStopTracker, deploymentId);
                        await syncResultBuilder.Enqueue(async (b) => b.AddInfo($"Service {serviceName} stopped"));
                    }
                }
            }

            commandContext.NotifyGlobal(new DeploymentEvent.ServiceSynchronized()
            {
                DeploymentId = deploymentId,
                NodeName = nodeName,
                ServiceName = serviceName
            });
        }
        catch (Exception ex)
        {
            commandContext.NotifyGlobal(new DeploymentEvent.ServiceSynchronized()
            {
                DeploymentId = deploymentId,
                NodeName = nodeName,
                ServiceName = serviceName,
                Error = ex.Message
            });

            commandContext.NotifyGlobal(new InfrastructureEvent()
            {
                Criticality = InfrastructureEventCriticality.Fatal,
                ShortDescription = "Deployment error",
                Source = nodeName,
                Type = InfrastructureEventType.ExceptionProcessing,
                FullDescription = $"Service {serviceName} could not be synchronized: {ex.Message}",
            });

            await syncResultBuilder.Enqueue(async (b) => b.AddError($"Service {serviceName} error: {ex.Message}"));
        }
    }

    public static async Task InstallServiceBinaries(
        this CommandContext commandContext,
        string serviceName,
        string projectName,
        string versionTag,
        string nodeName,
        string servicesBasePath,
        List<TemporaryBinaries> temporaryZipBinaries,
        Guid deploymentId)
    {
        string serviceFolderPath = System.IO.Path.Combine(servicesBasePath, serviceName);
        System.IO.Directory.CreateDirectory(serviceFolderPath); // Works even it it already exists

        var binaries = temporaryZipBinaries.SingleOrDefault(x => x.ProjectName == projectName && x.ProjectVersion == versionTag);
        if (binaries == null)
        {
            throw new System.Exception($"Binaries not found: {projectName} {versionTag}");
        }

        System.IO.Compression.ZipFile.ExtractToDirectory(binaries.TemporaryZipPath, serviceFolderPath, false);
        string originalProjectExePath = System.IO.Path.Combine(serviceFolderPath, projectName);
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            originalProjectExePath += ".exe";
        }

        string serviceExePath = System.IO.Path.Combine(servicesBasePath, serviceName, GetServiceExeName(nodeName, serviceName));

        System.IO.File.Move(originalProjectExePath, serviceExePath);
        string exeConfigPath = originalProjectExePath + ".config";
        if (System.IO.File.Exists(exeConfigPath))
        {
            System.IO.File.Move(exeConfigPath, serviceExePath + ".config");
        }

        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            int executePermission = S_IRUSR | S_IXUSR | S_IWUSR;
            int result = chmod(serviceExePath, executePermission);
        }

        commandContext.NotifyGlobal(new DeploymentEvent.ServiceInstall()
        {
            DeploymentId = deploymentId,
            NodeName = nodeName,
            ServiceName = serviceName,
        });
    }

    public static async Task StopServiceProcess(
        CommandContext commandContext,
        string nodeName,
        string serviceName,
        string servicesBaseDataPath,
        MdsLocalApplication.PendingStopTracker pendingStopTracker,
        Guid deploymentId)
    {

        var process = GetServiceProcess(nodeName, serviceName);

        if (process != null)
        {
            await pendingStopTracker.TaskQueue.Enqueue(async () => pendingStopTracker.PendingStops.Add(new MdsLocalApplication.PendingStopTracker.PendingStop()
            {
                DeploymentId = deploymentId,
                Pid = process.Id
            }));
            await Mds.WriteCommand(Mds.GetServiceCommandDbFile(servicesBaseDataPath, serviceName), new Mds.Shutdown() { });

            var exited = process.WaitForExit(15000);
            if (!exited)
            {
                process.Kill();
                exited = process.WaitForExit(5000);
            }

            commandContext.NotifyGlobal(new DeploymentEvent.ServiceStop()
            {
                DeploymentId = deploymentId,
                NodeName = nodeName,
                ServiceName = serviceName
            });
        }
    }

    public static async Task StartServiceProcess(
        CommandContext commandContext,
        string serviceName,
        string nodeName,
        string servicesBasePath,
        Guid deploymentId)
    {
        var alreadyRunning = GetServiceProcess(nodeName, serviceName);

        if (alreadyRunning != null)
        {
            Console.WriteLine($"Service {serviceName} already running on {nodeName}");
            return;
        }

        string serviceExePath = System.IO.Path.Combine(servicesBasePath, serviceName, GetServiceExeName(nodeName, serviceName));
        string serviceDirectory = System.IO.Path.Combine(servicesBasePath, serviceName);

        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo()
        {
            CreateNoWindow = true,
            FileName = serviceExePath,
            WorkingDirectory = serviceDirectory
        };

        System.Diagnostics.Process process = new System.Diagnostics.Process()
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        process.Start();
        AttachExitHandler(process, nodeName, servicesBasePath, sp =>
        {
            commandContext.PostEvent(new ProcessExited()
            {
                ExitCode = process.ExitCode,
                ServiceName = serviceName,
                FullExePath = serviceExePath,
                Pid = process.Id
            });
        });

        commandContext.NotifyGlobal(new DeploymentEvent.ServiceStart()
        {
            DeploymentId = deploymentId,
            NodeName = nodeName,
            ServiceName = serviceName
        });
    }

    public static void AttachExitHandler(
        System.Diagnostics.Process process,
        string nodeName,
        string servicesBasePath,
        Action<ServiceProcess> onExit)
    {
        process.EnableRaisingEvents = true;

        ServiceProcess serviceProcess = new ServiceProcess()
        {
            Pid = process.Id,
        };
        int processId = process.Id;
        string mainModulePath = string.Empty;

        while (true)
        {
            try
            {
                // Works before or after catch just as well
                if (process.HasExited)
                {
                    break;
                }

                if (string.IsNullOrEmpty(mainModulePath))
                {
                    // Seems to sometimes crash while accessing .MainModule;
                    mainModulePath = process.MainModule.FileName;
                }
                if (string.IsNullOrEmpty(serviceProcess.FullExePath))
                {
                    serviceProcess.FullExePath = process.ProcessName;
                    if (!System.IO.Path.IsPathFullyQualified(serviceProcess.FullExePath))
                    {
                        string servicePath = GuessServiceName(nodeName, process.ProcessName);
                        serviceProcess.FullExePath = System.IO.Path.Combine(servicesBasePath, servicePath, serviceProcess.FullExePath);
                    }
                }

                Console.WriteLine($"AttachExitHandler process module: {mainModulePath}");
                break;
            }
            catch (Exception ex)
            {
                Task.Delay(1000).Wait();
            }
        }

        process.Exited += (object sender, EventArgs e) =>
        {
            process.WaitForExit();
            if (onExit != null)
            {
                onExit(serviceProcess);
            }
        };
    }

    // Service must be stopped beforehand
    public static async Task UninstallService(CommandContext commandContext, string servicesBasePath, string serviceName, string nodeName, Guid deploymentId)
    {
        // On first installation directory is not even created
        if (System.IO.Directory.Exists(servicesBasePath))
        {
            string serviceFullPath = System.IO.Path.Combine(servicesBasePath, serviceName);

            if (System.IO.Directory.Exists(serviceFullPath))
            {
                System.IO.Directory.Delete(serviceFullPath, true);
            }
        }

        commandContext.NotifyGlobal(new DeploymentEvent.ServiceUninstall()
        {
            DeploymentId = deploymentId,
            ServiceName = serviceName,
            NodeName = nodeName
        });
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


    public static async Task<List<ServiceProcess>> GetNodeServiceProcesses(string nodeName, int maxRetriesPerProcess = 10)
    {
        List<ServiceProcess> processes = new();

        foreach (var osProcess in IdentifyOwnedProcesses(nodeName))
        {
            var serviceProcessData = await GetServiceProcessData(osProcess, nodeName, maxRetriesPerProcess);
            if (serviceProcessData != null)
            {
                processes.Add(serviceProcessData);
            }
        }

        return processes;
    }

    public static async Task<List<string>> GetInstalledServices(string basePath)
    {
        if (!System.IO.Directory.Exists(basePath))
        {
            return new List<string>();
        }

        var enumerated = System.IO.Directory.EnumerateDirectories(basePath);

        return enumerated.Select(x => System.IO.Path.GetFileName(x)).ToList();
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

    public static System.Diagnostics.Process GetServiceProcess(string nodeName, string serviceName)
    {
        string processName = GetProcessName(GetServiceExeName(nodeName, serviceName));
        System.Diagnostics.Process[] matchingProcesses = System.Diagnostics.Process.GetProcessesByName(processName);
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


    public static async Task<ServiceProcess> GetServiceProcessData(
        System.Diagnostics.Process process,
        string nodeName,
        int maxRetries = 10)
    {
        int retryCount = 0;
        while (true)
        {
            try
            {
                string exePath = process.MainModule.FileName;

                return new ServiceProcess()
                {
                    FullExePath = exePath,
                    ServiceName = GuessServiceName(nodeName, exePath),
                    Pid = process.Id,
                    StartTimestampUtc = process.StartTime.ToUniversalTime(),
                    UsedRamMB = (int)(process.WorkingSet64 / (1024 * 1024))
                };
            }
            catch (Exception ex)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    Console.Error.WriteLine($"Cannot get exe path of {process.Id}");
                    break;
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
        }

        return null;
    }

    public static string ExePrefix(string nodeName)
    {
        return $"_{nodeName}.";
    }

    public static Os GetOs()
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

    public static Dictionary<string, string> GetServiceInstallParameters(
        string servicesBasePath,
        string serviceName)
    {
        string parametersFullPath = GetServiceParametersPath(servicesBasePath, serviceName);

        if (!System.IO.File.Exists(parametersFullPath))
            return new Dictionary<string, string>();

        var installedParameters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(System.IO.File.ReadAllText(parametersFullPath));
        return installedParameters;
    }

    public static Metapsi.Mds.InstallationData GetServiceInstallData(
        string servicesBasePath,
        string serviceName)
    {
        var mdsInstallFileFullPath = GetMdsInstallFilePath(servicesBasePath, serviceName);
        if (!System.IO.File.Exists(mdsInstallFileFullPath))
        {
            return null;
        }

        return Metapsi.Serialize.FromJson<Metapsi.Mds.InstallationData>(System.IO.File.ReadAllText(mdsInstallFileFullPath));
    }

    public static string GetMdsInstallFilePath(string baseServicesFolder, string serviceName)
    {
        string supervisorConfigPath = System.IO.Path.Combine(baseServicesFolder, serviceName, "mds.json");
        return supervisorConfigPath;
    }

    public static string GetServiceParametersPath(string baseServicesFolder, string serviceName)
    {
        string parametersPath = System.IO.Path.Combine(baseServicesFolder, serviceName, "parameters.json");
        return parametersPath;
    }

    public static string GuessServiceName(string nodeName, string fullExePath)
    {
        string processName = System.IO.Path.GetFileName(fullExePath);
        string serviceName = processName.Replace(ExePrefix(nodeName), string.Empty).Replace(".exe", string.Empty);
        return serviceName;
    }


    public static async Task<byte[]> GetBinariesZipContent(
        HttpClient httpClient,
        string projectName,
        string projectVersion,
        string buildTarget,
        string binariesApiUrl)
    {
        string relativeUrl = $"GetBinaries/{buildTarget}/{projectName}/{projectVersion}";

        var fullUri = new System.Uri(new System.Uri(binariesApiUrl), relativeUrl);

        var response = await httpClient.GetAsync(fullUri, System.Net.Http.HttpCompletionOption.ResponseHeadersRead);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            var alternativeTarget = string.Empty;
            if (buildTarget == "win10-x64")
            {
                alternativeTarget = "win-x64";
            }

            if (buildTarget == "win-x64")
            {
                alternativeTarget = "win10-x64";
            }

            var alternativeUri = new System.Uri(
                new System.Uri(binariesApiUrl),
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

        return await response.Content.ReadAsByteArrayAsync();
    }

    public static async Task CreateServiceParametersFile(
        CommandContext commandContext,
        MdsCommon.ServiceConfigurationSnapshot serviceConfiguration,
        string servicesBasePath,
        Guid deploymentId)
    {
        string parametersFullPath = GetServiceParametersPath(servicesBasePath, serviceConfiguration.ServiceName);
        await System.IO.File.WriteAllTextAsync(parametersFullPath, Metapsi.Serialize.ToJson(serviceConfiguration.GetParametersDictionary()));
        commandContext.NotifyGlobal(new DeploymentEvent.ParametersSet()
        {
            DeploymentId = deploymentId,
            NodeName = serviceConfiguration.NodeName,
            ServiceName = serviceConfiguration.ServiceName,
        });
    }

    public static bool ServiceParametersAreIdentical(ServiceConfigurationSnapshot snapshot, Dictionary<string, string> installParameters)
    {
        if (snapshot.ServiceConfigurationSnapshotParameters.Count() != installParameters.Count())
            return false;

        var snapshotParameters = snapshot.GetParametersDictionary();

        var snapshotParamsConcat = string.Join("|", snapshotParameters.OrderBy(x => x.Key).Select(x => $"{x.Key}|{x.Value}"));
        var installParamsConcat = string.Join("|", installParameters.OrderBy(x => x.Key).Select(x => $"{x.Key}|{x.Value}"));
        return snapshotParamsConcat == installParamsConcat;
    }

    public static Dictionary<string, string> GetParametersDictionary(this ServiceConfigurationSnapshot serviceConfigurationSnapshot)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();

        foreach (var serviceConfigurationSnapshotParameter in serviceConfigurationSnapshot.ServiceConfigurationSnapshotParameters)
        {
            parameters.Add(serviceConfigurationSnapshotParameter.ParameterName, serviceConfigurationSnapshotParameter.DeployedValue);
        }

        return parameters;
    }

    public static async Task CreateServiceInstallFile(
        MdsCommon.ServiceConfigurationSnapshot serviceSnapshot,
        string infrastructureName,
        string servicesBaseDataFolder,
        string servicesBaseFolder)
    {
        Metapsi.Mds.InstallationData serviceVersion = new Metapsi.Mds.InstallationData()
        {
            ConfigurationId = serviceSnapshot.Id.ToString(),
            NodeName = serviceSnapshot.NodeName,
            ProjectName = serviceSnapshot.ProjectName,
            ServiceName = serviceSnapshot.ServiceName,
            Version = serviceSnapshot.ProjectVersionTag,
            InfrastructureName = infrastructureName,
            InstalledOn = DateTime.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
            DataFolder = System.IO.Path.Combine(servicesBaseDataFolder, serviceSnapshot.ServiceName)
        };

        string json = System.Text.Json.JsonSerializer.Serialize(serviceVersion, options: new System.Text.Json.JsonSerializerOptions()
        {
            WriteIndented = true
        });

        string supervisorParametersPath = GetMdsInstallFilePath(servicesBaseFolder, serviceSnapshot.ServiceName);
        await System.IO.File.WriteAllTextAsync(supervisorParametersPath, json);
    }
}
