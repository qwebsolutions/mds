using Metapsi;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using M = MdsInfrastructure.Node;

namespace MdsInfrastructure.Flow;

public static class Node
{
    public class List : Metapsi.Http.Get<Routes.Node.List>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
        {
            M.List nodesList = new()
            {
                EnvironmentTypes = await commandContext.Do(Api.LoadEnvironmentTypes),
                InfrastructureNodes = await commandContext.Do(Api.LoadAllNodes),
                InfrastructureServices = await commandContext.Do(Api.LoadAllServices)
            };

            return Page.Result(nodesList);
        }
    }

    public class Edit : Metapsi.Http.Get<Routes.Node.Edit, Guid>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext, Guid nodeId)
        {
            var allNodes = await commandContext.Do(Api.LoadAllNodes);
            M.EditPage editPage = new()
            {
                EnvironmentTypes = await commandContext.Do(Api.LoadEnvironmentTypes),
                InfrastructureNode = allNodes.Single(x => x.Id == nodeId),
                User = httpContext.User()
            };

            return Page.Result(editPage);
        }
    }
}
