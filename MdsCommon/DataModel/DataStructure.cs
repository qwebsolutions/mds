using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metapsi;

namespace MdsCommon
{
    //public class ServiceSnapshots : IDataStructure, IServiceSnapshotData
    //{
    //    [Order(1)]
    //    public RecordCollection<ServiceConfigurationSnapshot> ServiceConfigurationSnapshots { get; set; } = new();

    //    [Order(2)]
    //    public RecordCollection<ServiceConfigurationSnapshotParameter> ServiceConfigurationSnapshotParameters { get; set; } = new();
    //}

    //public class ServiceSnapshot : ServiceSnapshots, IDataStructure, IServiceSnapshotData
    //{

    //}
}

namespace MdsCommon
{
    public static class ServiceSnapshotBehavior
    {
        //public static bool IsEmpty(this ServiceSnapshots serviceSnapshots)
        //{
        //    if (!serviceSnapshots.ServiceConfigurationSnapshots.Any())
        //        return true;

        //    return serviceSnapshots.ServiceConfigurationSnapshots.Single().IsEmpty();
        //}

        //public static ServiceConfigurationSnapshot Service(this ServiceSnapshot serviceSnapshot)
        //{
        //    return serviceSnapshot.ServiceConfigurationSnapshots.Single();
        //}

        //public static RecordCollection<ServiceConfigurationSnapshotParameter> Parameters(this ServiceSnapshot serviceSnapshot)
        //{
        //    if (serviceSnapshot.ServiceConfigurationSnapshotParameters.Any(x => x.ServiceConfigurationSnapshotId != serviceSnapshot.Service().Id))
        //        throw new Exception("Service snapshot not valid");

        //    return serviceSnapshot.ServiceConfigurationSnapshotParameters;
        //}
    }
}
