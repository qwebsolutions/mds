﻿using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;
using System;
using System.Collections.Generic;
using MdsCommon.Controls;
using Metapsi.Html;

namespace MdsInfrastructure.Render
{
    public class InfrastructureServiceRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Project { get; set; }
        public string Application { get; set; }
        public string Node { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public static partial class EditConfiguration
    {
        public static Var<EditConfigurationPage> OnAddService(SyntaxBuilder b, Var<EditConfigurationPage> clientModel)
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
            b.Push(b.Get(clientModel, x => x.Configuration.InfrastructureServices), newService);

            b.Set(clientModel, x => x.EditServiceId, newId);
            return b.EditView<EditConfigurationPage>(clientModel, EditService);
        }

        public static Var<EditConfigurationPage> OnRemoveService(SyntaxBuilder b, Var<EditConfigurationPage> page, Var<InfrastructureServiceRow> row)
        {
            var serviceId = b.Get(row, x => x.Id);
            var serviceRemoved = b.Get(page, serviceId, (x, serviceId) => x.Configuration.InfrastructureServices.Where(x => x.Id != serviceId).ToList());
            b.Set(b.Get(page, x => x.Configuration), x => x.InfrastructureServices, serviceRemoved);
            return b.Clone(page);
        }

        public static Var<string> GetApplicationLabel(this SyntaxBuilder b, Var<EditConfigurationPage> model, Var<Guid> applicationId)
        {
            return b.Get(model, applicationId, (x, applicationId) => x.Configuration.Applications.SingleOrDefault(y => y.Id == applicationId, new Application() { Name = "(not set)" }).Name);
        }

        public static Var<string> GetNodeLabel(this SyntaxBuilder b, Var<EditConfigurationPage> model, Var<Guid> nodeId)
        {
            return b.Get(model, nodeId, (x, nodeId) => x.InfrastructureNodes.SingleOrDefault(y => y.Id == nodeId, new InfrastructureNode() { NodeName = "(not set)" }).NodeName);
        }

        public static Var<InfrastructureServiceRow> ServiceRow(
            this LayoutBuilder b,
            Var<EditConfigurationPage> model,
            Var<InfrastructureService> service)
        {
            var configuration = b.Get(model, x => x.Configuration);
            var row = b.NewObj<InfrastructureServiceRow>();
            b.Set(row, x => x.Id, b.Get(service, x => x.Id));
            b.Set(row, x => x.Name, b.Get(service, x => x.ServiceName));
            b.Set(row, x => x.Project, GetProjectLabel(b, model, service));
            b.Set(row, x => x.Application, b.GetApplicationLabel(model, b.Get(service, x => x.ApplicationId)));
            b.Set(row, x => x.Node, b.GetNodeLabel(model, b.Get(service, x => x.InfrastructureNodeId)));

            b.If(
                b.Get(service, x => x.Enabled),
                b => b.Push(b.Get(row, x => x.Tags), b.Const("enabled")),
                b => b.Push(b.Get(row, x => x.Tags), b.Const("disabled")));

            return row;
        }

        public static Var<IVNode> RenderServiceNameCell(this LayoutBuilder b, Var<InfrastructureServiceRow> row)
        {
            var serviceName = b.Get(row, x => x.Name);
            var serviceDisabled = b.Includes(b.Get(row, x => x.Tags), b.Const("disabled"));

            var goToEditService = (SyntaxBuilder b, Var<EditConfigurationPage> clientModel) =>
            {
                b.Set(clientModel, x => x.EditServiceId, b.Get(row, x => x.Id));
                return b.EditView<EditConfigurationPage>(clientModel, EditService);
            };

            return b.HtmlSpan(
                b =>
                {
                },
                b.Link<EditConfigurationPage>(b.WithDefault(serviceName), b.MakeAction<EditConfigurationPage>(goToEditService)),
                b.Optional(
                    serviceDisabled,
                    b =>
                    {
                        return b.Badge(b.Const("disabled"), b.Const("bg-gray-400"));
                    }));
        }

        public static Var<IVNode> TabServices(
           LayoutBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            var allServices = b.Get(clientModel, x => x.Configuration.InfrastructureServices.OrderBy(x => x.ServiceName).ToList());
            var serviceRows = b.Map(allServices, (b, service) => b.ServiceRow(clientModel, service));
            var filteredServices = b.FilterList(serviceRows, b.Get(clientModel, x => x.ServicesFilter));

            var gridBuilder = MdsDefaultBuilder.DataGrid<InfrastructureServiceRow>();
            gridBuilder.CreateToolbarActions = (b) =>
            {
                return b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex w-full items-center justify-between");
                    },
                    b.HtmlButton(
                        b =>
                        {
                            b.OnClickAction<EditConfigurationPage, HtmlButton>(OnAddService);
                            b.SetClass("py-2 px-4 rounded shadow bg-sky-500 text-white");
                        },
                        b.Text("Add service")),
                    b.Filter(clientModel, x => x.ServicesFilter));
                        
            };

