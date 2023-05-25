using Metapsi;
using Metapsi.Syntax;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Linq.Expressions;
using Metapsi.Hyperapp;
using MdsCommon;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        public static async Task<IResponse> Edit(CommandContext commandContext, HttpContext requestData)
        {
            var selectedConfigurationId = Guid.Parse(requestData.EntityId());

            var savedConfiguration = await commandContext.Do(Api.LoadConfiguration, selectedConfigurationId);
            var serverModel = await MdsInfrastructureFunctions.InitializeEditConfiguration(commandContext, savedConfiguration);

            return await BuildModulePage(commandContext, requestData, serverModel, requestData.User());
        }

        public static async Task<IResponse> BuildModulePage(
            CommandContext commandContext, 
            HttpContext requestData,
            EditConfigurationPage serverModel,
            WebServer.User user)
        {
            return Page.Response(
                serverModel,
                (b, clientModel) =>
                {
                    var header = b.NewObj(new Header.Props()
                    {
                        Main = new Header.Title() { Operation = "Edit configuration", Entity = serverModel.Configuration.Name },
                        User = user
                    });
                    b.Set(b.Get(header, x => x.Main), x => x.Entity, b.Get(clientModel, x => x.Configuration.Name));

                    var layout = b.Layout(
                        b.InfraMenu(nameof(Configuration), user.IsSignedIn()),
                        b.Render(header),
                        b.RenderCurrentView(
                            clientModel,
                            x => x.EditStack,
                            // Default page view
                            (BlockBuilder b, Var<EditConfigurationPage> clientModel) =>
                            {
                                var hasChanges = b.Not(b.AreEqual(b.Serialize(clientModel), b.Serialize(b.Const(serverModel))));
                                return MainPage(b, clientModel, hasChanges);
                            }));
                    return layout;
                });
        }
    }
}
