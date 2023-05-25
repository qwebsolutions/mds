//using System.Linq;
//using Metapsi.Reflection;

//namespace MdsCommon
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    [Metapsi.Reflection.DataStructure("c9fe8ab9-f4db-411e-9f37-925c1e3cb041")]
//    public partial class AvailableProjects : Metapsi.Reflection.IDataStructure, MdsCommon.IAvailableProjects
//    {
//        public Metapsi.Reflection.RecordCollection<MdsCommon.Project> Projects { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.Project>();
//        public Metapsi.Reflection.RecordCollection<MdsCommon.ProjectVersion> ProjectVersions { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ProjectVersion>();
//        public Diff.DataStructureDiff DiffToPrevious(MdsCommon.AvailableProjects previous)
//        {
//            var diff = new Diff.DataStructureDiff();
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.Projects, this.Projects, "Projects"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ProjectVersions, this.ProjectVersions, "ProjectVersions"));
//            return diff;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsCommon.Project> GetProjects(MdsCommon.AvailableProjects dataStructure)
//        {
//            return dataStructure.Projects;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetProjects(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("Projects").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsCommon.ProjectVersion> GetProjectVersions(MdsCommon.AvailableProjects dataStructure)
//        {
//            return dataStructure.ProjectVersions;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetProjectVersions(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("ProjectVersions").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }
//    }

//    /// <summary>
//    /// 
//    /// </summary>
//    [Metapsi.Reflection.DataItem("94680c81-e324-4625-8ba8-45203aea26c4")]
//    public partial class InfrastructureEvent : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsCommon.InfrastructureEvent>
//    {
//        [Metapsi.Reflection.DataItemField("e0f061d6-bb3d-4caf-a00c-7ce4b00621b5")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [Metapsi.Reflection.DataItemField("9633f2c1-5cc6-40c1-8033-1ab7ed982576")]
//        [Metapsi.Reflection.ScalarTypeName("Timestamp")]
//        public System.DateTime Timestamp { get; set; }

//        [Metapsi.Reflection.DataItemField("45f3d6ed-d3d0-4b2f-a364-5cd3d466cf94")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String Type { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("00c6232e-1b10-4f22-918d-e93fb45bcbdb")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String Source { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("3720a7c5-99dc-4956-b2d5-c5cf7b3b93e5")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String Criticality { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("34df453a-7f7f-4d37-9101-fc7a7d232bd8")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String ShortDescription { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("17b71c12-630e-46ea-bc3a-7cef5cb2c0a2")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String FullDescription { get; set; } = System.String.Empty;
//        public MdsCommon.InfrastructureEvent Clone()
//        {
//            var clone = new MdsCommon.InfrastructureEvent();
//            clone.Id = this.Id;
//            clone.Timestamp = this.Timestamp;
//            clone.Type = this.Type;
//            clone.Source = this.Source;
//            clone.Criticality = this.Criticality;
//            clone.ShortDescription = this.ShortDescription;
//            clone.FullDescription = this.FullDescription;
//            return clone;
//        }

//        public static System.Guid GetId(MdsCommon.InfrastructureEvent dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.InfrastructureEvent).Id;
//        }

//        public static System.DateTime GetTimestamp(MdsCommon.InfrastructureEvent dataRecord)
//        {
//            return dataRecord.Timestamp;
//        }

//        public static System.DateTime GetTimestamp(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.InfrastructureEvent).Timestamp;
//        }

//        public static System.String GetType(MdsCommon.InfrastructureEvent dataRecord)
//        {
//            return dataRecord.Type;
//        }

//        public static System.String GetType(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.InfrastructureEvent).Type;
//        }

//        public static System.String GetSource(MdsCommon.InfrastructureEvent dataRecord)
//        {
//            return dataRecord.Source;
//        }

//        public static System.String GetSource(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.InfrastructureEvent).Source;
//        }

