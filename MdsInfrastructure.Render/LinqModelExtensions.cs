using Metapsi.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MdsInfrastructure.Render
{
    public static class LinqModelExtensions
    {
        public static Var<List<MdsCommon.ServiceConfigurationSnapshot>> GetDeployedServices(this SyntaxBuilder b, Var<MdsInfrastructure.InfrastructureStatus> status)
        {
            var hasObject = b.Def<SyntaxBuilder, MdsCommon.ServiceConfigurationSnapshot,bool>(Metapsi.Syntax.Core.HasObject);
            return b.Get(status, hasObject, (status, hasObject) => status.Deployment.Transitions.Where(x => hasObject(x.ToSnapshot)).Select(x => x.ToSnapshot).ToList());
        }
    }
}
