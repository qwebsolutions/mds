using Metapsi;
using System.Collections.Generic;
using System.Linq;

namespace MdsInfrastructure
{
    public enum ChangeType
    {
        None,
        Added,
        Removed,
        Changed
    }

    public class ServiceChange
    {
        public string ServiceName { get; set; }
        public ChangeType ServiceChangeType { get; set; }

        public ServicePropertyChange NodeName { get; set; } = new ServicePropertyChange();
        public ServicePropertyChange ProjectName { get; set; } = new ServicePropertyChange();
        public ServicePropertyChange ProjectVersionTag { get; set; } = new ServicePropertyChange();

        //public List<ServicePropertyChange> ServiceConfigurationChanges { get; set; } = new List<ServicePropertyChange>();
        public List<ServicePropertyChange> ServiceParameterChanges { get; set; } = new List<ServicePropertyChange>();
    }

    public partial class ServicePropertyChange //: IRecord
    {
        public System.String PropertyName { get; set; } = System.String.Empty;
        public System.String OldValue { get; set; } // null value is relevant here 
        public System.String NewValue { get; set; } // null value is relevant here 
    }

    public class ChangesReport
    {
        public static ServicePropertyChange GetServicePropertyChange<TProp>(
            MdsCommon.ServiceConfigurationSnapshot before,
            MdsCommon.ServiceConfigurationSnapshot after,
            System.Linq.Expressions.Expression<System.Func<MdsCommon.ServiceConfigurationSnapshot, TProp>> property)
        {
            var beforeValue = property.Compile()(before);
            var afterValue = property.Compile()(after);

            return new ServicePropertyChange()
            {
                PropertyName = property.PropertyName(),
                NewValue = afterValue.ToString(),
                OldValue = beforeValue.ToString()
            };
        }

        public static void AddServicePropertyChange<TProp>(
            List<ServicePropertyChange> servicePropertyChanges,
            MdsCommon.ServiceConfigurationSnapshot before,
            MdsCommon.ServiceConfigurationSnapshot after,
            System.Linq.Expressions.Expression<System.Func<MdsCommon.ServiceConfigurationSnapshot, TProp>> property)
        {
            var beforeValue = property.Compile()(before);
            var afterValue = property.Compile()(after);

            if (!object.Equals(beforeValue, afterValue))
            {
                servicePropertyChanges.Add(new ServicePropertyChange()
                {
                    OldValue = beforeValue.ToString(),
                    NewValue = afterValue.ToString(),
                    PropertyName = property.PropertyName(),
                    //ServiceName = before.ServiceName
                });
            }
        }

        public static List<ServicePropertyChange> GetServiceConfigurationChanges(
            MdsCommon.ServiceConfigurationSnapshot before,
            MdsCommon.ServiceConfigurationSnapshot after)
        {
            List<ServicePropertyChange> servicePropertyChanges = new List<ServicePropertyChange>();

            AddServicePropertyChange(servicePropertyChanges, before, after, x => x.ApplicationName);
            AddServicePropertyChange(servicePropertyChanges, before, after, x => x.NodeName);
            AddServicePropertyChange(servicePropertyChanges, before, after, x => x.ProjectName);
            AddServicePropertyChange(servicePropertyChanges, before, after, x => x.ProjectVersionTag);

            return servicePropertyChanges;
        }

        public static List<ServicePropertyChange> GetParameterChanges(
            MdsCommon.ServiceConfigurationSnapshot before,
            MdsCommon.ServiceConfigurationSnapshot after)
        {
            List<ServicePropertyChange> paramChanges = new List<ServicePropertyChange>();

            var beforeParamNames = before.ServiceConfigurationSnapshotParameters.Select(x => x.ParameterName);
            var afterParamNames = after.ServiceConfigurationSnapshotParameters.Select(x => x.ParameterName);

            foreach (var paramName in beforeParamNames.Union(afterParamNames))
            {
                var beforeParam = before.ServiceConfigurationSnapshotParameters.SingleOrDefault(x => x.ParameterName == paramName);
                var afterParam = after.ServiceConfigurationSnapshotParameters.SingleOrDefault(x => x.ParameterName == paramName);

                switch (beforeParam, afterParam)
                {
                    case (null, MdsCommon.ServiceConfigurationSnapshotParameter addedParameter):
                        {
                            paramChanges.Add(new ServicePropertyChange()
                            {
                                //ServiceName = after.ServiceName,
                                PropertyName = addedParameter.ParameterName,
                                NewValue = ProcessParameterValue(addedParameter)
                            });
                        }
                        break;
                    case (MdsCommon.ServiceConfigurationSnapshotParameter removedParameter, null):
                        {
                            paramChanges.Add(new ServicePropertyChange()
                            {
                                //ServiceName = before.ServiceName,
                                OldValue = ProcessParameterValue(removedParameter),
                                PropertyName = removedParameter.ParameterName
                            });
                        }
                        break;
                    default:
                        {
                            if (beforeParam.DeployedValue != afterParam.DeployedValue)
                            {
                                paramChanges.Add(new ServicePropertyChange()
                                {
                                    OldValue = ProcessParameterValue(beforeParam),
                                    NewValue = ProcessParameterValue(afterParam),
                                    PropertyName = paramName,
                                    //ServiceName = before.ServiceName
                                });
                            }
                        }
                        break;
                }
            }

            return paramChanges;
        }

