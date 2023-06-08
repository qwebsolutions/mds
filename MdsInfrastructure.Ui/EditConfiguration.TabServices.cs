using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;
using System;
using System.Collections.Generic;
using MdsCommon;
using System.Linq.Expressions;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        public static Var<List<TItem>> ContainingText<TItem>(this BlockBuilder b, Var<List<TItem>> items, Var<string> filterText)
        {
           
            return b.Get(
                items,
                b.Def<TItem, string>(Native.ConcatObjectValues),
                b.Def<string, string, bool>(Native.Includes),
                b.Def<string, string>(Native.ToLowercase),
                filterText,
                (items, getValues, includes, lower, filterText) => items.Where(x => includes(lower(getValues(x)), lower(filterText))).ToList());
        }
        public static void ApplyFilter(this BlockBuilder b, Var<EditConfigurationPage> clientModel)
        {
            b.Set(
                 clientModel,
                 x => x.FilteredServices,
                 b.ContainingText(
                     b.Get(
                         clientModel,
                         x => x.Configuration.InfrastructureServices),
                     b.Get(clientModel, x => x.FilterValue)));

        }
        public static Var<HyperNode> Filter<TPage>(this BlockBuilder b,
            Var<EditConfigurationPage> page,
            Expression<Func<EditConfigurationPage, string>> filterProperty,
            Action<BlockBuilder, Var<EditConfigurationPage>> applyFilter,
            string placeholder)
        {
            var filterValue = b.Get(page, filterProperty);
            var filterInput =
            b.Input(
                     filterValue,

                      b.MakeAction((BlockBuilder b, Var<EditConfigurationPage> page, Var<string> inputString) =>
                      {
                          b.Set(page, filterProperty, inputString);

                          return b.MakeStateWithEffects(
                              page,
                              b.Debounce(
                                  b.Const(1000),
                                  b.MakeAction((BlockBuilder b, Var<EditConfigurationPage> page) =>
                                  {
                                      b.Call(applyFilter, page);
                                      return b.Clone(page);
                                  })));
                      }),

                      b.Const(placeholder),

                      b =>
                      {
                          b.Set(x => x.CssClass, "w-96 border py-2 pl-10 pr-2 rounded-lg text-gray-600");
                      }
                   );
            var clearButton = b.Node(
               "button",
               "",
               b => b.Svg(Icon.XCircle, "w-8 h-8 flex-none text-gray-400"));

            b.SetOnClick(clearButton, b.MakeAction((BlockBuilder b, Var<EditConfigurationPage> page) =>
            {
                b.Set(page, filterProperty, b.Const(string.Empty));
                b.Call(ApplyFilter, page);

                return b.Clone(page);
            }));

            return b.Div(
                "flex flex-row items-center",
                b => b.Svg(Icon.MagnifyingGlass, "w-8 h-8 text-gray-400"),
                b => filterInput,
                b => b.Optional(b.HasValue(b.Get(page, filterProperty)), b => b.Div("right-2", b => clearButton)));

        }
        public static Var<HyperNode> TabServices(
           BlockBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            //var rows = b.Get(clientModel, x => x.Configuration.InfrastructureServices.OrderBy(x => x.ServiceName).ToList());
            var rows = b.Get(clientModel, x => x.FilteredServices.OrderBy(x => x.ServiceName).ToList());

            var onAddService = b.MakeAction((BlockBuilder b, Var<EditConfigurationPage> clientModel) =>
            {
                var configHeaderId = b.Get(clientModel, x => x.Configuration.Id);
                //var services = b.Get(clientModel, x => x.Configuration.InfrastructureServices);
                var newId = b.NewId();

                var newService = b.NewObj<InfrastructureService>(b =>
                {
                    b.Set(x => x.Id, newId);
                    b.Set(x => x.ConfigurationHeaderId, configHeaderId);
                    b.Set(x => x.Enabled, true);
                });
                b.Comment("onAddService");
                b.Push(b.Get(clientModel, x => x.Configuration.InfrastructureServices), newService);

                return b.GoTo(clientModel, EditService, newService);
            });

            var rc = b.RenderCell((BlockBuilder b, Var<InfrastructureService> row, Var<DataTable.Column> col) =>
            {
                var columnName = b.Get(col, x => x.Name);
                Var<InfrastructureService> service = row.As<InfrastructureService>();
                Var<Guid> applicationId = b.Get(service, x => x.ApplicationId);
                Var<Guid> nodeId = b.Get(service, x => x.InfrastructureNodeId);
                Var<string> serviceName = b.Get(service, x => x.ServiceName);
                Var<bool> serviceDisabled = b.Get(service, service => !service.Enabled);
                
                var goToEditService = (BlockBuilder b, Var<EditConfigurationPage> clientModel) => b.GoTo(clientModel, EditService, service);

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
                    b=>b.CommandButton<EditConfigurationPage>(b=>
                    {
                        b.Set(x=>x.Label, "Add service");
                        b.Set(x => x.OnClick, onAddService);
                    }),
                      b => b.Filter<EditConfigurationPage>(
                                clientModel,
                                x => x.FilterValue,
                                ApplyFilter,
                                "")
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
                }) ;
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
