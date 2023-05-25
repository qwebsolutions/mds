using MdsCommon;
using Metapsi.Hyperapp;
using Microsoft.AspNetCore.Builder;

namespace MdsInfrastructure
{
    public static class Setup
    {
        public static void AddInfraUi(this Metapsi.WebServer.References webServer)
        {
            var h = webServer.AddHyperapp(Status.Infra, allowAnonymous: true);
            h.RegisterModule(typeof(Configuration));
            h.RegisterModule(typeof(EditConfiguration));
            h.RegisterModule(typeof(Deployments), true);
            h.RegisterModule(typeof(EventsLog), true);
            h.RegisterModule(typeof(Projects));
            h.RegisterModule(typeof(Nodes));
            h.RegisterModule(typeof(Docs), true);
            h.RegisterModule(typeof(SignIn), true);
        }
    }
}

