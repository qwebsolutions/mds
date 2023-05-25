using System;
using System.Collections.Generic;
using System.Linq;

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

        //public static IEnumerable<InfrastructureNode> GetSupportedNodesForProjectVersion(
        //    InfrastructureConfiguration infrastructureConfiguration,
        //    List<MdsCommon.Project> allProjects,
        //    Metapsi.RecordCollection<EnvironmentType> environmentTypes,
        //    Guid selectedConfigurationId,
        //    Guid projectVersionId)
        //{
        //    var versionBinaries = allProjects.SelectMany(x=>x.Versions). .SelectMany(x => x.Binaries).Where(x => x.ProjectVersionId == projectVersionId);
        //    List<string> buildOses = versionBinaries.Select(x => NodeExtensions.TargetToEnvironment(x.Target)).ToList();
        //    var compatibleNodes = infrastructureConfiguration.InfrastructureNodes.Where(x => x.ConfigurationHeaderId == selectedConfigurationId).Where(x =>
        //    {
        //        var envType = environmentTypes.ById(x.EnvironmentTypeId);
        //        return buildOses.Contains(envType.OsType);
        //    });

        //    return compatibleNodes;
        //}

    }
}