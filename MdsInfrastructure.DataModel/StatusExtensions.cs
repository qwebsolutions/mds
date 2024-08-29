//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace MdsInfrastructure
//{
//    //public enum GeneralStatus
//    //{
//    //    Ok,
//    //    Warning,
//    //    Danger,
//    //    NoData
//    //}

//    //public class StatusValue
//    //{
//    //    public string Name { get; set; }
//    //    public GeneralStatus GeneralStatus { get; set; }
//    //    public string CurrentValue { get; set; }
//    //    public string NormalValue { get; set; }
//    //    public string Message { get; set; }
//    //}

//    //public class FullStatus<TEntity>
//    //{
//    //    public TEntity Entity { get; set; }
//    //    public List<StatusValue> StatusValues { get; set; } = new List<StatusValue>();
//    //}

//    public static class StatusExtensions
//    {
//        public const string ServiceRunningSince = "ServiceRunningSince";
//        public const string ServiceRunningFor = "ServiceRunningFor";
//        public const string ServiceSyncTimestamp = "ServiceSyncTimestamp";
//        public const string ServiceSyncAgo = "SyncAgo";
//        public const string ServiceUsedRam = "ServiceUsedRam";
//        public const string HasErrors = "HasErrors";
//        public const string StartCount = "StartCount";
//        public const string CrashCount = "CrashCount";

//        public const int LimitSyncSecondsAgo = 60;
//        public const decimal LimitServiceUsedRam = 800;

//        public const string AvailableHddPercent = "AvailableHddPercent";
//        public const string AvailableHddGb = "AvailableHddGb";

//        public const string AvailableRamPercent = "AvailableRamPercent";
//        public const string AvailableRamGb = "AvailableRamGb";

//        // Percent limits don't make much sense, really
//        public const decimal LimitHddPercent = 10;
//        public const decimal LimitAvailableHddGb = 1;

//        public const decimal LimitAvailableRamPercent = 10;
//        public const decimal LimitAvailableRamGb = 0.75m;

//        public static FullStatus<MdsCommon.ServiceConfigurationSnapshot> GetServiceStatus(
//            Deployment singleDeployment,
//            List<MdsCommon.MachineStatus> healthStatus,
//            MdsCommon.ServiceConfigurationSnapshot serviceConfigurationSnapshot,
//            List<MdsCommon.InfrastructureEvent> allInfrastructureEvents)
//        {
//            var chronologicalEvents = allInfrastructureEvents.Where(x => x.Source == serviceConfigurationSnapshot.ServiceName).OrderBy(x => x.Timestamp);

//            FullStatus<MdsCommon.ServiceConfigurationSnapshot> fullStatus = new FullStatus<MdsCommon.ServiceConfigurationSnapshot>();
//            //InfrastructureService service = infrastructureConfiguration.InfrastructureServices.ById(serviceId);
//            fullStatus.Entity = serviceConfigurationSnapshot;
//            var serviceStatus = healthStatus.SelectMany(x => x.ServiceStatuses).SingleOrDefault(x => x.ServiceName == serviceConfigurationSnapshot.ServiceName);


//            if (serviceStatus == null)
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    GeneralStatus = GeneralStatus.NoData,
//                    Message = "Data not available!",
//                    Name = "NoData"
//                });

//                return fullStatus;
//            }
//            var lastConfigurationChange = singleDeployment.LastConfigurationChanges.Single(x => x.ServiceName == serviceConfigurationSnapshot.ServiceName);
//            // Kinda flimsy based on timestamp, ain't it?
//            var eventsSinceLastReconfigured = chronologicalEvents.Where(x => x.Timestamp > lastConfigurationChange.LastConfigurationChangeTimestamp);

//            // If started multiple times since last deployment, could be a problem
//            var multipleStarts = eventsSinceLastReconfigured.Where(x => x.Type == MdsCommon.InfrastructureEventType.ProcessStart);

//            fullStatus.StatusValues.Add(new StatusValue()
//            {
//                GeneralStatus = GeneralStatus.Ok,
//                Message = StartCount,
//                Name = StartCount,
//                CurrentValue = multipleStarts.Count().ToString()
//            });

//            var exitEvents = eventsSinceLastReconfigured.Where(x => x.Type == MdsCommon.InfrastructureEventType.ProcessExit);


//            if (exitEvents.Count() > 30)
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    GeneralStatus = GeneralStatus.Danger,
//                    Message = CrashCount,
//                    Name = CrashCount,
//                    CurrentValue = exitEvents.Count().ToString(),
//                    NormalValue = "0"
//                });
//            }


