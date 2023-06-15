using Metapsi;

namespace MdsInfrastructure.Routes
{
    public class EditConfiguration : Route.IGet { }
    public class ListDeployments : Route.IGet { }

    public class Status
    {
        public class Infra : Route.IGet { }
        public class Node : Route.IGet<string> { }
        public class Application : Route.IGet<string> { }
    }

    public class Configuration
    {
        public class List : Route.IGet { }
    }
    public class Deployments
    {
        public class List : Route.IGet { }
    }

    public class Nodes
    {
        public class List : Route.IGet { }
    }

    public class Projects
    {
        public class List : Route.IGet { }
    }

    public class EventsLog : Route.IGet
    {
        public class List : Route.IGet { }
    }

    public class Docs
    {
        public class Service : Route.IGet<string> { }
    }
}
