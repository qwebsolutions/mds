namespace MdsCommon
{
    public static class NotificationChannel
    {
        //public static string SubstituteInfrastructureName(string configuredChannel, string infrastructureName)
        //{
        //    return configuredChannel.Replace(Metapsi.Mds.Constant.InfrastructureNameVariable, infrastructureName);
        //}

        //public static string BroadcastDeployment(string configuredChannel, string infrastructureName)
        //{
        //    string substitutedQueue = configuredChannel.Replace(Metapsi.Mds.Constant.InfrastructureNameVariable, infrastructureName);
        //    return substitutedQueue;
        //}

        //public static string HealthStatus(string infrastructureName)
        //{
        //    return $"{infrastructureName}.HealthStatus";
        //}

        //public static string BinariesAvailable()
        //{
        //    return "BinariesAvailable";
        //}

        //public static string NodeCommand(string infrastructureName, string nodeName)
        //{
        //    return $"{infrastructureName}.{nodeName}";
        //}

        //public static string InfrastructureEvent(string configuredChannel, string infrastructureName)
        //{
        //    return $"{infrastructureName}.InfrastructureEvent";
        //}
    }

    public static class NodeCommand
    {
        public const string StopService = "StopService";
    }
}