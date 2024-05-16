using Metapsi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metapsi.Hyperapp;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Web;
using System.Text;
using MdsInfrastructure.Flow;
using Metapsi.Ui;
using MdsCommon;
using System.Diagnostics.Contracts;
using System.Text.Json;
using Metapsi.Heroicons;
using MdsCommon.Controls;
using static Metapsi.Hyperapp.HyperType;
using Microsoft.AspNetCore.Components.Forms;
using System.IO.Compression;
using System.IO;
using System.Reflection;
using System.Net.Http;
using System.Net.Http.Json;

namespace MdsInfrastructure
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            DateTime start = DateTime.UtcNow;

            if (args.Length < 1)
            {
                throw new Exception("Provide input configuration file!");
            }

            string inputFileName = args[0];
            var inputFullPath = Mds.GetParametersFilePath(inputFileName);
            Console.WriteLine($"Using parameters file {inputFullPath}");
            var parameters = Mds.LoadParameters(inputFullPath);
            Console.WriteLine(Serialize.ToJson(parameters));
            string inputFileFolder = System.IO.Path.GetDirectoryName(inputFullPath);

            MdsCommon.PathParameter.SetRelativeToFolder(inputFileFolder, parameters, nameof(MdsInfrastructureApplication.InputArguments.DbPath));
            MdsCommon.PathParameter.SetRelativeToFolder(inputFileFolder, parameters, nameof(MdsInfrastructureApplication.InputArguments.LogFilePath));

            await Mds.ValidateMissingParameters(parameters, new List<string>()
            {
                nameof(MdsInfrastructureApplication.InputArguments.BinariesAvailableInputChannel),
                nameof(MdsInfrastructureApplication.InputArguments.BroadcastDeploymentOutputChannel),
                nameof(MdsInfrastructureApplication.InputArguments.BuildManagerRedisUrl),
                nameof(MdsInfrastructureApplication.InputArguments.BuildManagerUrl),
                nameof(MdsInfrastructureApplication.InputArguments.DbPath),
                nameof(MdsInfrastructureApplication.InputArguments.HealthStatusInputChannel),
                nameof(MdsInfrastructureApplication.InputArguments.InfrastructureApiPort),
                nameof(MdsInfrastructureApplication.InputArguments.InfrastructureEventsInputChannel),
                nameof(MdsInfrastructureApplication.InputArguments.NodeCommandOutputChannel),
                nameof(MdsInfrastructureApplication.InputArguments.InfrastructureName),
                nameof(MdsInfrastructureApplication.InputArguments.UiPort),
                nameof(MdsInfrastructureApplication.InputArguments.LogFilePath)
            },
            async (logMessage) =>
            {
                Console.Error.WriteLine(logMessage);

                if (parameters.ContainsKey(nameof(MdsInfrastructureApplication.InputArguments.LogFilePath)))
                {
                    await Mds.LogToServiceText(parameters[nameof(MdsInfrastructureApplication.InputArguments.LogFilePath)], start, logMessage);
                }
            });

            MdsInfrastructureApplication.InputArguments arguments = Mds.ParametersAs<MdsInfrastructureApplication.InputArguments>(parameters);

            var webServerRefs = await SetupGlobalController(arguments, start);

            var app = webServerRefs.ApplicationSetup.Revive();
            webServerRefs.WebApplication.Lifetime.ApplicationStopping.Register(async () =>
            {
                Console.WriteLine("Shutdown started");
                await Task.Delay(10000);
            });
            webServerRefs.WebApplication.Lifetime.ApplicationStopped.Register(async () =>
            {
                Console.WriteLine("Stop triggered from web app");
                await app.Suspend();
            });

            await app.SuspendComplete;
        }

        public static async Task<WebServer.References> SetupGlobalController(MdsInfrastructureApplication.InputArguments arguments, DateTime start)
        {
            Metapsi.Sqlite.Converters.RegisterAll();

            HttpClient httpClient = new HttpClient();

            await Migrate.All(arguments.DbPath);

            var references = MdsInfrastructureApplication.Setup(arguments, start);

            var webServerRefs = references.ApplicationSetup.AddWebServer(
                references.ImplementationGroup,
                arguments.UiPort,
                buildServices: AddServices,
                buildApp: app =>
                {
                    //app.UseAuthorization();
                    app.UseSwagger();
                    app.UseSwaggerUI();
                });

            {
                var app = webServerRefs.WebApplication;

                // Redirect to default page
                app.MapGet("/", () => Results.Redirect(WebServer.Url<Routes.Status.Infra>())).AllowAnonymous().ExcludeFromDescription();

                var api = app.MapGroup("api");

                api.MapRequest(Frontend.SaveConfiguration, async (commandContext, httpContext, input) =>
                {
                    try
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
                            return new SaveConfigurationResponse() { ResultCode = ApiResultCode.Ok };
                        }
                        else
                        {
                            return new SaveConfigurationResponse()
                            {
                                ConflictMessages = Conflict.BuildDiffMessages(sneakyEdits)
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        return new SaveConfigurationResponse()
                        {
                            ResultCode = ApiResultCode.Error,
                            ErrorMessage = ex.Message
                        };
                    }
                },
                WebServer.Authorization.Require);

                api.MapRequest(Frontend.MergeConfiguration, async (commandContext, httpContext, input) =>
                {
                    try
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
                    }
                    catch (Exception ex)
                    {
                        return new MergeConfigurationResponse()
                        {
                            ResultCode = ApiResultCode.Error,
                            ErrorMessage = ex.Message
                        };
                    }
                },
                WebServer.Authorization.Require);

                api.MapRequest(Frontend.ConfirmDeployment, async (commandContext, httpContext, configurationId) =>
                {
                    try
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

                        return new Frontend.ConfirmDeploymentResponse() { ResultCode = ApiResultCode.Ok };
                    }
                    catch (Exception ex)
                    {
                        return new Frontend.ConfirmDeploymentResponse()
                        {
                            ResultCode = ApiResultCode.Error,
                            ErrorMessage = ex.Message
                        };
                    }
                }, WebServer.Authorization.Require);

                api.MapRequest(Frontend.GetConfigurationJson, async (CommandContext commandContext, HttpContext httpContext, InfrastructureConfiguration edited) =>
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
                },
                WebServer.Authorization.Require);


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

                api.MapRequest(
                    Frontend.RemoveBuilds,
                    async (CommandContext commandContext, HttpContext httpContext, RemoveBuildsRequest input) =>
                    {
                        try
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

                            if(result.StatusCode == HttpStatusCode.NotFound)
                            {
                                return new RemoveBuildsResponse()
                                {
                                    ErrorMessage = "Cannot delete, repository does not support this operation",
                                    ResultCode = ApiResultCode.Error
                                };
                            }

                            result.EnsureSuccessStatusCode();

                            input.ToRemove.ForEach(x => x.Removed = true);

                            return new RemoveBuildsResponse()
                            {
                                Removed = input.ToRemove
                            };
                        }
                        catch (Exception ex)
                        {
                            commandContext.Logger.LogException(ex);
                            return new RemoveBuildsResponse()
                            {
                                ErrorMessage = "Cannot delete, an error has occured",
                                ResultCode = ApiResultCode.Error
                            };
                        }
                    }, 
                    WebServer.Authorization.Require);

                api.MapRequest(
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
                    }, WebServer.Authorization.Require);

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

                app.MapGet("/signin-redirect", (HttpContext httpContext) =>
                {
                    var rootPath = httpContext.GetHostedRootPath();
                    //return Results.Redirect(rootPath + WebServer.Path(state.DefaultPage.Method, state.DefaultParameter));
                    return Results.Redirect("/Status/Infra");
                }).RequireAuthorization().ExcludeFromDescription();

                app.MapGet("/signout", (HttpContext httpContext) =>
                {
                    var rootPath = httpContext.GetHostedRootPath();
                    foreach (var cookie in httpContext.Request.Cookies)
                    {
                        httpContext.Response.Cookies.Delete(cookie.Key);
                    }
                    httpContext.Response.Redirect(httpContext.GetHostedRootPath() + "/");
                }).RequireAuthorization().ExcludeFromDescription();
            }


            Register.Everything(webServerRefs);
            webServerRefs.WebApplication.AddInfraApi(arguments);
            return webServerRefs;
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

        public static void AddActiveDirectory(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = options.DefaultPolicy;
            });
        }


        public class DynamicRedirect : CookieAuthenticationEvents
        {
            public override async Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
            {
                if(!string.IsNullOrEmpty(context.HttpContext.GetHostedRootPath()))
                {
                    if(context.RedirectUri.Contains("?ReturnUrl=%2"))
                    {
                        string encodedRootPath = HttpUtility.UrlEncode(context.HttpContext.GetHostedRootPath());
                        context.RedirectUri = context.RedirectUri.Replace("?ReturnUrl=%2", $"?ReturnUrl={encodedRootPath}%2");
                        context.RedirectUri = context.RedirectUri.Replace($"{context.HttpContext.Request.Headers.Host}/", $"{context.HttpContext.Request.Headers.Host + context.HttpContext.GetRelativeRootPath()}/");
                    }
                }

                await base.RedirectToLogin(context);
            }
        }

        public static void AddServices(WebApplicationBuilder builder)
        {
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            builder.Services.AddScoped<DynamicRedirect>();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                    options.LoginPath = "/signin";
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.EventsType = typeof(DynamicRedirect);
                });

            builder.Services.AddAuthorization(options =>
            {
                //options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            });

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.AllowedHosts.Clear();
            });

            builder.Services.AddHttpLogging(options =>
            {
                options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders;
            });

            //builder.Services.AddScoped<DynamicRedirect>();

            //builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //.AddCookie(options =>
            //{
            //    options.ExpireTimeSpan = TimeSpan.FromDays(1);
            //    options.LoginPath = "/signin/credentials";
            //    options.Cookie.SameSite = SameSiteMode.Lax;
            //    options.EventsType = typeof(DynamicRedirect);
            //});

            //builder.Services.AddAuthorization(options =>
            //{
            //    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            //});
        }
    }

    public class MergeResult
    {
        public List<string> ConflictMessages { get; set; } = new();
        public List<string> SuccessMessages { get; set; } = new();
        public InfrastructureConfiguration ResultConfiguration { get; set; }
    }

    public class ConfigurationChanges
    {
        public Diff.CollectionChanges<InfrastructureService> ServiceChanges { get; set; }
        public Diff.CollectionChanges<Application> ApplicationChanges { get; set; }
        public Diff.CollectionChanges<InfrastructureVariable> VariableChanges { get; set; }
    }

    public static class Conflict
    {
        public static bool Any(this ConfigurationChanges changes)
        {
            if (changes.ServiceChanges.Any())
                return true;

            if (changes.ApplicationChanges.Any())
                return true;

            if (changes.VariableChanges.Any())
                return true;

            return false;
        }

        public static List<InfrastructureService> AddedServices(this ConfigurationChanges changes)
        {
            return changes.ServiceChanges.JustInSecond;
        }

        public static List<InfrastructureService> RemovedServices(this ConfigurationChanges changes)
        {
            return changes.ServiceChanges.JustInFirst;
        }

        public static List<Application> AddedApplications(this ConfigurationChanges changes)
        {
            return changes.ApplicationChanges.JustInSecond;
        }

        public static List<Application> RemovedApplications(this ConfigurationChanges changes)
        {
            return changes.ApplicationChanges.JustInFirst;
        }

        public static List<InfrastructureVariable> AddedVariables(this ConfigurationChanges changes)
        {
            return changes.VariableChanges.JustInSecond;
        }

        public static List<InfrastructureVariable> RemovedVariables(this ConfigurationChanges changes)
        {
            return changes.VariableChanges.JustInFirst;
        }

        public static ConfigurationChanges GetConfigurationChanges(InfrastructureConfiguration oldConfiguration, InfrastructureConfiguration newConfiguration)
        {
            return new ConfigurationChanges()
            {
                ServiceChanges = Diff.CollectionsByKey(oldConfiguration.InfrastructureServices, newConfiguration.InfrastructureServices, x => x.Id),
                ApplicationChanges = Diff.CollectionsByKey(oldConfiguration.Applications, newConfiguration.Applications, x => x.Id),
                VariableChanges = Diff.CollectionsByKey(oldConfiguration.InfrastructureVariables, newConfiguration.InfrastructureVariables, x => x.Id)
            };
        }

        public static List<string> BuildDiffMessages<T>(Diff.CollectionChanges<T> diff, Func<T, string> getName, string entityType)
        {
            // First is source version of current edit
            // Second is saved version

            List<string> messages = new List<string>();
            foreach (var item in diff.JustInFirst)
            {
                messages.Add($"{entityType} {getName(item)} removed");
            }

            foreach (var item in diff.JustInSecond)
            {
                messages.Add($"{entityType} {getName(item)} added");
            }

            foreach (var item in diff.Different)
            {
                messages.Add($"{entityType} {getName(item.InFirst)} edited");
            }

            return messages;
        }

        public static List<string> BuildDiffMessages(ConfigurationChanges configurationChanges)
        {
            List<string> messages = new List<string>();
            messages.AddRange(BuildDiffMessages(configurationChanges.ServiceChanges, x => x.ServiceName, "Service"));
            messages.AddRange(BuildDiffMessages(configurationChanges.ApplicationChanges, x => x.Name, "Application"));
            messages.AddRange(BuildDiffMessages(configurationChanges.VariableChanges, x => x.VariableName, "Variable"));
            return messages;
        }

        public static MergeResult Merge(InfrastructureConfiguration saved, InfrastructureConfiguration source, InfrastructureConfiguration edited)
        {
            MergeResult mergeResult = new MergeResult()
            {
                ResultConfiguration = Metapsi.Serialize.FromJson<InfrastructureConfiguration>(Metapsi.Serialize.ToJson(edited))
            };
            
            // Source is older than saved

            var bySomeoneElse = GetConfigurationChanges(source, saved);
            var byMe = GetConfigurationChanges(source, edited);

            // Their changes are merged into my edited configuration

            // They removed a service
            foreach (var removedService in bySomeoneElse.RemovedServices())
            {
                // If they removed a service that I edited, I probably want to keep it
                var isEditedByMe = byMe.ServiceChanges.Different.Any(x => x.InFirst.Id == removedService.Id);
                if (isEditedByMe)
                {
                    mergeResult.ConflictMessages.Add($"Service {removedService.ServiceName} conflict");
                }
                else
                {
                    // Keep their edits & inform
                    mergeResult.ResultConfiguration.InfrastructureServices.RemoveAll(x => x.Id == removedService.Id);
                    mergeResult.SuccessMessages.Add($"Service {removedService.ServiceName} removed");
                }
            }

            // They added a service
            foreach (var addedService in bySomeoneElse.AddedServices())
            {
                // There's a slight chance that we added a service with the same name
                // The ID cannot be the same, so we search by name
                var alsoAddedByMe = byMe.AddedServices().Any(x => x.ServiceName == addedService.ServiceName);
                if (alsoAddedByMe)
                {
                    mergeResult.ConflictMessages.Add($"Service {addedService.ServiceName} conflict");
                }
                else
                {
                    // Keep their edits & inform
                    mergeResult.ResultConfiguration.InfrastructureServices.Add(addedService);
                    mergeResult.ResultConfiguration.InfrastructureServices = mergeResult.ResultConfiguration.InfrastructureServices.OrderBy(x => x.ServiceName).ToList();
                    mergeResult.SuccessMessages.Add($"Service {addedService.ServiceName} merged");
                }
            }

            // They edited a service
            foreach (var editedService in bySomeoneElse.ServiceChanges.Different)
            {
                // If they edited a service that I removed, they probably need it
                if (byMe.RemovedServices().Any(x => x.Id == editedService.InFirst.Id))
                {
                    mergeResult.ConflictMessages.Add($"Service {editedService.InFirst.ServiceName} conflict");
                }
                else
                {
                    // If they edited a service that I also edited, it's a conflict if the edits are not exactly the same
                    var myEdit = byMe.ServiceChanges.Different.SingleOrDefault(x => x.InSecond.Id == editedService.InSecond.Id);

                    if (myEdit != null)
                    {
                        var theirJson = Metapsi.Serialize.ToJson(editedService.InSecond);
                        var myJson = Metapsi.Serialize.ToJson(myEdit.InSecond);

                        if (theirJson != myJson)
                        {
                            mergeResult.ConflictMessages.Add($"Service {editedService.InFirst.ServiceName} conflict");
                        }
                    }
                    else
                    {
                        // If they edited a service that I did not touch, merge their edit into my configuration
                        if (byMe.ServiceChanges.Common.Any(x => x.Id == editedService.InFirst.Id))
                        {
                            mergeResult.ResultConfiguration.InfrastructureServices.RemoveAll(x => x.Id == editedService.InSecond.Id);
                            mergeResult.ResultConfiguration.InfrastructureServices.Add(editedService.InSecond);
                            mergeResult.ResultConfiguration.InfrastructureServices = mergeResult.ResultConfiguration.InfrastructureServices.OrderBy(x => x.ServiceName).ToList();
                            mergeResult.SuccessMessages.Add($"Service {editedService.InSecond.ServiceName} merged");
                        }
                    }
                }
            }

            foreach (var addedApplication in bySomeoneElse.AddedApplications())
            {
                // There's a slight chance that we added an application with the same name
                // The ID cannot be the same, so we search by name
                var alsoAddedByMe = byMe.AddedApplications().Any(x => x.Name == addedApplication.Name);
                if (alsoAddedByMe)
                {
                    // Application has no property other than name
                    // If the same application name was added, keep the saved one & update all references
                    var mine = byMe.AddedApplications().Single(x => x.Name == addedApplication.Name);
                    foreach (var service in mergeResult.ResultConfiguration.InfrastructureServices)
                    {
                        if (service.ApplicationId == mine.Id)
                        {
                            service.ApplicationId = addedApplication.Id;
                        }
                    }
                    mine.Id = addedApplication.Id;

                    mergeResult.SuccessMessages.Add($"Application {addedApplication.Name} merged");
                }
                else
                {
                    // Keep their edits & inform
                    mergeResult.ResultConfiguration.Applications.Add(addedApplication);
                    mergeResult.ResultConfiguration.Applications = mergeResult.ResultConfiguration.Applications.OrderBy(x => x.Name).ToList();
                    mergeResult.SuccessMessages.Add($"Application {addedApplication.Name} merged");
                }
            }

            foreach (var changedApplication in bySomeoneElse.ApplicationChanges.Different)
            {
                // If they changed the name of the application and I also changed it, it's a conflict
                var alsoChangedByMe = byMe.ApplicationChanges.Different.SingleOrDefault(x => x.InFirst.Id == changedApplication.InFirst.Id);
                if (alsoChangedByMe != null)
                {
                    mergeResult.ConflictMessages.Add($"Application {alsoChangedByMe.InFirst.Name} conflict");
                }
                else
                {
                    // If I didn't touch it just keep their version
                    mergeResult.ResultConfiguration.Applications.RemoveAll(x => x.Id == changedApplication.InFirst.Id);
                    mergeResult.ResultConfiguration.Applications.Add(changedApplication.InSecond);
                    mergeResult.ResultConfiguration.Applications = mergeResult.ResultConfiguration.Applications.OrderBy(x => x.Name).ToList();
                    mergeResult.SuccessMessages.Add($"Application {changedApplication.InSecond.Name} merged");
                }
            }

            foreach (var removedApplication in bySomeoneElse.RemovedApplications())
            {
                // The services that point to the removed application must be already in conflict 
                // as they now use a different application ID

                // If they removed an application that I edited it's a conflict
                var changedByMe = byMe.ApplicationChanges.Different.Any(x => x.InFirst.Id == removedApplication.Id);
                if (changedByMe)
                {
                    mergeResult.ConflictMessages.Add($"Application {removedApplication.Name} conflict");
                }
                else
                {
                    // I didn't touch that application, remove it
                    mergeResult.ResultConfiguration.Applications.RemoveAll(x => x.Id == removedApplication.Id);
                    mergeResult.SuccessMessages.Add($"Application {removedApplication.Name} removed");
                }
            }

            foreach (var addedVariable in bySomeoneElse.AddedVariables())
            {
                // There's a slight chance that we added a variable with the same name
                // The ID cannot be the same, so we search by name
                var alsoAddedByMe = byMe.AddedVariables().Any(x => x.VariableName == addedVariable.VariableName);
                if (alsoAddedByMe)
                {
                    // If the variable has the same name and same value, keep the saved one and update references

                    var mine = byMe.AddedVariables().Single(x => x.VariableName == addedVariable.VariableName);

                    if (addedVariable.VariableValue == mine.VariableValue)
                    {
                        foreach (var boundParameter in
                            mergeResult.ResultConfiguration.InfrastructureServices.SelectMany(
                                x => x.InfrastructureServiceParameterDeclarations).SelectMany(
                                x => x.InfrastructureServiceParameterBindings))
                        {
                            if (boundParameter.InfrastructureVariableId == mine.Id)
                            {
                                boundParameter.InfrastructureVariableId = addedVariable.Id;
                            }
                        }
                        mine.Id = addedVariable.Id;

                        mergeResult.SuccessMessages.Add($"Variable {addedVariable.VariableName} merged");
                    }
                    else
                    {
                        // If they have different values it's a conflict
                        mergeResult.ConflictMessages.Add($"Variable {addedVariable.VariableName} conflict");
                    }
                }
                else
                {
                    // Keep their edits & inform
                    mergeResult.ResultConfiguration.InfrastructureVariables.Add(addedVariable);
                    mergeResult.ResultConfiguration.InfrastructureVariables = mergeResult.ResultConfiguration.InfrastructureVariables.OrderBy(x => x.VariableValue).ToList();
                    mergeResult.SuccessMessages.Add($"Variable  {addedVariable.VariableName} merged");
                }
            }

            foreach (var changedVariable in bySomeoneElse.VariableChanges.Different)
            {
                // If we both changed the variable value, it's a conflict if the values are different
                var mine = byMe.VariableChanges.Different.SingleOrDefault(x => x.InSecond.Id == changedVariable.InSecond.Id);
                if (mine != null)
                {
                    if (changedVariable.InSecond.VariableValue != mine.InSecond.VariableValue)
                    {
                        mergeResult.ConflictMessages.Add($"Variable {mine.InSecond.VariableName} conflict");
                        // If the changed value is the same there's nothing to merge
                    }
                }
                else
                {
                    // If the changed variable is removed by me, they probably need it
                    if (byMe.RemovedVariables().Any(x => x.Id == changedVariable.InFirst.Id))
                    {
                        mergeResult.ConflictMessages.Add($"Variable {mine.InSecond.VariableName} conflict");
                    }
                    else
                    {
                        // They changed a variable that I didn't touch, so merge it

                        mergeResult.ResultConfiguration.InfrastructureVariables.RemoveAll(x => x.Id == changedVariable.InFirst.Id);
                        mergeResult.ResultConfiguration.InfrastructureVariables.Add(changedVariable.InSecond);
                        mergeResult.ResultConfiguration.InfrastructureVariables = mergeResult.ResultConfiguration.InfrastructureVariables.OrderBy(x => x.VariableName).ToList();
                        mergeResult.SuccessMessages.Add($"Variable {changedVariable.InSecond.VariableName} merged");
                    }
                }
            }

            foreach (var removedVariable in bySomeoneElse.RemovedVariables())
            {
                // If they removed a variable that I edited, I probably need it
                if (byMe.VariableChanges.Different.Any(x => x.InSecond.Id == removedVariable.Id))
                {
                    mergeResult.ConflictMessages.Add($"Variable {removedVariable.VariableName} conflict");
                }
                else
                {
                    // If they removed a variable that I didn't touch, remove it for me as well
                    mergeResult.ResultConfiguration.InfrastructureVariables.RemoveAll(x => x.Id == removedVariable.Id);
                    mergeResult.SuccessMessages.Add($"Variable {removedVariable.VariableName} removed");
                }
            }

            return mergeResult;
        }

        public static InfrastructureConfiguration FixReferencedIds(InfrastructureConfiguration saved, InfrastructureConfiguration edited)
        {
            return edited;
        }
    }
}
