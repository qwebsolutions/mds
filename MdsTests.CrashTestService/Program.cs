using Metapsi;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public class InputArguments
{
    public int CrashAfterSeconds { get; set; }
    public int ExitCode { get; set; }
}

public static class Program
{
    public static async Task Main()
    {
        var serviceSetup = Metapsi.Mds.ServiceSetup.New();
        var parameters = serviceSetup.ParametersAs<InputArguments>();

        await Task.Delay(TimeSpan.FromSeconds(parameters.CrashAfterSeconds));

        Environment.Exit(parameters.ExitCode);
    }
}