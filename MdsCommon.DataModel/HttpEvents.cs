using Metapsi;
using System;
using System.Collections.Generic;

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


///// <summary>
///// Sent by local when a service is stopped by the controller
///// </summary>
//public class ServiceStop
//{
//    public Guid DeploymentId { get; set; }
//    public string ServiceName { get; set; }
//    public string NodeName { get; set; }
//    public int ExitCode { get; set; }
//    public string ServicePath { get; set; }
//}

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

//public static class NodeEvent
//{
//    public class ControllerStarted
//    {
//        public Guid Id { get; set; } = Guid.NewGuid();
//        public string NodeName { get; set; }
//        public string TimestampIso { get; set; } = DateTime.Now.Roundtrip();
//    }
//}

//public static class GlobalEvent
//{
//    public class ControllerStarted
//    {
//        public Guid Id { get; set; } = Guid.NewGuid();
//    }
//}

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

    public class ServiceSynchronized
    {
        public Guid DeploymentId { get; set; }
        public string ServiceName { get; set; }
        public string NodeName { get; set; }
        public string ServiceStatus { get; set; }
        public string Error { get; set; }
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
