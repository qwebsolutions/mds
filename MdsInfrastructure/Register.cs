using MdsCommon;
using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Sqlite;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;

namespace MdsInfrastructure
{
    public static class Register
    {
        public static void Everything(WebServer.References refs, SqliteQueue dbQueue)
        {
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Status.Infra, Routes.Status.Infra>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Status.Application, Routes.Status.Application, string>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Status.Node, Routes.Status.Node, string>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Docs.Service, Routes.Docs.Service,string>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Configuration.List, Routes.Configuration.List>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Configuration.Edit, Routes.Configuration.Edit, Guid>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Configuration.Review, Routes.Configuration.Review, Guid>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.SignIn.Form, Routes.SignIn>();
            refs.WebApplication.RegisterPostHandler<MdsInfrastructure.Flow.SignIn.Credentials, Routes.SignIn.Credentials, MdsCommon.InputCredentials>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Node.List, Routes.Node.List>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Node.Edit, Routes.Node.Edit, Guid>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Node.Add, Routes.Node.Add>();
            refs.WebApplication.MapPost("/node/save", async (CommandContext commandContext, HttpContext httpContext, MdsInfrastructure.InfrastructureNode node) =>
            {
                await commandContext.Do(Backend.SaveNode, node);
                await commandContext.RegisterNodesMessaging(dbQueue);
            });
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Project.List, Routes.Project.List>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Deployment.List, Routes.Deployment.List>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Deployment.Review, Routes.Deployment.Review, Guid>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Deployment.Preview, Routes.Deployment.ConfigurationPreview, Guid>();
            refs.WebApplication.RegisterGetHandler<MdsCommon.EventsLogHandler, MdsCommon.Routes.EventsLog.List>();



            refs.WebApplication.UseRenderer<InfrastructureStatus>(MdsInfrastructure.Render.InfrastructureStatus.Render);
            refs.WebApplication.UseRenderer<MdsInfrastructure.ApplicationStatus>(MdsInfrastructure.Render.ApplicationStatus.Render);
            refs.WebApplication.UseRenderer<NodeStatus>(MdsInfrastructure.Render.NodeStatus.Render);
            refs.WebApplication.UseRenderer<MdsInfrastructure.Docs.ServicePage>(MdsInfrastructure.Render.Docs.RenderService);
            refs.WebApplication.UseRenderer<MdsInfrastructure.ListConfigurationsPage>(MdsInfrastructure.Render.Configuration.List.Render);
            refs.WebApplication.UseRenderer<MdsInfrastructure.EditConfigurationPage>(MdsInfrastructure.Render.Configuration.Edit.Render);
            refs.WebApplication.UseRenderer<MdsInfrastructure.ReviewConfigurationPage>(MdsInfrastructure.Render.Configuration.Review.Render);
            refs.WebApplication.UseRenderer<SignInPage>(MdsInfrastructure.Render.SignIn.Credentials.Render);
            refs.WebApplication.UseRenderer<MdsInfrastructure.Node.List>(MdsInfrastructure.Render.Node.List.Render);
            refs.WebApplication.UseRenderer<MdsInfrastructure.Node.EditPage>(MdsInfrastructure.Render.Node.Edit.Render);
            refs.WebApplication.UseRenderer<MdsInfrastructure.DeploymentHistory>(MdsInfrastructure.Render.Deployment.List.Render);
            refs.WebApplication.UseRenderer<MdsInfrastructure.DeploymentReview>(MdsInfrastructure.Render.Deployment.Review.Render);
            refs.WebApplication.UseRenderer<MdsInfrastructure.DeploymentPreview>(MdsInfrastructure.Render.Deployment.Preview.Render);
            refs.WebApplication.UseRenderer<MdsInfrastructure.ListProjectsPage>(MdsInfrastructure.Render.Project.List.Render);
            refs.WebApplication.UseRenderer<MdsCommon.ListInfrastructureEventsPage>(MdsInfrastructure.RenderInfrastructureEventsList.Render);
            refs.WebApplication.UseRenderer<System.Exception>(Metapsi.WebServer.DefaultExceptionHandler);
        }
    }
}
