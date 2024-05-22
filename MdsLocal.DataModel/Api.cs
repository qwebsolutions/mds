using MdsCommon;
using Metapsi;
using Metapsi.Ui;
using System;
using System.Collections.Generic;

namespace MdsLocal
{
    public static class Api
    {
        public static Request<MdsCommon.InfrastructureNodeSettings> GetInfrastructureNodeSettings { get; set; } = new(nameof(GetInfrastructureNodeSettings));
        public static Request<List<MdsCommon.ServiceConfigurationSnapshot>> GetUpToDateConfiguration { get; set; } = new(nameof(GetUpToDateConfiguration));
    }

    public class ReloadedOverviewModel 
    {
        public OverviewPage Model { get; set; }
    }

    public class FullSyncResultResponse
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
