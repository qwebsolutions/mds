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
using Metapsi.Dom;
using System.ComponentModel.DataAnnotations.Schema;

namespace MdsInfrastructure.Render
{
    //public class Filter
    //{
    //    public string Value { get; set; }
    //}

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
        public static Var<EditConfigurationPage> OnAddService(BlockBuilder b, Var<EditConfigurationPage> clientModel)
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
        }

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

        public static Var<IVNode> RenderServiceNameCell(this BlockBuilder b, Var<InfrastructureServiceRow> row)
        {
            var serviceName = b.Get(row, x => x.Name);
            var serviceDisabled = b.Includes(b.Get(row, x => x.Tags), b.Const("disabled"));
            b.Log("service disabled", serviceDisabled);

            var goToEditService = (BlockBuilder b, Var<EditConfigurationPage> clientModel) =>
            {
                b.Set(clientModel, x => x.EditServiceId, b.Get(row, x => x.Id));
                return b.EditView<EditConfigurationPage>(clientModel, EditService);
            };

            var container = b.Span();
            b.Add(container, b.Link<EditConfigurationPage>(b.WithDefault(serviceName), b.MakeAction<EditConfigurationPage>(goToEditService)));
            b.If(serviceDisabled, (BlockBuilder b) =>
            {
                var badge = b.Add(container, b.Badge(b.Const("disabled")));
                b.AddClass(badge, "bg-gray-400");
            });
            return container.As<IVNode>();
        }

        public static Var<HyperNode> TabServices(
           BlockBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            var container = b.Div("w-full h-full");

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
                var serviceName = b.Get(row, x => x.Name);
                var serviceDisabled = b.Includes(b.Get(row, x => x.Tags), b.Const("disabled"));

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

            var contextToolbar = b.Div("flex flex-row items-end");

            b.OnModel(
                clientModel,
                (bParent, context) =>
                {
                    var b = new LayoutBuilder();
                    b.Init(bParent.ModuleBuilder, bParent.Block);
                    b.Add(container, b.DataGrid<InfrastructureServiceRow>(
                        b =>
                        {
                            b.Toolbar.BuildControl = (b, props) =>
                            {
                                return b.Div(
                                    b.EditProps(props, b=>
                                    {
                                        b.Log("EditProps", props);
                                    }),
                                    b.Button(
                                        b =>
                                        {
                                            b.OnClickAction<EditConfigurationPage>(OnAddService);
                                            b.SetClass("rounded py-2 px-4 shadow bg-sky-500 text-white");
                                        },
                                        b.T("Add service")),
                                    b.Button(
                                        b =>
                                        {
                                            b.OnClickAction<EditConfigurationPage>(OnAddService);
                                            b.SetClass("rounded py-2 px-4 shadow bg-sky-500 text-white");
                                        },
                                        b.T("Add service"))
                                    );
                            };

                            b.GuessColumns(
                                b.Table,
                                except: new()
                                {
                                    nameof(InfrastructureServiceRow.Id),
                                    nameof(InfrastructureServiceRow.Tags)
                                });

                            b.SetCommonStyle(b.Table);

                            b.Table.TableCell.WrapBuildControl((b, cellData, props, baseBuilder) =>
                            {
                                return b.If(
                                    b.Get(cellData, x => x.Column == nameof(InfrastructureServiceRow.Name)),
                                    b =>
                                    {
                                        return b.H(
                                            "td",
                                            props,
                                            b.RenderServiceNameCell(b.Get(cellData, x => x.Row)));
                                    },
                                    b => baseBuilder(b, cellData, props));
                            });

                            b.Set(b.Table.Data, x => x.Rows, filteredServices);
                        }).As<HyperNode>());


                    //b.Add(container, b.DataTable<InfrastructureServiceRow>(
                    //    (b, builder) =>
                    //    {

                    //        b.GuessColumns(
                    //            builder,
                    //            except: new()
                    //            {
                    //                nameof(InfrastructureServiceRow.Id),
                    //                nameof(InfrastructureServiceRow.Tags)
                    //            });

                    //        b.SetCommonStyle(builder);

                    //        builder.TableCell.WrapBuildControl((b, cellData, props, baseBuilder) =>
                    //        {
                    //            return b.If(
                    //                b.Get(cellData, x => x.Column == nameof(InfrastructureServiceRow.Name)),
                    //                b =>
                    //                {
                    //                    return b.H(
                    //                        "td",
                    //                        props,
                    //                        b.RenderServiceNameCell(b.Get(cellData, x => x.Row)));
                    //                },
                    //                b => baseBuilder(b, cellData, props));
                    //        });

                    //        b.Set(builder.Data, x => x.Rows, filteredServices);
                    //    }).As<HyperNode>());

                    b.Add(contextToolbar, b.Filter(
                        (b, filter) =>
                        {
                            b.InBindingContext(
                                context,
                                filter.Data,
                                b =>
                                {
                                    b.BindOneWay(x => x.Value, x => x.ServicesFilter);
                                });

                            b.Set(filter.Data, x => x.SetValue, b.DefineAction<LayoutBuilder, object, string>((b, o, s) =>
                            {
                                b.Set(o.As<EditConfigurationPage>(), x => x.ServicesFilter, s);
                            }));

                            filter.ClearButton.SetProps(b =>
                            {
                                b.AddClass(b.Const("text-green-400"));
                            });
                        }).As<HyperNode>());
                });

            b.Add(container, b.DataGrid<InfrastructureServiceRow>(
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
                }));

            return container;
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
