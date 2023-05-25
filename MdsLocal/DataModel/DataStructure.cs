using Metapsi;

namespace MdsLocal
{
    /// <summary>
    /// 
    /// </summary>
    [DataStructure("a4ff0fd9-82e7-46c9-b171-8e109578b016")]
    public partial class FullLocalStatus : IDataStructure
    {
        public RecordCollection<MdsCommon.ServiceConfigurationSnapshot> LocalServiceSnapshots { get; set; } = new RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
        public RecordCollection<MdsLocal.SyncResult> SyncResults { get; set; } = new RecordCollection<MdsLocal.SyncResult>();
    }

    /// <summary>
    /// Configuration changes as detected by the synchronization mechanism
    /// </summary>
    [DataStructure("7cbbf409-b36e-4ced-842e-eba3ac8e6cd8")]
    public partial class LocalServicesConfigurationDiff : IDataStructure
    {
        public RecordCollection<MdsCommon.ServiceConfigurationSnapshot> AddedServices { get; set; } = new RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
        public RecordCollection<MdsCommon.ServiceConfigurationSnapshot> RemovedServices { get; set; } = new RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
        public RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ChangedServices { get; set; } = new RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
        public RecordCollection<MdsCommon.ServiceConfigurationSnapshot> IdenticalServices { get; set; } = new RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
        public RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> Parameters { get; set; } = new RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter>();
    }


    /// <summary>
    /// 
    /// </summary>
    [DataStructure("bf32b0a6-1fd4-4446-b727-065daa61f3bf")]
    public partial class ServiceAlreadyAtVersion : IDataStructure, IData
    {
        public RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; } = new RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
        public RecordCollection<MdsLocal.ServiceBinary> ServiceBinary { get; set; } = new RecordCollection<MdsLocal.ServiceBinary>();   
    }


    /// <summary>
    /// Triggered when the local configuration has any change 
    /// </summary>
    [DataStructure("e0ab876c-5b28-4684-9693-76811a798f68")]
    public partial class ConfigurationChanged : IDataStructure, IData
    {
        public LocalServicesConfigurationDiff LocalServicesConfigurationDiff { get; set; }
        //public RecordCollection<MdsCommon.ServiceConfigurationSnapshot> AddedServices { get; set; } = new RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
        //public RecordCollection<MdsCommon.ServiceConfigurationSnapshot> RemovedServices { get; set; } = new RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
        //public RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ChangedServices { get; set; } = new RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
        //public RecordCollection<MdsCommon.ServiceConfigurationSnapshot> IdenticalServices { get; set; } = new RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
        //public RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> Parameters { get; set; } = new RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter>();
    }
}
