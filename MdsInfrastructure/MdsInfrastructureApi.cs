using MdsCommon;
using MdsInfrastructure.Flow;
using Metapsi;
using Metapsi.Ui;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
using System.Net.Http.Json;

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

        public static void AddWebApi(this WebApplication webApp, MdsInfrastructureApplication.InputArguments arguments)
        {
            HttpClient httpClient = new HttpClient();

            string fullDbPath = arguments.DbPath;
            string infrastructureName = arguments.InfrastructureName;

            var api = webApp.MapGroup("api");

            api.MapGet($"/{MdsCommon.Api.GetInfrastructureNodeSettings.Name}/{{nodeName}}",
                async (CommandContext cc, HttpContext http, string nodeName) =>
                {
                    var allNodes = await Db.LoadAllNodes(fullDbPath);
                    InfrastructureNode node = allNodes.Single(x => x.NodeName == nodeName);

                    var buildManagerNodeUrl = arguments.BuildManagerNodeUrl;
                    if (string.IsNullOrEmpty(buildManagerNodeUrl))
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
                }).AllowAnonymous();

            api.MapGet(
                Backend.LoadAllConfigurationHeaders.Name,
                async (CommandContext commandContext) =>
                {
                    return await commandContext.Do(Backend.LoadAllConfigurationHeaders);
                }).RequireAuthorization();

            api.MapGet($"/{MdsCommon.Api.GetCurrentNodeSnapshot.Name}/{{nodeName}}",
                async (CommandContext cc, HttpContext http, string nodeName) =>
                {
                    return await Db.LoadNodeConfiguration(arguments.DbPath, nodeName);
                }).AllowAnonymous();

            api.MapPost(
                Backend.SaveVersionEnabled.Name,
                async (CommandContext cc, HttpContext http, [FromBody] ProjectVersion version) =>
                {
                    await cc.Do(Backend.SaveVersionEnabled, version);
                }).AllowAnonymous();


            api.MapPostRequest(Frontend.SaveConfiguration, async (CommandContext commandContext, HttpContext httpContext, SaveConfigurationInput input) =>
            {
                ExternalConfiguration externalConfiguration = new ExternalConfiguration()
                {
                    Nodes = await commandContext.Do(Backend.LoadAllNodes),
                    NoteTypes = await commandContext.Do(Backend.GetAllNoteTypes),
                    ParameterTypes = await commandContext.Do(Backend.GetAllParameterTypes),
                    Projects = await commandContext.Do(Backend.LoadAllProjects)
                };

                var validationMessages = input.InfrastructureConfiguration.ValidateConfiguration(externalConfiguration);
                if (validationMessages.Any())
                {
                    return new SaveConfigurationResponse()
                    {
                        SaveValidationMessages = validationMessages
                    };
                }

                var lastSavedConfiguration = await commandContext.Do(Backend.LoadConfiguration, input.InfrastructureConfiguration.Id);
                var initialConfiguration = Metapsi.Serialize.FromJson<InfrastructureConfiguration>(input.OriginalJson);

                var sneakyEdits = Conflict.GetConfigurationChanges(initialConfiguration, lastSavedConfiguration);

                if (!sneakyEdits.Any())
                {
                    await commandContext.Do(Backend.SaveConfiguration, input.InfrastructureConfiguration);
                    return new SaveConfigurationResponse();
                }
                else
                {
                    return new SaveConfigurationResponse()
                    {
                        ConflictMessages = Conflict.BuildDiffMessages(sneakyEdits)
                    };
                }
            }).RequireAuthorization();

            api.MapPostRequest(Frontend.MergeConfiguration, async (commandContext, httpContext, input) =>
            {
                var lastSavedConfiguration = await commandContext.Do(Backend.LoadConfiguration, input.EditedConfiguration.Id);

                ExternalConfiguration externalConfiguration = new ExternalConfiguration()
                {
                    Nodes = await commandContext.Do(Backend.LoadAllNodes),
                    NoteTypes = await commandContext.Do(Backend.GetAllNoteTypes),
                    ParameterTypes = await commandContext.Do(Backend.GetAllParameterTypes),
                    Projects = await commandContext.Do(Backend.LoadAllProjects)
                };

                var sourceConfiguration = Metapsi.Serialize.FromJson<InfrastructureConfiguration>(input.SourceConfigurationJson);

                var mergeResult = Conflict.Merge(lastSavedConfiguration, sourceConfiguration, input.EditedConfiguration);

                if (mergeResult.ConflictMessages.Any())
                {
                    // If there are conflicts return the edited configuration with no changes
                    // Also return the JSON that can be used for manual merge

                    var simpleConfiguration = input.EditedConfiguration.Simplify(externalConfiguration);
                    var simpleConfigurationJson = JsonSerializer.Serialize(simpleConfiguration, Simplified.SerializerOptions);

                    return new MergeConfigurationResponse()
                    {
                        ConflictMessages = mergeResult.ConflictMessages,
                        Configuration = input.EditedConfiguration,
                        ConfigurationJson = simpleConfigurationJson
                    };
                }
                else
                {
                    // If there are no conflicts return the updated configuration and the saved one as new source
                    return new MergeConfigurationResponse()
                    {
                        SuccessMessages = mergeResult.SuccessMessages,
                        Configuration = mergeResult.ResultConfiguration,
                        SourceConfiguration = lastSavedConfiguration
                    };
                }
            }).RequireAuthorization();

            api.MapGetRequest(Frontend.ConfirmDeployment, async (commandContext, httpContext, configurationId) =>
            {
                var savedConfiguration = await commandContext.Do(Backend.LoadConfiguration, configurationId);
                var serverModel = await MdsInfrastructureFunctions.InitializeEditConfiguration(commandContext, savedConfiguration);
                var newInfraSnapshot = await MdsInfrastructureFunctions.TakeConfigurationSnapshot(
                    commandContext,
                    savedConfiguration,
                    serverModel.AllProjects,
                    serverModel.InfrastructureNodes);
                await commandContext.Do(Backend.ConfirmDeployment, new ConfirmDeploymentInput()
                {
                    Snapshots = newInfraSnapshot,
                    Configuration = savedConfiguration
                });

                Backend.Event.BroadcastDeployment broadcastDeployment = new();
                commandContext.PostEvent(broadcastDeployment);

                return new Frontend.ConfirmDeploymentResponse();
            }).RequireAuthorization();

            api.MapPostRequest(Frontend.GetConfigurationJson, async (CommandContext commandContext, HttpContext httpContext, InfrastructureConfiguration edited) =>
            {
                ExternalConfiguration externalConfiguration = new ExternalConfiguration()
                {
                    Nodes = await commandContext.Do(Backend.LoadAllNodes),
                    NoteTypes = await commandContext.Do(Backend.GetAllNoteTypes),
                    ParameterTypes = await commandContext.Do(Backend.GetAllParameterTypes),
                    Projects = await commandContext.Do(Backend.LoadAllProjects)
                };

                return new GetConfigurationJsonResponse()
                {
                    Json = await edited.SerializeSimplified(externalConfiguration)
                };
            }).RequireAuthorization();


            api.MapPost("/windows-scripts", async (CommandContext commandContext, HttpContext httpContext, InfrastructureConfiguration edited) =>
            {
                var lastSavedConfiguration = await commandContext.Do(Backend.LoadConfiguration, edited.Id);

                ExternalConfiguration externalConfiguration = new ExternalConfiguration()
                {
                    Nodes = await commandContext.Do(Backend.LoadAllNodes),
                    NoteTypes = await commandContext.Do(Backend.GetAllNoteTypes),
                    ParameterTypes = await commandContext.Do(Backend.GetAllParameterTypes),
                    Projects = await commandContext.Do(Backend.LoadAllProjects)
                };

                var assembly = typeof(Program).Assembly;
                await using (MemoryStream outZip = new MemoryStream())
                {
                    using (ZipArchive archive = new ZipArchive(outZip, ZipArchiveMode.Create, true))
                    {
                        ZipArchiveEntry curlExe = archive.CreateEntry("curl.exe");
                        using (StreamWriter writer = new StreamWriter(curlExe.Open()))
                        {
                            var stream = assembly.GetManifestResourceStream("curl.exe");
                            stream.Position = 0;
                            await stream.CopyToAsync(writer.BaseStream);
                        }

                        ZipArchiveEntry curlCaBundleCrt = archive.CreateEntry("curl-ca-bundle.crt");
                        using (StreamWriter writer = new StreamWriter(curlCaBundleCrt.Open()))
                        {
                            var stream = assembly.GetManifestResourceStream("curl-ca-bundle.crt");
                            stream.Position = 0;
                            await stream.CopyToAsync(writer.BaseStream);
                        }

                        ZipArchiveEntry checkBat = archive.CreateEntry("check.bat");
                        using (StreamWriter writer = new StreamWriter(checkBat.Open()))
                        {
                            var stream = assembly.GetManifestResourceStream("check.bat");
                            stream.Position = 0;
                            await stream.CopyToAsync(writer.BaseStream);
                            await stream.FlushAsync();
                        }

                        ZipArchiveEntry uploadBat = archive.CreateEntry("upload.bat");
                        using (StreamWriter writer = new StreamWriter(uploadBat.Open()))
                        {
                            var stream = assembly.GetManifestResourceStream("upload.bat");
                            stream.Position = 0;
                            await stream.CopyToAsync(writer.BaseStream);
                            await stream.FlushAsync();
                        }

                        ZipArchiveEntry currentJson = archive.CreateEntry("configuration.current.json");
                        using (StreamWriter writer = new StreamWriter(currentJson.Open()))
                        {
                            await writer.WriteAsync(await lastSavedConfiguration.SerializeSimplified(externalConfiguration));
                            await writer.FlushAsync();
                        }

                        ZipArchiveEntry nextJson = archive.CreateEntry("configuration.next.json");
                        using (StreamWriter writer = new StreamWriter(nextJson.Open()))
                        {
                            await writer.WriteAsync(await edited.SerializeSimplified(externalConfiguration));
                            await writer.FlushAsync();
                        }

                        ZipArchiveEntry infraUrlTxt = archive.CreateEntry("infra_url.txt");
                        using (StreamWriter writer = new StreamWriter(infraUrlTxt.Open()))
                        {
                            await writer.WriteAsync($"{httpContext.Request.Scheme}://{httpContext.Request.Host}");
                            await writer.FlushAsync();
                        }

                        ZipArchiveEntry configurationIdTxt = archive.CreateEntry("configuration_id.txt");
                        using (StreamWriter writer = new StreamWriter(configurationIdTxt.Open()))
                        {
                            await writer.WriteAsync($"{edited.Id}");
                            await writer.FlushAsync();
                        }

                        ZipArchiveEntry credentialsJson = archive.CreateEntry("credentials.json");
                        using (StreamWriter writer = new StreamWriter(credentialsJson.Open()))
                        {
                            var adminCredentials = await commandContext.Do(MdsCommon.Api.GetAdminCredentials);
                            await writer.WriteAsync(Metapsi.Serialize.ToJson(new InputCredentials()
                            {
                                Password = adminCredentials.AdminPassword,
                                UserName = adminCredentials.AdminUserName
                            }));
                            await writer.FlushAsync();
                        }

                        ZipArchiveEntry gitIgnore = archive.CreateEntry(".gitignore");
                        using (StreamWriter writer = new StreamWriter(gitIgnore.Open()))
                        {
                            await writer.WriteLineAsync("credentials.json");
                            await writer.WriteLineAsync("cookies.txt");
                            await writer.WriteLineAsync("configuration.next.json");
                            await writer.WriteLineAsync("usegit.txt");
                            await writer.FlushAsync();
                        }
                    }
                    var outZipArray = outZip.ToArray();
                    return Results.File(outZipArray, "application/zip", $"{edited.Name}.zip");
                }
            }).RequireAuthorization();

            api.MapGet(
                "/configuration/{id}",
                async (CommandContext commandContext, HttpContext httpContext, Guid id) =>
                {
                    var lastSavedConfiguration = await commandContext.Do(Backend.LoadConfiguration, id);

                    ExternalConfiguration externalConfiguration = new ExternalConfiguration()
                    {
                        Nodes = await commandContext.Do(Backend.LoadAllNodes),
                        NoteTypes = await commandContext.Do(Backend.GetAllNoteTypes),
                        ParameterTypes = await commandContext.Do(Backend.GetAllParameterTypes),
                        Projects = await commandContext.Do(Backend.LoadAllProjects)
                    };

                    httpContext.Response.ContentType = "application/json";
                    var json = await lastSavedConfiguration.SerializeSimplified(externalConfiguration);
                    // await using... otherwise, it says I'm using sync operations. Weird
                    await using (var writer = new StreamWriter(httpContext.Response.Body))
                    {
                        await writer.WriteAsync(json);
                        await writer.FlushAsync();
                    }
                }).RequireAuthorization();

            api.MapPost(
                "/configuration",
                async (CommandContext commandContext, HttpContext httpContext, Simplified.Configuration input) =>
                {
                    var (messages, configuration) = await ConvertSimplified(commandContext, input);

                    if (messages.Errors.Any())
                    {
                        httpContext.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
                        messages.Result = "Not saved, validation failed";
                        return messages;
                    }

                    if (!messages.Changes.Any())
                    {
                        httpContext.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
                        messages.Result = "Not saved, configuration has no changes";
                        return messages;
                    }

                    await commandContext.Do(Backend.SaveConfiguration, configuration);
                    messages.Result = "Configuration saved";
                    return messages;
                }).RequireAuthorization();

            api.MapPost(
                "/checkconfiguration",
                async (CommandContext commandContext, HttpContext httpContext, Simplified.Configuration input) =>
                {
                    var (messages, configuration) = await ConvertSimplified(commandContext, input);

                    if (messages.Errors.Any())
                    {
                        messages.Result = "Validation of file configuration.next.json failed";
                    }
                    else
                    {
                        if (!messages.Changes.Any())
                        {
                            return new ConversionResult()
                            {
                                Result = "Configuration has no changes"
                            };
                        }
                        else
                        {
                            messages.Result = "Configuration file configuration.next.json is valid";
                        }
                    }
                    return messages;
                }).RequireAuthorization();

            api.MapPostRequest(
                Frontend.RemoveBuilds,
                async (CommandContext commandContext, HttpContext httpContext, RemoveBuildsRequest input) =>
                {

                    List<AlgorithmInfo> toRemoveAlgorithms = input.ToRemove.Where(x => x.Selected).Select(
                        x => new AlgorithmInfo()
                        {
                            Name = x.ProjectName,
                            BuildNumber = x.BuildNumber,
                            Target = x.Target,
                            Version = x.ProjectVersion
                        }).ToList();

                    var url = arguments.BuildManagerUrl + "/DeleteBuilds";

                    var result = await httpClient.PostAsJsonAsync(url, toRemoveAlgorithms);

                    if (result.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("Cannot delete, repository does not support this operation");
                    }

                    result.EnsureSuccessStatusCode();

                    input.ToRemove.ForEach(x => x.Removed = true);

                    return new RemoveBuildsResponse()
                    {
                        Removed = input.ToRemove
                    };
                }).RequireAuthorization();

            api.MapGetRequest(
                Frontend.ReloadListProjectsPageModel,
                async (CommandContext commandContext, HttpContext httpContext) =>
                {
                    // This is called after delete, if new binaries happen to be incoming we don't
                    // care to notify. We want to refresh the list of removed binaries
                    var buildsList = await commandContext.Do(Backend.GetRemoteBuilds);
                    await commandContext.Do(Backend.RefreshBinaries, buildsList);

                    var model = await MdsInfrastructure.Flow.Project.List.LoadAll(commandContext, httpContext, buildsList);
                    return new ReloadListProjectsPageModel()
                    {
                        Model = model
                    };
                }).RequireAuthorization();

            api.MapPost("/signin", async (CommandContext commandContext, HttpContext httpContext, InputCredentials inputCredentials) =>
            {
                var adminCredentials = await commandContext.Do(MdsCommon.Api.GetAdminCredentials);

                if (inputCredentials.UserName != adminCredentials.AdminUserName || inputCredentials.Password != adminCredentials.AdminPassword)
                {
                    var queryString = httpContext.Request.QueryString.Value;

                    return Results.Unauthorized();
                }

                var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, inputCredentials.UserName),
                            new Claim(ClaimTypes.Name, inputCredentials.UserName)
                        };

                //var identity = new ClaimsIdentity(claims, "LDAP");
                var identity = new ClaimsIdentity(claims, "OIDC");

                var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                await httpContext.SignInAsync(principal);
                return Results.Ok();
            });
        }


        private static async Task<(ConversionResult, InfrastructureConfiguration)> ConvertSimplified(CommandContext commandContext, Simplified.Configuration input)
        {
            ExternalConfiguration externalConfiguration = new ExternalConfiguration()
            {
                Nodes = await commandContext.Do(Backend.LoadAllNodes),
                NoteTypes = await commandContext.Do(Backend.GetAllNoteTypes),
                ParameterTypes = await commandContext.Do(Backend.GetAllParameterTypes),
                Projects = await commandContext.Do(Backend.LoadAllProjects)
            };

            var validationMessages = input.Validate(externalConfiguration);

            if (validationMessages.Any())
            {
                return (new ConversionResult()
                {
                    Errors = validationMessages
                }, null);
            }
            else
            {
                List<string> updateMessages = new List<string>();

                var configurationHeaders = await commandContext.Do(Backend.LoadAllConfigurationHeaders);

                var matchingHeader = configurationHeaders.ConfigurationHeaders.SingleOrDefault(x => x.Name == input.Name);

                InfrastructureConfiguration saved = null;
                if (matchingHeader == null)
                {
                    saved = new InfrastructureConfiguration()
                    {
                        Name = input.Name,
                    };

                    updateMessages.Add($"New configuration {saved.Name} created");
                }
                else
                {
                    saved = await commandContext.Do(Backend.LoadConfiguration, matchingHeader.Id);
                }

                var result = input.Complicate(saved, externalConfiguration).Sorted();

                var serviceChanges = Diff.CollectionsByKey(saved.InfrastructureServices, result.InfrastructureServices, x => x.Id);
                foreach (var removedService in serviceChanges.JustInFirst)
                {
                    updateMessages.Add($"Service {removedService.ServiceName} removed");
                }

                foreach (var addedService in serviceChanges.JustInSecond)
                {
                    updateMessages.Add($"Service {addedService.ServiceName} added");
                }

                foreach (var changedService in serviceChanges.Different)
                {
                    updateMessages.Add($"Service {changedService.InFirst.ServiceName} changed");
                }

                var applicationChanges = Diff.CollectionsByKey(saved.Applications, result.Applications, x => x.Id);

                updateMessages.AddRange(applicationChanges.JustInFirst.Select(x => $"Application {x.Name} removed"));
                updateMessages.AddRange(applicationChanges.JustInSecond.Select(x => $"Application {x.Name} added"));
                updateMessages.AddRange(applicationChanges.Different.Select(x => $"Application {x.InFirst.Name} renamed to {x.InSecond.Name}"));

                var variableChanges = Diff.CollectionsByKey(saved.InfrastructureVariables, result.InfrastructureVariables, x => x.Id);

                updateMessages.AddRange(variableChanges.JustInFirst.Select(x => $"Variable {x.VariableName} removed"));
                updateMessages.AddRange(variableChanges.JustInSecond.Select(x => $"Variable  {x.VariableName} added"));
                updateMessages.AddRange(variableChanges.Different.Select(x => $"Variable {x.InFirst.VariableName} changed"));

                return (new() { Changes = updateMessages }, result);
            }
        }

        public static void ImplementBackend(this ImplementationGroup ig, MdsInfrastructureApplication.InputArguments arguments)
        {
            string fullDbPath = arguments.DbPath;
            string infrastructureName = arguments.InfrastructureName;

            ig.MapRequest(Backend.ValidateSchema, (RequestRoutingContext cc) => Db.ValidateSchema(fullDbPath));
            ig.MapRequest(Backend.LoadInfraStatus, (RequestRoutingContext cc) => Db.LoadInfrastructureStatus(fullDbPath));
            ig.MapRequest(Backend.LoadAllConfigurationHeaders, (RequestRoutingContext cc) => Db.LoadConfigurationHeaders(fullDbPath));
            ig.MapRequest(Backend.LoadConfiguration, (RequestRoutingContext cc, Guid id) => Db.LoadSpecificConfiguration(fullDbPath, id));
            ig.MapCommand(Backend.SaveNode, (CommandRoutingContext cc, InfrastructureNode node) => Db.SaveNode(fullDbPath, node));
            ig.MapCommand(Backend.DeleteConfigurationById, (CommandRoutingContext cc, Guid id) => Db.DeleteConfiguration(fullDbPath, id));
            ig.MapRequest(Backend.LoadCurrentDeployment, (RequestRoutingContext cc) => Db.LoadActiveDeployment(fullDbPath));
            ig.MapRequest(Backend.LoadLastConfigurationDeployment, (RequestRoutingContext cc, Guid id) => Db.LoadLastDeploymentOfConfiguration(fullDbPath, id));
            ig.MapRequest(Backend.LoadCurrentConfiguration, (RequestRoutingContext cc) => Db.LoadCurrentConfiguration(fullDbPath));
            ig.MapRequest(Backend.LoadDeploymentById, (RequestRoutingContext cc, Guid id) => Db.LoadSpecificDeployment(fullDbPath, id));
            ig.MapRequest(Backend.LoadDeploymentsHistory, (RequestRoutingContext cc) => Db.LoadDeploymentHistory(fullDbPath));
            ig.MapRequest(Backend.LoadAllProjects, (RequestRoutingContext cc) => Db.LoadAllProjects(fullDbPath));
            ig.MapRequest(Backend.LoadAllNodes, (RequestRoutingContext cc) => Db.LoadAllNodes(fullDbPath));
            ig.MapRequest(Backend.LoadAllServices, (RequestRoutingContext cc) => Db.LoadAllServices(fullDbPath));
            ig.MapCommand(Backend.ConfirmDeployment, (CommandRoutingContext cc, ConfirmDeploymentInput input) => Db.ConfirmDeployment(fullDbPath, input.Snapshots, input.Configuration));
            ig.MapRequest(Backend.GetAllParameterTypes, (RequestRoutingContext cc) => Db.LoadParameterTypes(fullDbPath));
            ig.MapRequest(Backend.LoadEnvironmentTypes, (RequestRoutingContext cc) => Db.LoadEnvironmentTypes(fullDbPath));
            ig.MapRequest(Backend.LoadHealthStatus, (RequestRoutingContext cc) => Db.LoadFullInfrastructureHealthStatus(fullDbPath));
            ig.MapRequest(Backend.GetInfrastructureName, (RequestRoutingContext cc) => Task.FromResult(infrastructureName));
            ig.MapRequest(Backend.GetAllNoteTypes, (RequestRoutingContext cc) => Db.LoadAllNoteTypes(fullDbPath));
            ig.MapCommand(Backend.StoreHealthStatus, (CommandRoutingContext cc, MachineStatus hs) => Db.StoreHealthStatus(fullDbPath, hs));
            ig.MapCommand(Backend.SaveVersionEnabled, (CommandRoutingContext cc, ProjectVersion version) => Db.SaveVersionEnabled(fullDbPath, version));
            ig.MapCommand(Backend.SaveConfiguration, (CommandRoutingContext cc, InfrastructureConfiguration c) => Db.SaveConfiguration(fullDbPath, c));
            ig.MapRequest(Backend.RefreshBinaries, (RequestRoutingContext cc, List<AlgorithmInfo> algInfo) => Db.RefreshBinaries(fullDbPath, algInfo));
            ig.MapRequest(Backend.LoadIdenticalSnapshot, async (rc, snapshot) => await Db.LoadIdenticalSnapshot(fullDbPath, snapshot));
            ig.MapRequest(MdsCommon.Api.GetAdminCredentials, async (rc) =>
            {
                return new MdsCommon.AdminCredentials()
                {
                    AdminUserName = arguments.AdminUserName,
                    AdminPassword = arguments.AdminPassword
                };
            });

            ig.MapRequest(Backend.GetRemoteBuilds,
                async (rc) =>
                {
                    if (string.IsNullOrEmpty(arguments.BuildManagerUrl))
                        return new List<AlgorithmInfo>();

                    System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();

                    var result = await httpClient.GetAsync($"{arguments.BuildManagerUrl}/ListBinaries");
                    var buildsJson = await result.Content.ReadAsStringAsync();
                    var THEOptions = new System.Text.Json.JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var buildsList = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<MdsCommon.AlgorithmInfo>>(buildsJson, THEOptions);
                    return buildsList;
                });

            ig.MapRequest(MdsCommon.Api.GetAllInfrastructureEvents, async (rc) =>
            {
                return await MdsCommon.Db.LoadAllInfrastructureEvents(fullDbPath);
            });

            ig.MapRequest(MdsCommon.Api.GetMostRecentEventOfService, async (rc, serviceName) =>
            {
                return await MdsCommon.Db.LoadMostRecentInfrastructureEvent(fullDbPath, serviceName);
            });

        }
    }
}
