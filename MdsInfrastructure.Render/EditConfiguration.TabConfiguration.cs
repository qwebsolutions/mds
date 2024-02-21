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
            var configName = b.Get(clientModel, x => x.Configuration.Name);

            var content = b.Div("flex flex-col space-y-4 w-full");
            var configuratioNameRow = b.Add(content, b.Div("flex flex-row justify-between items-center gap-8"));
            b.Add(configuratioNameRow, b.Text("Configuration name", "whitespace-nowrap"));
            b.Add(configuratioNameRow, b.BoundInput(clientModel, x => x.Configuration, x => x.Name));
            b.Add(content, b.Text(b.Get(clientModel, x => x.LastDeployed)));

            return content.As<IVNode>();
        }
    }
}
