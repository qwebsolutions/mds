//using System.Linq;
//using Metapsi.Reflection;

//namespace MdsLocal
//{
//    /// <summary>
//        /// 
//        /// </summary>
//    public static partial class ApiBinariesRetriever
//    {
//        /// <summary>
//                /// 
//                /// </summary>
//        public static partial class SupplySpecifications
//        {
//            /// <summary>
//                        /// 
//                        /// </summary>
//            public partial interface IApiProjectBinaries : Metapsi.Reflection.IDataStructure
//            {
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; }

//                public Metapsi.Reflection.RecordCollection<MdsLocal.GeneralPath> IntoPath { get; set; }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataStructure("c83c0927-981d-43b4-83a7-3c36b5c4263d")]
//            public partial class ApiProjectBinaries : Metapsi.Reflection.IDataStructure, MdsLocal.ApiBinariesRetriever.SupplySpecifications.IApiProjectBinaries
//            {
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
//                public Metapsi.Reflection.RecordCollection<MdsLocal.GeneralPath> IntoPath { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.GeneralPath>();
//                public Diff.DataStructureDiff DiffToPrevious(MdsLocal.ApiBinariesRetriever.SupplySpecifications.ApiProjectBinaries previous)
//                {
//                    var diff = new Diff.DataStructureDiff();
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshot, this.ServiceConfigurationSnapshot, "ServiceConfigurationSnapshot"));
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.IntoPath, this.IntoPath, "IntoPath"));
//                    return diff;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> GetServiceConfigurationSnapshot(MdsLocal.ApiBinariesRetriever.SupplySpecifications.ApiProjectBinaries dataStructure)
//                {
//                    return dataStructure.ServiceConfigurationSnapshot;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshot(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshot").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsLocal.GeneralPath> GetIntoPath(MdsLocal.ApiBinariesRetriever.SupplySpecifications.ApiProjectBinaries dataStructure)
//                {
//                    return dataStructure.IntoPath;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetIntoPath(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("IntoPath").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }
//            }
//        }
//    }

//    /// <summary>
//        /// 
//        /// </summary>
//    [Metapsi.Reflection.DataStructure("a4ff0fd9-82e7-46c9-b171-8e109578b016")]
//    public partial class FullLocalStatus : Metapsi.Reflection.IDataStructure, MdsLocal.IFullLocalStatus
//    {
//        public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> LocalServiceSnapshots { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
//        public Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess> ServiceProcesses { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess>();
//        public Metapsi.Reflection.RecordCollection<MdsLocal.SyncResult> SyncResults { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.SyncResult>();
//        public Diff.DataStructureDiff DiffToPrevious(MdsLocal.FullLocalStatus previous)
//        {
//            var diff = new Diff.DataStructureDiff();
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.LocalServiceSnapshots, this.LocalServiceSnapshots, "LocalServiceSnapshots"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceProcesses, this.ServiceProcesses, "ServiceProcesses"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.SyncResults, this.SyncResults, "SyncResults"));
//            return diff;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> GetLocalServiceSnapshots(MdsLocal.FullLocalStatus dataStructure)
//        {
//            return dataStructure.LocalServiceSnapshots;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetLocalServiceSnapshots(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("LocalServiceSnapshots").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess> GetServiceProcesses(MdsLocal.FullLocalStatus dataStructure)
//        {
//            return dataStructure.ServiceProcesses;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetServiceProcesses(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("ServiceProcesses").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsLocal.SyncResult> GetSyncResults(MdsLocal.FullLocalStatus dataStructure)
//        {
//            return dataStructure.SyncResults;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetSyncResults(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("SyncResults").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }
//    }

//    /// <summary>
//        /// A path. I don't know why it's not just a string. Static typing, maybe?
//        /// </summary>
//    [Metapsi.Reflection.DataItem("879aebca-ad51-4ec6-9ba3-e6bdfcc8a2fe")]
//    public partial class GeneralPath : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.GeneralPath>
//    {
//        [Metapsi.Reflection.DataItemField("6887144d-40dc-4eb6-9b3c-7f8942c65eb9")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [Metapsi.Reflection.DataItemField("cbf70360-c202-4925-915d-3f39cb6fa38b")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String Path { get; set; } = System.String.Empty;
//        public MdsLocal.GeneralPath Clone()
//        {
//            var clone = new MdsLocal.GeneralPath();
//            clone.Id = this.Id;
//            clone.Path = this.Path;
//            return clone;
//        }

//        public static System.Guid GetId(MdsLocal.GeneralPath dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.GeneralPath).Id;
//        }

//        public static System.String GetPath(MdsLocal.GeneralPath dataRecord)
//        {
//            return dataRecord.Path;
//        }

//        public static System.String GetPath(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.GeneralPath).Path;
//        }
//    }

//    /// <summary>
//        /// 
//        /// </summary>
//    public partial interface IFullLocalStatus : Metapsi.Reflection.IDataStructure
//    {
//        public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> LocalServiceSnapshots { get; set; }

//        public Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess> ServiceProcesses { get; set; }

//        public Metapsi.Reflection.RecordCollection<MdsLocal.SyncResult> SyncResults { get; set; }
//    }

//    /// <summary>
//        /// Configuration changes as detected by the synchronization mechanism
//        /// </summary>
//    public partial interface ILocalServicesConfigurationDiff : Metapsi.Reflection.IDataStructure
//    {
//        public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> AddedServices { get; set; }

//        public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> RemovedServices { get; set; }

//        public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ChangedServices { get; set; }

//        public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> IdenticalServices { get; set; }

//        public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> Parameters { get; set; }
//    }

//    /// <summary>
//        /// 
//        /// </summary>
//    public partial interface IOverviewPage : Metapsi.Reflection.IDataStructure, MdsLocal.IFullLocalStatus, UI.Svelte.IPageBehavior
//    {
//        public Metapsi.Reflection.RecordCollection<MdsLocal.LocalSettings> LocalSettings { get; set; }
//    }

//    /// <summary>
//        /// Running OS processes of services owned by a local controller
//        /// </summary>
//    public partial interface IOwnedRunningServiceProcesses : Metapsi.Reflection.IDataStructure
//    {
//        public Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess> ServiceProcesses { get; set; }
//    }

//    /// <summary>
//        /// Diff from configured processes (should be running) to current OS processes (actually running); Can be overlapped (first stop, then start again=restart)
//        /// </summary>
//    public partial interface IRunningProcessesDiff : Metapsi.Reflection.IDataStructure
//    {
//        public Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess> ToStart { get; set; }

//        public Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess> ToStop { get; set; }
//    }

//    /// <summary>
//        /// 
//        /// </summary>
//    public partial interface ISyncHistory : Metapsi.Reflection.IDataStructure
//    {
//        public Metapsi.Reflection.RecordCollection<MdsLocal.SyncResult> SyncResults { get; set; }

//        public Metapsi.Reflection.RecordCollection<MdsLocal.SyncUpdatedConfiguration> UpdatedConfigurations { get; set; }
//    }

//    /// <summary>
//        /// 
//        /// </summary>
//    public partial interface ISyncHistoryPage : Metapsi.Reflection.IDataStructure, MdsLocal.ISyncHistory, UI.Svelte.IPageBehavior
//    {
//    }


//    /// <summary>
//        /// Main application logic
//        /// </summary>
//    public static partial class MdsLocalApplication
//    {
//        /// <summary>
//                /// 
//                /// </summary>
//        public static partial class Command
//        {
//            /// <summary>
//                        /// Accumulates data about machine health
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("8308a9c0-2ceb-4dc6-9766-878db4ea42e3")]
//            public partial class CheckMachineStatus : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.Command.CheckMachineStatus>
//            {
//                [Metapsi.Reflection.DataItemField("71165c18-21cf-48ab-8fea-3d0e66447cd7")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                [Metapsi.Reflection.DataItemField("f56d80fd-78a7-4b16-af50-5ba0958aed9d")]
//                [Metapsi.Reflection.ScalarTypeName("String")]
//                public System.String NodeName { get; set; } = System.String.Empty;
//                public MdsLocal.MdsLocalApplication.Command.CheckMachineStatus Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.Command.CheckMachineStatus();
//                    clone.Id = this.Id;
//                    clone.NodeName = this.NodeName;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.Command.CheckMachineStatus dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Command.CheckMachineStatus).Id;
//                }

//                public static System.String GetNodeName(MdsLocal.MdsLocalApplication.Command.CheckMachineStatus dataRecord)
//                {
//                    return dataRecord.NodeName;
//                }

//                public static System.String GetNodeName(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Command.CheckMachineStatus).NodeName;
//                }
//            }

//            /// <summary>
//                        /// Creates service folder containing project binaries and configuration files
//                        /// </summary>
//            public partial interface ISetupService : Metapsi.Reflection.IDataStructure, MdsCommon.ISingleServiceConfigurationSnapshot
//            {
//            }

