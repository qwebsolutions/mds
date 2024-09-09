using Microsoft.AspNetCore.Routing;
using Metapsi;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using MdsCommon;
using Metapsi.Sqlite;
using System.Net.Sockets;
using System.Net;

namespace MdsInfrastructure;

[Metapsi.DocDescription("Keys for configuration of infrastructure controllers. If a key is deleted it will be reinitialized with the default value when controller starts")]
public class ConfigKey
{
    public string Key { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }

    public const string InfrastructureInternalBaseUrl = nameof(InfrastructureInternalBaseUrl);
    //public const string InfrastructurePublicBaseUrl = nameof(InfrastructurePublicBaseUrl);

    public const string CleanupRunDailyAt = nameof(CleanupRunDailyAt);
    public const string CleanupDeploymentsKeepMaxCount = nameof(CleanupDeploymentsKeepMaxCount);
    public const string CleanupDeploymentsKeepMaxDays = nameof(CleanupDeploymentsKeepMaxDays);
    public const string CleanupEventsKeepMaxCount = nameof(CleanupEventsKeepMaxCount);
    public const string CleanupEventsKeepMaxDays = nameof(CleanupEventsKeepMaxDays);
}

public static class ConfigExtensions
{
    public static async Task MapConfigDocs(
        this IEndpointRouteBuilder endpoint,
        ApplicationSetup applicationSetup,
        ImplementationGroup implementationGroup,
        SqliteQueue sqliteQueue,
        MdsInfrastructureApplication.State infrastructureState,
        MdsInfrastructureApplication.InputArguments inputArguments)
    {
        var rootEndpoint = await endpoint.UseDocs(
            applicationSetup,
            implementationGroup,
            sqliteQueue,
            b =>
            {
                b.SetOverviewUrl("config");
                b.ConfigureGroupRoutes(b =>
                {
                    b.RequireAuthorization(b =>
                    {
                        b.RequireAuthenticatedUser();
                    });
                });

                b.AddDoc<ConfigKey>(x => x.Key);
            });
    }

    private static async Task InitializeConfigKey(this CommandContext commandContext, string keyName,string defaultValue, string description)
    {
        var configKey = await commandContext.GetDoc<ConfigKey>(keyName);
        if (configKey == null)
        {
            await commandContext.SaveDoc(new ConfigKey()
            {
                Key = keyName,
                Value = defaultValue,
                Description = description
            });
        }
    }

    public static async Task InitializeDefaultConfigKeys(this CommandContext commandContext, MdsInfrastructureApplication.InputArguments inputArguments)
    {
        string localIP;
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            localIP = endPoint.Address.ToString();
        }

        await commandContext.InitializeConfigKey(
            ConfigKey.InfrastructureInternalBaseUrl,
            $"http://{localIP}:{inputArguments.UiPort}",
            "The base url for internal API calls.");

        //await commandContext.InitializeConfigKey(
        //    ConfigKey.InfrastructurePublicBaseUrl,
        //    $"http://localhost",
        //    "The base url for external HTTP.");

        await commandContext.InitializeConfigKey(
            ConfigKey.CleanupRunDailyAt,
            "02:00",
            "Runs all cleanup procedures daily at the specified hour. Must be in 24 hours format. Empty value disables automatic cleanup");

        await commandContext.InitializeConfigKey(
            ConfigKey.CleanupDeploymentsKeepMaxCount,
            "10",
            "Maximum amount of recent deployments to keep after a cleanup (excluding current deployment). Must be an int number. The most recent deployment is active and never deleted. 0 = always keep just current. Empty value disables cleanup based on count");

        await commandContext.InitializeConfigKey(
            ConfigKey.CleanupDeploymentsKeepMaxDays,
            "30",
            "Only deployments newer than X days are kept after cleanup. Must be an int number. The most recent deployment is active and never deleted. Empty value disables cleanup based on timestamp");

        await commandContext.InitializeConfigKey(
            ConfigKey.CleanupEventsKeepMaxCount,
            "100",
            "Maximum amount of recent infrastructure events to keep after a cleanup. Must be an int number. Empty value disables cleanup based on count");

        await commandContext.InitializeConfigKey(
            ConfigKey.CleanupEventsKeepMaxDays,
            "30",
            "Only infrastructure events newer than X days are kept after cleanup. Must be an int number. Empty value disables cleanup based on timestamp");
    }
}
