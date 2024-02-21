﻿using Metapsi;
using Metapsi.Ui;
using System.Collections.Generic;

namespace MdsLocal
{
    public static class Api
    {
        public static Request<MdsCommon.InfrastructureNodeSettings> GetInfrastructureNodeSettings { get; set; } = new(nameof(GetInfrastructureNodeSettings));
        public static Request<List<MdsCommon.ServiceConfigurationSnapshot>> GetUpToDateConfiguration { get; set; } = new(nameof(GetUpToDateConfiguration));
    }

    public class ReloadedOverviewModel : ApiResponse
    {
        public OverviewPage Model { get; set; }
    }

    public static class Frontend
    {
        public static Request<ApiResponse, string> KillProcessByPid { get; set; } = new(nameof(KillProcessByPid));
        public static Request<ReloadedOverviewModel> ReloadProcesses { get; set; } = new(nameof(ReloadProcesses));
    }
}
