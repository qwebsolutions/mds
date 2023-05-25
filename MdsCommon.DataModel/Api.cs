using Metapsi;
using System.Collections.Generic;

namespace MdsCommon
{
    public static class NodeCommand
    {
        public const string StopService = "StopService";
    }

    public static class Api
    {
        public static Request<MdsCommon.InfrastructureNodeSettings, string> GetInfrastructureNodeSettings { get; set; } = new(nameof(GetInfrastructureNodeSettings));
        public static Request<List<MdsCommon.ServiceConfigurationSnapshot>, string> GetCurrentNodeSnapshot { get; set; } = new(nameof(GetCurrentNodeSnapshot));
        public static Request<List<InfrastructureEvent>> GetAllInfrastructureEvents { get; set; } = new(nameof(GetAllInfrastructureEvents));
        public static Request<InfrastructureEvent, string> GetMostRecentEventOfService { get; set; } = new(nameof(GetMostRecentEventOfService));
        public static Request<AdminCredentials> GetAdminCredentials { get; set; } = new(nameof(GetAdminCredentials));
    }

    public class AdminCredentials
    {
        public string AdminUserName { get; set; }
        public string AdminPassword { get; set; }
    }
}