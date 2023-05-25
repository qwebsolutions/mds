namespace MdsInfrastructure
{
    public class ServiceParameter
    {
        public string ParameterName { get; set; }
        public string ParameterType { get; set; } = string.Empty;
        public string ParameterTypeDescription { get; set; } = string.Empty;
        public string ParameterComment { get; set; } = string.Empty;
        public string DeployedValue { get; set; }
    }


}
