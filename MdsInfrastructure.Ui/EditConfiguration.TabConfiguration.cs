using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        public static Var<HyperNode> TabConfiguration(
            BlockBuilder b,
            Var<EditConfigurationPage> clientModel)
        {
            var configName = b.Get(clientModel, x => x.Configuration.Name);

            var content = b.Div("flex flex-col space-y-4 w-full");
            var configuratioNameRow = b.Add(content, b.Div("flex flex-row justify-between"));
            b.Add(configuratioNameRow, b.Text("Configuration name"));
            b.Add(configuratioNameRow, b.BoundInput(clientModel, x => x.Configuration, x => x.Name));
            b.Add(content, b.Text(b.Get(clientModel, x => x.LastDeployed)));

            return content;
        }
    }
}