//            // Last start could be prior to last deployment. Any fatal error since last started is relevant
//            var lastStart = chronologicalEvents.LastOrDefault(x => x.Type == MdsCommon.InfrastructureEventType.ProcessStart);
//            if (lastStart != null)
//            {
//                var eventsSinceLastStart = chronologicalEvents.SkipWhile(x => x.Id != lastStart.Id);
//                var fatalError = eventsSinceLastStart.FirstOrDefault(x => x.Criticality == MdsCommon.InfrastructureEventCriticality.Fatal);
//                if (fatalError != null)
//                {
//                    fullStatus.StatusValues.Add(new StatusValue()
//                    {
//                        GeneralStatus = GeneralStatus.Danger,
//                        Message = HasErrors,
//                        Name = HasErrors,
//                        CurrentValue = fatalError.ShortDescription
//                    });
//                }
//            }


//            TimeSpan running = DateTime.UtcNow - serviceStatus.StartTimeUtc.ToUniversalTime();
//            TimeSpan rounded = TimeSpan.FromSeconds((int)running.TotalSeconds);

//            string runningSince = $"{serviceStatus.StartTimeUtc} UTC ({rounded.ToString("c")})";

//            fullStatus.StatusValues.Add(new StatusValue()
//            {
//                Name = ServiceRunningSince,
//                CurrentValue = serviceStatus.StartTimeUtc.ToLocalTime().ToString("G", System.Globalization.CultureInfo.GetCultureInfo("it-IT")),
//                GeneralStatus = GeneralStatus.Ok,
//                Message = "OK"
//            });

//            if (rounded.TotalSeconds < 0) // service is not actually running!
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    Name = ServiceRunningFor,
//                    CurrentValue = rounded.ToString("c"),
//                    GeneralStatus = GeneralStatus.Danger,
//                    Message = "OK"
//                });
//            }
//            else
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    Name = ServiceRunningFor,
//                    CurrentValue = rounded.ToString("c"),
//                    GeneralStatus = GeneralStatus.Ok,
//                    Message = "OK"
//                });
//            }

//            fullStatus.StatusValues.Add(new StatusValue()
//            {
//                Name = ServiceSyncTimestamp,
//                CurrentValue = serviceStatus.StatusTimestamp.ToLocalTime().ToString("G"),
//                GeneralStatus = GeneralStatus.Ok
//            });

//            TimeSpan syncAgo = TimeSpan.FromSeconds((int)(DateTime.UtcNow - serviceStatus.StatusTimestamp.ToUniversalTime()).TotalSeconds);

//            if (syncAgo.TotalSeconds > LimitSyncSecondsAgo)
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    CurrentValue = syncAgo.TotalSeconds.ToString(),
//                    GeneralStatus = GeneralStatus.Danger,
//                    Message = "Status not received recently!",
//                    Name = ServiceSyncAgo,
//                    NormalValue = LimitSyncSecondsAgo.ToString()
//                });
//            }
//            else
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    CurrentValue = syncAgo.TotalSeconds.ToString(),
//                    GeneralStatus = GeneralStatus.Ok,
//                    Message = "Status received",
//                    Name = ServiceSyncAgo,
//                    NormalValue = LimitSyncSecondsAgo.ToString()
//                });
//            }

//            if (serviceStatus.UsedRamMb > LimitServiceUsedRam)
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    CurrentValue = serviceStatus.UsedRamMb.ToString(),
//                    GeneralStatus = GeneralStatus.Warning,
//                    Message = "RAM usage is high!",
//                    Name = ServiceUsedRam,
//                    NormalValue = LimitServiceUsedRam.ToString()
//                });
//            }
//            else
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    CurrentValue = serviceStatus.UsedRamMb.ToString(),
//                    GeneralStatus = GeneralStatus.Ok,
//                    Message = "RAM usage is normal",
//                    Name = ServiceUsedRam,
//                    NormalValue = LimitServiceUsedRam.ToString()
//                });
//            }

//            return fullStatus;
//        }

//        //public static FullStatus<InfrastructureService> GetServiceStatus(
//        //    InfrastructureConfiguration infrastructureConfiguration,
//        //    MdsCommon.HealthStatus healthStatus,
//        //    Guid serviceId)
//        //{
//        //    FullStatus<InfrastructureService> fullStatus = new FullStatus<InfrastructureService>();
//        //    InfrastructureService service = infrastructureConfiguration.InfrastructureServices.ById(serviceId);
//        //    fullStatus.Entity = service;
//        //    var serviceStatus = healthStatus.ServiceStatuses.SingleOrDefault(x => x.ServiceName == service.ServiceName);

