using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsInfrastructure
{
    public static class Docs
    { 
        public class ServicePage
        {
            public InfrastructureSummary InfrastructureSummary { get; set; }
            public ServiceSummary ServiceSummary { get; set; }
            public Metapsi.Ui.User User { get; set; }
        }

        public class RedisMap
        {
            public InfrastructureSummary InfrastructureSummary { get; set; }
            public ServiceSummary ServiceSummary { get; set; }
            public Metapsi.Ui.User User { get; set; }
        }

        public static List<ServiceSummary> GetWriterServices(this InfrastructureSummary summary, string queueName)
        {
            return summary.ServiceReferences.Where(x => x.OutputQueues.Contains(queueName)).ToList();
        }

        public static List<ServiceSummary> GetReaderServices(this InfrastructureSummary summary, string queueName)
        {
            return summary.ServiceReferences.Where(x => x.InputQueues.Contains(queueName)).ToList();
        }

        public static List<ServiceSummary> GetNotificationServices(this InfrastructureSummary summary, string channelName)
        {
            return summary.ServiceReferences.Where(x => x.OutputChannels.Contains(channelName)).ToList();
        }

        public static List<ServiceSummary> GetListenerServices(this InfrastructureSummary summary, string channelName)
        {
            return summary.ServiceReferences.Where(x => x.InputChannels.Contains(channelName)).ToList();
        }

        public static List<ServiceSummary> GetClientServices(this InfrastructureSummary summary, string machineIp, int port)
        {
            List<ServiceSummary> clientServices = new();

            foreach (var service in summary.ServiceReferences)
            {
                foreach (string url in service.AccessedUrls)
                {
                    var urlData = ParseUrl(url);
                    if (urlData.Machine == machineIp && urlData.Port == port)
                    {
                        clientServices.Add(service);
                    }
                }
            }

            return clientServices;
        }

        public static string GetServer(this InfrastructureSummary summary, string url)
        {
            var urlData = ParseUrl(url);

            foreach (var service in summary.ServiceReferences)
            {
                if (service.MachineIp == urlData.Machine)
                {
                    if (service.ListeningPorts.Contains(urlData.Port))
                        return service.ServiceName;
                }
            }

            return String.Empty;
        }


        public class UrlData
        {
            public string Machine { get; set; }
            public int Port { get; set; } = 80;
        }

        public static UrlData ParseUrl(string url)
        {
            UrlData urlData = new UrlData();

            if (url.Replace("http://", string.Empty).Replace("https://", string.Empty).Contains(":"))
            {
                urlData.Port = Int32.Parse(url.Split(':').Last().Split('/', '?').First());
            }

            urlData.Machine = url.Replace("http://", string.Empty).Split(new char[] { ':', '/' }).First();

            return urlData;
        }
    }
}
