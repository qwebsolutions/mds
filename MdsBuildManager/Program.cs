using Microsoft.Extensions.DependencyInjection;
using System;
using Metapsi;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using MdsCommon;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Builder;
using Metapsi.Sqlite;

namespace MdsBuildManager
{
    public class Program
    {
        // I'm tired
        public static ServiceDoc.DbQueue DbQueue { get; set; }

        public static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Provide input configuration file!");
                return;
            }


            InputArguments inputArguments = InputArguments.GetInputArguments(Environment.GetCommandLineArgs()[1]);
            var refs = await StartBuildController(args, inputArguments);

            var app = refs.ApplicationSetup.Revive();
            //Task.Run(() =>
            //{
            //    refs.WebApplication.Run();
            //});
            

            //var stopToken = new System.Threading.CancellationToken();
            //var shutdownTask = refs.WebApplication.RunAsync(stopToken);

            //shutdownTask.ContinueWith(async (t) => app.Suspend());
            ////await BindStop(refs, app);

            await app.SuspendComplete;
        }

        public class BuildControllerReferences
        {
            public ApplicationSetup ApplicationSetup { get; set; }
            public Microsoft.AspNetCore.Builder.WebApplication WebApplication { get; set; }
        }

        public static async Task<BuildControllerReferences> StartBuildController(string[] args, InputArguments inputArguments)
        {
            var setup = Metapsi.ApplicationBuilder.New();
            var ig = setup.AddImplementationGroup();


            var dbPath = System.IO.Path.Combine(inputArguments.BinariesFolder, "MdsBuildManager.db");

            Program.DbQueue = await HashHelper.GetDbQueue(dbPath);

            HashHelper hashHelper = new HashHelper(inputArguments);
            var azureQueue = setup.AddBusinessState(new AzureBuilds.State());
            setup.MapEvent<ApplicationRevived>(e =>
            {
                e.Using(azureQueue, ig).EnqueueCommand(async (cc, state) => await AzureBuilds.CheckForever(cc, inputArguments, hashHelper, Program.DbQueue.SqliteQueue));
            });

            var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);
            builder.AddMetapsi(setup, ig);
            var services = builder.Services;
            services.AddOptions();
            services.AddMvcCore();


            services.AddSingleton(inputArguments);
            services.AddSingleton<HashHelper>();

            var assembly = typeof(Program).Assembly;
            // This creates an AssemblyPart, but does not create any related parts for items such as views.
            var part = new AssemblyPart(assembly);
            services.AddControllersWithViews()
                .ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(part));

            var app = builder.Build();
            app.UseMetapsi(setup);
            app.Urls.Add(inputArguments.ListeningUrl);

            // Call Configure method
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            await app.UseDocs(Program.DbQueue,
                b =>
                {
                    b.SetOverviewUrl("config");
                    b.AddDoc<InfrastructureController>(x => x.Name);
                });

            var eventsEndpoint = app.MapGroup("event");

            eventsEndpoint.OnMessage<InfrastructureControllerStarted>(async (message) =>
            {
                var knownInfrastructure = await DbQueue.GetDocument<InfrastructureController>(message.InfrastructureName);
                if (knownInfrastructure == null)
                {
                    await DbQueue.SaveDocument(new InfrastructureController()
                    {
                        Name = message.InfrastructureName,
                        InternalBaseUrl = message.InternalBaseUrl
                    });
                }
            });

            setup.SetupMessagingApi(ig);

            //setup.MapEvent<ApplicationRevived>(e =>
            //{
            //    app.Run();
            //});

            //var webService = setup.AddBusinessState(new WebService.State() { Args = args, InputArguments = inputArguments });

            var redisNotifier = setup.AddBusinessState(new RedisNotifier.State());

            MailSender.State mailSender = null;

            if (!string.IsNullOrEmpty(inputArguments.FromMailAddress))
            {
                mailSender = setup.AddMailSender(inputArguments.SmtpHostName, inputArguments.FromMailAddress, inputArguments.SenderMailPassword);
            }

            var binariesNotifierState = setup.AddBusinessState(new object());

            BinariesPublishData binariesPublishData = new BinariesPublishData();

            void NotifyBinariesAvailable<TEvent>(EventContext<TEvent> e)
                where TEvent: IData
            {
                if (!string.IsNullOrWhiteSpace(inputArguments.BinariesAvailableOutputChannel))
                {
                    if (binariesPublishData.NewBinaries.Any())
                    {
                        e.Using(redisNotifier).EnqueueCommand(
                            RedisNotifier.NotifyChannel,
                            new RedisChannelMessage(
                            inputArguments.BinariesAvailableOutputChannel,
                            "BinariesAvailable",
                            string.Empty));
                    }
                }

                e.Using(binariesNotifierState, ig).EnqueueCommand(async (cc, state) =>
                {
                    var allControllers = await DbQueue.ListDocuments<InfrastructureController>();
                    foreach (var controller in allControllers)
                    {
                        cc.NotifyUrl(controller.InternalBaseUrl.TrimEnd('/') + "/api/event", new BinariesAvailable());
                    }
                });

                if (mailSender != null)
                {
                    if (binariesPublishData.DuplicateBinaries.Any() || binariesPublishData.NewBinaries.Any())
                    {
                        List<string> newBinaries = new List<string>();

                        foreach (var nb in binariesPublishData.NewBinaries)
                        {
                            newBinaries.Add($"New binaries: {nb.BinaryPath}, version: {nb.NewVersion}, commit: {nb.NewCommitSha}");
                        }

                        List<string> droppedBinaries = new List<string>();

                        foreach (var db in binariesPublishData.DuplicateBinaries)
                        {
                            droppedBinaries.Add($"Dropped: {db.NewVersion}/{db.NewCommitSha}, identical to {db.PreviousVersion}/{db.PreviousCommitSha}");
                        }

                        string body = string.Join("\n", newBinaries) + "\n" + string.Join("\n", droppedBinaries);

                        e.Using(mailSender, ig).EnqueueCommand(MailSender.Send, new MailSender.Mail()
                        {
                            Subject = "Binaries sync status",
                            ToAddresses = inputArguments.ToMailAddresses,
                            Body = body
                        });
                    }
                }

                binariesPublishData.DuplicateBinaries.Clear();
                binariesPublishData.NewBinaries.Clear();
            }

            //setup.MapEvent<ApplicationRevived>(e =>
            //{
            //    e.Using(webService, ig).EnqueueCommand(WebService.StartListening);
            //});

            setup.MapEvent<DuplicateBinaries>(e => binariesPublishData.DuplicateBinaries.Add(e.EventData));
            setup.MapEvent<NewBinaries>(e => binariesPublishData.NewBinaries.Add(e.EventData));

            setup.MapEvent<PollingComplete>(e =>
            {
                NotifyBinariesAvailable(e);
            });

            setup.MapEvent<UploadComplete>(e =>
            {
                NotifyBinariesAvailable(e);
            });

            return new BuildControllerReferences()
            {
                ApplicationSetup = setup,
                WebApplication = app
            };
        }

        public class BinariesPublishData
        {
            public List<NewBinaries> NewBinaries { get; set; } = new List<NewBinaries>();
            public List<DuplicateBinaries> DuplicateBinaries { get; set; } = new List<DuplicateBinaries>();
        }

        /// <summary>
        /// On blazor task stop, stop application also
        /// </summary>
        /// <param name="references"></param>
        /// <param name="application"></param>
        /// <returns></returns>
        public static async Task BindStop(BuildControllerReferences references, Application application)
        {
            //Task webHostTask = null;
            //while (true)
            //{
            //    await Task.Delay(200);
            //    if (references.WebService != null)
            //        if (references.WebService.RunningServiceTask != null)
            //        {
            //            webHostTask = references.WebService.RunningServiceTask;
            //            break;
            //        }
            //}

            //var _ = webHostTask.ContinueWith(async (_) => application.Suspend());
        }
    }

    //public static class WebService
    //{
    //    public class State
    //    {
    //        public string[] Args { get; set; }
    //        public InputArguments InputArguments { get; set; }
    //        public IHost Host { get; set; }
    //        public Task RunningServiceTask { get; set; }
    //    }


    //    public static async Task StartListening(CommandContext commandContext, State state)
    //    {
    //        state.Host = CreateHostBuilder(state.Args, state.InputArguments, commandContext).Build();
    //        state.RunningServiceTask = state.Host.RunAsync();
    //    }

    //    public static IHostBuilder CreateHostBuilder(string[] args, InputArguments inputArguments, CommandContext commandContext) =>
    //        Host.CreateDefaultBuilder(args)
    //        .UseSystemd()
    //        .UseWindowsService()
    //        .ConfigureWebHostDefaults(webBuilder =>
    //        {
    //            // I still don't understand how to use this thing
    //            webBuilder.ConfigureServices(services => services.AddSingleton(typeof(InputArguments), inputArguments));
    //            webBuilder.ConfigureServices(services => services.AddSingleton(typeof(CommandContext), commandContext));
    //            webBuilder.UseStartup<Startup>();
    //            webBuilder.UseUrls(inputArguments.ListeningUrl);
    //        });
    //}

    public class NewBinaries : IData
    {
        public string BinaryPath { get; set; }
        public string NewVersion { get; set; }
        public string NewCommitSha { get; set; }
    }

    public class DuplicateBinaries : IData
    {
        public string Base64Hash { get; set; }
        public string BinaryPath { get; set; }
        public string NewVersion { get; set; }
        public string NewCommitSha { get; set; }
        public string PreviousVersion { get; set; }
        public string PreviousCommitSha { get; set; }
    }

    public class PollingComplete : IData
    {

    }

    public class UploadComplete : IData
    {

    }
}
