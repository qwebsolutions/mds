using System.Collections.Generic;

namespace MdsInfrastructure
{
    public class InfrastructureSummary
    {
        public string InfrastructureName { get; set; }
        public List<ServiceSummary> ServiceReferences { get; set; } = new List<ServiceSummary>();
    }
}
