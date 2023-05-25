namespace MdsCommon
{
    public class InfrastructureNodeSettings
    {
        public string InfrastructureName { get; set; }
        public string BinariesApiUrl { get; set; }
        public int NodeUiPort { get; set; } = 9234;

        public string BroadcastDeploymentInputChannel { get; set; }
        public string NodeCommandInputChannel { get; set; }

        public string HealthStatusOutputChannel { get; set; }
        public string InfrastructureEventsOutputChannel { get; set; }
    }

}
