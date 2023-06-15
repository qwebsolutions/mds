using Metapsi;

namespace MdsInfrastructure
{
    public static class Register
    {
        public static void Everything(WebServer.References refs)
        {
            refs.WebApplication.RegisterRouteHandler<MdsInfrastructure.Flow.Status, Routes.Status.Infra>();
            refs.RegisterPageBuilder<MdsInfrastructure.InfrastructureStatusRenderer, InfrastructureStatus>(new InfrastructureStatusRenderer());
        }
    }
}
