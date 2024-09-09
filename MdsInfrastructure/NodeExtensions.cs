using MdsCommon;
using Metapsi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure
{
    public static class NodeExtensions
    {
        public static bool IsNodeInUse(
            this InfrastructureConfiguration infrastructureConfiguration,
            InfrastructureNode infrastructureNode)
        {
            if (infrastructureConfiguration.InfrastructureServices.Any(x => x.InfrastructureNodeId == infrastructureNode.Id))
            {
                return true;
            }

            return false;
        }

        public static string TargetToEnvironment(string target)
        {
            if (target == "win10-x64") return "Windows";
            if (target == "linux-x64") return "Linux";
            return target;
        }

        public static async Task RegisterNodesMessaging(
            this CommandContext commandContext,
            Metapsi.Sqlite.SqliteQueue dbQueue)
        {
            var allNodes = await dbQueue.LoadAllNodes();
            foreach (var node in allNodes)
            {
                commandContext.MapMessaging(node.NodeName, $"http://{node.MachineIp}:{node.UiPort}/event");
            }
        }
    }
}