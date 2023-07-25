using Metapsi;
using System;

namespace MdsInfrastructure.Routes
{
    public class Status
    {
        public class Infra : Route.IGet { }
        public class Node : Route.IGet<string> { }
        public class Application : Route.IGet<string> { }
    }

    public class Configuration
    {
        public class List : Route.IGet { }
        public class Add : Route.IGet { }
        public class Edit : Route.IGet { }
    }

    public class Deployment
    {
        public class List : Route.IGet { }
        public class Review : Route.IGet<Guid> { }
    }

    public class Node
    {
        public class List : Route.IGet { }
        public class Edit : Route.IGet<Guid> { }
    }

    public class Project
    {
        public class List : Route.IGet { }
    }

    public class Docs
    {
        public class Service : Route.IGet<string> { }
        public class RedisMap: Route.IGet<string> { }
    }
}