//        //    if (serviceStatus == null)
//        //    {
//        //        fullStatus.StatusValues.Add(new StatusValue()
//        //        {
//        //            GeneralStatus = GeneralStatus.NoData,
//        //            Message = "Data not available!",
//        //            Name = "NoData"
//        //        });

//        //        return fullStatus;
//        //    }

//        //    InfrastructureNode node = infrastructureConfiguration.InfrastructureNodes.ById(service.InfrastructureNodeId);

//        //    TimeSpan running = DateTime.UtcNow - serviceStatus.StartTimeUtc.ToUniversalTime();
//        //    TimeSpan rounded = TimeSpan.FromSeconds((int)running.TotalSeconds);

//        //    string runningSince = $"{serviceStatus.StartTimeUtc} UTC ({rounded.ToString("c")})";

//        //    fullStatus.StatusValues.Add(new StatusValue()
//        //    {
//        //        Name = ServiceRunningSince,
//        //        CurrentValue = serviceStatus.StartTimeUtc.ToLocalTime().ToString("G"),
//        //        GeneralStatus = GeneralStatus.Ok,
//        //        Message = "OK"
//        //    });

//        //    if (rounded.TotalSeconds < 0) // service is not actually running!
//        //    {
//        //        fullStatus.StatusValues.Add(new StatusValue()
//        //        {
//        //            Name = ServiceRunningFor,
//        //            CurrentValue = rounded.ToString("c"),
//        //            GeneralStatus = GeneralStatus.Danger,
//        //            Message = "OK"
//        //        });
//        //    }
//        //    else
//        //    {
//        //        fullStatus.StatusValues.Add(new StatusValue()
//        //        {
//        //            Name = ServiceRunningFor,
//        //            CurrentValue = rounded.ToString("c"),
//        //            GeneralStatus = GeneralStatus.Ok,
//        //            Message = "OK"
//        //        });
//        //    }

//        //    fullStatus.StatusValues.Add(new StatusValue()
//        //    {
//        //        Name = ServiceSyncTimestamp,
//        //        CurrentValue = serviceStatus.StatusTimestamp.ToLocalTime().ToString("G"),
//        //        GeneralStatus = GeneralStatus.Ok
//        //    });

//        //    TimeSpan syncAgo = TimeSpan.FromSeconds((int)(DateTime.UtcNow - serviceStatus.StatusTimestamp.ToUniversalTime()).TotalSeconds);

//        //    if (syncAgo.TotalSeconds > LimitSyncSecondsAgo)
//        //    {
//        //        fullStatus.StatusValues.Add(new StatusValue()
//        //        {
//        //            CurrentValue = syncAgo.TotalSeconds.ToString(),
//        //            GeneralStatus = GeneralStatus.Danger,
//        //            Message = "Status not received recently!",
//        //            Name = ServiceSyncAgo,
//        //            NormalValue = LimitSyncSecondsAgo.ToString()
//        //        });
//        //    }
//        //    else
//        //    {
//        //        fullStatus.StatusValues.Add(new StatusValue()
//        //        {
//        //            CurrentValue = syncAgo.TotalSeconds.ToString(),
//        //            GeneralStatus = GeneralStatus.Ok,
//        //            Message = "Status received",
//        //            Name = ServiceSyncAgo,
//        //            NormalValue = LimitSyncSecondsAgo.ToString()
//        //        });
//        //    }

//        //    if (serviceStatus.UsedRamMb > LimitServiceUsedRam)
//        //    {
//        //        fullStatus.StatusValues.Add(new StatusValue()
//        //        {
//        //            CurrentValue = serviceStatus.UsedRamMb.ToString(),
//        //            GeneralStatus = GeneralStatus.Danger,
//        //            Message = "RAM usage is high!",
//        //            Name = ServiceUsedRam,
//        //            NormalValue = LimitServiceUsedRam.ToString()
//        //        });
//        //    }
//        //    else
//        //    {
//        //        fullStatus.StatusValues.Add(new StatusValue()
//        //        {
//        //            CurrentValue = serviceStatus.UsedRamMb.ToString(),
//        //            GeneralStatus = GeneralStatus.Ok,
//        //            Message = "RAM usage is normal",
//        //            Name = ServiceUsedRam,
//        //            NormalValue = LimitServiceUsedRam.ToString()
//        //        });
//        //    }