//        public static System.String GetCriticality(MdsCommon.InfrastructureEvent dataRecord)
//        {
//            return dataRecord.Criticality;
//        }

//        public static System.String GetCriticality(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.InfrastructureEvent).Criticality;
//        }

//        public static System.String GetShortDescription(MdsCommon.InfrastructureEvent dataRecord)
//        {
//            return dataRecord.ShortDescription;
//        }

//        public static System.String GetShortDescription(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.InfrastructureEvent).ShortDescription;
//        }

//        public static System.String GetFullDescription(MdsCommon.InfrastructureEvent dataRecord)
//        {
//            return dataRecord.FullDescription;
//        }

//        public static System.String GetFullDescription(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.InfrastructureEvent).FullDescription;
//        }
//    }


//    /// <summary>
//    /// 
//    /// </summary>
//    [Metapsi.Reflection.DataItem("07796d96-b9fd-4430-b8b0-a2778ef5c9d0")]
//    public partial class MachineStatus : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsCommon.MachineStatus>
//    {
//        [Metapsi.Reflection.DataItemField("5923eaaf-1430-41ca-899c-9af05112fb25")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [Metapsi.Reflection.DataItemField("ffc55db9-125e-4b38-82d5-57ecf76f7489")]
//        [Metapsi.Reflection.ScalarTypeName("Int")]
//        public System.Int32 HddTotalMb { get; set; }

//        [Metapsi.Reflection.DataItemField("4ff795bd-0295-4e5a-8428-8878fffa683f")]
//        [Metapsi.Reflection.ScalarTypeName("Int")]
//        public System.Int32 HddAvailableMb { get; set; }

//        [Metapsi.Reflection.DataItemField("d1fa4888-a008-4dc4-b607-aa027827c35f")]
//        [Metapsi.Reflection.ScalarTypeName("Int")]
//        public System.Int32 RamTotalMb { get; set; }

//        [Metapsi.Reflection.DataItemField("8026698e-1e63-4d82-b1e1-7d1ba91f0821")]
//        [Metapsi.Reflection.ScalarTypeName("Int")]
//        public System.Int32 RamAvailableMb { get; set; }

//        [Metapsi.Reflection.DataItemField("c7077a2a-9ff3-489a-abc9-37be61527acb")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String MachineName { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("860b568e-df08-45b5-818a-c4a56e56de97")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String NodeName { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("8ef942dc-68d7-4be1-9427-8b20fe3de455")]
//        [Metapsi.Reflection.ScalarTypeName("Timestamp")]
//        public System.DateTime TimestampUtc { get; set; }

//        public MdsCommon.MachineStatus Clone()
//        {
//            var clone = new MdsCommon.MachineStatus();
//            clone.Id = this.Id;
//            clone.HddTotalMb = this.HddTotalMb;
//            clone.HddAvailableMb = this.HddAvailableMb;
//            clone.RamTotalMb = this.RamTotalMb;
//            clone.RamAvailableMb = this.RamAvailableMb;
//            clone.MachineName = this.MachineName;
//            clone.NodeName = this.NodeName;
//            clone.TimestampUtc = this.TimestampUtc;
//            return clone;
//        }

//        public static System.Guid GetId(MdsCommon.MachineStatus dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.MachineStatus).Id;
//        }

//        public static System.Int32 GetHddTotalMb(MdsCommon.MachineStatus dataRecord)
//        {
//            return dataRecord.HddTotalMb;
//        }

//        public static System.Int32 GetHddTotalMb(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.MachineStatus).HddTotalMb;
//        }

//        public static System.Int32 GetHddAvailableMb(MdsCommon.MachineStatus dataRecord)
//        {
//            return dataRecord.HddAvailableMb;
//        }

//        public static System.Int32 GetHddAvailableMb(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.MachineStatus).HddAvailableMb;
//        }

//        public static System.Int32 GetRamTotalMb(MdsCommon.MachineStatus dataRecord)
//        {
//            return dataRecord.RamTotalMb;
//        }

