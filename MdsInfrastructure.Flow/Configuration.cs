using Metapsi;
using Microsoft.AspNetCore.Http;
using System;
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

    public class Edit : Metapsi.Http.Get<Routes.Configuration.Edit, Guid>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext, Guid configurationId)
        {
            var savedConfiguration = await commandContext.Do(Api.LoadConfiguration, configurationId);
            var serverModel = await MdsInfrastructureFunctions.InitializeEditConfiguration(commandContext, savedConfiguration);
            serverModel.User = httpContext.User();
            return Page.Result(serverModel);
        }
    }
}
