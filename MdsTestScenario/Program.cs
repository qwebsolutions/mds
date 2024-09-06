

using MdsInfrastructure;
using MdsLocal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public static class Program
{
    public static async Task Main()
    {
        try
        {
            await Initialize();
            await Task.Delay(TimeSpan.FromSeconds(5));
            var configuration = await CreateConfiguration(1, 1);
            await DeployConfiguration(configuration);
            await Task.Delay(System.TimeSpan.FromSeconds(30));

            //foreach (var service in configuration.Services)
            //{
            //    service.Enabled = false;
            //}

            //await DeployConfiguration(configuration);

        }
        finally
        {
            await Task.Delay(System.TimeSpan.FromMinutes(30));
        }
    }

    public const string LocalNodeName = "ms-test-node";
    public const string TestProjectName = "MdsTests.CrashTestService";
    public const string CleanStartFolder = "c:\\github\\qwebsolutions\\mds\\MdsTests\\CleanStart";
    public const string MdsFolder = "c:\\github\\qwebsolutions\\mds\\MdsTests\\mds\\";
    public const string MdsBinariesApiUrl = "http://localhost:5011";

    private static HttpClient httpClient = new HttpClient(new HttpClientHandler()
    {
        AllowAutoRedirect = false,
        UseCookies = true
    });

    //private static List<Process> GetNodeProcesses(string nodeName)
    //{
    //    List<Process> nodeProcesses = new();

    //    foreach (var osProcess in System.Diagnostics.Process.GetProcesses())
    //    {
    //        if (osProcess.ProcessName.StartsWith(MdsLocalApplication.ExePrefix(nodeName).TrimEnd('.')))
    //        {
    //            nodeProcesses.Add(osProcess);
    //        }
    //    }

    //    return nodeProcesses;
    //}

    private static void KillProcesses(string prefix)
    {
        var ownedProcesses = System.Diagnostics.Process.GetProcesses().Where(x => x.ProcessName.StartsWith("_" + prefix)).ToList();
        foreach (var osProcess in ownedProcesses)
        {
            osProcess.Kill();
            osProcess.WaitForExit();
        }
    }

    private static void RecreateFolder(string path)
    {
        if (System.IO.Directory.Exists(path))
        {
            System.IO.Directory.Delete(path, true);
        }
        System.IO.Directory.CreateDirectory(path);
    }

    //private static string LocalDbPath(string mdsFolder)
    //{
    //    var localDbPath = System.IO.Path.Combine(mdsFolder, "MdsLocal.db");
    //    return localDbPath;
    //}

    private static void InitializeBuildManagerDatabase(string intoDb = "MdsBuildManager.db")
    {
        var buildManagerCleanDbPath = System.IO.Path.Combine(CleanStartFolder, "MdsBuildManager.db");

        var buildManagerDbPath = System.IO.Path.Combine(MdsFolder, intoDb);
        System.IO.File.Copy(buildManagerCleanDbPath, buildManagerDbPath, true);
    }

    private static void InitializeInfraDatabase(string intoDb = "MdsInfrastructure.db")
    {
        var infraCleanDbPath = System.IO.Path.Combine(CleanStartFolder, "MdsInfrastructure.db");
        var infraDbPath = System.IO.Path.Combine(MdsFolder, intoDb);
        System.IO.File.Copy(infraCleanDbPath, infraDbPath, true);
    }

    private static void InitializeLocalDatabase(string intoDb = "MdsLocal.db")
    {
        var localCleanDbPath = System.IO.Path.Combine(CleanStartFolder, "MdsLocal.db");
        var localDbPath = System.IO.Path.Combine(MdsFolder, intoDb);
        System.IO.File.Copy(localCleanDbPath, localDbPath, true);
    }

    //private static async Task DeclareLocalNode(
    //    string mdsFolder,
    //    string nodeName,
    //    string ip,
    //    int uiPort = 9234)
    //{
    //    var localDbPath = System.IO.Path.Combine(mdsFolder, "MdsInfrastructure.db");
    //    await MdsInfrastructure.Db.SaveNode(
    //        localDbPath,
    //        new MdsInfrastructure.InfrastructureNode()
    //        {
    //            Active = true,
    //            EnvironmentTypeId = Guid.Parse("80c2e861-19e8-4328-a317-cc6623bbe812"),
    //            MachineIp = ip,
    //            NodeName = nodeName,
    //            UiPort = uiPort
    //        });
    //}

    private static async Task SaveLocalNode(
        string infraApi,
        string nodeName,
        string ip,
        int uiPort = 9234)
    {
        var node = new MdsInfrastructure.InfrastructureNode()
        {
            Active = true,
            EnvironmentTypeId = Guid.Parse("80c2e861-19e8-4328-a317-cc6623bbe812"),
            MachineIp = ip,
            NodeName = nodeName,
            UiPort = uiPort
        };

        var r = await httpClient.PostAsJsonAsync(infraApi + "/node/save", node);

        r.EnsureSuccessStatusCode();
    }

    private static async Task StartBuildController()
    {
        var mdsBinariesFolder = System.IO.Path.Combine(MdsFolder, "Release");

        var mdsBinariesUrl = "http://localhost:5011";

        var buildControllerRefs = await MdsBuildManager.Program.StartBuildController(new string[0], new MdsBuildManager.InputArguments()
        {
            BinariesFolder = mdsBinariesFolder,
            ListeningUrl = mdsBinariesUrl
        });

        var buildController = buildControllerRefs.ApplicationSetup.Revive();

        await Task.Delay(TimeSpan.FromSeconds(5));
    }

    private static async Task UploadBinaries(
        string binariesManagerBaseUrl,
        string projectName,
        string version,
        string revision,
        string target,
        string filePath)
    {
        var requestContent = new MultipartFormDataContent();

        var fileContent = System.IO.File.ReadAllBytes(filePath);
        var zipBinaries = new ByteArrayContent(fileContent);

        requestContent.Add(zipBinaries, "binaries", System.IO.Path.GetFileName(filePath));
        requestContent.Add(new StringContent(projectName), "project");
        requestContent.Add(new StringContent(version), "version");
        requestContent.Add(new StringContent(revision), "revision");
        requestContent.Add(new StringContent(target), "target");

        await httpClient.PostAsync(new Uri(new Uri(binariesManagerBaseUrl), "/UploadBinaries"), requestContent);
    }

    private static async Task UploadTestProjectBinaries()
    {
        await UploadBinaries(
            MdsBinariesApiUrl,
            TestProjectName,
            "0.2",
            "0fb5d33167268a188bb44d575073ccb0ce61f1b5",
            "win-x64",
            System.IO.Path.Combine(CleanStartFolder, "MdsTests.CrashTestService.0fb5d33167268a188bb44d575073ccb0ce61f1b5.zip"));

        await UploadBinaries(
            MdsBinariesApiUrl,
            TestProjectName,
            "0.1",
            "e9e58d251de5d3b4612acb9a03fc7dea1d184773",
            "win-x64",
            System.IO.Path.Combine(CleanStartFolder, "MdsTests.CrashTestService.e9e58d251de5d3b4612acb9a03fc7dea1d184773.zip"));
    }

    private static async Task StartInfraController()
    {
        var infraReferences = await MdsInfrastructure.Program.SetupGlobalController(new MdsInfrastructure.MdsInfrastructureApplication.InputArguments()
        {
            AdminPassword = "admin!",
            AdminUserName = "admin",
            InfrastructureName = "mstest",
            UiPort = 9125,
            DbPath = System.IO.Path.Combine(MdsFolder, "MdsInfrastructure.db"),
            NodeCommandOutputChannel = "",
            BroadcastDeploymentOutputChannel = "161.35.193.157/ms-test.BroadcastDeployment",
            HealthStatusInputChannel = "161.35.193.157/ms-test.HealthStatus",
            InfrastructureEventsInputChannel = "161.35.193.157/ms-test.Events",
            BuildManagerUrl = "http://localhost:5011"
        }, DateTime.UtcNow);

        infraReferences.ApplicationSetup.Revive();

        await CheckUntilUrlAvailable("http://localhost:9125");
    }

    private static async Task StartLocalController(string localNodeName, int uiPort)
    {
        var localControllerRefs = await MdsLocal.Program.SetupLocalController(new MdsLocal.InputArguments()
        {
            DbPath = System.IO.Path.Combine(MdsFolder, $"{localNodeName}.db"),
            NodeName = localNodeName,
            InfrastructureApiUrl = "http://localhost:9125/api/",
            ServicesBasePath = System.IO.Path.Combine(MdsFolder, "services", localNodeName),
            BuildTarget = "win10-x64",
            ServicesDataPath = System.IO.Path.Combine(MdsFolder, "data"),
        }, uiPort, DateTime.UtcNow);

        localControllerRefs.ApplicationSetup.Revive();

        await CheckUntilUrlAvailable($"http://localhost:{uiPort}");
    }

    private static async Task<HttpResponseMessage> SignIn()
    {
        var requestContent = new MultipartFormDataContent();

        var signInPayload = Metapsi.Serialize.ToJson(new MdsCommon.InputCredentials()
        {
            Password = "admin!",
            UserName = "admin"
        });

        requestContent.Add(new StringContent(signInPayload), "payload");

        return await httpClient.PostAsync("http://localhost:9125/signin/credentials", requestContent);
    }

    public static Simplified.Configuration CreateConfiguration(string localNodeName, int servicesCount)
    {
        Simplified.Configuration configuration = new Simplified.Configuration()
        {
            Applications = new() { "TestApp" },
            Name = "TestConfiguration",
        };

        for (int i = 1; i <= servicesCount; i++)
        {
            configuration.Services.Add(
                new Simplified.Service()
                {
                    Application = "TestApp",
                    Enabled = true,
                    Name = "DoesNotCrash" + i,
                    Node = localNodeName,
                    Project = TestProjectName,
                    Version = "0.2",
                    Parameters = new()
                    {
                        new Simplified.Parameter()
                        {
                            Name = "CrashAfterSeconds",
                            Value = "1000",
                            Type = "Value"
                        },
                        new Simplified.Parameter()
                        {
                            Name = "ExitCode",
                            Value = i.ToString(),
                            Type = "Value"
                        }
                    }
                });
        }

        return configuration;
    }

    private static async Task DeployConfiguration(Simplified.Configuration configuration)
    {
        var response = await SignIn();

        response = await httpClient.PostAsJsonAsync("http://localhost:9125/api/configuration/", configuration);
        response.EnsureSuccessStatusCode();
        var responseMessage = await response.Content.ReadAsStringAsync();

        response = await httpClient.GetAsync("http://localhost:9125/api/LoadAllConfigurationHeaders");
        response.EnsureSuccessStatusCode();
        var allHeaders = Metapsi.Serialize.FromJson<MdsInfrastructure.ConfigurationHeadersList>(await response.Content.ReadAsStringAsync());

        response = await httpClient.GetAsync($"http://localhost:9125/api/ConfirmDeployment/{allHeaders.ConfigurationHeaders.First().Id}");
        response.EnsureSuccessStatusCode();
    }

    private static async Task CheckUntil(Func<Task<bool>> check, int attempts = 10)
    {
        for (int i = 0; i < attempts; i++)
        {
            if (await check())
            {
                return;
            }
            else
            {
                await Task.Delay(1000);
            }
        }

        throw new Exception($"{attempts} attempts exceeded!");
    }

    private static async Task CheckUntilUrlAvailable(string url, int attempts = 5)
    {
        await CheckUntil(async () =>
        {
            try
            {
                var uiResult = await httpClient.GetAsync(url);
                if (uiResult.StatusCode == System.Net.HttpStatusCode.Found)
                {
                    // Why is 302 not succes?!
                    return true;
                }
                uiResult.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }, attempts);
    }

    public static async Task Initialize()
    {
        Metapsi.Sqlite.Converters.RegisterAll();

        KillProcesses(LocalNodeName);
        RecreateFolder(MdsFolder);
        InitializeBuildManagerDatabase();
        InitializeInfraDatabase();
        await StartBuildController();
        await UploadTestProjectBinaries();
        await StartInfraController();
    }

    public static async Task Cleanup()
    {
        KillProcesses(LocalNodeName);
    }

    //public static async Task ConvertOldDateFormat()
    //{
    //    Metapsi.Sqlite.Converters.RegisterAll();

    //    const string localNodeName = "ms-test-node-convert-datetime";

    //    var dateTimeConversionOriginalDbsFolder = "c:\\github\\qwebsolutions\\mds\\MdsTests\\LocalDateTimeConversionStart";

    //    var mdsFolder = "c:\\github\\qwebsolutions\\mds\\MdsTests\\mds\\";
    //    if (System.IO.Directory.Exists(mdsFolder))
    //    {
    //        System.IO.Directory.Delete(mdsFolder, true);
    //    }
    //    System.IO.Directory.CreateDirectory(mdsFolder);

    //    var cleanStartFolder = "c:\\github\\qwebsolutions\\mds\\MdsTests\\CleanStart";

    //    var infraCleanDbPath = System.IO.Path.Combine(cleanStartFolder, "MdsInfrastructure.db");
    //    var infraDbPath = System.IO.Path.Combine(mdsFolder, "MdsInfrastructure.db");
    //    System.IO.File.Copy(infraCleanDbPath, infraDbPath, true);

    //    await DeclareLocalNode(mdsFolder, localNodeName, "127.0.0.1");

    //    var infraReferences = await MdsInfrastructure.Program.SetupGlobalController(new MdsInfrastructure.MdsInfrastructureApplication.InputArguments()
    //    {
    //        AdminPassword = "admin!",
    //        AdminUserName = "admin",
    //        InfrastructureName = "mstest",
    //        UiPort = 9125,
    //        DbPath = System.IO.Path.Combine(mdsFolder, "MdsInfrastructure.db"),
    //        NodeCommandOutputChannel = "",
    //        BroadcastDeploymentOutputChannel = "161.35.193.157/ms-test.BroadcastDeployment",
    //        HealthStatusInputChannel = "161.35.193.157/ms-test.HealthStatus",
    //        BuildManagerUrl = "http://localhost:5011"
    //    }, DateTime.UtcNow);

    //    infraReferences.ApplicationSetup.Revive();

    //    await Task.Delay(5000);


    //    var localCleanDbPath = System.IO.Path.Combine(dateTimeConversionOriginalDbsFolder, "MdsLocal.db");
    //    var localDbPath = System.IO.Path.Combine(mdsFolder, "MdsLocal.db");
    //    System.IO.File.Copy(localCleanDbPath, localDbPath, true);

    //    await MdsLocal.Migrate.All(localDbPath);

    //    var localControllerRefs = await MdsLocal.Program.SetupLocalController(new MdsLocal.InputArguments()
    //    {
    //        DbPath = System.IO.Path.Combine(mdsFolder, "MdsLocal.db"),
    //        NodeName = localNodeName,
    //        InfrastructureApiUrl = "http://localhost:9125/api/",
    //        ServicesBasePath = System.IO.Path.Combine(mdsFolder, "services"),
    //        BuildTarget = "win10-x64",
    //        ServicesDataPath = System.IO.Path.Combine(mdsFolder, "data")
    //    }, 0, DateTime.UtcNow);

    //    var app = localControllerRefs.ApplicationSetup.Revive();

    //    await app.SuspendComplete;
    //}

    //public static async Task RedeployTwoServicesDisableOneOfThem()
    //{
    //    await DeclareLocalNode(MdsFolder, LocalNodeName, "127.0.0.1");
    //    await StartLocalController(LocalNodeName, 9234);
    //    var configuration = CreateConfiguration(LocalNodeName, 20);
    //    await DeployConfiguration(configuration);

    //    await CheckUntil(async () =>
    //    {
    //        var knownConfiguration = await LocalDb.LoadKnownConfiguration(System.IO.Path.Combine(MdsFolder, "MdsLocal.db"));
    //        return knownConfiguration.Count() == 20;
    //    });

    //    await CheckUntil(async () =>
    //    {
    //        return MdsLocal.ServiceProcessExtensions.IdentifyOwnedProcesses(LocalNodeName).Count == 20;
    //    }, 100);

    //    var initialPids = MdsLocal.ServiceProcessExtensions.IdentifyOwnedProcesses(LocalNodeName).Select(x => x.Id).ToList();

    //    configuration.Services[1].Enabled = false;

    //    await DeployConfiguration(configuration);

    //    await CheckUntil(async () =>
    //    {
    //        var knownConfiguration = await LocalDb.LoadKnownConfiguration(System.IO.Path.Combine(MdsFolder, "MdsLocal.db"));
    //        return knownConfiguration.Count() == 19;
    //    });

    //    await CheckUntil(async () =>
    //    {
    //        var runningProcesses = MdsLocal.ServiceProcessExtensions.IdentifyOwnedProcesses(LocalNodeName);
    //        var newPids = runningProcesses.Select(x => x.Id).ToList();
    //        if (newPids.Any(x => !initialPids.Contains(x)))
    //        {
    //            throw new System.Exception("Process restarted!");
    //        }
    //        return newPids.Count() == 19;
    //    }, 20);

    //    //await Task.Delay(System.TimeSpan.FromMinutes(10));
    //}

    public static async Task<Simplified.Configuration> CreateConfiguration(int nodesCount, int servicesPerNode)
    {
        var configuration = CreateConfiguration(LocalNodeName, nodesCount * servicesPerNode);
        for (int nodeIndex = 0; nodeIndex < nodesCount; nodeIndex++)
        {
            var nodeName = LocalNodeName + "-" + (nodeIndex + 1);
            var uiPort = 9234 + nodeIndex;
            KillProcesses(nodeName);
            InitializeLocalDatabase(nodeName + ".db");
            await SaveLocalNode("http://localhost:9125" , nodeName, "127.0.0.1", uiPort);
            await StartLocalController(nodeName, uiPort);
            for (int serviceIndex = 0; serviceIndex < servicesPerNode; serviceIndex++)
            {
                configuration.Services[nodeIndex * servicesPerNode + (serviceIndex % servicesPerNode)].Node = nodeName;
            }
        }

        return configuration;
    }

}