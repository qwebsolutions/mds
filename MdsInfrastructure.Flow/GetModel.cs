using MdsCommon;
using Metapsi;
using Metapsi.Sqlite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace MdsInfrastructure.Flow;

public static class GetModel
{
    public static async Task<InfrastructureStatus> InfrastructureStatus(HttpContext httpContext, SqliteQueue sqliteQueue)
    {
        var pageModel = await Status.LoadInfrastructureStatusPageModel(sqliteQueue);
        pageModel.User = httpContext.User();
        return pageModel;
    }

    public static async Task<DeploymentReview> DeploymentReview(CommandContext commandContext, HttpContext httpContext, Guid deploymentId)
    {
        var deploymentReview = await Deployment.Review.Load(commandContext, deploymentId);
        deploymentReview.User = httpContext.User();
        return deploymentReview;
    }

    public static void RegisterModelApi(this IEndpointRouteBuilder endpointRouteBuilder, SqliteQueue sqliteQueue)
    {
        endpointRouteBuilder.MapGet(
            nameof(MdsInfrastructure.InfrastructureStatus),
            async (HttpContext httpContext) =>
            {
                var status = await InfrastructureStatus(httpContext, sqliteQueue);
                return status;
            }).AllowAnonymous();
        endpointRouteBuilder.MapGet(nameof(MdsInfrastructure.DeploymentReview) + "/{deploymentId}", DeploymentReview).AllowAnonymous();
    }
}

public static class HttpEventsExtensions
{

}
