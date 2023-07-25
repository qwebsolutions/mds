using Metapsi;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MdsInfrastructure.Flow;

public static class Configuration
{
    public class List: Metapsi.Http.Get<Routes.Configuration.List>
    {
        public async override Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
        {
            var configurationsList = await MdsInfrastructureFunctions.Configurations(commandContext);

            return Page.Result(new ListConfigurationsPage()
            {
                ConfigurationHeadersList = configurationsList,
                User = httpContext.User()
            });
        }
    }

    //public class Add : Metapsi.Http.Get<Routes.Configuration.Add>
    //{
    //    public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
    //    {
    //        var newConfig = new InfrastructureConfiguration();
    //        var serverModel = await MdsInfrastructureFunctions.InitializeEditConfiguration(commandContext, newConfig);

    //        return Page.Result(new AddConfigurationPage()
    //        {
    //            EditConfigurationPage = serverModel
    //        });
    //    }
    //}
}
