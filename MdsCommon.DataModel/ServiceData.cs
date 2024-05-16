namespace MdsCommon
{
    /// <summary>
    /// Just the data that is relevant for upgrades
    /// </summary>
    public class ServiceData
    {
        public string ServiceName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectVersionTag { get; set; }
        public string NodeName { get; set; }
        public bool Enabled { get; set; }
    }

    public class ServiceParameterData
    {
        public string ParameterName { get; set; }
        public string DeployedValue { get; set; }
    }



    public static class ServiceDataExtensions
    {
        public static ServiceData GetServiceData(this MdsCommon.ServiceConfigurationSnapshot snapshot)
        {
            return new ServiceData()
            {
                Enabled = snapshot.Enabled,
                NodeName = snapshot.NodeName,
                ProjectName = snapshot.ProjectName,
                ProjectVersionTag = snapshot.ProjectVersionTag,
                ServiceName = snapshot.ServiceName
            };
        }

        public static ServiceParameterData GetServiceParameterData(this MdsCommon.ServiceConfigurationSnapshotParameter parameter)
        {
            return new ServiceParameterData()
            {
                ParameterName = parameter.ParameterName,
                DeployedValue = parameter.DeployedValue
            };
        }
    }
}