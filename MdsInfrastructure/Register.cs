using Metapsi;

namespace MdsInfrastructure
{
    public static class Register
    {
        public static void Everything(WebServer.References refs)
        {
            refs.WebApplication.RegisterRouteHandler<MdsInfrastructure.Flow.Status.Infra, Routes.Status.Infra>();
            refs.WebApplication.RegisterRouteHandler<MdsInfrastructure.Flow.Status.Application, Routes.Status.Application, string>();

            refs.RegisterPageBuilder<MdsInfrastructure.Render.InfrastructureStatus, InfrastructureStatus>(new Render.InfrastructureStatus());
            refs.RegisterPageBuilder<MdsInfrastructure.Render.ApplicationStatus, ApplicationStatus>(new Render.ApplicationStatus());
        }
    }
}
