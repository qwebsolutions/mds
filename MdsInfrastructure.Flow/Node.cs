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
                EnvironmentTypes = await commandContext.Do(Backend.LoadEnvironmentTypes),
                InfrastructureNodes = await commandContext.Do(Backend.LoadAllNodes),
                InfrastructureServices = await commandContext.Do(Backend.LoadAllServices),
                User = httpContext.User()
            };

            return Page.Result(nodesList);
        }
    }

    public class Edit : Metapsi.Http.Get<Routes.Node.Edit, Guid>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext, Guid nodeId)
        {
            var allNodes = await commandContext.Do(Backend.LoadAllNodes);
            M.EditPage editPage = new()
            {
                EnvironmentTypes = await commandContext.Do(Backend.LoadEnvironmentTypes),
                InfrastructureNode = allNodes.Single(x => x.Id == nodeId),
                User = httpContext.User()
            };

            return Page.Result(editPage);
        }
    }

    public class Add : Metapsi.Http.Get<Routes.Node.Add>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
        {
            M.EditPage editPage = new()
            {
                EnvironmentTypes = await commandContext.Do(Backend.LoadEnvironmentTypes),
                InfrastructureNode = new InfrastructureNode(),
                User = httpContext.User()
            };

            return Page.Result(editPage);
        }
    }

    public class Save : Metapsi.Http.Post<Routes.Node.Save, MdsInfrastructure.InfrastructureNode>
    {
        public override async Task<IResult> OnPost(CommandContext commandContext, HttpContext httpContext, InfrastructureNode p1)
        {
            await commandContext.Do(Backend.SaveNode, p1);
            return Results.Redirect(WebServer.Url<Routes.Node.Edit, Guid>(p1.Id));
        }
    }
}
