using Metapsi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdsCommon;

/// <summary>
/// Sent by global at deploy
/// </summary>
public class NodeConfigurationUpdate
{
    public string InfrastructureName { get; set; }
    public Guid DeploymentId { get; set; }
    public string BinariesApiUrl { get; set; }
    public List<MdsCommon.ServiceConfigurationSnapshot> Snapshots { get; set; } = new();
}

/// <summary>
/// Sent by local when a service crashes by itself
/// </summary>
public class ServiceCrash
{
    public string TimestampIso { get; set; } = DateTime.Now.Roundtrip();
    public string ServiceName { get; set; }
    public string NodeName { get; set; }
    public int ExitCode { get; set; }
    public string ServicePath { get; set; }
}

/// <summary>
/// Sent by local when a service is restarted after a crash
/// </summary>
public class ServiceRecovered
{
    public string TimestampIso { get; set; } = DateTime.Now.Roundtrip();
    public string ServiceName { get; set; }
    public string NodeName { get; set; }
    public string ServicePath { get; set; }
}

public static class NodeEvent
{
    public class Started
    {
        public string TimestampIso { get; set; } = DateTime.Now.Roundtrip();
        public string NodeName { get; set; }
        public MachineStatus NodeStatus { get; set; }
        public List<string> InstalledServices { get; set; } = new();
        public List<string> RunningServices { get; set; } = new();
        public List<string> NotRunningServices { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}

public class CleanupInfrastructureEvents
{
    public int KeepMaxCount { get; set; }
    public int KeepMaxDays { get; set; }
}

public static class NodeEventExtensions
{
    public static string GetFullDescription(this NodeEvent.Started message)
    {
        StringBuilder stringBuilder = new StringBuilder();

        if (message.Errors.Any())
        {
            stringBuilder.AppendLine("Errors:");
        }

        foreach (var error in message.Errors)
        {
            stringBuilder.AppendLine(error);
        }

        if (message.NotRunningServices.Any())
        {
            stringBuilder.AppendLine("Not running:");
        }

        foreach (var notRunning in message.NotRunningServices)
        {
            stringBuilder.AppendLine(notRunning);
        }

        if (message.RunningServices.Any())
        {
            stringBuilder.AppendLine("Running:");
        }
        foreach (var running in message.RunningServices)
        {
            stringBuilder.AppendLine(running);
        }

        var fullDescription = stringBuilder.ToString();

        if (string.IsNullOrEmpty(fullDescription))
            fullDescription = "No service running";

        return fullDescription;
    }
}

public static class DeploymentEvent
{
    public class DeploymentStart
    {
        public Guid DeploymentId { get; set; }
        public string TimestampIso { get; set; } = DateTime.UtcNow.Roundtrip();
    }

    public class DeploymentComplete
    {
        public Guid DeploymentId { get; set; }
        public string TimestampIso { get; set; } = DateTime.UtcNow.Roundtrip();
        public bool HasErrors { get; set; }
    }

    public class ServiceStart
    {
        public Guid DeploymentId { get; set; }
        public string ServiceName { get; set; }
        public string NodeName { get; set; }
        public string TimestampIso { get; set; } = DateTime.UtcNow.Roundtrip();
    }

    public class ServiceStop
    {
        public Guid DeploymentId { get; set; }
        public string ServiceName { get; set; }
        public string NodeName { get; set; }
        public string TimestampIso { get; set; } = DateTime.UtcNow.Roundtrip();
    }

    public class ServiceInstall
    {
        public Guid DeploymentId { get; set; }
        public string ServiceName { get; set; }
        public string NodeName { get; set; }
        public string TimestampIso { get; set; } = DateTime.UtcNow.Roundtrip();
    }

    public class ServiceUninstall
    {
        public Guid DeploymentId { get; set; }
        public string ServiceName { get; set; }
        public string NodeName { get; set; }
        public string TimestampIso { get; set; } = DateTime.UtcNow.Roundtrip();
    }

    public class ParametersSet
    {
        public Guid DeploymentId { get; set; }
        public string ServiceName { get; set; }
        public string NodeName { get; set; }
        public string TimestampIso { get; set; } = DateTime.UtcNow.Roundtrip();
    }

    public class ServiceSynchronized
    {
        public Guid DeploymentId { get; set; }
        public string ServiceName { get; set; }
        public string NodeName { get; set; }
        public string ServiceStatus { get; set; }
        public string Error { get; set; } = string.Empty;
    }
}

//public static class DeploymentEventType
//{
//    public const string DeploymentStarted = nameof(DeploymentStarted);
//    public const string DeploymentDone = nameof(DeploymentDone);
//    public const string ServiceStarted = nameof(ServiceStarted);
//    public const string ServiceStopped = nameof(ServiceStopped);
//    public const string ServiceUninstalled = nameof(ServiceUninstalled);
//}