//            /// <summary>
//                        /// Creates service folder containing project binaries and configuration files
//                        /// </summary>
//            [Metapsi.Reflection.DataStructure("1a914097-1479-4cd3-bac4-24fb7e06c5a5")]
//            public partial class SetupService : Metapsi.Reflection.IDataStructure, MdsLocal.MdsLocalApplication.Command.ISetupService, MdsCommon.ISingleServiceConfigurationSnapshot
//            {
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> ServiceConfigurationSnapshotParameters { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter>();
//                public MdsLocal.MdsLocalApplication.Command.SetupService AddSingleServiceConfigurationSnapshot(MdsCommon.ISingleServiceConfigurationSnapshot source)
//                {
//                    this.ServiceConfigurationSnapshot.AddRange(source.ServiceConfigurationSnapshot.Clone());
//                    this.ServiceConfigurationSnapshotParameters.AddRange(source.ServiceConfigurationSnapshotParameters.Clone());
//                    return this;
//                }

//                public MdsLocal.MdsLocalApplication.Command.SetupService UpdateSingleServiceConfigurationSnapshot(MdsCommon.ISingleServiceConfigurationSnapshot source)
//                {
//                    this.ServiceConfigurationSnapshot.Update(source.ServiceConfigurationSnapshot.Clone());
//                    this.ServiceConfigurationSnapshotParameters.Update(source.ServiceConfigurationSnapshotParameters.Clone());
//                    return this;
//                }

//                public MdsCommon.SingleServiceConfigurationSnapshot ExtractSingleServiceConfigurationSnapshot()
//                {
//                    var output = new MdsCommon.SingleServiceConfigurationSnapshot();
//                    output.ServiceConfigurationSnapshot.AddRange(this.ServiceConfigurationSnapshot.Clone());
//                    output.ServiceConfigurationSnapshotParameters.AddRange(this.ServiceConfigurationSnapshotParameters.Clone());
//                    return output;
//                }

//                public Diff.DataStructureDiff DiffToPrevious(MdsLocal.MdsLocalApplication.Command.SetupService previous)
//                {
//                    var diff = new Diff.DataStructureDiff();
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshot, this.ServiceConfigurationSnapshot, "ServiceConfigurationSnapshot"));
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshotParameters, this.ServiceConfigurationSnapshotParameters, "ServiceConfigurationSnapshotParameters"));
//                    return diff;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> GetServiceConfigurationSnapshot(MdsLocal.MdsLocalApplication.Command.SetupService dataStructure)
//                {
//                    return dataStructure.ServiceConfigurationSnapshot;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshot(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshot").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> GetServiceConfigurationSnapshotParameters(MdsLocal.MdsLocalApplication.Command.SetupService dataStructure)
//                {
//                    return dataStructure.ServiceConfigurationSnapshotParameters;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshotParameters(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshotParameters").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }
//            }

//            /// <summary>
//                        /// Start processes 
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("7752cf9c-8ae3-4dc0-bbc2-36f12b959a19")]
//            public partial class StartProcesses : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.Command.StartProcesses>
//            {
//                [Metapsi.Reflection.DataItemField("0d00ed84-11b1-4b7e-87ca-93024889cc60")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                public MdsLocal.MdsLocalApplication.Command.StartProcesses Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.Command.StartProcesses();
//                    clone.Id = this.Id;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.Command.StartProcesses dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Command.StartProcesses).Id;
//                }
//            }

//            /// <summary>
//                        /// Stop removed/changed services
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("699466c9-086f-45af-a076-61b0ecbd9ff3")]
//            public partial class StopServices : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.Command.StopServices>
//            {
//                [Metapsi.Reflection.DataItemField("219b5f2b-32a6-4875-8f74-361b5291b866")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                public MdsLocal.MdsLocalApplication.Command.StopServices Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.Command.StopServices();
//                    clone.Id = this.Id;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.Command.StopServices dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Command.StopServices).Id;
//                }
//            }

//            /// <summary>
//                        /// Update local stored configuration with remote one, if different
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("6fd93528-0dbf-4c60-83ae-08a38ca0908c")]
//            public partial class SynchronizeConfiguration : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.Command.SynchronizeConfiguration>
//            {
//                [Metapsi.Reflection.DataItemField("6e997803-c9a3-434c-bc7b-5afa001dd84e")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                [Metapsi.Reflection.DataItemField("5624dc46-0927-4ee6-9a00-12ebb50895cd")]
//                [Metapsi.Reflection.ScalarTypeName("String")]
//                public System.String Trigger { get; set; } = System.String.Empty;
//                public MdsLocal.MdsLocalApplication.Command.SynchronizeConfiguration Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.Command.SynchronizeConfiguration();
//                    clone.Id = this.Id;
//                    clone.Trigger = this.Trigger;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.Command.SynchronizeConfiguration dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Command.SynchronizeConfiguration).Id;
//                }

//                public static System.String GetTrigger(MdsLocal.MdsLocalApplication.Command.SynchronizeConfiguration dataRecord)
//                {
//                    return dataRecord.Trigger;
//                }

//                public static System.String GetTrigger(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Command.SynchronizeConfiguration).Trigger;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("420df0b6-f626-4aef-83ef-2f69c0e453d0")]
//            public partial class SynchronizeProcessesToConfiguration : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.Command.SynchronizeProcessesToConfiguration>
//            {
//                [Metapsi.Reflection.DataItemField("19f41a99-0621-4d5a-a9a6-da8266650691")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                public MdsLocal.MdsLocalApplication.Command.SynchronizeProcessesToConfiguration Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.Command.SynchronizeProcessesToConfiguration();
//                    clone.Id = this.Id;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.Command.SynchronizeProcessesToConfiguration dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Command.SynchronizeProcessesToConfiguration).Id;
//                }
//            }

//            /// <summary>
//                        /// Add/update/remove service folders
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("4f7ca28b-254b-4cca-8fd2-5493d2010656")]
//            public partial class SynchronizeServicesSetup : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.Command.SynchronizeServicesSetup>
//            {
//                [Metapsi.Reflection.DataItemField("3893ecf7-2ee8-49a4-89fa-af925bbc8ed5")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                public MdsLocal.MdsLocalApplication.Command.SynchronizeServicesSetup Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.Command.SynchronizeServicesSetup();
//                    clone.Id = this.Id;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.Command.SynchronizeServicesSetup dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Command.SynchronizeServicesSetup).Id;
//                }
//            }
//        }

//        /// <summary>
//                /// 
//                /// </summary>
//        public static partial class DemandSpecifications
//        {
//            /// <summary>
//                        /// 
//                        /// </summary>
//            public partial interface IFillServiceProjectBinary : Metapsi.Reflection.IDataStructure
//            {
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; }

//                public Metapsi.Reflection.RecordCollection<MdsLocal.GeneralPath> ServiceBinaryPath { get; set; }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataStructure("bf42f1d1-f1a3-49fb-9000-9ee2b0b7bf19")]
//            public partial class FillServiceProjectBinary : Metapsi.Reflection.IDataStructure, MdsLocal.MdsLocalApplication.DemandSpecifications.IFillServiceProjectBinary
//            {
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
//                public Metapsi.Reflection.RecordCollection<MdsLocal.GeneralPath> ServiceBinaryPath { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.GeneralPath>();
//                public Diff.DataStructureDiff DiffToPrevious(MdsLocal.MdsLocalApplication.DemandSpecifications.FillServiceProjectBinary previous)
//                {
//                    var diff = new Diff.DataStructureDiff();
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshot, this.ServiceConfigurationSnapshot, "ServiceConfigurationSnapshot"));
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceBinaryPath, this.ServiceBinaryPath, "ServiceBinaryPath"));
//                    return diff;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> GetServiceConfigurationSnapshot(MdsLocal.MdsLocalApplication.DemandSpecifications.FillServiceProjectBinary dataStructure)
//                {
//                    return dataStructure.ServiceConfigurationSnapshot;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshot(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshot").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsLocal.GeneralPath> GetServiceBinaryPath(MdsLocal.MdsLocalApplication.DemandSpecifications.FillServiceProjectBinary dataStructure)
//                {
//                    return dataStructure.ServiceBinaryPath;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceBinaryPath(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceBinaryPath").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }
//            }

//            /// <summary>
//                        /// Loads the current configuration, as known (and persisted) up to this point
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("e88e7550-62cc-4f13-a7b2-297bf37e477e")]
//            public partial class GetLocalKnownConfiguration : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.DemandSpecifications.GetLocalKnownConfiguration>
//            {
//                [Metapsi.Reflection.DataItemField("e6203b9b-d7bf-4173-a50c-0e5505a5c403")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                public MdsLocal.MdsLocalApplication.DemandSpecifications.GetLocalKnownConfiguration Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.DemandSpecifications.GetLocalKnownConfiguration();
//                    clone.Id = this.Id;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.DemandSpecifications.GetLocalKnownConfiguration dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.DemandSpecifications.GetLocalKnownConfiguration).Id;
//                }
//            }

//            /// <summary>
//                        /// Gets the local settings, whatever that means ...
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("60639064-7af7-4710-953d-30ab7a77c8af")]
//            public partial class GetLocalSettings : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.DemandSpecifications.GetLocalSettings>
//            {
//                [Metapsi.Reflection.DataItemField("7dc5ea75-92ba-4522-ae16-ffb8f2a075c8")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                public MdsLocal.MdsLocalApplication.DemandSpecifications.GetLocalSettings Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.DemandSpecifications.GetLocalSettings();
//                    clone.Id = this.Id;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.DemandSpecifications.GetLocalSettings dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.DemandSpecifications.GetLocalSettings).Id;
//                }
//            }

