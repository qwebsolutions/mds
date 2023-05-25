namespace MdsCommon
{
    public static class InfrastructureEventType
    {
        public const string MdsLocalRestart = "MdsLocalRestart";
        public const string ProcessStart = "ProcessStart";
        public const string ProcessExit = "ProcessExit";
        public const string ProcessDropped = "ProcessDropped";
        public const string ExceptionProcessing = "ExceptionProcessing";
        public const string ConfigurationMismatchDetected = "ConfigurationMismatchDetected";
    }
}