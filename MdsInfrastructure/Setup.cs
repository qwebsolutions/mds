using Metapsi;
using MdsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using Metapsi.Sqlite;
using System.Collections;
using System.Net.Http;

namespace MdsInfrastructure
{


    public static partial class MdsInfrastructureApplication
    {




        //public static string DebugFile = System.IO.Path.Combine(Metapsi.RelativePath.SearchUpfolder(RelativePath.From.EntryPath, "mds"), "mdsinfra.log");

        public class References
        {
            public ApplicationSetup ApplicationSetup { get; set; }
            public ImplementationGroup ImplementationGroup { get; set; }
            public SqliteQueue DbQueue { get; set; }
            public MdsInfrastructureApplication.State InfrastructureState { get; set; }
            public Microsoft.AspNetCore.Builder.WebApplication WebApplication { get; set; }
            public MailSender.State MailSender { get; set; }
            public HttpClient HttpClient { get; set; }
        }

        public static References Setup(
            MdsInfrastructureApplication.InputArguments arguments,
            DateTime start)
        {
            string fullDbPath = Metapsi.RelativePath.SearchUpfolder(RelativePath.From.EntryPath, arguments.DbPath);

            var dbQueue = new SqliteQueue(fullDbPath);

            HttpClient httpClient = new HttpClient();

            Metapsi.Mds.LogToServiceText(arguments.LogFilePath, start, new LogMessage()
            {
                Message = new Metapsi.Log.Info($"MdsInfra: using {fullDbPath}")
            }).Wait();

            #region Application setup

            ApplicationSetup applicationSetup = ApplicationBuilder.New();
            ImplementationGroup implementationGroup = applicationSetup.AddImplementationGroup();

            applicationSetup.OnLogMessage = async (logMessage) =>
            {
                await Mds.LogToServiceText(arguments.LogFilePath, start, logMessage);

                switch (logMessage.Message)
                {
                    case Metapsi.Log.Error error:
                        {
                            await MdsCommon.Db.SaveInfrastructureEvent(dbQueue, new InfrastructureEvent()
                            {
                                Criticality = "Error",
                                FullDescription = error.ToString(),
                                ShortDescription = "Internal error",
                                Source = arguments.InfrastructureName,
                                Timestamp = DateTime.UtcNow,
                                Type = "Error"
                            });
                        }
                        break;

                    case Metapsi.Log.Exception ex:
                        {
                            await MdsCommon.Db.SaveInfrastructureEvent(dbQueue, new InfrastructureEvent()
                            {
                                Criticality = "Error",
                                FullDescription = ex.ToString(),
                                ShortDescription = "Internal error",
                                Source = arguments.InfrastructureName,
                                Timestamp = DateTime.UtcNow,
                                Type = "Error"
                            });
                        }
                        break;
                }
            };

            #endregion Application setup

            #region Execution queue states

            MdsInfrastructureApplication.State infrastructure = applicationSetup.AddBusinessState(new MdsInfrastructureApplication.State());
            HttpServer.State httpGateway = applicationSetup.AddBusinessState(new HttpServer.State());

            //var redisNotifier = applicationSetup.AddBusinessState(new RedisNotifier.State());

            //var binariesListener = applicationSetup.AddBusinessState(new RedisListener.State());
            //var healthListener = applicationSetup.AddBusinessState(new RedisListener.State());
            //var eventsListener = applicationSetup.AddBusinessState(new RedisListener.State());

            applicationSetup.AddCleanup(implementationGroup, infrastructure, dbQueue);

            applicationSetup.SetupMessagingApi(implementationGroup);

            implementationGroup.MapRequest(MdsCommon.Api.GetAllInfrastructureEvents, async (rc) =>
            {
                return await MdsCommon.Db.LoadAllInfrastructureEvents(dbQueue);
            });

            implementationGroup.MapRequest(MdsCommon.Api.GetMostRecentEventOfService, async (rc, serviceName) =>
            {
                return await MdsCommon.Db.LoadMostRecentInfrastructureEvent(dbQueue, serviceName);
            });

            MailSender.State mailSender = null;

            if (!string.IsNullOrWhiteSpace(arguments.ErrorEmails))
            {
                mailSender = applicationSetup.AddMailSender(
                    arguments.SmtpHost,
                    arguments.From,
                    arguments.Password,
                    arguments.CertificateThumbprint);
            }

            #endregion Execution queue states

            #region Event mappings

            applicationSetup.MapEvent<ApplicationRevived>(
                e =>
                {
                    e.Using(httpGateway, implementationGroup).EnqueueCommand(HttpServer.StartListening, new HttpServer.Configuration()
                    {
                        Port = arguments.InfrastructureApiPort
                    });

                    //e.Using(binariesListener, implementationGroup).EnqueueCommand(RedisListener.StartListening, new RedisChannel(arguments.BinariesAvailableInputChannel));
                    //e.Using(healthListener, implementationGroup).EnqueueCommand(RedisListener.StartListening, new RedisChannel(arguments.HealthStatusInputChannel));
                    //e.Using(eventsListener, implementationGroup).EnqueueCommand(RedisListener.StartListening, new RedisChannel(arguments.InfrastructureEventsInputChannel));
                    e.Using(infrastructure, implementationGroup).EnqueueCommand(SyncBuilds);
                    e.Using(infrastructure, implementationGroup).EnqueueCommand(async (cc, state) =>
                    {
                        await cc.RegisterNodesMessaging(dbQueue);
                    });
                    e.Using(infrastructure, implementationGroup).EnqueueCommand(async (cc, state) =>
                    {
                        await cc.InitializeDefaultConfigKeys(arguments);
                    });
                    e.Using(infrastructure, implementationGroup).EnqueueCommand(async (cc, state) =>
                    {
                        if (!string.IsNullOrEmpty(arguments.BuildManagerUrl))
                        {
                            var ownBaseUrl = await cc.GetDoc<ConfigKey>(ConfigKey.InfrastructureInternalBaseUrl);

                            if (ownBaseUrl != null)
                            {
                                if (!string.IsNullOrWhiteSpace(ownBaseUrl.Value))
                                {
                                    cc.NotifyUrl(arguments.BuildManagerUrl.TrimEnd('/') + "/event", new InfrastructureControllerStarted()
                                    {
                                        InfrastructureName = arguments.InfrastructureName,
                                        InternalBaseUrl = ownBaseUrl.Value
                                    });
                                }
                            }
                        }
                    });
                });

            applicationSetup.MapEvent<BinariesAvailable>(e =>
            {
                e.Using(infrastructure, implementationGroup).EnqueueCommand(SyncBuilds);
            });

            applicationSetup.MapEvent<ApplicationIsShuttingDown>(
                e => e.Using(httpGateway, implementationGroup).EnqueueCommand(HttpServer.Stop));

            //applicationSetup.MapEventIf<RedisListener.Event.NotificationReceived>(
            //    e => e.NotificationType == nameof(NodeStatus) || e.NotificationType == "HealthStatus",
            //    async e =>
            //    {
            //        e.Logger.LogInfo("NodeStatus");
            //        e.Logger.LogInfo(e.EventData.Payload);
            //        MdsCommon.SerializableHealthStatus serializableHealthStatus = Metapsi.Serialize.FromJson<MdsCommon.SerializableHealthStatus>(e.EventData.Payload);
            //        var healthStatus = MdsCommon.NodeStatus.FromSerializable(serializableHealthStatus);
            //        e.Logger.LogInfo($"HealthStatus notification {Metapsi.Serialize.ToJson(healthStatus)}");

            //        e.Using(infrastructure, implementationGroup).EnqueueCommand(async (cc, state) =>
            //        {
            //            await cc.Do(Backend.StoreHealthStatus, healthStatus);
            //        });
            //    });

            //applicationSetup.MapEvent<Backend.Event.BroadcastDeployment>(
            //    e =>
            //    {
            //        e.Logger.LogDebug($"Broadcast deployment on {arguments.BroadcastDeploymentOutputChannel}");
            //        e.Using(redisNotifier, implementationGroup).EnqueueCommand(
            //            RedisNotifier.NotifyChannel,
            //            new RedisChannelMessage(
            //                arguments.BroadcastDeploymentOutputChannel,
            //                "ConfigurationUpdate",
            //                String.Empty));
            //    });

            applicationSetup.MapEvent<Backend.Event.BinariesSynchronized>(
                e =>
                {
                    var _ = MdsCommon.Db.SaveInfrastructureEvent(dbQueue, new MdsCommon.InfrastructureEvent()
                    {
                        Criticality = "Info",
                        ShortDescription = "Binaries synchronized",
                        //FullDescription = Ubiquitous.Serialize.ToJson(newProjects),
                        FullDescription = string.Join("\n", e.EventData.BinariesDescription),
                        Source = "Binaries repository",
                        Timestamp = System.DateTime.UtcNow,
                        Type = "BinariesSynchronized"
                    });
                });

            //applicationSetup.MapEventIf<RedisListener.Event.NotificationReceived>(
            //    e => e.ChannelName == new RedisChannel(arguments.BinariesAvailableInputChannel).ChannelName,
            //    e =>
            //    {
            //        e.Using(infrastructure, implementationGroup).EnqueueCommand(SyncBuilds);
            //    });

            #endregion Event mappings

            #region Operation mappings

            implementationGroup.MapBackendApi(dbQueue, arguments.InfrastructureName);

            implementationGroup.MapRequest(MdsCommon.Api.GetAdminCredentials, async (rc) =>
            {
                return new MdsCommon.AdminCredentials()
                {
                    AdminUserName = arguments.AdminUserName,
                    AdminPassword = arguments.AdminPassword
                };
            });

            //implementationGroup.MapCommand(Backend.RestartService, async (rc, serviceName) =>
            //{
            //    // TODO: Does this even compile?
            //    await rc.Using(infrastructure, implementationGroup).EnqueueCommand(async (cc, state) =>
            //    {
            //        var serviceConfiguration = await Db.LoadServiceConfiguration(dbQueue, serviceName);

            //        var service = serviceConfiguration;

            //        if (service == null)
            //        {
            //            throw new Exception($"Service {serviceName} not configured");
            //        }

            //        var nodes = await cc.Do(Backend.LoadAllNodes);
            //        var node = nodes.FirstOrDefault(x => x.NodeName == service.NodeName);

            //        if (node == null)
            //        {
            //            throw new Exception($"Service {serviceName} misconfigured on node {service.NodeName}");
            //        }

            //        var nodeCommandChannel = arguments.NodeCommandOutputChannel.Replace("$NodeName", node.NodeName);

            //        await rc.Using(redisNotifier, implementationGroup).EnqueueCommand(
            //            RedisNotifier.NotifyChannel,
            //            new RedisChannelMessage(
            //                nodeCommandChannel,
            //                "RestartService",
            //                serviceName));
                //});
            //});

            implementationGroup.MapRequest(Backend.GetRemoteBuilds,
                async (rc) =>
                {
                    if (string.IsNullOrEmpty(arguments.BuildManagerUrl))
                        return new List<AlgorithmInfo>();

                    var result = await httpClient.GetAsync($"{arguments.BuildManagerUrl}/ListBinaries");
                    var buildsJson = await result.Content.ReadAsStringAsync();
                    var THEOptions = new System.Text.Json.JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var buildsList = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<MdsCommon.AlgorithmInfo>>(buildsJson, THEOptions);
                    return buildsList;
                });
            //implementationGroup.MapRequest(MdsInfrastructureApplication.LoadPendingInfrastructureEvents,
            //    async (rc) =>
            //    {
            //        List<MdsCommon.InfrastructureEvent> pendingEvents = new List<MdsCommon.InfrastructureEvent>();

            //        List<string> availableMessages =
            //        await rc.Using(redisReader, implementationGroup).EnqueueRequest(
            //            RedisConnector.RedisReader.GetAvailableMessages,
            //            new RedisConnector.RedisChannel(arguments.RedisUrl, arguments.InfrastructureName));

            //        foreach (string message in availableMessages)
            //        {
            //            MdsCommon.InfrastructureEvent infrastructureEvent = Ubiquitous.Serialize.FromJson<InfrastructureEvent>(message);
            //            pendingEvents.Add(infrastructureEvent);
            //        }

            //        return pendingEvents;
            //    });

            implementationGroup.MapRequest(Metapsi.HttpServer.GetResponse,
                async (rc, getRequest) =>
                {
                    Dictionary<string, string> substitutionValues = new Dictionary<string, string>();
                    substitutionValues.Add("InfrastructureName", arguments.InfrastructureName);

                    try
                    {
                        switch (getRequest.Segments.First().ToLower())
                        {
                            case "getinfrastructureconfiguration":
                                {
                                    string nodeName = getRequest.Segments.ElementAt(1);
                                    var allNodes = await Db.LoadAllNodes(dbQueue);
                                    InfrastructureNode node = allNodes.Single(x => x.NodeName == nodeName);
                                    rc.Logger.LogDebug($"GetInfrastructureConfiguration: node name {nodeName}");


                                    //InfrastructureNode node = null;

                                    //var deployedConfiguration = await Db.LoadCurrentConfiguration(fullDbPath);

                                    //if (deployedConfiguration.IsEmpty())
                                    //{
                                    //    var allConfigs = await Db.LoadConfigurationHeaders(fullDbPath);
                                    //    foreach(var configHeader in allConfigs.ConfigurationHeaders)
                                    //    {
                                    //        var config = await Db.LoadSpecificConfiguration(fullDbPath, configHeader.Id);
                                    //        if(config.
                                    //    }
                                    //    node = allConfigs.InfrastructureNodes.First(x => x.NodeName == nodeName);
                                    //}
                                    //else
                                    //{
                                    //    node = deployedConfiguration.InfrastructureNodes.SingleOrDefault(x => x.NodeName == nodeName);
                                    //}

                                    return ToJsonResponse(new MdsCommon.InfrastructureNodeSettings()
                                    {
                                        InfrastructureName = arguments.InfrastructureName,
                                        BinariesApiUrl = arguments.BuildManagerUrl,
                                        BroadcastDeploymentInputChannel = arguments.BroadcastDeploymentOutputChannel,
                                        HealthStatusOutputChannel = arguments.HealthStatusInputChannel,
                                        InfrastructureEventsOutputChannel = arguments.InfrastructureEventsInputChannel,
                                        NodeCommandInputChannel = Mds.SubstituteVariable(arguments.NodeCommandOutputChannel, "NodeName", nodeName),
                                        NodeUiPort = node.UiPort
                                    });
                                }
                            case "getcontrollerconfiguration":
                                {
                                    string nodeName = getRequest.Segments.ElementAt(1);
                                    var nodeServicesSnapshot = await Db.LoadNodeConfiguration(dbQueue, nodeName);
                                    return ToJsonResponse(nodeServicesSnapshot);
                                }
                            case "getserviceconfiguration":
                                {
                                    string serviceName = getRequest.Segments.ElementAt(1);
                                    var serviceSnapshot = await Db.LoadServiceConfiguration(dbQueue, serviceName);
                                    substitutionValues.Add("ServiceName", serviceName);
                                    return ToJsonResponse(serviceSnapshot);
                                }
                            case "getcurrentdeployment":
                                {
                                    var deployment = await Db.LoadActiveDeployment(dbQueue);
                                    return ToJsonResponse(deployment);
                                }
                            case "getinfrastructurestatus":
                                {
                                    return ToJsonResponse(await Db.LoadFullInfrastructureHealthStatus(dbQueue));
                                }
                            case "getservicestatus":
                                {
                                    string serviceName = getRequest.Segments.ElementAt(1);
                                    var fullStatus = await Db.LoadFullInfrastructureHealthStatus(dbQueue);
                                    if (!fullStatus.SelectMany(x => x.ServiceStatuses).Any(x => x.ServiceName == serviceName))
                                    {
                                        return ToTypedJsonResponse(new ServiceStatus() { ServiceName = serviceName });
                                    }
                                    return ToTypedJsonResponse(fullStatus.SelectMany(x => x.ServiceStatuses).Single(x => x.ServiceName == serviceName));
                                }

                            case "restartservice":
                                {
                                    return new HttpServer.Response()
                                    {
                                        ResponseCode = 404,
                                        ResponseContent = $"Not supported not found"
                                    };

                                    //string serviceName = getRequest.Segments.ElementAt(1);
                                    //var serviceConfiguration = await Db.LoadServiceConfiguration(dbQueue, serviceName);

                                    //var service = serviceConfiguration;

                                    //if (service == null)
                                    //{
                                    //    return new HttpServer.Response()
                                    //    {
                                    //        ResponseCode = 404,
                                    //        ResponseContent = $"Service {serviceName} not found"
                                    //    };
                                    //}

                                    //var nodes = await Db.LoadAllNodes(dbQueue);
                                    //var node = nodes.FirstOrDefault(x => x.NodeName == service.NodeName);

                                    //if (node == null)
                                    //{
                                    //    return new HttpServer.Response()
                                    //    {
                                    //        ResponseCode = 404,
                                    //        ResponseContent = $"Node {node.NodeName} not configured correctly"
                                    //    };
                                    //}

                                    //var nodeCommandChannel = arguments.NodeCommandOutputChannel.Replace("$NodeName", node.NodeName);

                                    //await rc.Using(redisNotifier, implementationGroup).EnqueueCommand(
                                    //    RedisNotifier.NotifyChannel,
                                    //    new RedisChannelMessage(
                                    //        nodeCommandChannel,
                                    //        "RestartService",
                                    //        serviceName));

                                    //return ToJsonResponse("OK");
                                }

                            default:
                                {
                                    return new HttpServer.Response()
                                    {
                                        ResponseCode = 404,
                                        ResponseContent = $""
                                    };
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        rc.Logger.LogException(ex);

                        return new HttpServer.Response()
                        {
                            ResponseCode = 404,
                            ResponseContent = $"{ex.Message}"
                        };
                    }
                });

            #endregion Operation mappings


            return new References()
            {
                ApplicationSetup = applicationSetup,
                ImplementationGroup = implementationGroup,
                DbQueue = dbQueue,
                InfrastructureState = infrastructure,
                MailSender = mailSender,
                HttpClient = httpClient
            };
        }

        public static HttpServer.Response ToJsonResponse(object response)
        {
            return new HttpServer.Response()
            {
                ContentType = "application/json",
                ResponseCode = 200,
                ResponseContent = Metapsi.Serialize.ToJson(response)
            };
        }

        public static HttpServer.Response ToTypedJsonResponse(object response)
        {
            return new HttpServer.Response()
            {
                ContentType = "application/json",
                ResponseCode = 200,
                ResponseContent = Metapsi.Serialize.ToTypedJson(response)
            };
        }
    }
}
