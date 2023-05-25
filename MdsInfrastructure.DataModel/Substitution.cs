using System.Collections.Generic;
using System.Linq;

namespace MdsInfrastructure
{
    public static class Substitution
    {
        public static Dictionary<string, string> GetSubstitutionValues(
            this InfrastructureConfiguration infrastructureConfiguration,
            List<InfrastructureNode> allNodes)
        {
            Dictionary<string, string> substitutionValues = new Dictionary<string, string>();

            foreach (var variable in infrastructureConfiguration.InfrastructureVariables)
            {
                substitutionValues[variable.VariableName] = variable.VariableValue;
            }

            foreach (var service in infrastructureConfiguration.InfrastructureServices)
            {
                var node = allNodes.Single(x => x.Id == service.InfrastructureNodeId);

                substitutionValues[$"{service.ServiceName}.NodeName"] = node.NodeName;
                substitutionValues[$"{service.ServiceName}.NodeAddress"] = node.MachineIp;

                foreach (var param in service.InfrastructureServiceParameterDeclarations)
                {
                    string paramValue = service.GetParameterValue(param, infrastructureConfiguration.InfrastructureVariables);
                    substitutionValues[$"{service.ServiceName}.{param.ParameterName}"] = paramValue;
                }
            }

            return substitutionValues;
        }
    }
}