using MdsCommon;
using Metapsi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure
{
    public static class MdsInfrastructureApi
    {
        public static void AddMdsLegacyApi(this WebApplication app, MdsInfrastructureApplication.InputArguments arguments)
        {
            string fullDbPath = Metapsi.RelativePath.SearchUpfolder(RelativePath.From.EntryPath, arguments.DbPath);

            app.MapGet("getinfrastructureconfiguration", async (CommandContext cc, string nodeName) =>
            {
                var allNodes = await Db.LoadAllNodes(fullDbPath);
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
                var nodeServicesSnapshot = await Db.LoadNodeConfiguration(fullDbPath, nodeName);
                return nodeServicesSnapshot;
            });

            app.MapGet("getserviceconfiguration", async (string serviceName) =>
            {
                var serviceSnapshot = await Db.LoadServiceConfiguration(fullDbPath, serviceName);
                return serviceSnapshot;
            });

            app.MapGet("getcurrentdeployment", async () =>
            {
                return await Db.LoadActiveDeployment(fullDbPath);
            });

            app.MapGet("getinfrastructurestatus", async () =>
            {
                return await Db.LoadFullInfrastructureHealthStatus(fullDbPath);
            });

            app.MapGet("getservicestatus/{serviceName}", async (string serviceName) =>
            {
                var fullStatus = await Db.LoadFullInfrastructureHealthStatus(fullDbPath);
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

        public static void AddInfraApi(this WebApplication webApp, MdsInfrastructureApplication.InputArguments arguments)
        {
            string fullDbPath = arguments.DbPath;
            string infrastructureName = arguments.InfrastructureName;

            var app = webApp.MapGroup("/api");//.AllowAnonymous();

            app.MapRequest(Backend.ValidateSchema, (CommandContext cc, HttpContext http) => Db.ValidateSchema(fullDbPath), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapRequest(Backend.LoadInfraStatus, (CommandContext cc, HttpContext http) => Db.LoadInfrastructureStatus(fullDbPath), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapRequest(Backend.LoadAllConfigurationHeaders, (CommandContext cc, HttpContext http) => Db.LoadConfigurationHeaders(fullDbPath), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapRequest(Backend.LoadConfiguration, (CommandContext cc,HttpContext http, Guid id) => Db.LoadSpecificConfiguration(fullDbPath, id), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            //app.MapCommand(Backend.SaveConfiguration, (CommandContext cc, HttpContext http, InfrastructureConfiguration c) => Db.SaveConfiguration(fullDbPath, c), WebServer.Authorization.Public, WebServer.SwaggerTryout.Block);
            app.MapCommand(Backend.SaveNode, (CommandContext cc, HttpContext http, InfrastructureNode node) => Db.SaveNode(fullDbPath, node), WebServer.Authorization.Public, WebServer.SwaggerTryout.Block);
            app.MapCommand(Backend.DeleteConfigurationById, (CommandContext cc, HttpContext http, Guid id) => Db.DeleteConfiguration(fullDbPath, id), WebServer.Authorization.Public, WebServer.SwaggerTryout.Block);
            app.MapRequest(Backend.LoadCurrentDeployment, (CommandContext cc, HttpContext http) => Db.LoadActiveDeployment(fullDbPath), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapRequest(Backend.LoadLastConfigurationDeployment, (CommandContext cc, HttpContext http, Guid id) => Db.LoadLastDeploymentOfConfiguration(fullDbPath, id), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapRequest(Backend.LoadCurrentConfiguration, (CommandContext cc, HttpContext http) => Db.LoadCurrentConfiguration(fullDbPath), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapRequest(Backend.LoadDeploymentById, (CommandContext cc, HttpContext http, Guid id) => Db.LoadSpecificDeployment(fullDbPath, id), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapRequest(Backend.LoadDeploymentsHistory, (CommandContext cc, HttpContext http) => Db.LoadDeploymentHistory(fullDbPath), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapRequest(Backend.LoadAllProjects, (CommandContext cc, HttpContext http) => Db.LoadAllProjects(fullDbPath), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapRequest(Backend.LoadAllNodes, (CommandContext cc, HttpContext http) => Db.LoadAllNodes(fullDbPath), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapRequest(Backend.LoadAllServices, (CommandContext cc, HttpContext http) => Db.LoadAllServices(fullDbPath), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapCommand(Backend.ConfirmDeployment, (CommandContext cc, HttpContext http, ConfirmDeploymentInput input) => Db.ConfirmDeployment(fullDbPath, input.Snapshots, input.Configuration), WebServer.Authorization.Public, WebServer.SwaggerTryout.Block);
            app.MapRequest(Backend.GetAllParameterTypes, (CommandContext cc, HttpContext http) => Db.LoadParameterTypes(fullDbPath), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapRequest(Backend.LoadEnvironmentTypes, (CommandContext cc, HttpContext http) => Db.LoadEnvironmentTypes(fullDbPath), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapRequest(Backend.LoadHealthStatus, (CommandContext cc, HttpContext http) => Db.LoadFullInfrastructureHealthStatus(fullDbPath), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapRequest(Backend.GetInfrastructureName, async (CommandContext cc, HttpContext http) => await Task.FromResult(infrastructureName), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapRequest(Backend.GetAllNoteTypes, (CommandContext cc, HttpContext http) => Db.LoadAllNoteTypes(fullDbPath), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapCommand(Backend.StoreHealthStatus, (CommandContext cc, HttpContext http, MachineStatus hs) => Db.StoreHealthStatus(fullDbPath, hs), WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
            app.MapCommand(Backend.SaveVersionEnabled, (CommandContext cc, HttpContext http, ProjectVersion version) => Db.SaveVersionEnabled(fullDbPath, version), WebServer.Authorization.Public, WebServer.SwaggerTryout.Block);
            app.MapRequest(MdsCommon.Api.GetInfrastructureNodeSettings,
                async (CommandContext cc, HttpContext http, string nodeName) =>
                {
                    var allNodes = await Db.LoadAllNodes(fullDbPath);
                    InfrastructureNode node = allNodes.Single(x => x.NodeName == nodeName);

                    var buildManagerNodeUrl = arguments.BuildManagerNodeUrl;
                    if(string.IsNullOrEmpty(buildManagerNodeUrl))
                    {
                        buildManagerNodeUrl = arguments.BuildManagerUrl;
                    }

                    return new MdsCommon.InfrastructureNodeSettings()
                    {
                        InfrastructureName = infrastructureName,
                        BinariesApiUrl = buildManagerNodeUrl,
                        BroadcastDeploymentInputChannel = arguments.BroadcastDeploymentOutputChannel,
                        HealthStatusOutputChannel = arguments.HealthStatusInputChannel,
                        InfrastructureEventsOutputChannel = arguments.InfrastructureEventsInputChannel,
                        NodeCommandInputChannel = Mds.SubstituteVariable(arguments.NodeCommandOutputChannel, "NodeName", nodeName),
                        NodeUiPort = node.UiPort
                    };
                }, WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);

            app.MapRequest(MdsCommon.Api.GetCurrentNodeSnapshot,
                async (CommandContext cc, HttpContext http, string nodeName) =>
                {
                    return await Db.LoadNodeConfiguration(arguments.DbPath, nodeName);
                }, WebServer.Authorization.Public, WebServer.SwaggerTryout.Allow);
        }
    }
}
