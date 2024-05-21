using System.Threading.Tasks;
using Metapsi;
using System;
using System.Linq;
using System.Collections.Generic;
using MdsCommon;

namespace MdsInfrastructure
{
    public static partial class MdsInfrastructureApplication
    {
        public class State
        {
            
        }

        public static partial class Event
        {

        }

        public class InputArguments
        {
            public int UiPort { get; set; }
            public int InfrastructureApiPort { get; set; }
            public string DbPath { get; set; }
            public string LogFilePath { get; set; }

            public string BuildManagerRedisUrl { get; set; }
            public string InfrastructureName { get; set; }
            public string BuildManagerUrl { get; set; }
            public string BuildManagerNodeUrl { get; set; } // In case the node needs a different url than the infra. closed ports...

            public string SmtpHost { get; set; }
            public string From { get; set; }
            public string Password { get; set; }
            public string ErrorEmails { get; set; }
            public string CertificateThumbprint { get; set; }

            public string InfrastructureEventsInputChannel { get; set; }
            public string BinariesAvailableInputChannel { get; set; }
            public string HealthStatusInputChannel { get; set; }

            public string BroadcastDeploymentOutputChannel { get; set; }
            public string NodeCommandOutputChannel { get; set; }

            public string AdminUserName { get; set; }
            public string AdminPassword { get; set; }
        }


        public class MdsInfraReferences
        {
            public ApplicationSetup ApplicationSetup { get; set; }
            public ImplementationGroup ImplementationGroup { get; set; }
            public HttpServer.State HttpGateway { get; set; }
            public RedisReader.State RedisReader { get; set; }
            public RedisWriter.State RedisWriter { get; set; }
            public RedisListener.State RedisListener { get; set; }
            public RedisNotifier.State RedisNotifier { get; set; }
        }

        public static async Task GatewayIsUp(this MdsInfraReferences references)
        {
            while (true)
            {
                if (references.HttpGateway.WebHostTask == null)
                {
                    await Task.Delay(500);
                }
                else break;
            }
        }
    }
}
