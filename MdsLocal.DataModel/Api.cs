using Metapsi;
using System;
using System.Collections.Generic;

namespace MdsLocal
{
    public class ApiResponse
    {

    }

    public class GetUpToDateConfigurationResponse
    {
        public List<MdsCommon.ServiceConfigurationSnapshot> ServiceSnapshots { get; set; }
        public Guid CurrentDeploymentId { get; set; }
    }

    public static class Api
    {
        public static Request<MdsCommon.InfrastructureNodeSettings> GetInfrastructureNodeSettings { get; set; } = new(nameof(GetInfrastructureNodeSettings));
        public static Request<GetUpToDateConfigurationResponse> GetUpToDateConfiguration { get; set; } = new(nameof(GetUpToDateConfiguration));
    }

    public class ReloadedOverviewModel : ApiResponse
    {
        public OverviewPage Model { get; set; }
    }

    public class FullSyncResultResponse : ApiResponse
    {
        public SyncResult SyncResult { get; set; }
    }

    public static class Frontend
    {
        public static Command<string> KillProcessByPid { get; set; } = new(nameof(KillProcessByPid));
        public static Request<ReloadedOverviewModel> ReloadProcesses { get; set; } = new(nameof(ReloadProcesses));
        public static Request<FullSyncResultResponse, Guid> LoadFullSyncResult { get; set; } = new(nameof(LoadFullSyncResult));
    }
}
