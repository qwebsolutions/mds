using MdsInfrastructure;
using MdsLocal;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MdsTests;

public partial class TestScenarios
{

    [TestMethod]
    public async Task CheckOctavsErrors()
    {
        string json = "{\r\n  \"Name\": \"Latofres\",\r\n  \"Services\": [],,\r\n  \"Variables\": [\r\n    {\r\n      \"Name\": \"LogChannel\",\r\n      \"Value\": \"$(Redis)/$(InfrastructureName).Logger.Channel\"\r\n    },\r\n    {\r\n      \"Name\": \"Redis\",\r\n      \"Value\": \"192.168.1.16:6379\"\r\n    }\r\n  ],\r\n  \"Applications\": []\r\n}\r\n";

        await Task.Delay(3000);

        await SignIn();

        var response = await httpClient.PostAsync(
            "http://localhost:9125/api/checkconfiguration", new StringContent("", Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
        
        await Task.Delay(10000000);
    }

    [TestMethod]
    public async Task RedeployTwoServicesDisableOneOfThem()
    {
        await DeclareLocalNode(MdsFolder, LocalNodeName, "127.0.0.1");
        await StartLocalController(LocalNodeName);
        var configuration = CreateConfiguration(LocalNodeName, 20);
        await DeployConfiguration(configuration);

        await CheckUntil(async () =>
        {
            var knownConfiguration = await LocalDb.LoadKnownConfiguration(System.IO.Path.Combine(MdsFolder, "MdsLocal.db"));
            return knownConfiguration.Count() == 20;
        });

        await CheckUntil(async () =>
        {
            return GetNodeProcesses(LocalNodeName).Count == 20;
        }, 100);

        var initialPids = GetNodeProcesses(LocalNodeName).Select(x => x.Id).ToList();

        configuration.Services[1].Enabled = false;

        await DeployConfiguration(configuration);

        await CheckUntil(async () =>
        {
            var knownConfiguration = await LocalDb.LoadKnownConfiguration(System.IO.Path.Combine(MdsFolder, "MdsLocal.db"));
            return knownConfiguration.Count() == 19;
        });

        await CheckUntil(async () =>
        {
            var runningProcesses = GetNodeProcesses(LocalNodeName);
            var newPids = runningProcesses.Select(x => x.Id).ToList();
            if (newPids.Any(x => !initialPids.Contains(x)))
            {
                throw new System.Exception("Process restarted!");
            }
            return newPids.Count() == 19;
        }, 20);

        //await Task.Delay(System.TimeSpan.FromMinutes(10));
    }

    [TestMethod]
    public async Task ManyNodes()
    {
        var nodesCount = 1;
        var servicesPerNode = 1;

        var configuration = CreateConfiguration(LocalNodeName, nodesCount * servicesPerNode);
        for (int nodeIndex = 0; nodeIndex < nodesCount; nodeIndex++)
        {
            var nodeName = LocalNodeName + "-" + (nodeIndex + 1);
            KillProcesses(nodeName);
            InitializeLocalDatabase(nodeName + ".db");
            await DeclareLocalNode(MdsFolder, nodeName, "127.0.0.1", 9234 + nodeIndex);
            await StartLocalController(nodeName);
            for (int serviceIndex = 0; serviceIndex < servicesPerNode; serviceIndex++)
            {
                configuration.Services[nodeIndex * nodesCount + (serviceIndex % servicesPerNode)].Node = nodeName;
            }
        }

        await DeployConfiguration(configuration);
        await Task.Delay(System.TimeSpan.FromSeconds(20));

        //configuration.Services.First().Enabled = false;
        //await DeployConfiguration(configuration);

        await Task.Delay(System.TimeSpan.FromMinutes(30));
    }
}