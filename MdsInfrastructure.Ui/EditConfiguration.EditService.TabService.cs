using MdsCommon;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Linq;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        public static Var<HyperNode> TabService(
           BlockBuilder b,
           Var<EditConfigurationPage> clientModel,
           Var<Guid> serviceId)
        {
            var service = b.Get(clientModel, serviceId, (x, id) => x.Configuration.InfrastructureServices.Single(x => x.Id == id));
            var allApplications = b.Get(clientModel, x => x.Configuration.Applications.ToList());
            var activeProjects = b.Get(clientModel, x => x.AllProjects.Where(x => x.Enabled).ToList());
            var selectedProjectId = b.Get(service, x => x.ProjectId);
            var projectOptions = b.Get(activeProjects, selectedProjectId, (x, selectedProjectId) => x.Select(y => new DropDown.Option() { label = y.Name, value = y.Id.ToString(), selected = y.Id == selectedProjectId }).ToList());
            var versions = b.Get(activeProjects, selectedProjectId, (x, selectedProjectId) => x.SelectMany(x => x.Versions).Where(x => x.Enabled && x.ProjectId == selectedProjectId).ToList());
            var selectedVersionId = b.Get(service, x => x.ProjectVersionId);
            var selectedVersion = b.Get(versions, selectedVersionId, (x, selectedVersionId) => x.SingleOrDefault(x => x.Id == selectedVersionId, new ProjectVersion() { VersionTag = "(not selected)" }));
            var versionOptions = b.Get(versions, selectedVersionId,  (x, selectedVersionId) => x.Select(y => new DropDown.Option() { label = y.VersionTag, value = y.Id.ToString(), selected = selectedVersionId == y.Id }).ToList());
            var versionBinaries = b.Get(selectedVersion, x => x.Binaries);
            var targetEnvironment = b.Def<string, string>((b, target) => b.If(b.AreEqual(target, b.Const("linux-x64")), b => b.Const("Linux"), b => b.Const("Windows")));
            var versionTargets = b.Get(versionBinaries, x => x.Select(x => x.Target).Distinct().ToList());
            var versionSystems = b.Get(versionTargets, targetEnvironment, (x, targetEnvironment) => x.Select(x => targetEnvironment(x)));
            var mathingEnvironment = b.Get(clientModel, versionSystems, (clientModel, versionSystems) => clientModel.EnvironmentTypes.Where(x => versionSystems.Contains(x.OsType)));
            var matchingEnvironmentIds = b.Get(mathingEnvironment, x => x.Select(x => x.Id));
            var matchingNodes = b.Get(clientModel, matchingEnvironmentIds, (m, envIds) => m.InfrastructureNodes.Where(x => envIds.Contains(x.EnvironmentTypeId)).ToList());
            var serviceEnabled = b.Get(service, x => x.Enabled);
                        
            var container = b.Div("grid grid-cols-2 place-items-center w-full gap-4");
            var serviceNamelabel = b.Add(container, b.Text("Service name"));
            b.AddClass(serviceNamelabel, "w-full");
            var serviceNameInput = b.Add(container, b.BoundInput(clientModel, serviceId, (x, id) => x.Configuration.InfrastructureServices.Single(x => x.Id == id), x => x.ServiceName, b.Const("Service name")));
            b.AddClass(serviceNameInput, "w-full");

            var applicationNameLabel = b.Add(container, b.Text("Application"));
            b.AddClass(applicationNameLabel, "w-full");
            var applicationDd = b.Add(container,
                b.BoundDropDown(
                    b.Const("ddServiceApplication"),
                    service,
                    x => x.ApplicationId,
                    allApplications,
                    b.Def((BlockBuilder b, Var<Application> app) => b.Get(app, app => new DropDown.Option() { label = app.Name, value = app.Id.ToString() }))));
            b.AddClass(applicationDd, "w-full");

            var projectLabel = b.Add(container, b.Text("Project")); b.AddClass(projectLabel, "w-full");

            var projectDd = b.Add(container, b.DropDown(b.Const("projectDd"),
                selectedProjectId.As<string>(),
                projectOptions,
                b.Def((BlockBuilder b, Var<string> inputValue) =>
                {
                    Var<System.Guid> newId = b.ToId(inputValue);
                    b.Set(service,x=> x.ProjectId, newId);
                    // Clear project version, it's from the previous project
                    b.Set(service, x => x.ProjectVersionId, b.EmptyId());
                    b.Set(service, x=> x.InfrastructureNodeId, b.EmptyId());
                }),
                b.Const("Project")));
            b.AddClass(projectDd, "w-full");

            var versionLabel = b.Add(container, b.Text("Version")); b.AddClass(versionLabel, "w-full");
            var versionDd = b.Add(container, b.DropDown(b.Const("versionDd"),
                selectedVersionId.As<string>(),
                versionOptions,
                b.Def((BlockBuilder b, Var<string> value) =>
                {
                    Var<System.Guid> newId = b.ToId(value);
                    b.Set(service,  x=> x.ProjectVersionId, newId);
                    b.Set(service,  x=> x.InfrastructureNodeId, b.EmptyId());
                }),
                b.Const("Version")));
            b.AddClass(versionDd, "w-full");

            var transform = (BlockBuilder b, Var<InfrastructureNode> node) =>
            {
                var nodeName = b.Get(node, x => x.NodeName);
                var nodeId = b.Get(node, x => x.Id.ToString());

                return b.NewObj<DropDown.Option>(b =>
                {
                    b.Set(x => x.label, nodeName);
                    b.Set(x => x.value, nodeId);
                });
            };

            var nodeLabel = b.Add(container, b.Text("Deployed on node"));
            b.AddClass(nodeLabel, "w-full");
            var nodeDd = b.Add(container, b.BoundDropDown(
                b.Const("ddServiceNode"),
                service,
                x => x.InfrastructureNodeId,
                matchingNodes,
                b.Def(transform)));
            b.AddClass(nodeDd, "w-full");

            var enabledLabel = b.Add(container, b.Text("Service status")); b.AddClass(enabledLabel, "w-full");
            var enabledToggle = b.Add(container, b.BoundToggle(
                service,
                x => x.Enabled,
                b.Const("Enabled"), b.Const("Disabled")));
            b.AddClass(enabledToggle, "w-full");

            return container;
        }
    }
}