//            ///// <summary>
//            //            /// Gets mds.json data related to installed service
//            //            /// </summary>
//            //[Metapsi.Reflection.DataItem("28217272-eaf4-4878-a1e9-248313f3485d")]
//            //public partial class GetServiceVersionData : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.DemandSpecifications.GetServiceVersionData>
//            //{
//            //    [Metapsi.Reflection.DataItemField("6a1fbce0-cfa3-4c19-9abb-4047649a3f16")]
//            //    [Metapsi.Reflection.ScalarTypeName("Id")]
//            //    public System.Guid Id { get; set; } = System.Guid.NewGuid();
//            //    [Metapsi.Reflection.DataItemField("377431ce-d55b-4f07-87bc-241765553841")]
//            //    [Metapsi.Reflection.ScalarTypeName("String")]
//            //    public System.String ServiceName { get; set; } = System.String.Empty;
//            //    public MdsLocal.MdsLocalApplication.DemandSpecifications.GetServiceVersionData Clone()
//            //    {
//            //        var clone = new MdsLocal.MdsLocalApplication.DemandSpecifications.GetServiceVersionData();
//            //        clone.Id = this.Id;
//            //        clone.ServiceName = this.ServiceName;
//            //        return clone;
//            //    }

//            //    public static System.Guid GetId(MdsLocal.MdsLocalApplication.DemandSpecifications.GetServiceVersionData dataRecord)
//            //    {
//            //        return dataRecord.Id;
//            //    }

//            //    public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//            //    {
//            //        return (dataRecord as MdsLocal.MdsLocalApplication.DemandSpecifications.GetServiceVersionData).Id;
//            //    }

//            //    public static System.String GetServiceName(MdsLocal.MdsLocalApplication.DemandSpecifications.GetServiceVersionData dataRecord)
//            //    {
//            //        return dataRecord.ServiceName;
//            //    }

//            //    public static System.String GetServiceName(Metapsi.Reflection.IRecord dataRecord)
//            //    {
//            //        return (dataRecord as MdsLocal.MdsLocalApplication.DemandSpecifications.GetServiceVersionData).ServiceName;
//            //    }
//            //}

//            /// <summary>
//                        /// Gets the (possibly changed) configuration from a remote service
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("6a70c8dd-5d5b-4fda-91cb-87004f259828")]
//            public partial class GetUpToDateConfiguration : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.DemandSpecifications.GetUpToDateConfiguration>
//            {
//                [Metapsi.Reflection.DataItemField("f6434fd3-cb9f-43cd-9fbb-369031474324")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                public MdsLocal.MdsLocalApplication.DemandSpecifications.GetUpToDateConfiguration Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.DemandSpecifications.GetUpToDateConfiguration();
//                    clone.Id = this.Id;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.DemandSpecifications.GetUpToDateConfiguration dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.DemandSpecifications.GetUpToDateConfiguration).Id;
//                }
//            }

//            /// <summary>
//                        /// Loads status to be displayed in overview page
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("29fb1d43-1647-44ec-83cf-a05a524e9684")]
//            public partial class LoadFullLocalStatus : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.DemandSpecifications.LoadFullLocalStatus>
//            {
//                [Metapsi.Reflection.DataItemField("704d161b-ed16-4f0e-8c93-16d33a5fd8e2")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                public MdsLocal.MdsLocalApplication.DemandSpecifications.LoadFullLocalStatus Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.DemandSpecifications.LoadFullLocalStatus();
//                    clone.Id = this.Id;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.DemandSpecifications.LoadFullLocalStatus dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.DemandSpecifications.LoadFullLocalStatus).Id;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("4ad442ec-6c2d-4b54-8ed9-216e0bcb3e86")]
//            public partial class LoadServiceConfiguration : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.DemandSpecifications.LoadServiceConfiguration>
//            {
//                [Metapsi.Reflection.DataItemField("64484e46-4370-4582-8740-d6a8d302910c")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                [Metapsi.Reflection.DataItemField("eb9ae424-3616-4946-a347-519e546a601c")]
//                [Metapsi.Reflection.ScalarTypeName("String")]
//                public System.String ServiceName { get; set; } = System.String.Empty;
//                public MdsLocal.MdsLocalApplication.DemandSpecifications.LoadServiceConfiguration Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.DemandSpecifications.LoadServiceConfiguration();
//                    clone.Id = this.Id;
//                    clone.ServiceName = this.ServiceName;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.DemandSpecifications.LoadServiceConfiguration dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.DemandSpecifications.LoadServiceConfiguration).Id;
//                }

//                public static System.String GetServiceName(MdsLocal.MdsLocalApplication.DemandSpecifications.LoadServiceConfiguration dataRecord)
//                {
//                    return dataRecord.ServiceName;
//                }

//                public static System.String GetServiceName(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.DemandSpecifications.LoadServiceConfiguration).ServiceName;
//                }
//            }

//            /// <summary>
//                        /// Loads running process data for all known processes
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("52c311f2-47f5-4d90-9c6e-2b37f4bb5e22")]
//            public partial class LoadServiceProcesses : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.DemandSpecifications.LoadServiceProcesses>
//            {
//                [Metapsi.Reflection.DataItemField("bc220b1b-b292-493b-afb2-bd14ebfc55ae")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                [Metapsi.Reflection.DataItemField("c8b53f37-9579-417c-9983-2a1d50b790e8")]
//                [Metapsi.Reflection.ScalarTypeName("String")]
//                public System.String LocalControllerName { get; set; } = System.String.Empty;
//                public MdsLocal.MdsLocalApplication.DemandSpecifications.LoadServiceProcesses Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.DemandSpecifications.LoadServiceProcesses();
//                    clone.Id = this.Id;
//                    clone.LocalControllerName = this.LocalControllerName;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.DemandSpecifications.LoadServiceProcesses dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.DemandSpecifications.LoadServiceProcesses).Id;
//                }

//                public static System.String GetLocalControllerName(MdsLocal.MdsLocalApplication.DemandSpecifications.LoadServiceProcesses dataRecord)
//                {
//                    return dataRecord.LocalControllerName;
//                }

//                public static System.String GetLocalControllerName(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.DemandSpecifications.LoadServiceProcesses).LocalControllerName;
//                }
//            }

//            /// <summary>
//                        /// Loads all sync history entries
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("95d16075-30fe-40f9-b1a1-6df50ecf251b")]
//            public partial class LoadSyncHistory : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.DemandSpecifications.LoadSyncHistory>
//            {
//                [Metapsi.Reflection.DataItemField("0e0b2e19-d817-4308-9998-0b678abc91fa")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                public MdsLocal.MdsLocalApplication.DemandSpecifications.LoadSyncHistory Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.DemandSpecifications.LoadSyncHistory();
//                    clone.Id = this.Id;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.DemandSpecifications.LoadSyncHistory dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.DemandSpecifications.LoadSyncHistory).Id;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            public partial interface IRegisterSyncResult : Metapsi.Reflection.IDataStructure, MdsLocal.ISyncHistory
//            {
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataStructure("5c4a19e9-9f9a-47a1-9a89-af04c6af53ab")]
//            public partial class RegisterSyncResult : Metapsi.Reflection.IDataStructure, MdsLocal.MdsLocalApplication.DemandSpecifications.IRegisterSyncResult, MdsLocal.ISyncHistory
//            {
//                public Metapsi.Reflection.RecordCollection<MdsLocal.SyncResult> SyncResults { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.SyncResult>();
//                public Metapsi.Reflection.RecordCollection<MdsLocal.SyncUpdatedConfiguration> UpdatedConfigurations { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.SyncUpdatedConfiguration>();
//                public MdsLocal.MdsLocalApplication.DemandSpecifications.RegisterSyncResult AddSyncHistory(MdsLocal.ISyncHistory source)
//                {
//                    this.SyncResults.AddRange(source.SyncResults.Clone());
//                    this.UpdatedConfigurations.AddRange(source.UpdatedConfigurations.Clone());
//                    return this;
//                }

//                public MdsLocal.MdsLocalApplication.DemandSpecifications.RegisterSyncResult UpdateSyncHistory(MdsLocal.ISyncHistory source)
//                {
//                    this.SyncResults.Update(source.SyncResults.Clone());
//                    this.UpdatedConfigurations.Update(source.UpdatedConfigurations.Clone());
//                    return this;
//                }

//                public MdsLocal.SyncHistory ExtractSyncHistory()
//                {
//                    var output = new MdsLocal.SyncHistory();
//                    output.SyncResults.AddRange(this.SyncResults.Clone());
//                    output.UpdatedConfigurations.AddRange(this.UpdatedConfigurations.Clone());
//                    return output;
//                }

