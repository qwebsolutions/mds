using Metapsi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsLocal
{
    public partial class SyncResult : IRecord
    {
        [DataItemField("ec670644-4e60-47fe-8498-bbc7fab9061a")]
        [ScalarTypeName("Id")]
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        [DataItemField("4aced480-ddc8-4868-85a0-d24931c5cf04")]
        [ScalarTypeName("Timestamp")]
        public System.DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [DataItemField("4d1ed7ba-0ae8-479a-bee0-3cac19c888c7")]
        [ScalarTypeName("String")]
        public System.String Trigger { get; set; } = System.String.Empty;
        [DataItemField("ae4a17c6-f711-4448-a000-fd036c87fc11")]
        [ScalarTypeName("String")]
        public System.String ResultCode { get; set; } = System.String.Empty;

        public List<SyncResultLog> Log { get; set; } = new();
    }

    public static class SyncResultLogType
    {
        public const string Error = "error";
        public const string Warning = "warning";
        public const string Info = "info";
    }

    public class SyncResultLog : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public int Index { get; set; }
        public System.Guid SyncResultId { get; set; }
        public string TimestampIso { get; set; } = DateTime.UtcNow.Roundtrip();
        public string Type { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public static class SyncResultExtensions
    {
        private static SyncResultLog AddLog(this SyncResult syncResult, string type, string message)
        {
            var log = new SyncResultLog()
            {
                Type = type,
                SyncResultId = syncResult.Id,
                Message = message,
                Index = syncResult.Log.Any() ? syncResult.Log.Max(x => x.Index) : 1
            };

            syncResult.Log.Add(log);
            return log;
        }

        public static SyncResultLog AddError(this SyncResult syncResult, string message)
        {
            return syncResult.AddLog(SyncResultLogType.Error, message);
        }

        public static SyncResultLog AddWarning(this SyncResult syncResult, string message)
        {
            return syncResult.AddLog(SyncResultLogType.Warning, message);
        }

        public static SyncResultLog AddInfo(this SyncResult syncResult, string message)
        {
            return syncResult.AddLog(SyncResultLogType.Info, message);
        }
    }

    /// <summary>
    /// Data about the project binaries, as retrieved from the repository. Project binaries are then 'installed' as ServiceBinaries (using service configuration)
    /// </summary>
    [DataItem("2ed24d2c-ca10-4232-b2de-cf32043f52df")]
    public partial class ProjectBinary : IRecord
    {
        [DataItemField("d6542408-34e8-4935-abc9-67ecf943fee2")]
        [ScalarTypeName("Id")]
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        [DataItemField("6e7e2463-d012-40a3-a2ac-d3b39dbd84e0")]
        [ScalarTypeName("String")]
        public System.String FullExePath { get; set; } = System.String.Empty;
        [DataItemField("9fefb615-863f-452c-b8f4-3ac40f380605")]
        [ScalarTypeName("String")]
        public System.String ProjectName { get; set; } = System.String.Empty;
        [DataItemField("4f84b99c-e005-4771-8b61-dbbbab211917")]
        [ScalarTypeName("String")]
        public System.String VersionTag { get; set; } = System.String.Empty;
    }

    /// <summary>
    /// Some settings from the startup configuration
    /// </summary>
    [DataItem("0aeb5149-9542-4bf0-918c-079f8cca2be8")]
    public partial class LocalSettings : IRecord
    {
        [DataItemField("df792c44-cdc6-41fb-bc3a-2696360503c0")]
        [ScalarTypeName("Id")]
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        [DataItemField("7af4b397-c6bc-484f-bd59-2eab3762611f")]
        [ScalarTypeName("String")]
        public System.String InfrastructureApiUrl { get; set; } = System.String.Empty;
        [DataItemField("49bfd7dc-3c3c-41af-b7bb-62a877e55d84")]
        [ScalarTypeName("String")]
        public System.String NodeName { get; set; } = System.String.Empty;
        [DataItemField("aede2366-2111-44f2-a850-be913e575b78")]
        [ScalarTypeName("String")]
        public System.String FullDbPath { get; set; } = System.String.Empty;
    }


    /// <summary>
    /// Service, Pid + some health data. Not saved to DB, data comes from processes themselves
    /// </summary>
    [DataItem("bdb2b3f1-dabb-432b-b5fa-1710e5136986")]
    public partial class RunningServiceProcess : IRecord
    {
        [DataItemField("9e3ac298-5152-423b-9df7-ab8d9fd969b2")]
        [ScalarTypeName("Id")]
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        [DataItemField("a0a95d79-413b-4628-a5ce-34059d41ec7e")]
        [ScalarTypeName("String")]
        public System.String ServiceName { get; set; } = System.String.Empty;
        [DataItemField("489f9199-6aa9-4b67-adac-2a51b7f9d192")]
        [ScalarTypeName("Int")]
        public System.Int32 Pid { get; set; }

        [DataItemField("431646b5-6f36-470f-9572-1bb12a62e853")]
        [ScalarTypeName("Timestamp")]
        public System.DateTime StartTimestampUtc { get; set; }

        [DataItemField("46e0a5ab-0070-4d0c-b58f-e26570b726d4")]
        [ScalarTypeName("String")]
        public System.String FullExePath { get; set; } = System.String.Empty;
        [DataItemField("818a36d9-5f0c-4652-8053-d81710c8638a")]
        [ScalarTypeName("Int")]
        public System.Int32 UsedRamMB { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataItem("f12d799c-dac4-4bb2-92be-b8c5caf4e23f")]
    public partial class ServiceBinaryCreated : IRecord, IData
    {
        [DataItemField("ab53819c-d052-45f9-b9fc-8ac52407ef8a")]
        [ScalarTypeName("Id")]
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        [DataItemField("fce9b8b0-61b1-41f5-8c0b-f5448cdc558f")]
        [ScalarTypeName("String")]
        public System.String ServiceName { get; set; } = System.String.Empty;
        [DataItemField("0002176d-43af-4da0-a9b0-0e2f00bdf59d")]
        [ScalarTypeName("String")]
        public System.String FullExePath { get; set; } = System.String.Empty;
    }


    /// <summary>
    /// Installed binaries (project binaries + configuration). Same project binaries can be installed multiple times for different services
    /// </summary>
    [DataItem("0e23e4e4-80e9-4adf-ad89-a2dea20ed25c")]
    public partial class ServiceBinary : IRecord
    {
        [DataItemField("3e670cd6-c88e-42c6-beee-6ce5f333d0c4")]
        [ScalarTypeName("Id")]
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        [DataItemField("146812ef-423a-40e0-b850-ee334eb0e730")]
        [ScalarTypeName("Id")]
        public System.Guid ProjectBinaryId { get; set; }

        [DataItemField("9a007a6a-1d7b-4b13-b544-db8a916e4047")]
        [ScalarTypeName("Id")]
        public System.Guid LocalServiceId { get; set; }

        [DataItemField("2f3efdc0-8f69-4779-8a55-b811c15b3bbc")]
        [ScalarTypeName("String")]
        public System.String ServiceFullExePath { get; set; } = System.String.Empty;
    }
}
