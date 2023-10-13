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

namespace MdsInfrastructure
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DateTime start = DateTime.UtcNow;

            if (args.Length < 1)
            {
                throw new Exception("Provide input configuration file!");
            }

            Metapsi.Sqlite.Converters.RegisterAll();

            string inputFileName = args[0];
            var inputFullPath = Mds.GetParametersFilePath(inputFileName);
            Console.WriteLine($"Using parameters file {inputFullPath}");
            var parameters = Mds.LoadParameters(inputFullPath);
            Console.WriteLine(Serialize.ToJson(parameters));
            string inputFileFolder = System.IO.Path.GetDirectoryName(inputFullPath);

            MdsCommon.PathParameter.SetRelativeToFolder(inputFileFolder, parameters, nameof(MdsInfrastructureApplication.InputArguments.DbPath));
            MdsCommon.PathParameter.SetRelativeToFolder(inputFileFolder, parameters, nameof(MdsInfrastructureApplication.InputArguments.LogFilePath));
            MdsCommon.PathParameter.SetRelativeToFolder(inputFileFolder, parameters, nameof(MdsInfrastructureApplication.InputArguments.WebRootPath));

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
                nameof(MdsInfrastructureApplication.InputArguments.WebRootPath),
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

            var references = MdsInfrastructureApplication.Setup(arguments, start);

            var webServerRefs = references.ApplicationSetup.AddWebServer(
                references.ImplementationGroup, 
                arguments.UiPort, 
                arguments.WebRootPath, 
                AddServices, 
                app =>
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

                api.MapRequest(Frontend.SaveConfiguration, async (commandContext, httpContext, configuration) =>
                {
                    try
                    {
                        await commandContext.Do(Backend.SaveConfiguration, configuration);
                        return new Frontend.SaveConfigurationResponse() { ResultCode = ApiResultCode.Ok };
                    }
                    catch (Exception ex)
                    {
                        return new Frontend.SaveConfigurationResponse()
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
            //webServerRefs.AddInfraUi();
            webServerRefs.WebApplication.AddInfraApi(arguments);
            webServerRefs.RegisterStaticFiles(typeof(MdsInfrastructure.MdsInfra).Assembly);
            webServerRefs.RegisterStaticFiles(typeof(MdsCommon.Header).Assembly);
            webServerRefs.RegisterStaticFiles(typeof(Metapsi.Hyperapp.HyperNode).Assembly);
            webServerRefs.RegisterStaticFiles(typeof(Metapsi.Syntax.BlockBuilder).Assembly);
            webServerRefs.RegisterStaticFiles(typeof(MdsCommon.HeaderRenderer).Assembly);
            webServerRefs.RegisterStaticFiles(typeof(MdsCommon.Controls.Control).Assembly);
            webServerRefs.RegisterStaticFiles(typeof(Metapsi.Dom.ClickTarget).Assembly);
            webServerRefs.RegisterStaticFiles(typeof(Metapsi.ChoicesJs.Control).Assembly);
            
            {
                var app = references.ApplicationSetup.Revive();
                webServerRefs.WebApplication.Lifetime.ApplicationStopped.Register(async () =>
                {
                    Console.WriteLine("Stop triggered from web app");
                    await app.Suspend();
                });
                await app.SuspendComplete;
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
}
