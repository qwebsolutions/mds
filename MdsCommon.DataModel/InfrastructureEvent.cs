using Metapsi;
using System;

namespace MdsCommon
{
    public partial class InfrastructureEvent : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public System.String Type { get; set; } = System.String.Empty;
        public System.String Source { get; set; } = System.String.Empty;
        public System.String Criticality { get; set; } = System.String.Empty;
        public System.String ShortDescription { get; set; } = System.String.Empty;
        public System.String FullDescription { get; set; } = System.String.Empty;
    }
}