//        public static System.Int32 GetRamTotalMb(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.MachineStatus).RamTotalMb;
//        }

//        public static System.Int32 GetRamAvailableMb(MdsCommon.MachineStatus dataRecord)
//        {
//            return dataRecord.RamAvailableMb;
//        }

//        public static System.Int32 GetRamAvailableMb(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.MachineStatus).RamAvailableMb;
//        }

//        public static System.String GetMachineName(MdsCommon.MachineStatus dataRecord)
//        {
//            return dataRecord.MachineName;
//        }

//        public static System.String GetMachineName(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.MachineStatus).MachineName;
//        }

//        public static System.String GetNodeName(MdsCommon.MachineStatus dataRecord)
//        {
//            return dataRecord.NodeName;
//        }

//        public static System.String GetNodeName(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.MachineStatus).NodeName;
//        }

//        public static System.DateTime GetTimestampUtc(MdsCommon.MachineStatus dataRecord)
//        {
//            return dataRecord.TimestampUtc;
//        }

//        public static System.DateTime GetTimestampUtc(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.MachineStatus).TimestampUtc;
//        }
//    }

//    /// <summary>
//    /// 
//    /// </summary>
//    [Metapsi.Reflection.DataStructure("3a65477f-af56-4772-8944-5adaf4bd5fe1")]
//    public partial class NodeServiceSnapshots : Metapsi.Reflection.IDataStructure, MdsCommon.INodeServiceSnapshots, MdsCommon.ISingleServiceConfigurationSnapshot
//    {
//        public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
//        public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> ServiceConfigurationSnapshotParameters { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter>();
//        public MdsCommon.NodeServiceSnapshots AddSingleServiceConfigurationSnapshot(MdsCommon.ISingleServiceConfigurationSnapshot source)
//        {
//            this.ServiceConfigurationSnapshot.AddRange(source.ServiceConfigurationSnapshot.Clone());
//            this.ServiceConfigurationSnapshotParameters.AddRange(source.ServiceConfigurationSnapshotParameters.Clone());
//            return this;
//        }

//        public MdsCommon.NodeServiceSnapshots UpdateSingleServiceConfigurationSnapshot(MdsCommon.ISingleServiceConfigurationSnapshot source)
//        {
//            this.ServiceConfigurationSnapshot.Update(source.ServiceConfigurationSnapshot.Clone());
//            this.ServiceConfigurationSnapshotParameters.Update(source.ServiceConfigurationSnapshotParameters.Clone());
//            return this;
//        }

//        public MdsCommon.SingleServiceConfigurationSnapshot ExtractSingleServiceConfigurationSnapshot()
//        {
//            var output = new MdsCommon.SingleServiceConfigurationSnapshot();
//            output.ServiceConfigurationSnapshot.AddRange(this.ServiceConfigurationSnapshot.Clone());
//            output.ServiceConfigurationSnapshotParameters.AddRange(this.ServiceConfigurationSnapshotParameters.Clone());
//            return output;
//        }

//        public Diff.DataStructureDiff DiffToPrevious(MdsCommon.NodeServiceSnapshots previous)
//        {
//            var diff = new Diff.DataStructureDiff();
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshot, this.ServiceConfigurationSnapshot, "ServiceConfigurationSnapshot"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshotParameters, this.ServiceConfigurationSnapshotParameters, "ServiceConfigurationSnapshotParameters"));
//            return diff;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> GetServiceConfigurationSnapshot(MdsCommon.NodeServiceSnapshots dataStructure)
//        {
//            return dataStructure.ServiceConfigurationSnapshot;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshot(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshot").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> GetServiceConfigurationSnapshotParameters(MdsCommon.NodeServiceSnapshots dataStructure)
//        {
//            return dataStructure.ServiceConfigurationSnapshotParameters;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshotParameters(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshotParameters").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }
//    }

