using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Ui;
using System.Collections.Generic;

namespace MdsLocal
{
    public static class Overview
    {
        public class ListProcesses : Metapsi.Route.IGet { }
    }

    public static class SyncHistory
    {
        public class List : Metapsi.Route.IGet { }
        
        public class DataModel : IHasUser, IHasLoadingPanel
        {
            public List<SyncResult> SyncHistory { get; set; } = new List<SyncResult>();
            public SyncResult SelectedResult { get; set; }
            public User User { get; set; } = new();
            public bool IsLoading { get; set; }
        }
    }

    public class ProcessRow
    {
        public string ServiceName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectVersion { get; set; }
        public string Pid { get; set; }
        public string UsedRam { get; set; }
        public string RunningSince { get; set; }
        public bool Disabled { get; set; } = false;
        public bool Running { get; set; } = true;
    }

    public partial class OverviewPage
    {
        public LocalSettings LocalSettings { get; set; } = new();
        public FullLocalStatus FullLocalStatus { get; set; } = new();
        public List<MdsLocal.RunningServiceProcess> ServiceProcesses { get; set; } = new();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<ProcessRow> Processes { get; set; } = new();
        public string OverviewText { get; set; } = string.Empty;
        public ProcessRow RestartProcess { get; set; } = new();
    }

    public static class SyncStatusCodes
    {
        public const string Failed = "Failed";
        public const string UpToDate = "UpToDate";
        public const string Changed = "Changed";
    }

}