//                public Diff.DataStructureDiff DiffToPrevious(MdsLocal.MdsLocalApplication.DemandSpecifications.RegisterSyncResult previous)
//                {
//                    var diff = new Diff.DataStructureDiff();
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.SyncResults, this.SyncResults, "SyncResults"));
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.UpdatedConfigurations, this.UpdatedConfigurations, "UpdatedConfigurations"));
//                    return diff;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsLocal.SyncResult> GetSyncResults(MdsLocal.MdsLocalApplication.DemandSpecifications.RegisterSyncResult dataStructure)
//                {
//                    return dataStructure.SyncResults;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetSyncResults(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("SyncResults").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsLocal.SyncUpdatedConfiguration> GetUpdatedConfigurations(MdsLocal.MdsLocalApplication.DemandSpecifications.RegisterSyncResult dataStructure)
//                {
//                    return dataStructure.UpdatedConfigurations;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetUpdatedConfigurations(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("UpdatedConfigurations").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            public partial interface ISetNewConfiguration : Metapsi.Reflection.IDataStructure, MdsCommon.INodeServiceSnapshots, MdsCommon.ISingleServiceConfigurationSnapshot
//            {
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataStructure("a621326b-f8d7-4b5c-b94d-2e89fa241bef")]
//            public partial class SetNewConfiguration : Metapsi.Reflection.IDataStructure, MdsLocal.MdsLocalApplication.DemandSpecifications.ISetNewConfiguration, MdsCommon.INodeServiceSnapshots, MdsCommon.ISingleServiceConfigurationSnapshot
//            {
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> ServiceConfigurationSnapshotParameters { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter>();
//                public MdsLocal.MdsLocalApplication.DemandSpecifications.SetNewConfiguration AddNodeServiceSnapshots(MdsCommon.INodeServiceSnapshots source)
//                {
//                    this.ServiceConfigurationSnapshot.AddRange(source.ServiceConfigurationSnapshot.Clone());
//                    this.ServiceConfigurationSnapshotParameters.AddRange(source.ServiceConfigurationSnapshotParameters.Clone());
//                    return this;
//                }

//                public MdsLocal.MdsLocalApplication.DemandSpecifications.SetNewConfiguration UpdateNodeServiceSnapshots(MdsCommon.INodeServiceSnapshots source)
//                {
//                    this.ServiceConfigurationSnapshot.Update(source.ServiceConfigurationSnapshot.Clone());
//                    this.ServiceConfigurationSnapshotParameters.Update(source.ServiceConfigurationSnapshotParameters.Clone());
//                    return this;
//                }

//                public MdsCommon.NodeServiceSnapshots ExtractNodeServiceSnapshots()
//                {
//                    var output = new MdsCommon.NodeServiceSnapshots();
//                    output.ServiceConfigurationSnapshot.AddRange(this.ServiceConfigurationSnapshot.Clone());
//                    output.ServiceConfigurationSnapshotParameters.AddRange(this.ServiceConfigurationSnapshotParameters.Clone());
//                    return output;
//                }

//                public Diff.DataStructureDiff DiffToPrevious(MdsLocal.MdsLocalApplication.DemandSpecifications.SetNewConfiguration previous)
//                {
//                    var diff = new Diff.DataStructureDiff();
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshot, this.ServiceConfigurationSnapshot, "ServiceConfigurationSnapshot"));
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshotParameters, this.ServiceConfigurationSnapshotParameters, "ServiceConfigurationSnapshotParameters"));
//                    return diff;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> GetServiceConfigurationSnapshot(MdsLocal.MdsLocalApplication.DemandSpecifications.SetNewConfiguration dataStructure)
//                {
//                    return dataStructure.ServiceConfigurationSnapshot;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshot(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshot").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> GetServiceConfigurationSnapshotParameters(MdsLocal.MdsLocalApplication.DemandSpecifications.SetNewConfiguration dataStructure)
//                {
//                    return dataStructure.ServiceConfigurationSnapshotParameters;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshotParameters(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshotParameters").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }
//            }
//        }

//        /// <summary>
//                /// 
//                /// </summary>
//        public static partial class Event
//        {
//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("63860c06-826c-4b0c-941a-2705ba6db641")]
//            public partial class ApplicationInitialized : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.Event.ApplicationInitialized>
//            {
//                [Metapsi.Reflection.DataItemField("39dd5d60-5fbd-4baa-a58c-665261b4072b")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                public MdsLocal.MdsLocalApplication.Event.ApplicationInitialized Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.Event.ApplicationInitialized();
//                    clone.Id = this.Id;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.Event.ApplicationInitialized dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Event.ApplicationInitialized).Id;
//                }
//            }

//            /// <summary>
//                        /// Triggered when the local configuration has any change 
//                        /// </summary>
//            public partial interface IConfigurationChanged : Metapsi.Reflection.IDataStructure, MdsLocal.ILocalServicesConfigurationDiff
//            {
//            }


//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("b3e65e9c-e2c1-4446-8251-8d885e12aeb8")]
//            public partial class ConfigurationSynchronized : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.Event.ConfigurationSynchronized>
//            {
//                [Metapsi.Reflection.DataItemField("49d553b4-d908-462b-8bf8-a86daf0ef5c6")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                [Metapsi.Reflection.DataItemField("77be2b6d-388b-4027-96b7-b428281259fc")]
//                [Metapsi.Reflection.ScalarTypeName("String")]
//                public System.String ResultCode { get; set; } = System.String.Empty;
//                public MdsLocal.MdsLocalApplication.Event.ConfigurationSynchronized Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.Event.ConfigurationSynchronized();
//                    clone.Id = this.Id;
//                    clone.ResultCode = this.ResultCode;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.Event.ConfigurationSynchronized dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Event.ConfigurationSynchronized).Id;
//                }

//                public static System.String GetResultCode(MdsLocal.MdsLocalApplication.Event.ConfigurationSynchronized dataRecord)
//                {
//                    return dataRecord.ResultCode;
//                }

//                public static System.String GetResultCode(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Event.ConfigurationSynchronized).ResultCode;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            public partial interface IHealthPing : Metapsi.Reflection.IDataStructure, MdsCommon.IHealthStatus
//            {
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataStructure("27ac1ecc-6345-4027-b18d-f36cbe403ac1")]
//            public partial class HealthPing : Metapsi.Reflection.IDataStructure, MdsLocal.MdsLocalApplication.Event.IHealthPing, MdsCommon.IHealthStatus
//            {
//                public Metapsi.Reflection.RecordCollection<MdsCommon.MachineStatus> MachineStatus { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.MachineStatus>();
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceStatus> ServiceStatuses { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceStatus>();
//                public MdsLocal.MdsLocalApplication.Event.HealthPing AddHealthStatus(MdsCommon.IHealthStatus source)
//                {
//                    this.MachineStatus.AddRange(source.MachineStatus.Clone());
//                    this.ServiceStatuses.AddRange(source.ServiceStatuses.Clone());
//                    return this;
//                }

//                public MdsLocal.MdsLocalApplication.Event.HealthPing UpdateHealthStatus(MdsCommon.IHealthStatus source)
//                {
//                    this.MachineStatus.Update(source.MachineStatus.Clone());
//                    this.ServiceStatuses.Update(source.ServiceStatuses.Clone());
//                    return this;
//                }

//                public MdsCommon.HealthStatus ExtractHealthStatus()
//                {
//                    var output = new MdsCommon.HealthStatus();
//                    output.MachineStatus.AddRange(this.MachineStatus.Clone());
//                    output.ServiceStatuses.AddRange(this.ServiceStatuses.Clone());
//                    return output;
//                }

//                public Diff.DataStructureDiff DiffToPrevious(MdsLocal.MdsLocalApplication.Event.HealthPing previous)
//                {
//                    var diff = new Diff.DataStructureDiff();
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.MachineStatus, this.MachineStatus, "MachineStatus"));
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceStatuses, this.ServiceStatuses, "ServiceStatuses"));
//                    return diff;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsCommon.MachineStatus> GetMachineStatus(MdsLocal.MdsLocalApplication.Event.HealthPing dataStructure)
//                {
//                    return dataStructure.MachineStatus;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetMachineStatus(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("MachineStatus").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceStatus> GetServiceStatuses(MdsLocal.MdsLocalApplication.Event.HealthPing dataStructure)
//                {
//                    return dataStructure.ServiceStatuses;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceStatuses(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceStatuses").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            public partial interface IServiceAdded : Metapsi.Reflection.IDataStructure, MdsCommon.ISingleServiceConfigurationSnapshot
//            {
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataStructure("8d451272-0825-4d50-99ff-aa65492b2fcc")]
//            public partial class ServiceAdded : Metapsi.Reflection.IDataStructure, MdsLocal.MdsLocalApplication.Event.IServiceAdded, MdsCommon.ISingleServiceConfigurationSnapshot
//            {
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> ServiceConfigurationSnapshotParameters { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter>();
//                public MdsLocal.MdsLocalApplication.Event.ServiceAdded AddSingleServiceConfigurationSnapshot(MdsCommon.ISingleServiceConfigurationSnapshot source)
//                {
//                    this.ServiceConfigurationSnapshot.AddRange(source.ServiceConfigurationSnapshot.Clone());
//                    this.ServiceConfigurationSnapshotParameters.AddRange(source.ServiceConfigurationSnapshotParameters.Clone());
//                    return this;
//                }

//                public MdsLocal.MdsLocalApplication.Event.ServiceAdded UpdateSingleServiceConfigurationSnapshot(MdsCommon.ISingleServiceConfigurationSnapshot source)
//                {
//                    this.ServiceConfigurationSnapshot.Update(source.ServiceConfigurationSnapshot.Clone());
//                    this.ServiceConfigurationSnapshotParameters.Update(source.ServiceConfigurationSnapshotParameters.Clone());
//                    return this;
//                }

//                public MdsCommon.SingleServiceConfigurationSnapshot ExtractSingleServiceConfigurationSnapshot()
//                {
//                    var output = new MdsCommon.SingleServiceConfigurationSnapshot();
//                    output.ServiceConfigurationSnapshot.AddRange(this.ServiceConfigurationSnapshot.Clone());
//                    output.ServiceConfigurationSnapshotParameters.AddRange(this.ServiceConfigurationSnapshotParameters.Clone());
//                    return output;
//                }

//                public Diff.DataStructureDiff DiffToPrevious(MdsLocal.MdsLocalApplication.Event.ServiceAdded previous)
//                {
//                    var diff = new Diff.DataStructureDiff();
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshot, this.ServiceConfigurationSnapshot, "ServiceConfigurationSnapshot"));
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshotParameters, this.ServiceConfigurationSnapshotParameters, "ServiceConfigurationSnapshotParameters"));
//                    return diff;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> GetServiceConfigurationSnapshot(MdsLocal.MdsLocalApplication.Event.ServiceAdded dataStructure)
//                {
//                    return dataStructure.ServiceConfigurationSnapshot;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshot(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshot").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> GetServiceConfigurationSnapshotParameters(MdsLocal.MdsLocalApplication.Event.ServiceAdded dataStructure)
//                {
//                    return dataStructure.ServiceConfigurationSnapshotParameters;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshotParameters(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshotParameters").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            public partial interface IServiceAlreadyAtVersion : Metapsi.Reflection.IDataStructure
//            {
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; }