//    /// <summary>
//    /// 
//    /// </summary>
//    [Metapsi.Reflection.DataItem("e23a57e7-79ff-4e43-9192-ce8ed0ea69c3")]
//    public partial class Project : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsCommon.Project>
//    {
//        [Metapsi.Reflection.DataItemField("b3f246b5-f4b2-4c37-a2d3-f496ffd75aae")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [Metapsi.Reflection.DataItemField("bd182031-a163-47e4-b3a2-65f96794ebe8")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String Name { get; set; } = System.String.Empty;
//        public MdsCommon.Project Clone()
//        {
//            var clone = new MdsCommon.Project();
//            clone.Id = this.Id;
//            clone.Name = this.Name;
//            return clone;
//        }

//        public static System.Guid GetId(MdsCommon.Project dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.Project).Id;
//        }

//        public static System.String GetName(MdsCommon.Project dataRecord)
//        {
//            return dataRecord.Name;
//        }

//        public static System.String GetName(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.Project).Name;
//        }
//    }

//    /// <summary>
//    /// 
//    /// </summary>
//    [Metapsi.Reflection.DataItem("6724c6e2-a288-444f-ae13-e48bfdff7bff")]
//    public partial class ProjectVersion : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsCommon.ProjectVersion>
//    {
//        [Metapsi.Reflection.DataItemField("49b40e88-1a53-41e1-acd6-46ba832e143b")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [Metapsi.Reflection.DataItemField("8a1e1cdf-eef4-49b1-a9cd-f9470a18c0e1")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid ProjectId { get; set; }

//        [Metapsi.Reflection.DataItemField("419b2950-41c7-4cdd-a858-77ff4f5348a8")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String VersionTag { get; set; } = System.String.Empty;
//        public MdsCommon.ProjectVersion Clone()
//        {
//            var clone = new MdsCommon.ProjectVersion();
//            clone.Id = this.Id;
//            clone.ProjectId = this.ProjectId;
//            clone.VersionTag = this.VersionTag;
//            return clone;
//        }

//        public static System.Guid GetId(MdsCommon.ProjectVersion dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ProjectVersion).Id;
//        }

//        public static System.Guid GetProjectId(MdsCommon.ProjectVersion dataRecord)
//        {
//            return dataRecord.ProjectId;
//        }

//        public static System.Guid GetProjectId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ProjectVersion).ProjectId;
//        }

//        public static System.String GetVersionTag(MdsCommon.ProjectVersion dataRecord)
//        {
//            return dataRecord.VersionTag;
//        }

//        public static System.String GetVersionTag(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ProjectVersion).VersionTag;
//        }
//    }

//    /// <summary>
//    /// 
//    /// </summary>
//    [Metapsi.Reflection.DataItem("7059b905-9a07-47f0-94ef-4a7f2dd004ed")]
//    public partial class ServiceConfigurationSnapshot : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsCommon.ServiceConfigurationSnapshot>
//    {
//        [Metapsi.Reflection.DataItemField("b0b42168-98b5-4e8b-a5b2-52102f04d428")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [Metapsi.Reflection.DataItemField("e7f2cc69-b62a-4b79-a03d-f509db7ab769")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String ServiceName { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("da75bc0c-f0dc-4295-abf1-f7d8dbd60a06")]
//        [Metapsi.Reflection.ScalarTypeName("Timestamp")]
//        public System.DateTime SnapshotTimestamp { get; set; }

