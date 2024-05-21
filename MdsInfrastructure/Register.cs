using MdsCommon;
using Metapsi;
using System;

namespace MdsInfrastructure
{
    public static class Register
    {
        public static void Everything(WebServer.References refs)
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
            refs.WebApplication.RegisterPostHandler<MdsInfrastructure.Flow.Node.Save, Routes.Node.Save, MdsInfrastructure.InfrastructureNode>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Project.List, Routes.Project.List>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Deployment.List, Routes.Deployment.List>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Deployment.Review, Routes.Deployment.Review, Guid>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Deployment.Preview, Routes.Deployment.ConfigurationPreview, Guid>();
            refs.WebApplication.RegisterGetHandler<MdsCommon.EventsLogHandler, MdsCommon.Routes.EventsLog.List>();

            refs.WebApplication.UseRenderer<InfrastructureStatus>(new MdsInfrastructure.Render.InfrastructureStatus().Render);
            refs.WebApplication.UseRenderer<ApplicationStatus>(new MdsInfrastructure.Render.ApplicationStatus().Render);
            refs.WebApplication.UseRenderer<NodeStatus>(new MdsInfrastructure.Render.NodeStatus().Render);
            refs.WebApplication.UseRenderer<Docs.ServicePage>(new MdsInfrastructure.Render.Docs.Service().Render);
            refs.WebApplication.UseRenderer<ListConfigurationsPage>(new MdsInfrastructure.Render.Configuration.List().Render);
            refs.WebApplication.UseRenderer<EditConfigurationPage>(new MdsInfrastructure.Render.Configuration.Edit().Render);
            refs.WebApplication.UseRenderer<ReviewConfigurationPage>(new MdsInfrastructure.Render.Configuration.Review().Render);
            refs.WebApplication.UseRenderer<SignInPage>(new MdsInfrastructure.Render.SignIn.Credentials().Render);
            refs.WebApplication.UseRenderer<Node.List>(new MdsInfrastructure.Render.Node.List().Render);
            refs.WebApplication.UseRenderer<Node.EditPage>(new MdsInfrastructure.Render.Node.Edit().Render);
            refs.WebApplication.UseRenderer<DeploymentHistory>(new MdsInfrastructure.Render.Deployment.List().Render);
            refs.WebApplication.UseRenderer<DeploymentReview>(new MdsInfrastructure.Render.Deployment.Review().Render);
            refs.WebApplication.UseRenderer<DeploymentPreview>(new MdsInfrastructure.Render.Deployment.Preview().Render);
            refs.WebApplication.UseRenderer<ListProjectsPage>(new MdsInfrastructure.Render.Project.List().Render);
            refs.WebApplication.UseRenderer<ListInfrastructureEventsPage>(new MdsInfrastructure.RenderInfrastructureEventsList().Render);
        }
    }
}
