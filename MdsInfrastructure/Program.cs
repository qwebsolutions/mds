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

                app.MapSignIn();
                var api = app.MapGroup("api");
                api.MapFrontendApi(arguments);

                Register.Everything(webServerRefs);
                return webServerRefs;
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