//                public Metapsi.Reflection.RecordCollection<MdsLocal.ServiceBinary> ServiceBinary { get; set; }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataStructure("bf32b0a6-1fd4-4446-b727-065daa61f3bf")]
//            public partial class ServiceAlreadyAtVersion : Metapsi.Reflection.IDataStructure, MdsLocal.MdsLocalApplication.Event.IServiceAlreadyAtVersion
//            {
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
//                public Metapsi.Reflection.RecordCollection<MdsLocal.ServiceBinary> ServiceBinary { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.ServiceBinary>();
//                public Diff.DataStructureDiff DiffToPrevious(MdsLocal.MdsLocalApplication.Event.ServiceAlreadyAtVersion previous)
//                {
//                    var diff = new Diff.DataStructureDiff();
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshot, this.ServiceConfigurationSnapshot, "ServiceConfigurationSnapshot"));
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceBinary, this.ServiceBinary, "ServiceBinary"));
//                    return diff;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> GetServiceConfigurationSnapshot(MdsLocal.MdsLocalApplication.Event.ServiceAlreadyAtVersion dataStructure)
//                {
//                    return dataStructure.ServiceConfigurationSnapshot;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshot(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshot").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsLocal.ServiceBinary> GetServiceBinary(MdsLocal.MdsLocalApplication.Event.ServiceAlreadyAtVersion dataStructure)
//                {
//                    return dataStructure.ServiceBinary;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceBinary(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceBinary").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("f12d799c-dac4-4bb2-92be-b8c5caf4e23f")]
//            public partial class ServiceBinaryCreated : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.Event.ServiceBinaryCreated>
//            {
//                [Metapsi.Reflection.DataItemField("ab53819c-d052-45f9-b9fc-8ac52407ef8a")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                [Metapsi.Reflection.DataItemField("fce9b8b0-61b1-41f5-8c0b-f5448cdc558f")]
//                [Metapsi.Reflection.ScalarTypeName("String")]
//                public System.String ServiceName { get; set; } = System.String.Empty;
//                [Metapsi.Reflection.DataItemField("0002176d-43af-4da0-a9b0-0e2f00bdf59d")]
//                [Metapsi.Reflection.ScalarTypeName("String")]
//                public System.String FullExePath { get; set; } = System.String.Empty;
//                public MdsLocal.MdsLocalApplication.Event.ServiceBinaryCreated Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.Event.ServiceBinaryCreated();
//                    clone.Id = this.Id;
//                    clone.ServiceName = this.ServiceName;
//                    clone.FullExePath = this.FullExePath;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.Event.ServiceBinaryCreated dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Event.ServiceBinaryCreated).Id;
//                }

//                public static System.String GetServiceName(MdsLocal.MdsLocalApplication.Event.ServiceBinaryCreated dataRecord)
//                {
//                    return dataRecord.ServiceName;
//                }

//                public static System.String GetServiceName(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Event.ServiceBinaryCreated).ServiceName;
//                }

//                public static System.String GetFullExePath(MdsLocal.MdsLocalApplication.Event.ServiceBinaryCreated dataRecord)
//                {
//                    return dataRecord.FullExePath;
//                }

//                public static System.String GetFullExePath(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Event.ServiceBinaryCreated).FullExePath;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            public partial interface IServiceConfigurationChanged : Metapsi.Reflection.IDataStructure, MdsCommon.ISingleServiceConfigurationSnapshot
//            {
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataStructure("a9989288-2506-409f-9839-d9c5677d3ab0")]
//            public partial class ServiceConfigurationChanged : Metapsi.Reflection.IDataStructure, MdsLocal.MdsLocalApplication.Event.IServiceConfigurationChanged, MdsCommon.ISingleServiceConfigurationSnapshot
//            {
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> ServiceConfigurationSnapshotParameters { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter>();
//                public MdsLocal.MdsLocalApplication.Event.ServiceConfigurationChanged AddSingleServiceConfigurationSnapshot(MdsCommon.ISingleServiceConfigurationSnapshot source)
//                {
//                    this.ServiceConfigurationSnapshot.AddRange(source.ServiceConfigurationSnapshot.Clone());
//                    this.ServiceConfigurationSnapshotParameters.AddRange(source.ServiceConfigurationSnapshotParameters.Clone());
//                    return this;
//                }

//                public MdsLocal.MdsLocalApplication.Event.ServiceConfigurationChanged UpdateSingleServiceConfigurationSnapshot(MdsCommon.ISingleServiceConfigurationSnapshot source)
//                {
//                    this.ServiceConfigurationSnapshot.Update(source.ServiceConfigurationSnapshot.Clone());
//                    this.ServiceConfigurationSnapshotParameters.Update(source.ServiceConfigurationSnapshotParameters.Clone());
//                    return this;
//                }

//                public MdsCommon.SingleServiceConfigurationSnapshot ExtractSingleServiceConfigurationSnapshot()
//                {
//                    var output = new MdsCommon.SingleServiceConfigurationSnapshot();
//                    output.ServiceConfigurationSnapshot.AddRange(this.ServiceConfigurationSnapshot.Clone());
//                    output.ServiceConfigurationSnapshotParameters.AddRange(this.ServiceConfigurationSnapshotParameters.Clone());
//                    return output;
//                }

//                public Diff.DataStructureDiff DiffToPrevious(MdsLocal.MdsLocalApplication.Event.ServiceConfigurationChanged previous)
//                {
//                    var diff = new Diff.DataStructureDiff();
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshot, this.ServiceConfigurationSnapshot, "ServiceConfigurationSnapshot"));
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshotParameters, this.ServiceConfigurationSnapshotParameters, "ServiceConfigurationSnapshotParameters"));
//                    return diff;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> GetServiceConfigurationSnapshot(MdsLocal.MdsLocalApplication.Event.ServiceConfigurationChanged dataStructure)
//                {
//                    return dataStructure.ServiceConfigurationSnapshot;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshot(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshot").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> GetServiceConfigurationSnapshotParameters(MdsLocal.MdsLocalApplication.Event.ServiceConfigurationChanged dataStructure)
//                {
//                    return dataStructure.ServiceConfigurationSnapshotParameters;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshotParameters(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshotParameters").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("e5ae81c2-24a6-4ee3-ad8e-bdf7526f7df4")]
//            public partial class ServiceRemoved : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.Event.ServiceRemoved>
//            {
//                [Metapsi.Reflection.DataItemField("9022b5b0-32aa-4863-8338-0b803b9d10d0")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                [Metapsi.Reflection.DataItemField("6e02f720-a253-42e6-964b-dd729d07380c")]
//                [Metapsi.Reflection.ScalarTypeName("String")]
//                public System.String ServiceName { get; set; } = System.String.Empty;
//                public MdsLocal.MdsLocalApplication.Event.ServiceRemoved Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.Event.ServiceRemoved();
//                    clone.Id = this.Id;
//                    clone.ServiceName = this.ServiceName;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.Event.ServiceRemoved dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Event.ServiceRemoved).Id;
//                }

//                public static System.String GetServiceName(MdsLocal.MdsLocalApplication.Event.ServiceRemoved dataRecord)
//                {
//                    return dataRecord.ServiceName;
//                }

//                public static System.String GetServiceName(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Event.ServiceRemoved).ServiceName;
//                }
//            }

//            /// <summary>
//                        /// Changes to the service folder are done on add/upgrade
//                        /// </summary>
//            [Metapsi.Reflection.DataItem("e5084862-0fbe-430c-9ac6-5faf8d825a7a")]
//            public partial class ServiceSetupComplete : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.MdsLocalApplication.Event.ServiceSetupComplete>
//            {
//                [Metapsi.Reflection.DataItemField("21cbcee7-2119-4137-937c-fce53b32cb1e")]
//                [Metapsi.Reflection.ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                [Metapsi.Reflection.DataItemField("897b7fa8-dd01-4064-9f44-0de393f6cb45")]
//                [Metapsi.Reflection.ScalarTypeName("String")]
//                public System.String ServiceName { get; set; } = System.String.Empty;
//                public MdsLocal.MdsLocalApplication.Event.ServiceSetupComplete Clone()
//                {
//                    var clone = new MdsLocal.MdsLocalApplication.Event.ServiceSetupComplete();
//                    clone.Id = this.Id;
//                    clone.ServiceName = this.ServiceName;
//                    return clone;
//                }

//                public static System.Guid GetId(MdsLocal.MdsLocalApplication.Event.ServiceSetupComplete dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Event.ServiceSetupComplete).Id;
//                }

//                public static System.String GetServiceName(MdsLocal.MdsLocalApplication.Event.ServiceSetupComplete dataRecord)
//                {
//                    return dataRecord.ServiceName;
//                }

//                public static System.String GetServiceName(Metapsi.Reflection.IRecord dataRecord)
//                {
//                    return (dataRecord as MdsLocal.MdsLocalApplication.Event.ServiceSetupComplete).ServiceName;
//                }
//            }
//        }
//    }

