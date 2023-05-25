using System.Collections.Generic;
using System.Linq;

namespace MdsInfrastructure
{
    public static class ServicesTransformations
    {

        public static string GetParameterValue(this InfrastructureService service, InfrastructureServiceParameterDeclaration parameter, IEnumerable<InfrastructureVariable> variables)
        {
            var value = parameter.InfrastructureServiceParameterValues.SingleOrDefault(x => x.InfrastructureServiceParameterDeclarationId == parameter.Id);
            if (value != null)
                return value.ParameterValue;
            var binding = parameter.InfrastructureServiceParameterBindings.Single(x => x.InfrastructureServiceParameterDeclarationId == parameter.Id);
            var variable = variables.Single(x => x.Id == binding.InfrastructureVariableId);
            return variable.VariableValue;
        }
    }
}