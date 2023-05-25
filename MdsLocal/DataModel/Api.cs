using Metapsi;
using System.Collections.Generic;

namespace MdsLocal
{
    public static class Api
    {
        public static Request<MdsCommon.InfrastructureNodeSettings> GetInfrastructureNodeSettings { get; set; } = new(nameof(GetInfrastructureNodeSettings));
        public static Request<List<MdsCommon.ServiceConfigurationSnapshot>> GetUpToDateConfiguration { get; set; } = new(nameof(GetUpToDateConfiguration));
    }
}
