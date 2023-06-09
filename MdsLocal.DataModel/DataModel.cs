using Metapsi;
using Metapsi.Ui;
using System.Collections.Generic;

namespace MdsLocal
{
    public static class Overview
    {
        public class ListProcesses : IMetapsiRoute { }
    }

    public static class SyncHistory
    {
        public class List : IMetapsiRoute { }
        
        public class DataModel : IHasUser
        {
            public List<SyncResult> SyncHistory { get; set; } = new List<SyncResult>();
            public User User { get; set; }
        }
    }

    public class ProcessRow
    {
        public string ServiceName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectVersionTag { get; set; }
        public string Pid { get; set; }
        public string UsedRamMB { get; set; }
        public string RunningStatus { get; set; }
        public bool HasError { get; set; } = false;
    }

    public partial class OverviewPage
    {
        public LocalSettings LocalSettings { get; set; } = new();
        public FullLocalStatus FullLocalStatus { get; set; } = new();
        public RecordCollection<MdsLocal.RunningServiceProcess> ServiceProcesses { get; set; } = new RecordCollection<MdsLocal.RunningServiceProcess>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
