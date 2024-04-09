using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using Metapsi;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace MdsBuildManager
{
    public class Program
    {
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

            await BindStop(refs, app);

            await app.SuspendComplete;
        }

        public class BuildControllerReferences
        {
            public ApplicationSetup ApplicationSetup { get; set; }
            public WebService.State WebService { get; set; }
        }

        public static async Task<BuildControllerReferences> StartBuildController(string[] args, InputArguments inputArguments)
        {
            var setup = ApplicationBuilder.New();
            var ig = setup.AddImplementationGroup();
            var webService = setup.AddBusinessState(new WebService.State() { Args = args, InputArguments = inputArguments });
            var redisNotifier = setup.AddBusinessState(new RedisNotifier.State());

            MailSender.State mailSender = null;

            if (!string.IsNullOrEmpty(inputArguments.FromMailAddress))
            {
                mailSender = setup.AddMailSender(inputArguments.SmtpHostName, inputArguments.FromMailAddress, inputArguments.SenderMailPassword);
            }

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

            setup.MapEvent<ApplicationRevived>(e =>
            {
                e.Using(webService, ig).EnqueueCommand(WebService.StartListening);
            });

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
                WebService = webService
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
            Task webHostTask = null;
            while (true)
            {
                await Task.Delay(200);
                if (references.WebService != null)
                    if (references.WebService.RunningServiceTask != null)
                    {
                        webHostTask = references.WebService.RunningServiceTask;
                        break;
                    }
            }

            var _ = webHostTask.ContinueWith(async (_) => application.Suspend());
        }
    }

    public static class WebService
    {
        public class State
        {
            public string[] Args { get; set; }
            public InputArguments InputArguments { get; set; }
            public IHost Host { get; set; }
            public Task RunningServiceTask { get; set; }
        }


        public static async Task StartListening(CommandContext commandContext, State state)
        {
            state.Host = CreateHostBuilder(state.Args, state.InputArguments, commandContext).Build();
            state.RunningServiceTask = state.Host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, InputArguments inputArguments, CommandContext commandContext) =>
            Host.CreateDefaultBuilder(args)
            .UseSystemd()
            .UseWindowsService()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                // I still don't understand how to use this thing
                webBuilder.ConfigureServices(services => services.AddSingleton(typeof(InputArguments), inputArguments));
                webBuilder.ConfigureServices(services => services.AddSingleton(typeof(CommandContext), commandContext));
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls(inputArguments.ListeningUrl);
            });
    }

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
