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


            refs.RegisterPageBuilder<MdsInfrastructure.Render.InfrastructureStatus, InfrastructureStatus>(new Render.InfrastructureStatus());
            refs.RegisterPageBuilder<MdsInfrastructure.Render.ApplicationStatus, ApplicationStatus>(new Render.ApplicationStatus());
            refs.RegisterPageBuilder<MdsInfrastructure.Render.Configuration.List, MdsInfrastructure.ListConfigurationsPage>(new Render.Configuration.List());
            refs.RegisterPageBuilder<MdsInfrastructure.Render.Configuration.Edit, MdsInfrastructure.EditConfigurationPage>(new Render.Configuration.Edit());
            refs.RegisterPageBuilder<MdsInfrastructure.Render.SignIn.Credentials, SignInPage>(new Render.SignIn.Credentials());
        }
    }
}
