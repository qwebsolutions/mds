using Metapsi;
using Metapsi.Hyperapp;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MdsInfrastructure
{
    // TODO: Most likely should be completely removed, multiple configurations were never used 
    // If kept, should be an API call 

    //public static partial class Configuration
    //{
    //    public static async Task<IResult> Add(CommandContext commandContext, HttpContext requestData)
    //    {
    //        var newConfig = new InfrastructureConfiguration();
    //        var serverModel = await MdsInfrastructureFunctions.InitializeEditConfiguration(commandContext, newConfig);

    //        return await EditConfiguration.BuildModulePage(commandContext, requestData, serverModel, requestData.User());
    //    }
    //}
}
