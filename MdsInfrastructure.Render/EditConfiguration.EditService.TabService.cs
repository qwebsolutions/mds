using MdsCommon;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.ChoicesJs;
using System.Collections.Generic;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<HyperNode> TabService(
           LayoutBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            var serviceId = b.Get(clientModel, x => x.EditServiceId);

            var service = b.Get(clientModel, serviceId, (x, id) => x.Configuration.InfrastructureServices.Single(x => x.Id == id));
            var allApplications = b.Get(clientModel, x => x.Configuration.Applications.OrderBy(x => x.Name).ToList());
            var activeProjects = b.Get(clientModel, x => x.AllProjects.Where(x => x.Enabled).ToList());
            var selectedProjectId = b.Get(service, x => x.ProjectId);
            var projectOptions = b.Get(
                b.MapChoices<MdsCommon.Project, Guid>(
                    activeProjects, 
                    x => x.Id, 
                    x => x.Name,
                    selectedProjectId), 
                x => x.OrderBy(x => x.label).ToList());

            var versions = b.Get(activeProjects, selectedProjectId, (x, selectedProjectId) => x.SelectMany(x => x.Versions).Where(x => x.Enabled && x.ProjectId == selectedProjectId).OrderByDescending(x => x.Binaries.First().BuildNumber).ToList());
            var selectedVersionId = b.Get(service, x => x.ProjectVersionId);
            var selectedVersion = b.Get(versions, selectedVersionId, (x, selectedVersionId) => x.SingleOrDefault(x => x.Id == selectedVersionId, new ProjectVersion() { VersionTag = "(not selected)" }));
            var versionOptions = b.MapChoices(versions, x => x.Id, x => x.VersionTag, selectedVersionId);
            var versionBinaries = b.Get(selectedVersion, x => x.Binaries);
            var targetEnvironment = b.Def<SyntaxBuilder, string, string>((b, target) => b.If(b.AreEqual(target, b.Const("linux-x64")), b => b.Const("Linux"), b => b.Const("Windows")));
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
            var getEditedService = b.Def<SyntaxBuilder, EditConfigurationPage, InfrastructureService>(EditEntity.EditedService);

            var appChoices = b.MapChoices(
                allApplications, 
                x => x.Id, 
                x => x.Name,
                b.Get(service, x => x.ApplicationId));
            var applicationDd = b.Add(container, b.DropDown(appChoices));
            Metapsi.ChoicesJs.Event.SetOnChange(b, applicationDd, b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<string> value) =>
            {
                var service = b.Call(getEditedService, page);
                var selectedId = b.ParseId(value);
                b.Set(service, x => x.ApplicationId, selectedId);
                return b.Clone(page);
            }));

            b.AddClass(applicationDd, "w-full");

            var projectLabel = b.Add(container, b.Text("Project")); b.AddClass(projectLabel, "w-full");

            var projectDd = b.Add(container, b.DropDown(projectOptions));
            Metapsi.ChoicesJs.Event.SetOnChange(b, projectDd, b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<string> value) =>
            {
                var service = b.Call(getEditedService, page);
                var selectedId = b.ParseId(value);
                b.Set(service, x => x.ProjectId, selectedId);
                b.Set(service, x => x.ProjectVersionId, b.EmptyId());
                b.Set(service, x => x.InfrastructureNodeId, b.EmptyId());
                return b.Clone(page);
            }));

            b.AddClass(projectDd, "w-full");

            var versionLabel = b.Add(container, b.Text("Version")); b.AddClass(versionLabel, "w-full");


            var versionDd = b.Add(container, b.DropDown(versionOptions));

            Metapsi.ChoicesJs.Event.SetOnChange(b, versionDd, b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<string> value) =>
            {
                var service = b.Call(getEditedService, page);
                var newVersionId = b.ParseId(value);
                b.Set(service, x => x.ProjectVersionId, newVersionId);

                var selectedNode = b.Get(
                    b.Get(page, x => x.InfrastructureNodes),
                    b.Get(service, x => x.InfrastructureNodeId),
                    (nodes, selectedNodeId) => nodes.SingleOrDefault(x => x.Id == selectedNodeId));

                b.If(
                    b.Not(b.HasObject(selectedNode)),
                    b =>
                    {
                        b.Set(service, x => x.InfrastructureNodeId, b.EmptyId());
                    },
                    b =>
                    {
                        var serviceProject = b.Get(activeProjects, b.Get(service, x => x.ProjectId), (projects, projectId) => projects.SingleOrDefault(x => x.Id == projectId));
                        b.If(b.Not(b.HasObject(serviceProject)),
                            b =>
                            {
                                b.Set(service, x => x.InfrastructureNodeId, b.EmptyId());
                            },
                            b =>
                            {
                                var selectedProjectVersion = b.Get(serviceProject, newVersionId, (project, versionId) => project.Versions.SingleOrDefault(x => x.Enabled && x.Id == versionId));
                                b.If(b.Not(b.HasObject(selectedProjectVersion)),
                                    b =>
                                    {
                                        b.Set(service, x => x.InfrastructureNodeId, b.EmptyId());
                                    },
                                    b =>
                                    {
                                        var selectedNodeEnvironment = b.Get(
                                            b.Get(page, x => x.EnvironmentTypes),
                                            b.Get(selectedNode, x => x.EnvironmentTypeId),
                                            (environmentTypes, environmentTypeId) => environmentTypes.SingleOrDefault(x => x.Id == environmentTypeId));

                                        var versionEnvironmentCodes = b.Map(
                                            b.Get(selectedProjectVersion, x => x.Binaries.Select(x => x.Target).ToList()),
                                            (b, s) => b.ToLowercase(s));

                                        var nodeEnvironmentCode = b.ToLowercase(b.Get(selectedNodeEnvironment, x => x.OsType));

                                        var binariesMatches = b.Filter(
                                            b.Get(selectedProjectVersion, x => x.Binaries),
                                            (b, binaries) => TargetMatches(b, selectedNodeEnvironment, binaries));

                                        b.Log(nameof(binariesMatches), binariesMatches);

                                        b.If(b.Not(b.Get(binariesMatches, x => x.Any())),
                                            b =>
                                            {
                                                b.Set(service, x => x.InfrastructureNodeId, b.EmptyId());
                                            });
                                    });
                            });
                    });

                return b.Clone(page);
            }));

            b.AddClass(versionDd, "w-full");


            var nodeLabel = b.Add(container, b.Text("Deployed on node"));
            b.AddClass(nodeLabel, "w-full");

            var nodeChoices = b.MapChoices(
                matchingNodes, 
                x => x.Id, 
                x => x.NodeName,
                b.Get(service, x => x.InfrastructureNodeId));

            var nodeDd = b.Add(container, b.DropDown(nodeChoices));

            Metapsi.ChoicesJs.Event.SetOnChange(b, nodeDd, b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<string> value) =>
            {
                var service = b.Call(getEditedService, page);
                var selectedId = b.ParseId(value);
                b.Set(service, x => x.InfrastructureNodeId, selectedId);
                return b.Clone(page);
            }));

            b.AddClass(nodeDd, "w-full");

            var enabledLabel = b.Add(container, b.Text("Service status")); b.AddClass(enabledLabel, "w-full");
            var enabledToggle = b.Add(container, b.BoundToggle(
                service,
                x => x.Enabled,
                b.Const("Enabled"), b.Const("Disabled")));
            b.AddClass(enabledToggle, "w-full");

            return container;
        }

        private static Var<bool> TargetMatches(this SyntaxBuilder b, Var<EnvironmentType> environmentType, Var<ProjectVersionBinaries> projectVersionBinaries)
        {
            var linuxTargets = b.Const(new List<string>()
            {
                "linux-x64"
            });

            var windowsTargets = b.Const(new List<string>()
            {
                "win10-x64",
                "win-x64"
            });

            return b.If(
                b.AreEqual(b.ToLowercase(b.Get(environmentType, x => x.OsType)), b.Const("linux")),
                b => b.Includes(linuxTargets, b.Get(projectVersionBinaries, x => x.Target)),
                b => b.Includes(windowsTargets, b.Get(projectVersionBinaries, x => x.Target)));
        }
    }

    public static class EditEntity
    {
        public static Var<InfrastructureService> EditedService(SyntaxBuilder b, Var<EditConfigurationPage> page)
        {
            return b.Get(page, page => page.Configuration.InfrastructureServices.Single(service => service.Id == page.EditServiceId));
        }
    }
}
