using Metapsi;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MdsLocal
{
    public static partial class MdsLocalApplication
    {
        public static async Task AttachToProcesses(CommandContext commandContext, State state)
        {
            Event.ProcessesAttached processesAttached = new Event.ProcessesAttached();

            var ownedProcesses = MdsLocalApplication.IdentifyOwnedProcesses(state.NodeName);

            foreach (var ownedProcess in ownedProcesses)
            {
                await ObserveProcess(commandContext, state, ownedProcess.Id);
                processesAttached.ProcessDescriptions.Add($"{ownedProcess.Id} {ownedProcess.ProcessName}");
            }

            commandContext.PostEvent(processesAttached);
        }

        private static void AttachExitHandler(
            CommandContext commandContext,
            State state,
            System.Diagnostics.Process process)
        {
            process.EnableRaisingEvents = true;
            int processId = process.Id;
            string mainModulePath = string.Empty;
            string fullExePath = string.Empty;

            while (true)
            {
                try
                {
                    // Works before or after catch just as well
                    if (process.HasExited)
                    {
                        break;
                    }

                    if (string.IsNullOrEmpty(mainModulePath))
                    {
                        // Seems to sometimes crash while accessing .MainModule;
                        mainModulePath = process.MainModule.FileName;
                    }
                    if(string.IsNullOrEmpty(fullExePath))
                    {
                        fullExePath = process.ProcessName;

                        if(!System.IO.Path.IsPathFullyQualified(fullExePath))
                        {
                            string servicePath = GuessServiceName(state.NodeName, process.ProcessName);
                            fullExePath = System.IO.Path.Combine(state.ServicesBasePath, servicePath, fullExePath);
                        }
                    }

                    commandContext.Logger.LogDebug($"AttachExitHandler process module: {mainModulePath}");
                    break;
                }
                catch (Exception ex)
                {
                    Task.Delay(1000).Wait();
                }
            }

            process.Exited += (object sender, EventArgs e) =>
            {
                process.WaitForExit();
                commandContext.Logger.LogDebug($"Exit process name: {fullExePath}");
                commandContext.Logger.LogDebug($"Exit process module: {mainModulePath}");

                if (!state.PendingStopPids.Contains(process.Id))
                {
                    commandContext.PostEvent(new Event.ProcessExited()
                    {
                        ExitCode = process.ExitCode,
                        Pid = processId,
                        FullExePath = fullExePath
                    });
                }
            };
        }


        private static async Task StartProcess(
            CommandContext commandContext,
            State state,
            string fullProcessPath,
            bool allowMultipleInstances,
            string inWorkingDirectory)
        {
            if (!allowMultipleInstances)
            {
                string processName = System.IO.Path.GetFileName(fullProcessPath);
                System.Diagnostics.Process[] alreadyRunning = System.Diagnostics.Process.GetProcessesByName(processName);
                if (alreadyRunning.Any())
                {
                    commandContext.PostEvent(new Event.ProcessAlreadyRunning()
                    {
                        ShortProcessName = processName,
                        Pid = alreadyRunning.First().Id
                    });
                    // Early return here!
                    return;
                }
            }

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo()
            {
                CreateNoWindow = true,
                FileName = fullProcessPath,
                WorkingDirectory = inWorkingDirectory
            };

            System.Diagnostics.Process process = new System.Diagnostics.Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.Start();
            AttachExitHandler(commandContext, state, process);

            commandContext.PostEvent(new Event.ProcessStarted()
            {
                FullExePath = fullProcessPath,
                Pid = process.Id,
                StartTimeUtc = DateTime.UtcNow
            });
        }

        private static async Task ObserveProcess(CommandContext commandContext, State state, int pid)
        {
            if (state.LiveProcesses.ContainsKey(pid))
            {

            }
            else
            {
                var process = System.Diagnostics.Process.GetProcessById(pid);
                AttachExitHandler(commandContext, state, process);
                state.LiveProcesses[process.Id] = process;
            }
        }
    }
}