//    /// <summary>
//        /// 
//        /// </summary>
//    [Metapsi.Reflection.DataStructure("07dd6330-e466-4957-b77f-36a6db138725")]
//    public partial class OverviewPage : Metapsi.Reflection.IDataStructure, MdsLocal.IOverviewPage, MdsLocal.IFullLocalStatus, UI.Svelte.IPageBehavior
//    {
//        public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> LocalServiceSnapshots { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
//        public Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess> ServiceProcesses { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess>();
//        public Metapsi.Reflection.RecordCollection<MdsLocal.SyncResult> SyncResults { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.SyncResult>();
//        public Metapsi.Reflection.RecordCollection<UI.Svelte.NamedValue> Variables { get; set; } = new Metapsi.Reflection.RecordCollection<UI.Svelte.NamedValue>();
//        public Metapsi.Reflection.RecordCollection<UI.Svelte.ValidationMessage> ValidationMessages { get; set; } = new Metapsi.Reflection.RecordCollection<UI.Svelte.ValidationMessage>();
//        public Metapsi.Reflection.RecordCollection<UI.Svelte.UiFilterValue> FilterValues { get; set; } = new Metapsi.Reflection.RecordCollection<UI.Svelte.UiFilterValue>();
//        public Metapsi.Reflection.RecordCollection<UI.Svelte.FilterDeclaration> FilterDeclarations { get; set; } = new Metapsi.Reflection.RecordCollection<UI.Svelte.FilterDeclaration>();
//        public Metapsi.Reflection.RecordCollection<MdsLocal.LocalSettings> LocalSettings { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.LocalSettings>();
//        public MdsLocal.OverviewPage AddFullLocalStatus(MdsLocal.IFullLocalStatus source)
//        {
//            this.LocalServiceSnapshots.AddRange(source.LocalServiceSnapshots.Clone());
//            this.ServiceProcesses.AddRange(source.ServiceProcesses.Clone());
//            this.SyncResults.AddRange(source.SyncResults.Clone());
//            return this;
//        }

//        public MdsLocal.OverviewPage UpdateFullLocalStatus(MdsLocal.IFullLocalStatus source)
//        {
//            this.LocalServiceSnapshots.Update(source.LocalServiceSnapshots.Clone());
//            this.ServiceProcesses.Update(source.ServiceProcesses.Clone());
//            this.SyncResults.Update(source.SyncResults.Clone());
//            return this;
//        }

//        public MdsLocal.FullLocalStatus ExtractFullLocalStatus()
//        {
//            var output = new MdsLocal.FullLocalStatus();
//            output.LocalServiceSnapshots.AddRange(this.LocalServiceSnapshots.Clone());
//            output.ServiceProcesses.AddRange(this.ServiceProcesses.Clone());
//            output.SyncResults.AddRange(this.SyncResults.Clone());
//            return output;
//        }

//        public MdsLocal.OverviewPage AddPageBehavior(UI.Svelte.IPageBehavior source)
//        {
//            this.Variables.AddRange(source.Variables.Clone());
//            this.ValidationMessages.AddRange(source.ValidationMessages.Clone());
//            this.FilterValues.AddRange(source.FilterValues.Clone());
//            this.FilterDeclarations.AddRange(source.FilterDeclarations.Clone());
//            return this;
//        }

//        public MdsLocal.OverviewPage UpdatePageBehavior(UI.Svelte.IPageBehavior source)
//        {
//            this.Variables.Update(source.Variables.Clone());
//            this.ValidationMessages.Update(source.ValidationMessages.Clone());
//            this.FilterValues.Update(source.FilterValues.Clone());
//            this.FilterDeclarations.Update(source.FilterDeclarations.Clone());
//            return this;
//        }

//        public UI.Svelte.PageBehavior ExtractPageBehavior()
//        {
//            var output = new UI.Svelte.PageBehavior();
//            output.Variables.AddRange(this.Variables.Clone());
//            output.ValidationMessages.AddRange(this.ValidationMessages.Clone());
//            output.FilterValues.AddRange(this.FilterValues.Clone());
//            output.FilterDeclarations.AddRange(this.FilterDeclarations.Clone());
//            return output;
//        }

//        public Diff.DataStructureDiff DiffToPrevious(MdsLocal.OverviewPage previous)
//        {
//            var diff = new Diff.DataStructureDiff();
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.LocalServiceSnapshots, this.LocalServiceSnapshots, "LocalServiceSnapshots"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceProcesses, this.ServiceProcesses, "ServiceProcesses"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.SyncResults, this.SyncResults, "SyncResults"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.Variables, this.Variables, "Variables"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ValidationMessages, this.ValidationMessages, "ValidationMessages"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.FilterValues, this.FilterValues, "FilterValues"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.FilterDeclarations, this.FilterDeclarations, "FilterDeclarations"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.LocalSettings, this.LocalSettings, "LocalSettings"));
//            return diff;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> GetLocalServiceSnapshots(MdsLocal.OverviewPage dataStructure)
//        {
//            return dataStructure.LocalServiceSnapshots;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetLocalServiceSnapshots(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("LocalServiceSnapshots").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess> GetServiceProcesses(MdsLocal.OverviewPage dataStructure)
//        {
//            return dataStructure.ServiceProcesses;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetServiceProcesses(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("ServiceProcesses").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsLocal.SyncResult> GetSyncResults(MdsLocal.OverviewPage dataStructure)
//        {
//            return dataStructure.SyncResults;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetSyncResults(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("SyncResults").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<UI.Svelte.NamedValue> GetVariables(MdsLocal.OverviewPage dataStructure)
//        {
//            return dataStructure.Variables;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetVariables(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("Variables").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<UI.Svelte.ValidationMessage> GetValidationMessages(MdsLocal.OverviewPage dataStructure)
//        {
//            return dataStructure.ValidationMessages;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetValidationMessages(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("ValidationMessages").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<UI.Svelte.UiFilterValue> GetFilterValues(MdsLocal.OverviewPage dataStructure)
//        {
//            return dataStructure.FilterValues;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetFilterValues(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("FilterValues").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<UI.Svelte.FilterDeclaration> GetFilterDeclarations(MdsLocal.OverviewPage dataStructure)
//        {
//            return dataStructure.FilterDeclarations;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetFilterDeclarations(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("FilterDeclarations").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsLocal.LocalSettings> GetLocalSettings(MdsLocal.OverviewPage dataStructure)
//        {
//            return dataStructure.LocalSettings;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetLocalSettings(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("LocalSettings").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }
//    }

//    /// <summary>
//        /// Running OS processes of services owned by a local controller
//        /// </summary>
//    [Metapsi.Reflection.DataStructure("657f78e8-d349-4e63-a655-71cec44fa81f")]
//    public partial class OwnedRunningServiceProcesses : Metapsi.Reflection.IDataStructure, MdsLocal.IOwnedRunningServiceProcesses
//    {
//        public Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess> ServiceProcesses { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess>();
//        public Diff.DataStructureDiff DiffToPrevious(MdsLocal.OwnedRunningServiceProcesses previous)
//        {
//            var diff = new Diff.DataStructureDiff();
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceProcesses, this.ServiceProcesses, "ServiceProcesses"));
//            return diff;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess> GetServiceProcesses(MdsLocal.OwnedRunningServiceProcesses dataStructure)
//        {
//            return dataStructure.ServiceProcesses;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetServiceProcesses(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("ServiceProcesses").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }
//    }

//    /// <summary>
//        /// Data about the project binaries, as retrieved from the repository. Project binaries are then 'installed' as ServiceBinaries (using service configuration)
//        /// </summary>
//    [Metapsi.Reflection.DataItem("2ed24d2c-ca10-4232-b2de-cf32043f52df")]
//    public partial class ProjectBinary : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.ProjectBinary>
//    {
//        [Metapsi.Reflection.DataItemField("d6542408-34e8-4935-abc9-67ecf943fee2")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [Metapsi.Reflection.DataItemField("6e7e2463-d012-40a3-a2ac-d3b39dbd84e0")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String FullExePath { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("9fefb615-863f-452c-b8f4-3ac40f380605")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String ProjectName { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("4f84b99c-e005-4771-8b61-dbbbab211917")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String VersionTag { get; set; } = System.String.Empty;
//        public MdsLocal.ProjectBinary Clone()
//        {
//            var clone = new MdsLocal.ProjectBinary();
//            clone.Id = this.Id;
//            clone.FullExePath = this.FullExePath;
//            clone.ProjectName = this.ProjectName;
//            clone.VersionTag = this.VersionTag;
//            return clone;
//        }

//        public static System.Guid GetId(MdsLocal.ProjectBinary dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.ProjectBinary).Id;
//        }

//        public static System.String GetFullExePath(MdsLocal.ProjectBinary dataRecord)
//        {
//            return dataRecord.FullExePath;
//        }

//        public static System.String GetFullExePath(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.ProjectBinary).FullExePath;
//        }

//        public static System.String GetProjectName(MdsLocal.ProjectBinary dataRecord)
//        {
//            return dataRecord.ProjectName;
//        }

//        public static System.String GetProjectName(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.ProjectBinary).ProjectName;
//        }

//        public static System.String GetVersionTag(MdsLocal.ProjectBinary dataRecord)
//        {
//            return dataRecord.VersionTag;
//        }

//        public static System.String GetVersionTag(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.ProjectBinary).VersionTag;
//        }
//    }

