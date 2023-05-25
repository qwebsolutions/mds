using Metapsi;
using Metapsi.Hyperapp;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        public static async Task<IResponse> ConfirmDeployment(CommandContext commandContext, HttpContext requestData)
        {
            var savedConfiguration = await commandContext.Do(Api.LoadConfiguration, System.Guid.Parse(requestData.EntityId()));
            var serverModel = await MdsInfrastructureFunctions.InitializeEditConfiguration(commandContext, savedConfiguration);
            var newInfraSnapshot = await MdsInfrastructureFunctions.TakeConfigurationSnapshot(
                commandContext, 
                savedConfiguration, 
                serverModel.AllProjects,
                serverModel.InfrastructureNodes);
            await commandContext.Do(Api.ConfirmDeployment, new ConfirmDeploymentInput()
            {
                Snapshots = newInfraSnapshot,
                Configuration = savedConfiguration
            });

            Api.Event.BroadcastDeployment broadcastDeployment = new Api.Event.BroadcastDeployment();
            commandContext.PostEvent(broadcastDeployment);

            return Page.Redirect(Status.Infra);
        }
    }
}