//        [Metapsi.Reflection.DataItemField("6ccebf47-8586-46ab-939a-57e187e8fc5d")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String ApplicationName { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("f79b3c35-e49f-44c0-a976-23dcd7d86c44")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String ProjectName { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("6d3c02a3-0ef2-47eb-9cc8-79f23f7e0b78")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String ProjectVersionTag { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("3cb1e1c0-237d-48de-815f-d262491f13d8")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String NodeName { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("9d92398f-8095-4981-a07b-23c3843edae2")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String Hash { get; set; } = System.String.Empty;
//        public MdsCommon.ServiceConfigurationSnapshot Clone()
//        {
//            var clone = new MdsCommon.ServiceConfigurationSnapshot();
//            clone.Id = this.Id;
//            clone.ServiceName = this.ServiceName;
//            clone.SnapshotTimestamp = this.SnapshotTimestamp;
//            clone.ApplicationName = this.ApplicationName;
//            clone.ProjectName = this.ProjectName;
//            clone.ProjectVersionTag = this.ProjectVersionTag;
//            clone.NodeName = this.NodeName;
//            clone.Hash = this.Hash;
//            return clone;
//        }

//        public static System.Guid GetId(MdsCommon.ServiceConfigurationSnapshot dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceConfigurationSnapshot).Id;
//        }

//        public static System.String GetServiceName(MdsCommon.ServiceConfigurationSnapshot dataRecord)
//        {
//            return dataRecord.ServiceName;
//        }

//        public static System.String GetServiceName(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceConfigurationSnapshot).ServiceName;
//        }

//        public static System.DateTime GetSnapshotTimestamp(MdsCommon.ServiceConfigurationSnapshot dataRecord)
//        {
//            return dataRecord.SnapshotTimestamp;
//        }

//        public static System.DateTime GetSnapshotTimestamp(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceConfigurationSnapshot).SnapshotTimestamp;
//        }

//        public static System.String GetApplicationName(MdsCommon.ServiceConfigurationSnapshot dataRecord)
//        {
//            return dataRecord.ApplicationName;
//        }

//        public static System.String GetApplicationName(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceConfigurationSnapshot).ApplicationName;
//        }

//        public static System.String GetProjectName(MdsCommon.ServiceConfigurationSnapshot dataRecord)
//        {
//            return dataRecord.ProjectName;
//        }

//        public static System.String GetProjectName(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceConfigurationSnapshot).ProjectName;
//        }

//        public static System.String GetProjectVersionTag(MdsCommon.ServiceConfigurationSnapshot dataRecord)
//        {
//            return dataRecord.ProjectVersionTag;
//        }

//        public static System.String GetProjectVersionTag(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceConfigurationSnapshot).ProjectVersionTag;
//        }

//        public static System.String GetNodeName(MdsCommon.ServiceConfigurationSnapshot dataRecord)
//        {
//            return dataRecord.NodeName;
//        }

//        public static System.String GetNodeName(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceConfigurationSnapshot).NodeName;
//        }

//        public static System.String GetHash(MdsCommon.ServiceConfigurationSnapshot dataRecord)
//        {
//            return dataRecord.Hash;
//        }

//        public static System.String GetHash(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceConfigurationSnapshot).Hash;
//        }
//    }

//    /// <summary>
//    /// 
//    /// </summary>
//    [Metapsi.Reflection.DataItem("c71a94b7-a01c-423c-a2ba-209bfa1eacd7")]
//    public partial class ServiceConfigurationSnapshotParameter : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsCommon.ServiceConfigurationSnapshotParameter>
//    {
//        [Metapsi.Reflection.DataItemField("862b2caa-26eb-40db-8ce5-b80c0995710f")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [Metapsi.Reflection.DataItemField("58ac0bdb-65ce-4059-b828-d594de63dcd6")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid ServiceConfigurationSnapshotId { get; set; }

//        [Metapsi.Reflection.DataItemField("88750eb4-606f-4275-8c96-663d4a6be697")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String ParameterName { get; set; } = System.String.Empty;

//        [Metapsi.Reflection.DataItemField("862b2caa-26eb-40db-8ce5-b80c0995710f")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid ParameterTypeId { get; set; }


//        [Metapsi.Reflection.DataItemField("d737ea0d-fbfc-4748-bd68-259fffe622c9")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String ConfiguredValue { get; set; } = System.String.Empty;

