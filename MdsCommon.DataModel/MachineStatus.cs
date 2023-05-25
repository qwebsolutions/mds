using Metapsi;
using System.Collections.Generic;

namespace MdsCommon
{
    public partial class MachineStatus : IRecord
    {
        public System.Guid Id { get; set; } = System.Guid.NewGuid();
        public System.Int32 HddTotalMb { get; set; }
        public System.Int32 HddAvailableMb { get; set; }
        public System.Int32 RamTotalMb { get; set; }
        public System.Int32 RamAvailableMb { get; set; }
        public System.String MachineName { get; set; } = System.String.Empty;
        public System.String NodeName { get; set; } = System.String.Empty;
        public System.DateTime TimestampUtc { get; set; }

        public List<ServiceStatus> ServiceStatuses { get; set; } = new();

        public static UptreeRelation<MachineStatus> Data =
            Relation.On<MachineStatus>(x =>
            {
                x.FromParentId(x => x.Id, x =>
                {
                    x.Children(x => x.ServiceStatuses, x => x.MachineStatusId, x => { });
                });
            });
    }
}