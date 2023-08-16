using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;
using System;
using System.Collections.Generic;
using MdsCommon;
using MdsCommon.Controls;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public static Var<HyperNode> TabServices(
           BlockBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            var rows = b.Get(clientModel, x => x.Configuration.InfrastructureServices.OrderBy(x => x.ServiceName).ToList());

            var onAddService = b.MakeAction((BlockBuilder b, Var<EditConfigurationPage> clientModel) =>
            {
                var configHeaderId = b.Get(clientModel, x => x.Configuration.Id);
                var services = b.Get(clientModel, x => x.Configuration.InfrastructureServices);
                var newId = b.NewId();

                var newService = b.NewObj<InfrastructureService>(b =>
                {
                    b.Set(x => x.Id, newId);
                    b.Set(x => x.ConfigurationHeaderId, configHeaderId);
                    b.Set(x => x.Enabled, true);
                });
                b.Comment("onAddService");
                b.Push(b.Get(clientModel, x => x.Configuration.InfrastructureServices), newService);

                b.Set(clientModel, x => x.EditServiceId, newId);
                b.Log("EditServiceId", newId);
                return b.EditView<EditConfigurationPage>(clientModel, EditService);
            });

            var rc = b.RenderCell((BlockBuilder b, Var<InfrastructureService> row, Var<DataTable.Column> col) =>
            {
                var columnName = b.Get(col, x => x.Name);
                Var<InfrastructureService> service = row.As<InfrastructureService>();
                Var<Guid> applicationId = b.Get(service, x => x.ApplicationId);
                Var<Guid> nodeId = b.Get(service, x => x.InfrastructureNodeId);
                Var<string> serviceName = b.Get(service, x => x.ServiceName);
                Var<bool> serviceDisabled = b.Get(service, service => !service.Enabled);

                var goToEditService = (BlockBuilder b, Var<EditConfigurationPage> clientModel) =>
                {
                    b.Set(clientModel, x => x.EditServiceId, b.Get(row, x => x.Id));
                    b.Log("EditServiceId", b.Get(row, x => x.Id));
                    return b.EditView<EditConfigurationPage>(clientModel, EditService);
                };

                var serviceNameCellBuilder = (BlockBuilder b) =>
                {
                    var container = b.Span();
                    b.Add(container, b.Link<EditConfigurationPage>(b.WithDefault(serviceName), b.MakeAction<EditConfigurationPage>(goToEditService)));
                    b.If(serviceDisabled, (BlockBuilder b) =>
                    {
                        var badge = b.Add(container, b.Badge(b.Const("disabled")));
                        b.AddClass(badge, "bg-gray-400");
                    });
                    return container;
                };

                return b.VPadded4(b.Switch(columnName,
                    x => b.Text("not supported"),
                    (nameof(InfrastructureService.ServiceName), serviceNameCellBuilder),
                    (nameof(InfrastructureService.ProjectId), b => b.Text(b.Call(GetProjectLabel, clientModel, service))),
                    (nameof(InfrastructureService.ApplicationId), b => b.Text(b.Get(clientModel, applicationId, (x, applicationId) => x.Configuration.Applications.SingleOrDefault(y => y.Id == applicationId, new Application() { Name = "(not set)" }).Name))),
                    (nameof(InfrastructureService.InfrastructureNodeId), b => b.Text(b.Get(clientModel, nodeId, (x, nodeId) => x.InfrastructureNodes.SingleOrDefault(y => y.Id == nodeId, new InfrastructureNode() { NodeName = "(not set)" }).NodeName)))
                    ));
            });

            return b.DataGrid<InfrastructureService>(
                new()
                {
                    b=> b.AddClass(b.CommandButton<EditConfigurationPage>(b=>
                    {
                        b.Set(x=>x.Label, "Add service");
                        b.Set(x => x.OnClick, onAddService);
                    }), "text-white")
                },
                b =>
                {
                    b.AddColumn(nameof(InfrastructureService.ServiceName), "Name");
                    b.AddColumn(nameof(InfrastructureService.ProjectId), "Project");
                    b.AddColumn(nameof(InfrastructureService.ApplicationId), "Application");
                    b.AddColumn(nameof(InfrastructureService.InfrastructureNodeId), "Node");
                    b.SetRows(rows);
                    b.SetRenderCell(rc);
                },
                (b, actions, item) =>
                {
                    var removeIcon = Icon.Remove;

                    var onRemove = b.Def((BlockBuilder b, Var<InfrastructureService> item) =>
                        {
                            var serviceId = b.Get(item, x => x.Id);
                            var serviceRemoved = b.Get(clientModel, serviceId, (x, serviceId) => x.Configuration.InfrastructureServices.Where(x => x.Id != serviceId).ToList());
                            b.Log(serviceRemoved);
                            b.Set(b.Get(clientModel, x => x.Configuration), x => x.InfrastructureServices, serviceRemoved);
                        });

                    b.Modify(actions, b => b.Commands, b =>
                    {
                        b.Add(b =>
                        {
                            b.Set(x => x.IconHtml, removeIcon);
                            b.Set(x => x.OnCommand, onRemove);
                        });
                    });
                });
        }

        public static Var<string> GetProjectLabel(BlockBuilder b, Var<EditConfigurationPage> clientModel, Var<InfrastructureService> service)
        {
            var projectName = b.WithDefault(b.Const(string.Empty));
            var versionTag = b.WithDefault(b.Const(string.Empty));

            var projectLabelRef = b.Ref(projectName);

            var projectId = b.Get(service, x => x.ProjectId);
            var versionId = b.Get(service, x => x.ProjectVersionId);
            var project = b.Get(clientModel, projectId, (x, projectId) => x.AllProjects.SingleOrDefault(x => x.Id == projectId));
            var version = b.Get(clientModel, versionId, (x, versionId) => x.AllProjects.SelectMany(x => x.Versions).SingleOrDefault(x => x.Id == versionId));

            b.If(b.HasObject(project), b =>
            {
                b.SetRef(projectLabelRef, b.Get(project, x => x.Name));

                b.If(b.HasObject(version), b =>
                {
                    b.SetRef(projectLabelRef, b.Concat(b.GetRef(projectLabelRef), b.Const(" "), b.Get(version, x => x.VersionTag)));
                },
                b =>
                {
                    b.SetRef(projectLabelRef, b.Concat(b.GetRef(projectLabelRef), b.Const(" "), versionTag));
                });
            });

            return b.GetRef(projectLabelRef);
        }
    }
}
