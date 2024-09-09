using MdsInfrastructure;
using Metapsi;
using static Metapsi.Hyperapp.HyperType;
using System;
using Microsoft.AspNetCore.Http;
using MdsCommon;
using System.Threading.Tasks;
using System.Collections.Generic;
using Metapsi.Sqlite;

namespace MdsInfrastructure
{
    public static partial class MdsInfrastructureApplication
    {
        public static void MapBackendApi(
            this ImplementationGroup ig,
            SqliteQueue dbQueue,
            string infrastructureName)
        {
            void MapRequest<T>(Request<T> request, Func<SqliteQueue, System.Threading.Tasks.Task<T>> dbCall)
            {
                ig.MapRequest(request, async (rc) => await dbCall(dbQueue));
            }

            void MapRequest1<T, P>(Request<T, P> request, Func<SqliteQueue, P, System.Threading.Tasks.Task<T>> dbCall)
            {
                ig.MapRequest(request, async (rc, p) => await dbCall(dbQueue, p));
            }

            void MapCommand<P>(Command<P> command, Func<SqliteQueue, P, System.Threading.Tasks.Task> dbCall)
            {
                ig.MapCommand(command, async (rc, p) => await dbCall(dbQueue, p));
            }

            MapRequest(Backend.ValidateSchema, Db.ValidateSchema);
            MapRequest(Backend.LoadInfraStatus, Db.LoadInfrastructureStatus);
            MapRequest(Backend.LoadAllConfigurationHeaders, Db.LoadConfigurationHeaders);
            MapRequest1(Backend.LoadConfiguration, Db.LoadSpecificConfiguration);
            MapCommand(Backend.SaveConfiguration, Db.SaveConfiguration);
            MapCommand(Backend.SaveNode, Db.SaveNode);
            MapCommand(Backend.DeleteConfigurationById, Db.DeleteConfiguration);
            MapRequest(Backend.LoadCurrentDeployment, Db.LoadActiveDeployment);
            MapRequest1(Backend.LoadLastConfigurationDeployment, Db.LoadLastDeploymentOfConfiguration);
            MapRequest(Backend.LoadCurrentConfiguration, Db.LoadCurrentConfiguration);
            MapRequest1(Backend.LoadDeploymentById, Db.LoadSpecificDeployment);
            MapRequest(Backend.LoadDeploymentsHistory, Db.LoadDeploymentHistory);
            MapRequest(Backend.LoadAllProjects, Db.LoadAllProjects);
            MapRequest(Backend.LoadAllNodes, Db.LoadAllNodes);
            MapRequest(Backend.LoadAllServices, Db.LoadAllServices);
            MapRequest1(Backend.RefreshBinaries, Db.RefreshBinaries);
            ig.MapRequest(Backend.ConfirmDeployment, async (rc, input) => await Db.ConfirmDeployment(dbQueue, input.Snapshots, input.Configuration));
            MapRequest(Backend.GetAllParameterTypes, Db.LoadParameterTypes);
            MapRequest(Backend.LoadEnvironmentTypes, Db.LoadEnvironmentTypes);
            MapRequest(Backend.LoadHealthStatus, Db.LoadFullInfrastructureHealthStatus);
            MapRequest(Backend.GetInfrastructureName, async (rc) => await Task.FromResult(infrastructureName));
            MapRequest(Backend.GetAllNoteTypes, Db.LoadAllNoteTypes);
            MapCommand(Backend.StoreHealthStatus, Db.StoreHealthStatus);
            MapCommand(Backend.SaveVersionEnabled, Db.SaveVersionEnabled);
            MapRequest1(Backend.LoadIdenticalSnapshot, Db.LoadIdenticalSnapshot);
            MapCommand(Backend.SaveDeploymentEvent, Db.SaveDeploymentEvent);
            MapRequest1(Backend.LoadDeploymentEvents, Db.GetDeploymentEvents);
            MapCommand(MdsCommon.Api.SaveInfrastructureEvent, MdsCommon.Db.SaveInfrastructureEvent);
        }
    }
}
