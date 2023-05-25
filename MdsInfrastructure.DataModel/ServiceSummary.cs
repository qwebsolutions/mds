using System.Collections.Generic;

namespace MdsInfrastructure
{
    public class ServiceSummary
    {
        public string ServiceName { get; set; }
        public string NodeName { get; set; }
        public string MachineIp { get; set; }
        public string Project { get; set; }
        public string Version { get; set; }

        public string ServiceDescription { get; set; }

        public List<ServiceParameter> ServiceParameters { get; set; } = new List<ServiceParameter>();

        public List<int> ListeningPorts { get; set; } = new List<int>();
        public List<string> AccessedUrls { get; set; } = new List<string>();

        public List<string> InputChannels { get; set; } = new List<string>();
        public List<string> OutputChannels { get; set; } = new List<string>();

        public List<string> InputQueues { get; set; } = new List<string>();
        public List<string> OutputQueues { get; set; } = new List<string>();

        public List<string> DbConnections { get; set; } = new List<string>();
    }


}
