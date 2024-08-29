using MdsCommon;
using Metapsi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure.Flow;

public static class GetModel
{
    public static async Task<InfrastructureStatus> InfrastructureStatus(CommandContext commandContext, HttpContext httpContext)
    {
        var pageModel = await Status.LoadInfrastructureStatusPageModel(commandContext);
        pageModel.User = httpContext.User();
        return pageModel;
    }

    public static async Task<DeploymentReview> DeploymentReview(CommandContext commandContext, HttpContext httpContext, Guid deploymentId)
    {
        var deploymentReview = await Deployment.Review.Load(commandContext, deploymentId);
        deploymentReview.User = httpContext.User();
        return deploymentReview;
    }

    public static void RegisterModelApi(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet(nameof(MdsInfrastructure.InfrastructureStatus), InfrastructureStatus).AllowAnonymous();
        endpointRouteBuilder.MapGet(nameof(MdsInfrastructure.DeploymentReview), DeploymentReview).AllowAnonymous();
    }
}
