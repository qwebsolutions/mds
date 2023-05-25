using Metapsi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure
{
    public static partial class MdsInfrastructureApplication
    {
        public static async Task SyncBuilds(CommandContext commandContext, State state)
        {
            var buildsList = await commandContext.Do(Api.GetRemoteBuilds);
            var newBinaries = await commandContext.Do(Api.RegisterNewBinaries, buildsList);

            if (newBinaries.Any())
            {
                List<string> newBinariesDescription = new List<string>();

                foreach (var newBinary in newBinaries)
                {
                    newBinariesDescription.Add($"Project: {newBinary.Name}, version {newBinary.Version}, target {newBinary.Target}");
                }

                commandContext.PostEvent(new Api.Event.BinariesSynchronized()
                {
                    BinariesDescription = string.Join("\n", newBinariesDescription),
                });
            }
        }
    }
}
