using Metapsi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metapsi.Hyperapp;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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


#if DEBUG
            WebServer.WebRootPaths.Add("D:\\qweb\\mes\\Mds\\MdsInfrastructure\\inline");
            WebServer.WebRootPaths.Add("D:\\qweb\\mes\\Metapsi.Hyperapp\\inline");
#endif

            MdsInfrastructureApplication.InputArguments arguments = Mds.ParametersAs<MdsInfrastructureApplication.InputArguments>(parameters);

            var references = MdsInfrastructureApplication.Setup(arguments, start);

            var webServerRefs = references.ApplicationSetup.AddWebServer(
                references.ImplementationGroup, 
                arguments.UiPort, 
                arguments.WebRootPath, 
                AddServices, 
                app =>
            {
                app.UseAuthorization();
                app.UseSwagger();
                app.UseSwaggerUI();
            });
            webServerRefs.AddInfraUi();
            webServerRefs.WebApplication.AddInfraApi(arguments);
            webServerRefs.RegisterStaticFiles(typeof(MdsInfrastructure.MdsInfra).Assembly);
            webServerRefs.RegisterStaticFiles(typeof(MdsCommon.SignIn).Assembly);
            webServerRefs.RegisterStaticFiles(typeof(Metapsi.Hyperapp.SidePanel).Assembly);

            var app = references.ApplicationSetup.Revive();
            webServerRefs.WebApplication.Lifetime.ApplicationStopped.Register(async () =>
            {
                Console.WriteLine("Stop triggered from web app");
                await app.Suspend();
            });
            await app.SuspendComplete;
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

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            builder.Services.AddScoped<DynamicRedirect>();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //builder.Services.AddAuthentication(options =>
            //{
            //    options.AddScheme("public", builder =>
            //    {
            //        builder.
            //    });
            //})
            //.AddJwtBearer(options =>
            //{
            //    options.Authority = $"http://192.168.100.130/auth";
            //})
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.LoginPath = "/signin/credentials";
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.EventsType = typeof(DynamicRedirect);
            });
            //.AddOpenIdConnect(options =>
            //{
            //    options.ClientId = "infrastructure";
            //    options.ClientSecret = "FibgF1vYkYZipQuV2TutW0HbKUNzk9NE";
            //    options.RequireHttpsMetadata = false;
            //    options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
            //    options.Prompt = "Login to ";
            //    //options.Authority = $"http://localhost:8080/auth/realms/FI";
            //    options.Authority = $"http://192.168.100.130/auth/realms/FI";
            //    options.ResponseType = "code";
            //    options.Events.OnMessageReceived = async (c) =>
            //    {

            //    };
            //    options.Events.OnUserInformationReceived = async (c) =>
            //    {

            //    };

            //    options.Events.OnAuthorizationCodeReceived = async (c) =>
            //    {

            //    };

            //    options.Events.OnAuthenticationFailed = async (c) =>
            //    {

            //    };
            //    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
            //    options.NonceCookie.SameSite = SameSiteMode.Lax;
            //    //options.CallbackPath = "/conf/";
            //    //options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
            //    //options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                
            //    // Zitadel
            //    //options.ClientId = "190968356826841091@fi";
            //    //options.ClientSecret = "0F4VQwGFQLSUmzwHgiYhmV5HvbnbpfaQsuW6bhQL7llPJrQlqCm7876Dl7OKSwjR";
            //    //options.RequireHttpsMetadata = false;
            //    //options.Prompt = "Login to ";
            //    ////options.Authority = $"http://192.168.100.130/auth";
            //    //options.Authority = $"http://localhost:8080";
            //    //options.ResponseType = "code";
            //    //options.GetClaimsFromUserInfoEndpoint = true;
            //});

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            });
        }
    }
}
