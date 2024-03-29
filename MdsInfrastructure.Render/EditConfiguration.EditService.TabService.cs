using MdsCommon;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Linq;
using MdsCommon.Controls;
using System.Collections.Generic;
using Metapsi.Html;
using Metapsi;
using System.Dynamic;
using Metapsi.TomSelect;
using Metapsi.Shoelace;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<InfrastructureService> GetSelectedService(this SyntaxBuilder b, Var<EditConfigurationPage> clientModel)
        {
            var serviceId = b.Get(clientModel, x => x.EditServiceId);
            return b.Get(clientModel, serviceId, (x, id) => x.Configuration.InfrastructureServices.Single(x => x.Id == id));
        }

        public static Var<List<Application>> GetOrderedApplications(this SyntaxBuilder b, Var<EditConfigurationPage> clientModel)
        {
            return b.Get(clientModel, x => x.Configuration.Applications.OrderBy(x => x.Name).ToList());
        }

        public static Var<List<MdsCommon.Project>> GetActiveProjects(this SyntaxBuilder b, Var<EditConfigurationPage> clientModel)
        {
            return b.Get(clientModel, x => x.AllProjects.Where(x => x.Enabled).ToList());
        }

        public static Var<IVNode> TabService(
           LayoutBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            var serviceId = b.Get(clientModel, x => x.EditServiceId);

            var service = b.GetSelectedService(clientModel);

            var allApplications = b.GetOrderedApplications(clientModel);

            var activeProjects = b.GetActiveProjects(clientModel);

            var selectedProjectId = b.Get(service, x => x.ProjectId);


            var versions = b.Get(activeProjects, selectedProjectId, (x, selectedProjectId) => x.SelectMany(x => x.Versions).Where(x => x.Enabled && x.ProjectId == selectedProjectId).OrderByDescending(x => x.Binaries.First().BuildNumber).ToList());
            var selectedVersionId = b.Get(service, x => x.ProjectVersionId);
            var selectedVersion = b.Get(versions, selectedVersionId, (x, selectedVersionId) => x.SingleOrDefault(x => x.Id == selectedVersionId, new ProjectVersion() { VersionTag = "(not selected)" }));

            var versionBinaries = b.Get(selectedVersion, x => x.Binaries);
            var targetEnvironment = b.Def<SyntaxBuilder, string, string>((b, target) => b.If(b.AreEqual(target, b.Const("linux-x64")), b => b.Const("Linux"), b => b.Const("Windows")));
            var versionTargets = b.Get(versionBinaries, x => x.Select(x => x.Target).Distinct().ToList());
            var versionSystems = b.Get(versionTargets, targetEnvironment, (x, targetEnvironment) => x.Select(x => targetEnvironment(x)));
            var mathingEnvironment = b.Get(clientModel, versionSystems, (clientModel, versionSystems) => clientModel.EnvironmentTypes.Where(x => versionSystems.Contains(x.OsType)));
            var matchingEnvironmentIds = b.Get(mathingEnvironment, x => x.Select(x => x.Id));
            var matchingNodes = b.Get(clientModel, matchingEnvironmentIds, (m, envIds) => m.InfrastructureNodes.Where(x => envIds.Contains(x.EnvironmentTypeId)).ToList());
            var serviceEnabled = b.Get(service, x => x.Enabled);

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("grid grid-cols-2 place-items-center w-full gap-4");
                },
                b.StyledSpan("w-full", b.TextSpan("Service name")),
                b.MdsInputText(
                    b =>
                    {
                        b.SetClass("w-full");
                        b.SetPlaceholder("Service name");
                        b.BindTo(clientModel, GetSelectedService, x => x.ServiceName);
                    }),
                b.StyledSpan("w-full", b.TextSpan("Application")),
                b.MdsDropDown(b =>
                {
                    b.SetClass("w-full");
                    b.SetOptions(allApplications, x => x.Id, x => x.Name);
                    b.BindTo(clientModel, GetSelectedService, x => x.ApplicationId);
                    b.SetPlaceholder("Application");
                }),
                b.StyledSpan("w-full", b.TextSpan("Project")),
                b.MdsDropDown(
                    b =>
                    {
                        b.SetPlaceholder("Project");
                        b.SetClass("w-full");
                        b.SetOptions(
                                b.Get(activeProjects, x => x.OrderBy(x => x.Name).ToList()),
                                x => x.Id,
                                x => x.Name);

                        b.SetItem(selectedProjectId);

                        b.OnChange(b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<string> value) =>
                        {
                            var service = b.GetSelectedService(page);
                            var selectedId = b.ParseId(value);
                            b.Set(service, x => x.ProjectId, selectedId);
                            b.Set(service, x => x.ProjectVersionId, b.EmptyId());
                            b.Set(service, x => x.InfrastructureNodeId, b.EmptyId());
                            return b.Clone(page);
                        }));
                    }),
                b.StyledSpan("w-full", b.TextSpan("Version")),
                b.MdsDropDown(
                    b =>
                    {
                        b.SetPlaceholder("Version");
                        b.SetClass("w-full");
                        b.SetOptions(versions, x => x.Id, x => x.VersionTag);
                        b.SetItem(selectedVersionId);
                        b.OnChange(b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<string> value) =>
                        {
                            var service = b.GetSelectedService(page);
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
                    }),
                b.StyledSpan("w-full", b.TextSpan("Deployed on node")),
                b.MdsDropDown(
                    b =>
                    {
                        b.SetPlaceholder("Deployment node");
                        b.SetClass("w-full");
                        b.SetOptions(matchingNodes, x => x.Id, x => x.NodeName);
                        b.SetItem(b.Get(service, x => x.InfrastructureNodeId));
                        b.OnChange(b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<string> value) =>
                        {
                            var service = b.GetSelectedService(page);
                            var selectedId = b.ParseId(value);
                            b.Set(service, x => x.InfrastructureNodeId, selectedId);
                            return b.Clone(page);
                        }));
                    }),
                b.StyledSpan("w-full", b.TextSpan("Service status")),
                b.BoundToggle(
                    service,
                    x => x.Enabled,
                    b.Const("Enabled"),
                    b.Const("Disabled"),
                    b => b.Set(x => x.ExtraRootCss, "w-full"))
                );
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