//        [Metapsi.Reflection.DataItemField("d737ea0d-fbfc-4748-bd68-259fffe622c9")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String DeployedValue { get; set; } = System.String.Empty;
//        public MdsCommon.ServiceConfigurationSnapshotParameter Clone()
//        {
//            var clone = new MdsCommon.ServiceConfigurationSnapshotParameter();
//            clone.Id = this.Id;
//            clone.ServiceConfigurationSnapshotId = this.ServiceConfigurationSnapshotId;
//            clone.ParameterName = this.ParameterName;
//            clone.ParameterTypeId = this.ParameterTypeId;
//            clone.ConfiguredValue = this.ConfiguredValue;
//            clone.DeployedValue = this.DeployedValue;
//            return clone;
//        }

//        public static System.Guid GetId(MdsCommon.ServiceConfigurationSnapshotParameter dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceConfigurationSnapshotParameter).Id;
//        }

//        public static System.Guid GetServiceConfigurationSnapshotId(MdsCommon.ServiceConfigurationSnapshotParameter dataRecord)
//        {
//            return dataRecord.ServiceConfigurationSnapshotId;
//        }

//        public static System.Guid GetServiceConfigurationSnapshotId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceConfigurationSnapshotParameter).ServiceConfigurationSnapshotId;
//        }

//        public static System.String GetParameterName(MdsCommon.ServiceConfigurationSnapshotParameter dataRecord)
//        {
//            return dataRecord.ParameterName;
//        }

//        public static System.String GetParameterName(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceConfigurationSnapshotParameter).ParameterName;
//        }
//    }

//    /// <summary>
//    /// 
//    /// </summary>
//    [Metapsi.Reflection.DataItem("cf1f36fe-08bf-4889-ac19-5de3dc9a1cb2")]
//    public partial class ServiceStatus : Metapsi.Reflection.IRecord, Metapsi.Reflection.IClonable<MdsCommon.ServiceStatus>
//    {
//        [Metapsi.Reflection.DataItemField("283c5d44-abf1-4239-b194-af4e0ea9aca7")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [Metapsi.Reflection.DataItemField("1b85316a-34ed-4742-a172-c8c7142131d8")]
//        [Metapsi.Reflection.ScalarTypeName("String")]
//        public System.String ServiceName { get; set; } = System.String.Empty;
//        [Metapsi.Reflection.DataItemField("9fb96e7a-8a83-4007-a3cb-7029f2510fcc")]
//        [Metapsi.Reflection.ScalarTypeName("Int")]
//        public System.Int32 Pid { get; set; }

//        [Metapsi.Reflection.DataItemField("6b833d21-7471-41e4-b95b-9d7762496542")]
//        [Metapsi.Reflection.ScalarTypeName("Timestamp")]
//        public System.DateTime StartTimeUtc { get; set; }

//        [Metapsi.Reflection.DataItemField("3c69f1fd-cd1b-44ee-adf7-344d40d51f29")]
//        [Metapsi.Reflection.ScalarTypeName("Int")]
//        public System.Int32 UsedRamMb { get; set; }

//        [Metapsi.Reflection.DataItemField("ec0c2ef1-6eb5-4f8b-88d5-5fc1071dfecf")]
//        [Metapsi.Reflection.ScalarTypeName("Id")]
//        public System.Guid MachineStatusId { get; set; }

//        [Metapsi.Reflection.DataItemField("3bbb4fb1-ece8-4fbf-8cd0-a897d9270481")]
//        [Metapsi.Reflection.ScalarTypeName("Timestamp")]
//        public System.DateTime StatusTimestamp { get; set; }

//        public MdsCommon.ServiceStatus Clone()
//        {
//            var clone = new MdsCommon.ServiceStatus();
//            clone.Id = this.Id;
//            clone.ServiceName = this.ServiceName;
//            clone.Pid = this.Pid;
//            clone.StartTimeUtc = this.StartTimeUtc;
//            clone.UsedRamMb = this.UsedRamMb;
//            clone.MachineStatusId = this.MachineStatusId;
//            clone.StatusTimestamp = this.StatusTimestamp;
//            return clone;
//        }

