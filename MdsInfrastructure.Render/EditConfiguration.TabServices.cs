using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;
using System;
using System.Collections.Generic;
using MdsCommon;
using MdsCommon.Controls;
using System.Xml.Serialization;
using Metapsi.ChoicesJs;
using MdsCommon.HtmlControls;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MdsInfrastructure.Render
{
    //public class Filter
    //{
    //    public string Value { get; set; }
    //}

    public class InfrastructureServiceRow
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; }
        public bool Enabled { get; set; }
        public string Project { get; set; }
        public string Application { get; set; }
        public string Node { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public static partial class EditConfiguration
    {
        public static Var<string> GetApplicationLabel(this BlockBuilder b, Var<EditConfigurationPage> model, Var<Guid> applicationId)
        {
            return b.Get(model, applicationId, (x, applicationId) => x.Configuration.Applications.SingleOrDefault(y => y.Id == applicationId, new Application() { Name = "(not set)" }).Name);
        }

        public static Var<string> GetNodeLabel(this BlockBuilder b, Var<EditConfigurationPage> model, Var<Guid> nodeId)
        {
            return b.Get(model, nodeId, (x, nodeId) => x.InfrastructureNodes.SingleOrDefault(y => y.Id == nodeId, new InfrastructureNode() { NodeName = "(not set)" }).NodeName);
        }

        public static Var<InfrastructureServiceRow> ServiceRow(
            this BlockBuilder b,
            Var<EditConfigurationPage> model,
            Var<InfrastructureService> service)
        {
            var configuration = b.Get(model, x => x.Configuration);
            var row = b.NewObj<InfrastructureServiceRow>();
            b.Set(row, x => x.Id, b.Get(service, x => x.Id));
            b.Set(row, x => x.ServiceName, b.Get(service, x => x.ServiceName));
            b.Set(row, x => x.Project, GetProjectLabel(b, model, service));
            b.Set(row, x => x.Application, b.GetApplicationLabel(model, b.Get(service, x => x.ApplicationId)));
            b.Set(row, x => x.Node, b.GetNodeLabel(model, b.Get(service, x => x.InfrastructureNodeId)));
            b.Set(row, x => x.Enabled, b.Get(service, x => x.Enabled));

            b.If(
                b.Get(row, x => x.Enabled),
                b => b.Push(b.Get(row, x => x.Tags), b.Const("enabled")),
                b => b.Push(b.Get(row, x => x.Tags), b.Const("disabled")));

            return row;
        }

        public static Var<HyperNode> TabServices(
           BlockBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            var allServices = b.Get(clientModel, x => x.Configuration.InfrastructureServices.OrderBy(x => x.ServiceName).ToList());
            var serviceRows = b.Map(allServices, (b, service) => b.ServiceRow(clientModel, service));
            var filteredServices = b.FilterList(serviceRows, b.Get(clientModel, x => x.ServicesFilter));

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

            var rc = b.RenderCell((BlockBuilder b, Var<InfrastructureServiceRow> row, Var<DataTable.Column> col) =>
            {
                var columnName = b.Get(col, x => x.Name);
                var serviceName = b.Get(row, x => x.ServiceName);
                var serviceDisabled = b.Get(row, x => !x.Enabled);

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
                    (nameof(InfrastructureService.ProjectId), b => b.Text(b.Get(row, x => x.Project))),
                    (nameof(InfrastructureService.ApplicationId), b => b.Text(b.Get(row, x => x.Application))),
                    (nameof(InfrastructureService.InfrastructureNodeId), b => b.Text(b.Get(row, x => x.Node)))
                    ));
            });

            var contextToolbar = b.Div("flex flex-row");

            var hInputInput = b.NewObj<HParams>();
            b.Set(hInputInput, x => x.Tag, "input");
            b.SetDynamic(b.Get(hInputInput, x => x.Props), Html.type, b.Const("text"));

            //var firstInputText = b.Add(contextToolbar, b.H(hInputInput));
            //var secondInputText = b.Add(contextToolbar, b.BuildControl(b.Const("input"), b.NewObj<InputText>(b =>
            //{
            //    b.Set(x => x.Value, "abc");
            //})));

            //var thirdInputText = b.Add(contextToolbar, b.BuildControl<InputText>(
            //    b.Const("input"),
            //    (b, props) =>
            //    {
            //        b.Set(props, x => x.Value, "from builder");
            //    }));


            b.OnModel(
                clientModel,
                (b, modelContext) =>
                {
                    b.Add(contextToolbar, b.Filter((b, filterBuilder) =>
                    {
                        b.Customize(filterBuilder, x => x.ClearButtonContent, (b, p) =>
                        {
                            b.SetProp(p, Html.@class, "text-red-500");
                        });

                        b.InBindingContext(modelContext, filterBuilder, b =>
                        {
                            b.BindFilter(x => x.ServicesFilter);
                        });

                        var plm = b.NewObj<InputText>();

                        b.InBindingContext(modelContext, plm, b =>
                        {
                            b.
                        });

                    }));
                });

            return b.DataGrid<InfrastructureServiceRow>(
                new()
                {
                    b=> b.AddClass(b.CommandButton<EditConfigurationPage>(b=>
                    {
                        b.Set(x=>x.Label, "Add service");
                        b.Set(x => x.OnClick, onAddService);
                    }), "text-white"),
                    b=> contextToolbar
                },
                b =>
                {
                    b.AddColumn(nameof(InfrastructureService.ServiceName), "Name");
                    b.AddColumn(nameof(InfrastructureService.ProjectId), "Project");
                    b.AddColumn(nameof(InfrastructureService.ApplicationId), "Application");
                    b.AddColumn(nameof(InfrastructureService.InfrastructureNodeId), "Node");
                    b.SetRows(filteredServices);
                    b.SetRenderCell(rc);
                },
                (b, actions, item) =>
                {
                    var removeIcon = Icon.Remove;

                    var onRemove = b.Def((BlockBuilder b, Var<InfrastructureServiceRow> item) =>
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