//    /// <summary>
//        /// Diff from configured processes (should be running) to current OS processes (actually running); Can be overlapped (first stop, then start again=restart)
//        /// </summary>
//    [Metapsi.Reflection.DataStructure("d8227202-eb0a-4725-a1c7-1df441ffd518")]
//    public partial class RunningProcessesDiff : Metapsi.Reflection.IDataStructure, MdsLocal.IRunningProcessesDiff
//    {
//        public Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess> ToStart { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess>();
//        public Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess> ToStop { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess>();
//        public Diff.DataStructureDiff DiffToPrevious(MdsLocal.RunningProcessesDiff previous)
//        {
//            var diff = new Diff.DataStructureDiff();
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ToStart, this.ToStart, "ToStart"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ToStop, this.ToStop, "ToStop"));
//            return diff;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess> GetToStart(MdsLocal.RunningProcessesDiff dataStructure)
//        {
//            return dataStructure.ToStart;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetToStart(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("ToStart").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsLocal.RunningServiceProcess> GetToStop(MdsLocal.RunningProcessesDiff dataStructure)
//        {
//            return dataStructure.ToStop;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetToStop(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("ToStop").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }
//    }

//    /// <summary>
//        /// Service, Pid + some health data. Not saved to DB, data comes from processes themselves
//        /// </summary>
//    [Metapsi.Reflection.DataItem("bdb2b3f1-dabb-432b-b5fa-1710e5136986")]
//    public partial class RunningServiceProcess : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.RunningServiceProcess>
//    {
//        [Metapsi.Reflection.DataItemField("9e3ac298-5152-423b-9df7-ab8d9fd969b2")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [Metapsi.Reflection.DataItemField("a0a95d79-413b-4628-a5ce-34059d41ec7e")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String ServiceName { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("489f9199-6aa9-4b67-adac-2a51b7f9d192")]
//        [Metapsi.Reflection.ScalarTypeName("Int")]
//        public System.Int32 Pid { get; set; }

//        [Metapsi.Reflection.DataItemField("431646b5-6f36-470f-9572-1bb12a62e853")]
//        [Metapsi.Reflection.ScalarTypeName("Timestamp")]
//        public System.DateTime StartTimestampUtc { get; set; }

//        [Metapsi.Reflection.DataItemField("46e0a5ab-0070-4d0c-b58f-e26570b726d4")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String FullExePath { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("818a36d9-5f0c-4652-8053-d81710c8638a")]
//        [Metapsi.Reflection.ScalarTypeName("Int")]
//        public System.Int32 UsedRamMB { get; set; }

//        public MdsLocal.RunningServiceProcess Clone()
//        {
//            var clone = new MdsLocal.RunningServiceProcess();
//            clone.Id = this.Id;
//            clone.ServiceName = this.ServiceName;
//            clone.Pid = this.Pid;
//            clone.StartTimestampUtc = this.StartTimestampUtc;
//            clone.FullExePath = this.FullExePath;
//            clone.UsedRamMB = this.UsedRamMB;
//            return clone;
//        }

//        public static System.Guid GetId(MdsLocal.RunningServiceProcess dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.RunningServiceProcess).Id;
//        }

//        public static System.String GetServiceName(MdsLocal.RunningServiceProcess dataRecord)
//        {
//            return dataRecord.ServiceName;
//        }

//        public static System.String GetServiceName(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.RunningServiceProcess).ServiceName;
//        }

//        public static System.Int32 GetPid(MdsLocal.RunningServiceProcess dataRecord)
//        {
//            return dataRecord.Pid;
//        }

//        public static System.Int32 GetPid(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.RunningServiceProcess).Pid;
//        }

//        public static System.DateTime GetStartTimestampUtc(MdsLocal.RunningServiceProcess dataRecord)
//        {
//            return dataRecord.StartTimestampUtc;
//        }

//        public static System.DateTime GetStartTimestampUtc(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.RunningServiceProcess).StartTimestampUtc;
//        }

//        public static System.String GetFullExePath(MdsLocal.RunningServiceProcess dataRecord)
//        {
//            return dataRecord.FullExePath;
//        }

//        public static System.String GetFullExePath(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.RunningServiceProcess).FullExePath;
//        }

//        public static System.Int32 GetUsedRamMB(MdsLocal.RunningServiceProcess dataRecord)
//        {
//            return dataRecord.UsedRamMB;
//        }

//        public static System.Int32 GetUsedRamMB(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.RunningServiceProcess).UsedRamMB;
//        }
//    }

//    /// <summary>
//        /// Installed binaries (project binaries + configuration). Same project binaries can be installed multiple times for different services
//        /// </summary>
//    [Metapsi.Reflection.DataItem("0e23e4e4-80e9-4adf-ad89-a2dea20ed25c")]
//    public partial class ServiceBinary : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.ServiceBinary>
//    {
//        [Metapsi.Reflection.DataItemField("3e670cd6-c88e-42c6-beee-6ce5f333d0c4")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [Metapsi.Reflection.DataItemField("146812ef-423a-40e0-b850-ee334eb0e730")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid ProjectBinaryId { get; set; }

//        [Metapsi.Reflection.DataItemField("9a007a6a-1d7b-4b13-b544-db8a916e4047")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid LocalServiceId { get; set; }

//        [Metapsi.Reflection.DataItemField("2f3efdc0-8f69-4779-8a55-b811c15b3bbc")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String ServiceFullExePath { get; set; } = System.String.Empty;
//        public MdsLocal.ServiceBinary Clone()
//        {
//            var clone = new MdsLocal.ServiceBinary();
//            clone.Id = this.Id;
//            clone.ProjectBinaryId = this.ProjectBinaryId;
//            clone.LocalServiceId = this.LocalServiceId;
//            clone.ServiceFullExePath = this.ServiceFullExePath;
//            return clone;
//        }

//        public static System.Guid GetId(MdsLocal.ServiceBinary dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.ServiceBinary).Id;
//        }

//        public static System.Guid GetProjectBinaryId(MdsLocal.ServiceBinary dataRecord)
//        {
//            return dataRecord.ProjectBinaryId;
//        }

//        public static System.Guid GetProjectBinaryId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.ServiceBinary).ProjectBinaryId;
//        }

//        public static System.Guid GetLocalServiceId(MdsLocal.ServiceBinary dataRecord)
//        {
//            return dataRecord.LocalServiceId;
//        }

//        public static System.Guid GetLocalServiceId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.ServiceBinary).LocalServiceId;
//        }

//        public static System.String GetServiceFullExePath(MdsLocal.ServiceBinary dataRecord)
//        {
//            return dataRecord.ServiceFullExePath;
//        }

//        public static System.String GetServiceFullExePath(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsLocal.ServiceBinary).ServiceFullExePath;
//        }
//    }

//    ///// <summary>
//    //    /// Data kept alongside the service exe to track configuration at the moment of service setup
//    //    /// </summary>
//    //[Metapsi.Reflection.DataItem("6a011d40-94cb-453e-8c9f-a2ca8536770f")]
//    //public partial class ServiceVersion : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsLocal.ServiceVersion>
//    //{
//    //    [Metapsi.Reflection.DataItemField("b22469f1-9f67-4aa3-95af-cb584380b2a9")]
//    //    [Metapsi.Reflection.ScalarTypeName("Id")]
//    //    public System.Guid Id { get; set; } = System.Guid.NewGuid();
//    //    [Metapsi.Reflection.DataItemField("da93d020-37ca-4190-b1b3-7e2a561201a8")]
//    //    [Metapsi.Reflection.ScalarTypeName("String")]
//    //    public System.String ServiceName { get; set; } = System.String.Empty;
//    //    [Metapsi.Reflection.DataItemField("cf5e7fd7-2da0-48a0-9ba3-561fd57ad9dc")]
//    //    [Metapsi.Reflection.ScalarTypeName("String")]
//    //    public System.String NodeName { get; set; } = System.String.Empty;
//    //    [Metapsi.Reflection.DataItemField("42a42973-a99b-4b46-ad6e-0e5c7ba6d58b")]
//    //    [Metapsi.Reflection.ScalarTypeName("String")]
//    //    public System.String ConfigurationId { get; set; } = System.String.Empty;
//    //    [Metapsi.Reflection.DataItemField("f27189c3-f98e-497b-b3ca-0fc376b9925e")]
//    //    [Metapsi.Reflection.ScalarTypeName("String")]
//    //    public System.String ProjectName { get; set; } = System.String.Empty;
//    //    [Metapsi.Reflection.DataItemField("fdc2c42d-2f42-4a39-bc4d-29ad5bbb50d3")]
//    //    [Metapsi.Reflection.ScalarTypeName("String")]
//    //    public System.String Version { get; set; } = System.String.Empty;
//    //    public MdsLocal.ServiceVersion Clone()
//    //    {
//    //        var clone = new MdsLocal.ServiceVersion();
//    //        clone.Id = this.Id;
//    //        clone.ServiceName = this.ServiceName;
//    //        clone.NodeName = this.NodeName;
//    //        clone.ConfigurationId = this.ConfigurationId;
//    //        clone.ProjectName = this.ProjectName;
//    //        clone.Version = this.Version;
//    //        return clone;
//    //    }

//    //    public static System.Guid GetId(MdsLocal.ServiceVersion dataRecord)
//    //    {
//    //        return dataRecord.Id;
//    //    }

//    //    public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//    //    {
//    //        return (dataRecord as MdsLocal.ServiceVersion).Id;
//    //    }

//    //    public static System.String GetServiceName(MdsLocal.ServiceVersion dataRecord)
//    //    {
//    //        return dataRecord.ServiceName;
//    //    }

//    //    public static System.String GetServiceName(Metapsi.Reflection.IRecord dataRecord)
//    //    {
//    //        return (dataRecord as MdsLocal.ServiceVersion).ServiceName;
//    //    }

