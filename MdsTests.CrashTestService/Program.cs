using Metapsi;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace CrashTestService;

public class InputArguments
{
    public string LogToTextFile { get; set; } = "log.txt";
    public int LogInfoIntervalSeconds { get; set; }
    public int LogErrorIntervalSeconds { get; set; }
    public int ThrowExceptionIntervalSeconds { get; set; }
    public int CrashAfterSeconds { get; set; }
    public int ExitCode { get; set; }
}

public static class Program
{
    public static Application app = null;

    public static async Task Main()
    {
        var serviceSetup = Metapsi.Mds.ServiceSetup.New();
        var parameters = serviceSetup.ParametersAs<InputArguments>();

        var textLogFile = string.Empty;
        if (!string.IsNullOrEmpty(parameters.LogToTextFile))
        {
            textLogFile = serviceSetup.GetServiceDataFile(parameters.LogToTextFile);
        }

        var ig = serviceSetup.ApplicationSetup.AddImplementationGroup();
        serviceSetup.ApplicationSetup.AddTestService(ig, parameters, textLogFile);
        app = serviceSetup.Revive();
        
        await app.SuspendComplete;
    }
}

public static class TestService
{
    public class State
    {
        public DateTime AppStartTimestamp { get; set; } = DateTime.Now;
        public DateTime LastInfoTimestamp { get; set; } = DateTime.Now;
        public DateTime LastErrorTimestamp { get; set; } = DateTime.Now;
        public DateTime LastExceptionTimestamp { get; set; } = DateTime.Now;
    }

    public class Tick : IData
    {
        public int Count { get; set; } = 0;
    }

    public class ThrowException : IData
    {

    }

    public class Crash : IData
    {
        public int ExitCode { get; set; }
    }

    public static void AddTestService(this ApplicationSetup applicationSetup, ImplementationGroup implementationGroup, InputArguments inputArguments, string textLogFile)
    {
        var testServiceState = applicationSetup.AddBusinessState(new State());

        applicationSetup.MapEvent<ApplicationRevived>(e =>
        {
            e.Using(testServiceState, implementationGroup).EnqueueCommand(async (cc, state) =>
            {
                var timer = new System.Timers.Timer(TimeSpan.FromSeconds(1));

                int tick = 0;

                timer.Elapsed += (s, e) =>
                {
                    tick++;
                    cc.PostEvent(new Tick()
                    {
                        Count = tick
                    });
                };

                timer.Start();
            });
        });

        applicationSetup.MapEvent<Tick>(e =>
        {
            e.Using(testServiceState, implementationGroup).EnqueueCommand(async (cc, state) =>
            {
                if (inputArguments.LogInfoIntervalSeconds > 0)
                {
                    if (Convert.ToInt32(Math.Abs((DateTime.Now - state.LastInfoTimestamp).TotalSeconds)) >= inputArguments.LogInfoIntervalSeconds)
                    {
                        state.LastInfoTimestamp = DateTime.Now;
                        cc.Logger.LogInfo($"Tick {e.EventData.Count}");
                    }
                }

                if (inputArguments.LogErrorIntervalSeconds > 0)
                {
                    if (Convert.ToInt32(Math.Abs((DateTime.Now - state.LastErrorTimestamp).TotalSeconds)) >= inputArguments.LogErrorIntervalSeconds)
                    {
                        state.LastErrorTimestamp = DateTime.Now;
                        cc.Logger.LogError($"Tick {e.EventData.Count}");
                    }
                }

                if (inputArguments.ThrowExceptionIntervalSeconds > 0)
                {
                    if (Convert.ToInt32(Math.Abs((DateTime.Now - state.LastExceptionTimestamp).TotalSeconds)) >= inputArguments.ThrowExceptionIntervalSeconds)
                    {
                        state.LastExceptionTimestamp = DateTime.Now;
                        throw new Exception(inputArguments.ThrowExceptionIntervalSeconds.ToString());
                    }
                }

                if (inputArguments.CrashAfterSeconds > 0)
                {
                    if (Convert.ToInt32(Math.Abs((DateTime.Now - state.AppStartTimestamp).TotalSeconds)) >= inputArguments.CrashAfterSeconds)
                    {
                        Environment.Exit(inputArguments.ExitCode);
                    }
                }
            });
        });

        applicationSetup.MapEvent<Mds.Event.Shutdown>(e =>
        {
            CustomLogToFile(textLogFile, "Mds.Event.Shutdown received");

            if (Program.app != null)
            {
                e.Using(testServiceState, implementationGroup).EnqueueCommand(async (cc, state) =>
                {
                    CustomLogToFile(textLogFile, "Application is shutting down");
                    cc.Logger.LogInfo("Application is shutting down");
                });
                Program.app.Suspend();
            }
        });
    }

    private static void CustomLogToFile(string fileName, string text)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(text);
            System.IO.File.AppendAllText(fileName, stringBuilder.ToString());
        }
    }
}