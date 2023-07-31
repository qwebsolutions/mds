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
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Configuration.List, Routes.Configuration.List>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Configuration.Edit, Routes.Configuration.Edit, Guid>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.SignIn.Form, Routes.SignIn>();
            refs.WebApplication.RegisterPostHandler<MdsInfrastructure.Flow.SignIn.Credentials, Routes.SignIn.Credentials, MdsCommon.InputCredentials>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Node.List, Routes.Node.List>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Node.Edit, Routes.Node.Edit, Guid>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Project.List, Routes.Project.List>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Deployment.List, Routes.Deployment.List>();
            refs.WebApplication.RegisterGetHandler<MdsInfrastructure.Flow.Deployment.Review, Routes.Deployment.Review, Guid>();
            refs.WebApplication.RegisterGetHandler<MdsCommon.EventsLogHandler, MdsCommon.Routes.EventsLog.List>();

            refs.RegisterPageBuilder<MdsInfrastructure.Render.InfrastructureStatus, InfrastructureStatus>();
            refs.RegisterPageBuilder<MdsInfrastructure.Render.ApplicationStatus, ApplicationStatus>();
            refs.RegisterPageBuilder<MdsInfrastructure.Render.Configuration.List, MdsInfrastructure.ListConfigurationsPage>();
            refs.RegisterPageBuilder<MdsInfrastructure.Render.Configuration.Edit, MdsInfrastructure.EditConfigurationPage>();
            refs.RegisterPageBuilder<MdsInfrastructure.Render.SignIn.Credentials, SignInPage>();
            refs.RegisterPageBuilder<MdsInfrastructure.Render.Node.List, MdsInfrastructure.Node.List>();
            refs.RegisterPageBuilder<MdsInfrastructure.Render.Node.Edit, MdsInfrastructure.Node.EditPage>();
            refs.RegisterPageBuilder<MdsInfrastructure.Render.Deployment.List, MdsInfrastructure.DeploymentHistory>();
            refs.RegisterPageBuilder<MdsInfrastructure.Render.Deployment.Review, MdsInfrastructure.DeploymentReview>();
            refs.RegisterPageBuilder<MdsInfrastructure.Render.Project.List, MdsInfrastructure.ListProjectsPage>();
            refs.RegisterPageBuilder<MdsInfrastructure.RenderInfrastructureEventsList, MdsCommon.ListInfrastructureEventsPage>();
        }
    }
}
