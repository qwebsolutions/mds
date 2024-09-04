using Metapsi;

namespace MdsLocal
{
    public static partial class MdsLocalApplication
    {
        public static MdsLocalApplication.State SetupLocalApp(
            ApplicationSetup applicationSetup,
            ImplementationGroup implementationGroup,
            MdsCommon.InfrastructureNodeSettings infrastructureConfiguration,
            EventBroadcaster.State eventBroadcaster,
            string servicesBasePath,
            string nodeName,
            string baseDataFolder,
            System.DateTime startTimestamp)
        {
            const string CheckServiceDbErrors = "CheckServiceDbErrors";

            var mdsLocalApplication = applicationSetup.AddBusinessState(new MdsLocalApplication.State()
            {
                ServicesBasePath = servicesBasePath,
                NodeName = nodeName,
                InfrastructureConfiguration = infrastructureConfiguration,
                BaseDataFolder = baseDataFolder
            });

            var commandsListener = applicationSetup.AddBusinessState(new RedisListener.State());

            var logPollTimer = applicationSetup.AddBusinessState(new Timer.State());

            applicationSetup.MapEvent<ApplicationRevived>(
                e =>
                {
                    e.Using(mdsLocalApplication, implementationGroup).EnqueueCommand(MdsLocalApplication.GetStartupInfrastructureConfiguration);
                });

            applicationSetup.MapEvent<MdsLocalApplication.Event.GlobalControllerReached>(
                e =>
                {
                    e.Using(mdsLocalApplication, implementationGroup).EnqueueCommand(MdsLocalApplication.ValidateSchema);
                    e.Using(mdsLocalApplication, implementationGroup).EnqueueCommand(MdsLocalApplication.AttachToProcesses);
                    e.Using(mdsLocalApplication, implementationGroup).EnqueueCommand(MdsLocalApplication.SynchronizeConfiguration, "Local restart");

                    // Listen for commands on own node channel
                    e.Using(commandsListener, implementationGroup).EnqueueCommand(
                        RedisListener.StartListening,
                        new Metapsi.RedisChannel(infrastructureConfiguration.NodeCommandInputChannel));

                    e.Using(logPollTimer, implementationGroup).EnqueueCommand(Timer.SetTimer, new Timer.Command.Set()
                    {
                        Name = CheckServiceDbErrors,
                        IntervalMilliseconds = 1000 * 10
                    });
                });

            applicationSetup.MapEventIf<RedisListener.Event.NotificationReceived>(
                e => e.ChannelName == infrastructureConfiguration.NodeCommandInputChannel,
                e => e.Using(mdsLocalApplication, implementationGroup).EnqueueCommand(MdsLocalApplication.SendCommand, nodeName, e.EventData.Payload));


            applicationSetup.MapEventIf<Timer.Event.Tick>(
                e => e.Name == CheckServiceDbErrors,
                e => e.Using(mdsLocalApplication, implementationGroup).EnqueueCommand(MdsLocalApplication.CheckAllServiceDbs));


            applicationSetup.MapEvent<UnstableServiceDropped>(
                e =>
                {
                    var infrastructureEvent = new MdsCommon.InfrastructureEvent()
                    {
                        Timestamp = System.DateTime.UtcNow,
                        Type = MdsCommon.InfrastructureEventType.ProcessDropped,
                        Source = nodeName,
                        Criticality = "Fatal",
                        ShortDescription = $"Service {e.EventData.ServiceName} dropped, restart loop detected",
                        FullDescription = $"Service {e.EventData.ServiceName} started {e.EventData.RestartCount} times in {e.EventData.InSeconds} seconds",
                    };

                    e.Using(eventBroadcaster, implementationGroup).EnqueueCommand(EventBroadcaster.Broadcast, infrastructureEvent);
                });

            applicationSetup.MapEvent<Event.ProcessesAttached>(
                e =>
                {
                    string fullDescription = $"Pid = {System.Diagnostics.Process.GetCurrentProcess().Id}";

                    fullDescription += "\n";
                    fullDescription += string.Join("\n", e.EventData.ProcessDescriptions);

                    var mdsLocalStarted = new MdsCommon.InfrastructureEvent()
                    {
                        Type = MdsCommon.InfrastructureEventType.MdsLocalRestart,
                        Criticality = MdsCommon.InfrastructureEventCriticality.Info,
                        Source = nodeName,
                        Timestamp = System.DateTime.UtcNow,
                        ShortDescription = "Controller started",
                        FullDescription = fullDescription
                    };

                    e.Using(eventBroadcaster, implementationGroup).EnqueueCommand(EventBroadcaster.Broadcast, mdsLocalStarted);
                });

            applicationSetup.MapEvent<Event.ProcessExited>(
                e =>
                {
                    // Register crash, counts towards process dropping

                    string error = string.Empty;

                    string serviceName = MdsLocalApplication.GuessServiceName(nodeName, e.EventData.FullExePath);

                    if (e.EventData.ExitCode != 0)
                    {
                        string dbPath = Mds.GetServiceLogDbFile(baseDataFolder, serviceName);
                        e.Logger.LogDebug($"Db path {dbPath}");
                        var lastError = MdsLocalApplication.GetLastDbLogError(dbPath, startTimestamp);
                        e.Logger.LogDebug($"Last error {Metapsi.Serialize.ToJson(lastError)}");
                        if (lastError != null)
                        {
                            error = $"{lastError.LogMessage}\n{lastError.CallStack}";
                        }

                        e.Using(mdsLocalApplication, implementationGroup).EnqueueCommand(
                            MdsLocalApplication.RegisterCrash, new MdsLocalApplication.ServiceCrashEvent()
                            {
                                CrashTimestamp = System.DateTime.UtcNow,
                                ServiceName = serviceName,
                                Error = error
                            });
                    }

                    string criticality = e.EventData.ExitCode == 0 ? MdsCommon.InfrastructureEventCriticality.Info : MdsCommon.InfrastructureEventCriticality.Fatal;

                    string shortDescription = "Service exit";

                    if (e.EventData.ExitCode != 0)
                    {
                        shortDescription = "Service crash";
                    }

                    var infraEvent = new MdsCommon.InfrastructureEvent()
                    {
                        Timestamp = System.DateTime.UtcNow,
                        Type = MdsCommon.InfrastructureEventType.ProcessExit,
                        Source = serviceName,
                        Criticality = criticality,
                        ShortDescription = shortDescription,
                        FullDescription = $"Process pid = {e.EventData.Pid}, node = {nodeName}, path= {e.EventData.FullExePath}, exit code = {e.EventData.ExitCode}\n{error}"
                    };

                    e.Using(eventBroadcaster, implementationGroup).EnqueueCommand(EventBroadcaster.Broadcast, infraEvent);

                    // And start all processes that should be running
                    e.Using(mdsLocalApplication, implementationGroup).EnqueueCommand(MdsLocalApplication.SynchronizeOnCrash);
                });

            applicationSetup.MapEvent<Event.ProcessStarted>(
                e =>
                {
                    var infraEvent = new MdsCommon.InfrastructureEvent()
                    {
                        Source = MdsLocalApplication.GuessServiceName(nodeName, e.EventData.FullExePath),
                        Type = MdsCommon.InfrastructureEventType.ProcessStart,
                        Criticality = MdsCommon.InfrastructureEventCriticality.Info,
                        ShortDescription = "Service start",
                        Timestamp = System.DateTime.UtcNow,
                        FullDescription = Serialize.ToJson(e.EventData)
                    };

                    e.Using(eventBroadcaster, implementationGroup).EnqueueCommand(EventBroadcaster.Broadcast, infraEvent);
                });

            applicationSetup.MapEvent<MdsLocalApplication.Event.ProcessesSynchronized>(
                e =>
                {
                    e.Using(mdsLocalApplication, implementationGroup).EnqueueCommand(async (cc, state) =>
                    {
                        state.PendingStopPids.Clear();
                    });
                });

            applicationSetup.MapEvent<MdsLocalApplication.Event.Error>(
                e =>
                {
                    var infrastructureEvent = new MdsCommon.InfrastructureEvent()
                    {
                        Source = e.EventData.ServiceName,
                        Type = MdsCommon.InfrastructureEventType.ExceptionProcessing,
                        Criticality = MdsCommon.InfrastructureEventCriticality.Critical,
                        ShortDescription = "Error while processing",
                        Timestamp = System.DateTime.UtcNow,
                        FullDescription = e.EventData.ErrorMessage
                    };

                    e.Using(eventBroadcaster, implementationGroup).EnqueueCommand(EventBroadcaster.Broadcast, infrastructureEvent);
                });

            applicationSetup.MapEvent<MdsLocalApplication.Event.StartupError>(
            e =>
            {
                var infrastructureEvent = new MdsCommon.InfrastructureEvent()
                {
                    Source = e.EventData.ServiceName,
                    Type = MdsCommon.InfrastructureEventType.ProcessDropped,
                    Criticality = MdsCommon.InfrastructureEventCriticality.Fatal,
                    ShortDescription = "Startup error, service dropped",
                    Timestamp = System.DateTime.UtcNow,
                    FullDescription = e.EventData.ErrorMessage
                };

                e.Using(eventBroadcaster, implementationGroup).EnqueueCommand(EventBroadcaster.Broadcast, infrastructureEvent);
            });


            return mdsLocalApplication;
        }
    }
}