//        public static System.Guid GetId(MdsCommon.ServiceStatus dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceStatus).Id;
//        }

//        public static System.String GetServiceName(MdsCommon.ServiceStatus dataRecord)
//        {
//            return dataRecord.ServiceName;
//        }

//        public static System.String GetServiceName(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceStatus).ServiceName;
//        }

//        public static System.Int32 GetPid(MdsCommon.ServiceStatus dataRecord)
//        {
//            return dataRecord.Pid;
//        }

//        public static System.Int32 GetPid(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceStatus).Pid;
//        }

//        public static System.DateTime GetStartTimeUtc(MdsCommon.ServiceStatus dataRecord)
//        {
//            return dataRecord.StartTimeUtc;
//        }

//        public static System.DateTime GetStartTimeUtc(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceStatus).StartTimeUtc;
//        }

//        public static System.Int32 GetUsedRamMb(MdsCommon.ServiceStatus dataRecord)
//        {
//            return dataRecord.UsedRamMb;
//        }

//        public static System.Int32 GetUsedRamMb(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceStatus).UsedRamMb;
//        }

//        public static System.Guid GetMachineStatusId(MdsCommon.ServiceStatus dataRecord)
//        {
//            return dataRecord.MachineStatusId;
//        }

//        public static System.Guid GetMachineStatusId(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceStatus).MachineStatusId;
//        }

//        public static System.DateTime GetStatusTimestamp(MdsCommon.ServiceStatus dataRecord)
//        {
//            return dataRecord.StatusTimestamp;
//        }

//        public static System.DateTime GetStatusTimestamp(Metapsi.Reflection.IRecord dataRecord)
//        {
//            return (dataRecord as MdsCommon.ServiceStatus).StatusTimestamp;
//        }
//    }

//    /// <summary>
//    /// 
//    /// </summary>
//    [Metapsi.Reflection.DataStructure("6c124ff6-9ab2-4e54-8bda-f163b6d63230")]
//    public partial class SingleServiceConfigurationSnapshot : Metapsi.Reflection.IDataStructure, MdsCommon.ISingleServiceConfigurationSnapshot
//    {
//        public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> ServiceConfigurationSnapshot { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot>();
//        public Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> ServiceConfigurationSnapshotParameters { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter>();
//        public Diff.DataStructureDiff DiffToPrevious(MdsCommon.SingleServiceConfigurationSnapshot previous)
//        {
//            var diff = new Diff.DataStructureDiff();
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshot, this.ServiceConfigurationSnapshot, "ServiceConfigurationSnapshot"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ServiceConfigurationSnapshotParameters, this.ServiceConfigurationSnapshotParameters, "ServiceConfigurationSnapshotParameters"));
//            return diff;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshot> GetServiceConfigurationSnapshot(MdsCommon.SingleServiceConfigurationSnapshot dataStructure)
//        {
//            return dataStructure.ServiceConfigurationSnapshot;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshot(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshot").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsCommon.ServiceConfigurationSnapshotParameter> GetServiceConfigurationSnapshotParameters(MdsCommon.SingleServiceConfigurationSnapshot dataStructure)
//        {
//            return dataStructure.ServiceConfigurationSnapshotParameters;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetServiceConfigurationSnapshotParameters(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("ServiceConfigurationSnapshotParameters").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }
//    }

