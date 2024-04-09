using Dapper;
using MdsInfrastructure;
using MdsLocal;
using Metapsi;
using Metapsi.JavaScript;
using Metapsi.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MdsTests;

[TestClass]
public class TestScenarios
{
    private static HttpClient httpClient = new HttpClient(new HttpClientHandler()
    {
        AllowAutoRedirect = false,
        UseCookies = true
    });

    [TestMethod]
    public async Task StartBuildManagerAndInfraAndLocal()
    {
        Metapsi.Sqlite.Converters.RegisterAll();


        //Metapsi.Sqlite.IdConverter.Register();


        //var local = await MdsLocal.LocalDb.LoadKnownConfiguration("c:\\github\\qwebsolutions\\mds\\tests\\MdsLocal.metapsi.dev.db");

        //var serialized = Metapsi.Serialize.ToJson(local);

        const string localNodeName = "ms-test-node";
        const string projectName = "MdsTests.CrashTestService";

        List<Process> stillRunningNodeProcesses = new();

        foreach (var osProcess in System.Diagnostics.Process.GetProcesses())
        {
            if (osProcess.ProcessName.StartsWith(MdsLocalApplication.ExePrefix(localNodeName)))
            {
                stillRunningNodeProcesses.Add(osProcess);
            }
        }


        foreach (var osProcess in stillRunningNodeProcesses)
        {
            osProcess.Kill();
        }

        var cleanStartFolder = "c:\\github\\qwebsolutions\\mds\\MdsTests\\CleanStart";

        var mdsFolder = "c:\\github\\qwebsolutions\\mds\\MdsTests\\mds\\";
        if (System.IO.Directory.Exists(mdsFolder))
        {
            System.IO.Directory.Delete(mdsFolder, true);
        }
        System.IO.Directory.CreateDirectory(mdsFolder);

        InitializeConfiguration(cleanStartFolder, mdsFolder);

        await DeclareLocalNode(mdsFolder, localNodeName, "127.0.0.1");

        var mdsBinariesFolder = System.IO.Path.Combine(mdsFolder, "Release");

        var mdsBinariesUrl = "http://localhost:5011";

        var buildControllerRefs = await MdsBuildManager.Program.StartBuildController(new string[0], new MdsBuildManager.InputArguments()
        {
            BinariesFolder = mdsBinariesFolder,
            ListeningUrl = mdsBinariesUrl
        });

        var buildController = buildControllerRefs.ApplicationSetup.Revive();

        await Task.Delay(TimeSpan.FromSeconds(5));

        await UploadBinaries(
            mdsBinariesUrl,
            projectName,
            "0.1",
            "e9e58d251de5d3b4612acb9a03fc7dea1d184773",
            "win-x64",
            System.IO.Path.Combine(cleanStartFolder, "MdsTests.CrashTestService.e9e58d251de5d3b4612acb9a03fc7dea1d184773.zip"));

        var infraReferences = await MdsInfrastructure.Program.SetupGlobalController(new MdsInfrastructure.MdsInfrastructureApplication.InputArguments()
        {
            AdminPassword = "admin!",
            AdminUserName = "admin",
            InfrastructureName = "mstest",
            UiPort = 9125,
            DbPath = System.IO.Path.Combine(mdsFolder, "MdsInfrastructure.db"),
            NodeCommandOutputChannel = "",
            BroadcastDeploymentOutputChannel = "161.35.193.157/ms-test.BroadcastDeployment",
            HealthStatusInputChannel = "161.35.193.157/ms-test.HealthStatus",
            BuildManagerUrl = "http://localhost:5011"
        }, DateTime.UtcNow);

        infraReferences.ApplicationSetup.Revive();

        await Task.Delay(5000);

        Simplified.Configuration configuration = new Simplified.Configuration()
        {
            Applications = new() { "TestApp" },
            Name = "TestConfiguration",
        };

        for (int i = 1; i <= 20; i++)
        {
            configuration.Services.Add(
                new Simplified.Service()
                {
                    Application = "TestApp",
                    Enabled = true,
                    Name = "DoesNotCrash" + i,
                    Node = localNodeName,
                    Project = projectName,
                    Version = "0.1",
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

        var response = await SignIn();

        response = await httpClient.PostAsJsonAsync("http://localhost:9125/api/configuration/", configuration);
        response.EnsureSuccessStatusCode();
        var responseMessage = await response.Content.ReadAsStringAsync();

        response = await httpClient.GetAsync("http://localhost:9125/api/LoadAllConfigurationHeaders");
        response.EnsureSuccessStatusCode();
        var allHeaders = Metapsi.Serialize.FromJson<MdsInfrastructure.ConfigurationHeadersList>(await response.Content.ReadAsStringAsync());

        response = await httpClient.GetAsync($"http://localhost:9125/api/ConfirmDeployment/{allHeaders.ConfigurationHeaders.First().Id}");
        response.EnsureSuccessStatusCode();

        var localControllerRefs = await MdsLocal.Program.SetupLocalController(new MdsLocal.InputArguments()
        {
            DbPath = System.IO.Path.Combine(mdsFolder, "MdsLocal.db"),
            NodeName = localNodeName,
            InfrastructureApiUrl = "http://localhost:9125/api/",
            ServicesBasePath = System.IO.Path.Combine(mdsFolder, "services"),
            BuildTarget = "win10-x64",
            ServicesDataPath = System.IO.Path.Combine(mdsFolder, "data")
        }, 0, DateTime.UtcNow);

        localControllerRefs.ApplicationSetup.Revive();

        await Task.Delay(TimeSpan.FromMinutes(30));
    }

    private static void InitializeConfiguration(string fromCleanStartFolder, string intoMdsFolder)
    {
        var buildManagerCleanDbPath = System.IO.Path.Combine(fromCleanStartFolder, "MdsBuildManager.db");

        var buildManagerDbPath = System.IO.Path.Combine(intoMdsFolder, "MdsBuildManager.db");
        System.IO.File.Copy(buildManagerCleanDbPath, buildManagerDbPath, true);

        var infraCleanDbPath = System.IO.Path.Combine(fromCleanStartFolder, "MdsInfrastructure.db");
        var infraDbPath = System.IO.Path.Combine(intoMdsFolder, "MdsInfrastructure.db");
        System.IO.File.Copy(infraCleanDbPath, infraDbPath, true);


        var localCleanDbPath = System.IO.Path.Combine(fromCleanStartFolder, "MdsLocal.db");
        var localDbPath = System.IO.Path.Combine(intoMdsFolder, "MdsLocal.db");
        System.IO.File.Copy(localCleanDbPath, localDbPath, true);
    }

    private static async Task DeclareLocalNode(string mdsFolder,
        string nodeName,
        string ip)
    {
        var localDbPath = System.IO.Path.Combine(mdsFolder, "MdsInfrastructure.db");
        await MdsInfrastructure.Db.SaveNode(
            localDbPath,
            new MdsInfrastructure.InfrastructureNode()
            {
                Active = true,
                EnvironmentTypeId = Guid.Parse("80c2e861-19e8-4328-a317-cc6623bbe812"),
                MachineIp = ip,
                NodeName = nodeName,
                UiPort = 9234
            });
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

    [TestMethod]
    public async Task ConvertOldDateFormat()
    {
        Metapsi.Sqlite.Converters.RegisterAll();

        const string localNodeName = "ms-test-node-convert-datetime";

        var dateTimeConversionOriginalDbsFolder = "c:\\github\\qwebsolutions\\mds\\MdsTests\\LocalDateTimeConversionStart";

        var mdsFolder = "c:\\github\\qwebsolutions\\mds\\MdsTests\\mds\\";
        if (System.IO.Directory.Exists(mdsFolder))
        {
            System.IO.Directory.Delete(mdsFolder, true);
        }
        System.IO.Directory.CreateDirectory(mdsFolder);

        var cleanStartFolder = "c:\\github\\qwebsolutions\\mds\\MdsTests\\CleanStart";

        var infraCleanDbPath = System.IO.Path.Combine(cleanStartFolder, "MdsInfrastructure.db");
        var infraDbPath = System.IO.Path.Combine(mdsFolder, "MdsInfrastructure.db");
        System.IO.File.Copy(infraCleanDbPath, infraDbPath, true);

        await DeclareLocalNode(mdsFolder, localNodeName, "127.0.0.1");

        var infraReferences = await MdsInfrastructure.Program.SetupGlobalController(new MdsInfrastructure.MdsInfrastructureApplication.InputArguments()
        {
            AdminPassword = "admin!",
            AdminUserName = "admin",
            InfrastructureName = "mstest",
            UiPort = 9125,
            DbPath = System.IO.Path.Combine(mdsFolder, "MdsInfrastructure.db"),
            NodeCommandOutputChannel = "",
            BroadcastDeploymentOutputChannel = "161.35.193.157/ms-test.BroadcastDeployment",
            HealthStatusInputChannel = "161.35.193.157/ms-test.HealthStatus",
            BuildManagerUrl = "http://localhost:5011"
        }, DateTime.UtcNow);

        infraReferences.ApplicationSetup.Revive();

        await Task.Delay(5000);


        var localCleanDbPath = System.IO.Path.Combine(dateTimeConversionOriginalDbsFolder, "MdsLocal.db");
        var localDbPath = System.IO.Path.Combine(mdsFolder, "MdsLocal.db");
        System.IO.File.Copy(localCleanDbPath, localDbPath, true);

        await MdsLocal.Program.ReplaceOldDateTimeFormat(localDbPath);

        var localControllerRefs = await MdsLocal.Program.SetupLocalController(new MdsLocal.InputArguments()
        {
            DbPath = System.IO.Path.Combine(mdsFolder, "MdsLocal.db"),
            NodeName = localNodeName,
            InfrastructureApiUrl = "http://localhost:9125/api/",
            ServicesBasePath = System.IO.Path.Combine(mdsFolder, "services"),
            BuildTarget = "win10-x64",
            ServicesDataPath = System.IO.Path.Combine(mdsFolder, "data")
        }, 0, DateTime.UtcNow);

        var app = localControllerRefs.ApplicationSetup.Revive();

        await app.SuspendComplete;
    }
}