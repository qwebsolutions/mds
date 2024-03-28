using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;
using MdsCommon.Controls;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<IVNode> TabConfiguration(
            LayoutBuilder b,
            Var<EditConfigurationPage> clientModel)
        {
            return b.StyledDiv(
                "flex flex-col space-y-4 w-full",
                b.StyledDiv(
                    "flex flex-row justify-between items-center gap-8",
                    b.StyledSpan(
                        "whitespace-nowrap",
                        b.TextSpan("Configuration name")),
                    b.BoundInput(clientModel, x => x.Configuration, x => x.Name)),
                b.TextSpan(b.Get(clientModel, x => x.LastDeployed)));
        }
    }
}