            gridBuilder.DataTableBuilder.OverrideDataCell(
                nameof(InfrastructureConfiguration.Name),
                (b, row) =>
                {
                    return b.RenderServiceNameCell(row);
                });

            gridBuilder.AddRowAction((b, row) =>
            {
                return b.DeleteRowIconAction(
                    b =>
                    {
                        //b.Log("DeleteRowAction", row);

                        var actionDescriptor = b.MakeActionDescriptor(
                                b.MakeAction<EditConfigurationPage, InfrastructureServiceRow>(OnRemoveService),
                                row);

                        b.Log("actionDescriptor", actionDescriptor);

                        b.OnClickAction(actionDescriptor);
                    });
            });

            var dataGrid = b.DataGrid(
                gridBuilder,
                filteredServices,
                b.Const(
                    DataTable.GetColumns<InfrastructureServiceRow>().Where(
                        x => x != "Id" && x != "Tags").ToList()));
            return dataGrid;

            //var container = b.Div("w-full h-full");

            //var allServices = b.Get(clientModel, x => x.Configuration.InfrastructureServices.OrderBy(x => x.ServiceName).ToList());
            //var serviceRows = b.Map(allServices, (b, service) => b.ServiceRow(clientModel, service));
            //var filteredServices = b.FilterList(serviceRows, b.Get(clientModel, x => x.ServicesFilter));

            //b.OnModel(
            //    clientModel,
            //    (bParent, context) =>
            //    {
            //        var b = new LayoutBuilder(bParent);

            //        b.Add(container, b.DataGrid<InfrastructureServiceRow>(
            //            b =>
            //            {
            //                b.OnTable(b =>
            //                {
            //                    b.FillFrom(filteredServices, exceptColumns: new()
            //                    {
            //                        nameof(InfrastructureServiceRow.Id),
            //                        nameof(InfrastructureServiceRow.Tags)
            //                    });

            //                    b.SetCommonStyle();

            //                    b.OverrideColumnCell(
            //                        nameof(InfrastructureServiceRow.Name),
            //                        (b, data) => b.RenderServiceNameCell(b.Get(data, x => x.Row)));

            //                });

            //                b.AddHoverRowAction<EditConfigurationPage, InfrastructureServiceRow>(OnRemoveService, Icon.Remove, (b, data, props)=>
            //                {
            //                    b.AddClass(props, "text-red-500");
            //                });

            //                b.AddToolbarChild(AddServiceButton);

            //                b.AddToolbarChild(
            //                    b => b.Filter(
            //                        b =>
            //                        {
            //                            b.BindFilter(context, x => x.ServicesFilter);
            //                        }),
            //                    HorizontalPlacement.Right);

            //            }).As<HyperNode>());
            //    });

            //return container;
        }

        public static Var<string> GetProjectLabel(SyntaxBuilder b, Var<EditConfigurationPage> clientModel, Var<InfrastructureService> service)
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

        //public static Var<IVNode> AddServiceButton(this LayoutBuilder b)
        //{
        //    return b.Render(ControlDefinition.New<object>(
        //        "button",
        //        (b, data, props) =>
        //        {
        //            b.OnClickAction<EditConfigurationPage>(props, OnAddService);
        //            b.SetClass(props, "rounded py-2 px-4 shadow bg-sky-500 text-white");
        //        },
        //        (b, data) => b.TextSpan("Add service")), 
        //        b.Const(new object()));

        //}
    }
}