//        //    return fullStatus;
//        //}



//        public static FullStatus<string> GetNodeStatus(
//            List<MdsCommon.MachineStatus> healthStatus,
//            string nodeName)
//        {
//            FullStatus<string> fullStatus = new FullStatus<string>();
//            fullStatus.Entity = nodeName;

//            //FullStatus<InfrastructureNode> fullStatus = new FullStatus<InfrastructureNode>();
//            //InfrastructureNode node = infrastructureConfiguration.InfrastructureNodes.ById(nodeId);
//            //fullStatus.Entity = node;

//            var nodeStatus = healthStatus.SingleOrDefault(x => x.NodeName == nodeName);

//            if (nodeStatus == null)
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    GeneralStatus = GeneralStatus.NoData,
//                    Message = "Data not available!",
//                    Name = "NoData"
//                });

//                return fullStatus;
//            }

//            decimal availableHddPercent = decimal.Round(decimal.Divide(nodeStatus.HddAvailableMb * 100, nodeStatus.HddTotalMb), 2, MidpointRounding.AwayFromZero);

//            if (availableHddPercent < LimitHddPercent)
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    Name = AvailableHddPercent,
//                    CurrentValue = availableHddPercent.ToString(),
//                    GeneralStatus = GeneralStatus.Ok,
//                    NormalValue = LimitAvailableHddGb.ToString(),
//                    Message = "HDD space low!"
//                });
//            }
//            else
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    Name = AvailableHddPercent,
//                    CurrentValue = availableHddPercent.ToString(),
//                    GeneralStatus = GeneralStatus.Ok,
//                    NormalValue = LimitAvailableHddGb.ToString(),
//                    Message = "HDD space OK"
//                });
//            }

//            decimal availableHddGb = decimal.Round(decimal.Divide(nodeStatus.HddAvailableMb, 1024), 2, MidpointRounding.AwayFromZero);

//            if (availableHddGb < LimitAvailableHddGb)
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    Name = AvailableHddGb,
//                    CurrentValue = availableHddGb.ToString(),
//                    GeneralStatus = GeneralStatus.Danger,
//                    Message = "HDD space low!",
//                    NormalValue = LimitAvailableHddGb.ToString()
//                });
//            }
//            else
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    Name = AvailableHddGb,
//                    CurrentValue = availableHddGb.ToString(),
//                    GeneralStatus = GeneralStatus.Ok,
//                    Message = "HDD space OK",
//                    NormalValue = LimitAvailableHddGb.ToString()
//                });
//            }

//            decimal availableRamPercent = decimal.Round(decimal.Divide(nodeStatus.RamAvailableMb * 100, nodeStatus.RamTotalMb), 2, MidpointRounding.AwayFromZero);

//            if (availableRamPercent < LimitAvailableRamPercent)
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    Name = AvailableRamPercent,
//                    CurrentValue = availableRamPercent.ToString(),
//                    GeneralStatus = GeneralStatus.Ok,
//                    Message = "Available RAM low!",
//                    NormalValue = LimitAvailableRamPercent.ToString()
//                });
//            }
//            else
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    Name = AvailableRamPercent,
//                    CurrentValue = availableRamPercent.ToString(),
//                    GeneralStatus = GeneralStatus.Ok,
//                    Message = "Available RAM OK",
//                    NormalValue = LimitAvailableRamPercent.ToString()
//                });
//            }


//            decimal availableRamGb = decimal.Round(decimal.Divide(nodeStatus.RamAvailableMb, 1024), 2, MidpointRounding.AwayFromZero);

//            if (availableRamGb < LimitAvailableRamGb)
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    Name = AvailableRamGb,
//                    CurrentValue = availableRamGb.ToString(),
//                    GeneralStatus = GeneralStatus.Danger,
//                    Message = "Available RAM low!",
//                    NormalValue = LimitAvailableRamGb.ToString()
//                });
//            }
//            else
//            {
//                fullStatus.StatusValues.Add(new StatusValue()
//                {
//                    Name = AvailableRamGb,
//                    CurrentValue = availableRamGb.ToString(),
//                    GeneralStatus = GeneralStatus.Ok,
//                    Message = "Available RAM ok",
//                    NormalValue = LimitAvailableRamGb.ToString()
//                });
//            }

//            return fullStatus;
//        }
//    }
//}