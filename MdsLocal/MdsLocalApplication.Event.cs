using Metapsi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsLocal
{
    public static partial class MdsLocalApplication
    {
        public static partial class Event
        {
            public partial class HealthPing : IData
            {
                public MdsCommon.MachineStatus HealthStatus { get; set; }
            }
            public partial class ServiceSetupComplete : IData 
            {
                public MdsCommon.ServiceConfigurationSnapshot ServiceSnapshot { get; set; }
            }
            //public partial class ConfigurationChanged : IData { }
            public partial class ConfigurationSynchronized : IData
            {
                public string ResultCode { get; set; }
            }
            public partial class GlobalControllerReached : IData
            {
                public MdsCommon.InfrastructureNodeSettings InfrastructureConfiguration { get; set; }
            }

            public class ServicePing : IData
            {
                public string ServiceName { get; set; }
            }

            public class StartupError : IData
            {
                public string ServiceName { get; set; }
                public string ErrorMessage { get; set; }
            }

            public class Error : IData
            {
                public string ServiceName { get; set; }
                public string ErrorMessage { get; set; }
            }

            public class Info : IData
            {
                public string ServiceName { get; set; }
                public string InfoMessage { get; set; }
            }

            public class ProcessExited : IData
            {
                public int Pid { get; set; }
                public int ExitCode { get; set; }
                public string FullExePath { get; set; } = string.Empty;
            }

            public class ProcessAlreadyRunning : IData
            {
                public string ShortProcessName { get; set; } = string.Empty;
                public int Pid { get; set; }
            }

            public class ProcessStarted : IData
            {
                public string FullExePath { get; set; } = string.Empty;
                public string ServiceName { get; set; } = String.Empty;
                public int Pid { get; set; }
                public DateTime StartTimeUtc { get; set; }
            }

            public class ProcessesAttached : IData
            {
                public List<string> ProcessDescriptions { get; set; } = new List<string>();
            }
        }

        public static LocalServicesConfigurationDiff DiffServices(List<MdsCommon.ServiceConfigurationSnapshot> previous, List<MdsCommon.ServiceConfigurationSnapshot> next)
        {
            LocalServicesConfigurationDiff localServicesDiff = new LocalServicesConfigurationDiff();

            if (previous == null)
            {
                previous = new List<MdsCommon.ServiceConfigurationSnapshot>();
            }

            HashSet<string> allServiceNames = previous.Select(x => x.ServiceName).Union(next.Select(x => x.ServiceName)).ToHashSet();

            foreach (var serviceName in allServiceNames)
            {
                var previousConfig = previous.SingleOrDefault(x => x.ServiceName == serviceName);
                var nextConfig = next.SingleOrDefault(x => x.ServiceName == serviceName);

                if (previousConfig != null && nextConfig == null)
                {
                    localServicesDiff.RemovedServices.Add(previousConfig);
                }

                if (previousConfig == null && nextConfig != null)
                {
                    localServicesDiff.AddedServices.Add(nextConfig);
                }

                if (previousConfig != null && nextConfig != null)
                {
                    var diff = Diff.Anything(previousConfig, nextConfig);
                }
            }

            return localServicesDiff;

            //    List<Guid> completelyIdenticalIds = new List<Guid>();

            //    if (previousConfig != null && nextConfig != null && previousConfig.Id == nextConfig.Id)
            //    {
            //        completelyIdenticalIds.Add(previousConfig.Id);
            //    }

            //    localServicesDiff.IdenticalServices.AddRange(next.Where(x => completelyIdenticalIds.Contains(x.Id)));
            //    localServicesDiff.Parameters.AddRange(next.SelectMany(x=>x.ServiceConfigurationSnapshotParameters).Where(x => completelyIdenticalIds.Contains(x.ServiceConfigurationSnapshotId)));
            //}

            //// ADDED

            //IEnumerable<string> addedNames = next.Select(x => x.ServiceName).Except(previous.Select(x => x.ServiceName));

            //foreach (string addedName in addedNames)
            //{
            //    var addedService = next.Single(x => x.ServiceName == addedName);
            //    localServicesDiff.AddedServices.Add(addedService);
            //    localServicesDiff.Parameters.AddRange(next.SelectMany(x => x.ServiceConfigurationSnapshotParameters).Where(x => x.ServiceConfigurationSnapshotId == addedService.Id));
            //}

            //IEnumerable<string> removedNames = previous.Select(x => x.ServiceName).Except(next.Select(x => x.ServiceName));
            //foreach (string removedName in removedNames)
            //{
            //    var removedService = previous.Single(x => x.ServiceName == removedName);
            //    localServicesDiff.RemovedServices.Add(removedService);
            //    localServicesDiff.Parameters.AddRange(previous.SelectMany(x => x.ServiceConfigurationSnapshotParameters).Where(x => x.ServiceConfigurationSnapshotId == removedService.Id));
            //}

            //IEnumerable<string> commonNames = previous.Select(x => x.ServiceName).Intersect(next.Select(x => x.ServiceName));
            //IEnumerable<string> changedServiceNames = commonNames.Except(localServicesDiff.IdenticalServices.Select(x => x.ServiceName));

            //foreach (string changedServiceName in changedServiceNames)
            //{
            //    var changedService = next.Single(x => x.ServiceName == changedServiceName);
            //    localServicesDiff.ChangedServices.Add(changedService);
            //    localServicesDiff.Parameters.AddRange(next.SelectMany(x => x.ServiceConfigurationSnapshotParameters).Where(x => x.ServiceConfigurationSnapshotId == changedService.Id));
            //}
            //return localServicesDiff;
        }

        public static bool HasAnyUpdate(this LocalServicesConfigurationDiff localServicesDiff)
        {
            if (localServicesDiff.AddedServices.Any())
                return true;

            if (localServicesDiff.ChangedServices.Any())
                return true;

            if (localServicesDiff.RemovedServices.Any())
                return true;

            return false;
        }
    }
}