//    /// <summary>
//    /// View specific event details
//    /// </summary>
//    [Metapsi.Reflection.DataStructure("a293b8dd-cde0-4eb8-b843-3affb0ac2ff1")]
//    public partial class ViewInfrastructureEventPage : Metapsi.Reflection.IDataStructure, MdsCommon.IViewInfrastructureEventPage, UI.Svelte.IPageBehavior
//    {
//        public Metapsi.Reflection.RecordCollection<UI.Svelte.NamedValue> Variables { get; set; } = new Metapsi.Reflection.RecordCollection<UI.Svelte.NamedValue>();
//        public Metapsi.Reflection.RecordCollection<UI.Svelte.ValidationMessage> ValidationMessages { get; set; } = new Metapsi.Reflection.RecordCollection<UI.Svelte.ValidationMessage>();
//        public Metapsi.Reflection.RecordCollection<UI.Svelte.UiFilterValue> FilterValues { get; set; } = new Metapsi.Reflection.RecordCollection<UI.Svelte.UiFilterValue>();
//        public Metapsi.Reflection.RecordCollection<UI.Svelte.FilterDeclaration> FilterDeclarations { get; set; } = new Metapsi.Reflection.RecordCollection<UI.Svelte.FilterDeclaration>();
//        public Metapsi.Reflection.RecordCollection<MdsCommon.InfrastructureEvent> InfrastructureEvents { get; set; } = new Metapsi.Reflection.RecordCollection<MdsCommon.InfrastructureEvent>();
//        public MdsCommon.ViewInfrastructureEventPage AddPageBehavior(UI.Svelte.IPageBehavior source)
//        {
//            this.Variables.AddRange(source.Variables.Clone());
//            this.ValidationMessages.AddRange(source.ValidationMessages.Clone());
//            this.FilterValues.AddRange(source.FilterValues.Clone());
//            this.FilterDeclarations.AddRange(source.FilterDeclarations.Clone());
//            return this;
//        }

//        public MdsCommon.ViewInfrastructureEventPage UpdatePageBehavior(UI.Svelte.IPageBehavior source)
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

//        public Diff.DataStructureDiff DiffToPrevious(MdsCommon.ViewInfrastructureEventPage previous)
//        {
//            var diff = new Diff.DataStructureDiff();
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.Variables, this.Variables, "Variables"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.ValidationMessages, this.ValidationMessages, "ValidationMessages"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.FilterValues, this.FilterValues, "FilterValues"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.FilterDeclarations, this.FilterDeclarations, "FilterDeclarations"));
//            diff = Diff.Compare.MergeDiffs(diff, Diff.Compare.GetDiff(previous.InfrastructureEvents, this.InfrastructureEvents, "InfrastructureEvents"));
//            return diff;
//        }

//        public static Metapsi.Reflection.RecordCollection<UI.Svelte.NamedValue> GetVariables(MdsCommon.ViewInfrastructureEventPage dataStructure)
//        {
//            return dataStructure.Variables;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetVariables(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("Variables").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<UI.Svelte.ValidationMessage> GetValidationMessages(MdsCommon.ViewInfrastructureEventPage dataStructure)
//        {
//            return dataStructure.ValidationMessages;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetValidationMessages(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("ValidationMessages").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<UI.Svelte.UiFilterValue> GetFilterValues(MdsCommon.ViewInfrastructureEventPage dataStructure)
//        {
//            return dataStructure.FilterValues;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetFilterValues(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("FilterValues").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<UI.Svelte.FilterDeclaration> GetFilterDeclarations(MdsCommon.ViewInfrastructureEventPage dataStructure)
//        {
//            return dataStructure.FilterDeclarations;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetFilterDeclarations(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("FilterDeclarations").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }

//        public static Metapsi.Reflection.RecordCollection<MdsCommon.InfrastructureEvent> GetInfrastructureEvents(MdsCommon.ViewInfrastructureEventPage dataStructure)
//        {
//            return dataStructure.InfrastructureEvents;
//        }

//        public static Metapsi.Reflection.IRecordCollection GetInfrastructureEvents(Metapsi.Reflection.IDataStructure dataStructure)
//        {
//            return dataStructure.GetType().GetProperty("InfrastructureEvents").GetValue(dataStructure) as Metapsi.Reflection.IRecordCollection;
//        }
//    }
//}