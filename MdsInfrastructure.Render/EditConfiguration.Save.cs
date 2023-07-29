using Metapsi;
using Metapsi.Hyperapp;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        public static async Task<IResponse> Save(CommandContext commandContext, HttpContext requestData)
        {
            InfrastructureConfiguration infraConfig = null;

            try
            {
                infraConfig = Metapsi.Serialize.FromJson<InfrastructureConfiguration>(await requestData.Payload());
                var page = await MdsInfrastructureFunctions.InitializeEditConfiguration(commandContext, infraConfig);

                var validationMessage = MdsInfrastructureFunctions.ValidateConfiguration(
                    infraConfig,
                    page.AllProjects,
                    page.InfrastructureNodes,
                    page.NoteTypes);

                if (validationMessage != null)
                {
                    page.ValidationMessage = validationMessage.Message;
                    return await EditConfiguration.BuildModulePage(commandContext, requestData, page, requestData.User());
                }
                else
                {
                    await commandContext.Do(Api.SaveConfiguration, infraConfig);
                    return Page.Redirect(EditConfiguration.Edit, infraConfig.Id);
                }
            }
            catch (Exception ex)
            {
                var page = await MdsInfrastructureFunctions.InitializeEditConfiguration(commandContext, infraConfig);
                return await EditConfiguration.BuildModulePage(commandContext, requestData, page, requestData.User());
            }
        }
    }
}
