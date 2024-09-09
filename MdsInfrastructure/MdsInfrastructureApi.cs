using MdsCommon;
using MdsInfrastructure.Flow;
using Metapsi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Metapsi.Sqlite;

namespace MdsInfrastructure
{

    public static class MdsInfrastructureApi
    {

        public static void AddMdsLegacyApi(
            this WebApplication app, 
            MdsInfrastructureApplication.InputArguments arguments,
            SqliteQueue sqliteQueue)
        {
            app.MapGet("getinfrastructureconfiguration", async (CommandContext cc, string nodeName) =>
            {
                var allNodes = await sqliteQueue.LoadAllNodes();
                InfrastructureNode node = allNodes.Single(x => x.NodeName == nodeName);

                cc.Logger.LogDebug($"GetInfrastructureConfiguration: node name {nodeName}");

                return new MdsCommon.InfrastructureNodeSettings()
                {
                    InfrastructureName = arguments.InfrastructureName,
                    BinariesApiUrl = arguments.BuildManagerUrl,
                    BroadcastDeploymentInputChannel = arguments.BroadcastDeploymentOutputChannel,
                    HealthStatusOutputChannel = arguments.HealthStatusInputChannel,
                    InfrastructureEventsOutputChannel = arguments.InfrastructureEventsInputChannel,
                    NodeCommandInputChannel = Mds.SubstituteVariable(arguments.NodeCommandOutputChannel, "NodeName", nodeName),
                    NodeUiPort = node.UiPort
                };
            });

            app.MapGet("getcontrollerconfiguration", async (string nodeName) =>
            {
                var nodeServicesSnapshot = await Db.LoadNodeConfiguration(sqliteQueue, nodeName);
                return nodeServicesSnapshot;
            });

            app.MapGet("getserviceconfiguration", async (string serviceName) =>
            {
                var serviceSnapshot = await Db.LoadServiceConfiguration(sqliteQueue, serviceName);
                return serviceSnapshot;
            });

            app.MapGet("getcurrentdeployment", async () =>
            {
                return await Db.LoadActiveDeployment(sqliteQueue);
            });

            app.MapGet("getinfrastructurestatus", async () =>
            {
                return await Db.LoadFullInfrastructureHealthStatus(sqliteQueue);
            });

            app.MapGet("getservicestatus/{serviceName}", async (string serviceName) =>
            {
                var fullStatus = await Db.LoadFullInfrastructureHealthStatus(sqliteQueue);
                if (!fullStatus.SelectMany(x => x.ServiceStatuses).Any(x => x.ServiceName == serviceName))
                {
                    //throw new System.NotImplementedException("Typed json response!"); // Keep the old API, maybe the new one can be attached to /api?
                    return new ServiceStatus() { ServiceName = serviceName };
                }
                return fullStatus.SelectMany(x => x.ServiceStatuses).Single(x => x.ServiceName == serviceName);
            });

            app.MapGet("restartservice/{servicename}", async (CommandContext cc, string serviceName) =>
            {
                await cc.Do(Backend.RestartService, serviceName);
                return Results.Ok();
            });
        }
    }
}