        private static string ProcessParameterValue(MdsCommon.ServiceConfigurationSnapshotParameter parameter)
        {
            if (parameter.ParameterTypeId == System.Guid.Parse("a7039392-4c9a-4e28-b692-9936992b09c5"))
            {
                return MdsCommon.Parameter.ParseConnectionString(parameter.DeployedValue).ToString();
            }

            if (parameter.ParameterName.ToLower().Contains("password"))
            {
                return "*****";
            }

            return parameter.DeployedValue;
        }

        public static ServiceChange GetChanges(
            MdsCommon.ServiceConfigurationSnapshot before,
            MdsCommon.ServiceConfigurationSnapshot after)
        {
            switch (before, after)
            {
                case (null, null):
                    throw new System.ArgumentException("This is absolutely not possible in this world, you must have reached the singularity!");
                case (null, MdsCommon.ServiceConfigurationSnapshot justAdded):
                    {
                        before = new MdsCommon.ServiceConfigurationSnapshot()
                        {
                            ServiceName = after.ServiceName,
                        };

                        return new ServiceChange()
                        {
                            ServiceChangeType = ChangeType.Added,
                            NodeName = GetServicePropertyChange(before, after, x => x.NodeName),
                            ProjectName = GetServicePropertyChange(before, after, x=> x.ProjectName),
                            ProjectVersionTag = GetServicePropertyChange(before, after, x=>x.ProjectVersionTag),
                            ServiceName = after.ServiceName,
                            ServiceParameterChanges = GetParameterChanges(before, after)
                        };
                    }
                case (MdsCommon.ServiceConfigurationSnapshot removed, null):
                    {
                        after = new MdsCommon.ServiceConfigurationSnapshot() { ServiceName = before.ServiceName };
                        return new ServiceChange()
                        {
                            ServiceChangeType = ChangeType.Removed,
                            NodeName = GetServicePropertyChange(before, after, x => x.NodeName),
                            ProjectName = GetServicePropertyChange(before, after, x => x.ProjectName),
                            ProjectVersionTag = GetServicePropertyChange(before, after, x => x.ProjectVersionTag),
                            ServiceName = removed.ServiceName,
                            ServiceParameterChanges = GetParameterChanges(before, after)
                        };
                    }
                default:
                    {
                        if (before.ServiceName != after.ServiceName)
                            throw new System.NotSupportedException("Cannot compare services with different name!");

                        var configChanges = GetServiceConfigurationChanges(before, after);
                        var paramChanges = GetParameterChanges(before, after);

                        if (configChanges.Count == 0 && paramChanges.Count == 0)
                        {
                            return new ServiceChange()
                            {
                                ServiceChangeType = ChangeType.None,
                                ServiceName = before.ServiceName
                            };
                        }
                        else
                        {
                            return new ServiceChange()
                            {
                                ServiceChangeType = ChangeType.Changed,
                                ServiceName = before.ServiceName,
                                NodeName = GetServicePropertyChange(before, after, x => x.NodeName),
                                ProjectName = GetServicePropertyChange(before, after, x => x.ProjectName),
                                ProjectVersionTag = GetServicePropertyChange(before, after, x => x.ProjectVersionTag),
                                ServiceParameterChanges = paramChanges,
                            };
                        }
                    }
            }
        }

        public List<ServiceChange> ServiceChanges { get; set; } = new();

        public static ChangesReport Get(List<MdsCommon.ServiceConfigurationSnapshot> previous, List<MdsCommon.ServiceConfigurationSnapshot> next)
        {
            ChangesReport changesReport = new();

            var allServiceNames = previous.Select(x => x.ServiceName).Union(next.Select(x => x.ServiceName)).OrderBy(x => x).ToList();

            foreach (var serviceName in allServiceNames)
            {
                var prevSnapshot = previous.SingleOrDefault(x => x.ServiceName == serviceName);
                var nextSnapshot = next.SingleOrDefault(x => x.ServiceName == serviceName);

                changesReport.ServiceChanges.Add(GetChanges(prevSnapshot, nextSnapshot));
            }

            return changesReport;
        }
    }
}