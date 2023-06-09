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
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        public static async Task<IResult> Edit(CommandContext commandContext, HttpContext requestData)
        {
            var selectedConfigurationId = Guid.Parse(requestData.EntityId());

            var savedConfiguration = await commandContext.Do(Api.LoadConfiguration, selectedConfigurationId);
            var serverModel = await MdsInfrastructureFunctions.InitializeEditConfiguration(commandContext, savedConfiguration);

            return await BuildModulePage(commandContext, requestData, serverModel, requestData.User());
        }

        public static async Task<IResult> BuildModulePage(
            CommandContext commandContext, 
            HttpContext requestData,
            EditConfigurationPage serverModel,
            WebServer.User user)
        {
            return Page.Result(serverModel);
                
        }

        public static void MapGet<TRoute>(this IEndpointRouteBuilder builder, Func<CommandContext, HttpContext, Task<IResult>> load) where TRoute : IMetapsiRoute
        {
            builder.MapGet(typeof(TRoute).Name, load);
        }

        public static void MapPost<TRoute>(this IEndpointRouteBuilder builder, Func<CommandContext, HttpContext, Task<IResult>> load) where TRoute : IMetapsiRoute
        {
            builder.MapPost(typeof(TRoute).Name, load);
        }
    }
}