//    //    public static System.String GetNodeName(MdsLocal.ServiceVersion dataRecord)
//    //    {
//    //        return dataRecord.NodeName;
//    //    }

//    //    public static System.String GetNodeName(Metapsi.Reflection.IRecord dataRecord)
//    //    {
//    //        return (dataRecord as MdsLocal.ServiceVersion).NodeName;
//    //    }

//    //    public static System.String GetConfigurationId(MdsLocal.ServiceVersion dataRecord)
//    //    {
//    //        return dataRecord.ConfigurationId;
//    //    }

//    //    public static System.String GetConfigurationId(Metapsi.Reflection.IRecord dataRecord)
//    //    {
//    //        return (dataRecord as MdsLocal.ServiceVersion).ConfigurationId;
//    //    }

//    //    public static System.String GetProjectName(MdsLocal.ServiceVersion dataRecord)
//    //    {
//    //        return dataRecord.ProjectName;
//    //    }

//    //    public static System.String GetProjectName(Metapsi.Reflection.IRecord dataRecord)
//    //    {
//    //        return (dataRecord as MdsLocal.ServiceVersion).ProjectName;
//    //    }

//    //    public static System.String GetVersion(MdsLocal.ServiceVersion dataRecord)
//    //    {
//    //        return dataRecord.Version;
//    //    }

//    //    public static System.String GetVersion(Metapsi.Reflection.IRecord dataRecord)
//    //    {
//    //        return (dataRecord as MdsLocal.ServiceVersion).Version;
//    //    }
//    //}

//    /// <summary>
//        /// 
//        /// </summary>
//    [Metapsi.Reflection.DataStructure("c240231b-6ecf-4aa9-8d61-a691d34a5ad7")]
//    public partial class SyncHistoryPage : Metapsi.Reflection.IDataStructure, MdsLocal.ISyncHistoryPage, MdsLocal.ISyncHistory, UI.Svelte.IPageBehavior
//    {
//        public Metapsi.Reflection.RecordCollection<MdsLocal.SyncResult> SyncResults { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.SyncResult>();
//        public Metapsi.Reflection.RecordCollection<MdsLocal.SyncUpdatedConfiguration> UpdatedConfigurations { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.SyncUpdatedConfiguration>();
//        public Metapsi.Reflection.RecordCollection<UI.Svelte.NamedValue> Variables { get; set; } = new Metapsi.Reflection.RecordCollection<UI.Svelte.NamedValue>();
//        public Metapsi.Reflection.RecordCollection<UI.Svelte.ValidationMessage> ValidationMessages { get; set; } = new Metapsi.Reflection.RecordCollection<UI.Svelte.ValidationMessage>();
//        public Metapsi.Reflection.RecordCollection<UI.Svelte.UiFilterValue> FilterValues { get; set; } = new Metapsi.Reflection.RecordCollection<UI.Svelte.UiFilterValue>();
//        public Metapsi.Reflection.RecordCollection<UI.Svelte.FilterDeclaration> FilterDeclarations { get; set; } = new Metapsi.Reflection.RecordCollection<UI.Svelte.FilterDeclaration>();
//        public MdsLocal.SyncHistoryPage AddSyncHistory(MdsLocal.ISyncHistory source)
//        {
//            this.SyncResults.AddRange(source.SyncResults.Clone());
//            this.UpdatedConfigurations.AddRange(source.UpdatedConfigurations.Clone());
//            return this;
//        }

//        public MdsLocal.SyncHistoryPage UpdateSyncHistory(MdsLocal.ISyncHistory source)
//        {
//            this.SyncResults.Update(source.SyncResults.Clone());
//            this.UpdatedConfigurations.Update(source.UpdatedConfigurations.Clone());
//            return this;
//        }

//        public MdsLocal.SyncHistory ExtractSyncHistory()
//        {
//            var output = new MdsLocal.SyncHistory();
//            output.SyncResults.AddRange(this.SyncResults.Clone());
//            output.UpdatedConfigurations.AddRange(this.UpdatedConfigurations.Clone());
//            return output;
//        }

//        public MdsLocal.SyncHistoryPage AddPageBehavior(UI.Svelte.IPageBehavior source)
//        {
//            this.Variables.AddRange(source.Variables.Clone());
//            this.ValidationMessages.AddRange(source.ValidationMessages.Clone());
//            this.FilterValues.AddRange(source.FilterValues.Clone());
//            this.FilterDeclarations.AddRange(source.FilterDeclarations.Clone());
//            return this;
//        }

//        public MdsLocal.SyncHistoryPage UpdatePageBehavior(UI.Svelte.IPageBehavior source)
//        {
//            this.Variables.Update(source.Variables.Clone());
//            this.ValidationMessages.Update(source.ValidationMessages.Clone());
//            this.FilterValues.Update(source.FilterValues.Clone());
//            this.FilterDeclarations.Update(source.FilterDeclarations.Clone());
//            return this;
//        }

//        public UI.Svelte.PageBehavior ExtractPageBehavior()
//        {
//            var output = new UI.Svelte.PageBehavior();
//            output.Variables.AddRange(this.Variables.Clone());
//            output.ValidationMessages.AddRange(this.ValidationMessages.Clone());
//            output.FilterValues.AddRange(this.FilterValues.Clone());
//            output.FilterDeclarations.AddRange(this.FilterDeclarations.Clone());
//            return output;
//        }

//        public Diff.DataStructureDiff DiffToPrevious(MdsLocal.SyncHistoryPage previous)
//        {
//            var diff = new Diff.DataStructureDiff();
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.SyncResults, this.SyncResults, "SyncResults"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.UpdatedConfigurations, this.UpdatedConfigurations, "UpdatedConfigurations"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.Variables, this.Variables, "Variables"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ValidationMessages, this.ValidationMessages, "ValidationMessages"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.FilterValues, this.FilterValues, "FilterValues"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.FilterDeclarations, this.FilterDeclarations, "FilterDeclarations"));
//            return diff;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsLocal.SyncResult> GetSyncResults(MdsLocal.SyncHistoryPage dataStructure)
//        {
//            return dataStructure.SyncResults;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetSyncResults(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("SyncResults").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsLocal.SyncUpdatedConfiguration> GetUpdatedConfigurations(MdsLocal.SyncHistoryPage dataStructure)
//        {
//            return dataStructure.UpdatedConfigurations;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetUpdatedConfigurations(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("UpdatedConfigurations").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<UI.Svelte.NamedValue> GetVariables(MdsLocal.SyncHistoryPage dataStructure)
//        {
//            return dataStructure.Variables;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetVariables(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("Variables").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<UI.Svelte.ValidationMessage> GetValidationMessages(MdsLocal.SyncHistoryPage dataStructure)
//        {
//            return dataStructure.ValidationMessages;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetValidationMessages(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("ValidationMessages").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<UI.Svelte.UiFilterValue> GetFilterValues(MdsLocal.SyncHistoryPage dataStructure)
//        {
//            return dataStructure.FilterValues;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetFilterValues(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("FilterValues").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<UI.Svelte.FilterDeclaration> GetFilterDeclarations(MdsLocal.SyncHistoryPage dataStructure)
//        {
//            return dataStructure.FilterDeclarations;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetFilterDeclarations(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("FilterDeclarations").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }
//    }


//    /// <summary>
//        /// 
//        /// </summary>
//    public static partial class ZippedProjectRetriever
//    {
//        /// <summary>
//                /// 
//                /// </summary>
//        public static partial class SupplySpecifications
//        {
//            /// <summary>
//                        /// 
//                        /// </summary>
//            public partial interface IProjectBinaries : Metapsi.Reflection.IDataStructure
//            {
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; }

//                public Metapsi.Reflection.RecordCollection<MdsLocal.GeneralPath> IntoPath { get; set; }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [Metapsi.Reflection.DataStructure("ea6220f9-ddfc-4954-8fe9-8dcff0ada94f")]
//            public partial class ProjectBinaries : Metapsi.Reflection.IDataStructure, MdsLocal.ZippedProjectRetriever.SupplySpecifications.IProjectBinaries
//            {
//                public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
//                public Metapsi.Reflection.RecordCollection<MdsLocal.GeneralPath> IntoPath { get; set; } = new Metapsi.Reflection.RecordCollection<MdsLocal.GeneralPath>();
//                public Diff.DataStructureDiff DiffToPrevious(MdsLocal.ZippedProjectRetriever.SupplySpecifications.ProjectBinaries previous)
//                {
//                    var diff = new Diff.DataStructureDiff();
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshot, this.ServiceConfigurationSnapshot, "ServiceConfigurationSnapshot"));
//                    diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.IntoPath, this.IntoPath, "IntoPath"));
//                    return diff;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> GetServiceConfigurationSnapshot(MdsLocal.ZippedProjectRetriever.SupplySpecifications.ProjectBinaries dataStructure)
//                {
//                    return dataStructure.ServiceConfigurationSnapshot;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshot(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshot").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }

//                public static Metapsi.Reflection.RecordCollection<MdsLocal.GeneralPath> GetIntoPath(MdsLocal.ZippedProjectRetriever.SupplySpecifications.ProjectBinaries dataStructure)
//                {
//                    return dataStructure.IntoPath;
//                }

//                public static Metapsi.Reflection.IRecordCollection GetIntoPath(Metapsi.Reflection.IDataStructure dataStructure)
//                {
//                    return dataStructure.GetType().GetProperty("IntoPath").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//                }
//            }
//        }
//    }
//}