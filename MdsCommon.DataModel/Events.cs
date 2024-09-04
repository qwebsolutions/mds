using Metapsi;
using System;

namespace MdsCommon;

public static class NodeEvent
{
    public class ControllerStarted
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string NodeName { get; set; }
        public string TimestampIso { get; set; } = DateTime.Now.Roundtrip();
    }
}

public static class GlobalEvent
{
    public class ControllerStarted
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}

public static class DeploymentEvent
{
    public class Started : IData
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DeploymentId { get; set; }
        public string TimestampIso { get; set; } = DateTime.UtcNow.Roundtrip();
    }

    public class Done : IData
    {
        public Guid Id { get; set; } = new Guid();
        public Guid DeploymentId { get; set; }
        public string TimestampIso { get; set; } = DateTime.UtcNow.Roundtrip();
    }

    public class ServiceStarted : IData
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DeploymentId { get; set; }
        public string ServiceName { get; set; }
        public string TimestampIso { get; set; } = DateTime.UtcNow.Roundtrip();
    }

    public class ServiceStopped : IData
    {
        public Guid Id { get; set; }
        public Guid DeploymentId { get; set; }
        public string ServiceName { get; set; }
        public string TimestampIso { get; set; } = DateTime.UtcNow.Roundtrip();
    }

    public class ServiceUninstalled : IData
    {
        public Guid Id { get; set; }
    }

    public static void NotifyGlobal(this CommandContext commandContext, object message)
    {
        commandContext.PostEvent(new InfrastructureMessage()
        {
            Message = message
        });
    }
}

public static class DeploymentEventType
{
    public const string DeploymentStarted = nameof(DeploymentStarted);
    public const string DeploymentDone = nameof(DeploymentDone);
    public const string ServiceStarted = nameof(ServiceStarted);
    public const string ServiceStopped = nameof(ServiceStopped);
    public const string ServiceUninstalled = nameof(ServiceUninstalled);
}

public class DbDeploymentEvent : IRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } // DeploymentEventType
    public Guid DeploymentId { get; set; }
    public string TimestampIso { get; set; } = DateTime.UtcNow.Roundtrip();
    public string ServiceName { get; set; }
}
