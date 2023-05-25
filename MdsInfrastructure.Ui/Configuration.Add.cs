using Metapsi;
using Metapsi.Hyperapp;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MdsInfrastructure
{
    public static partial class Configuration
    {
        public static async Task<IResponse> Add(CommandContext commandContext, HttpContext requestData)
        {
            var newConfig = new InfrastructureConfiguration();
            var serverModel = await MdsInfrastructureFunctions.InitializeEditConfiguration(commandContext, newConfig);

            return await EditConfiguration.BuildModulePage(commandContext, requestData, serverModel, requestData.User());
        }
    }
}
