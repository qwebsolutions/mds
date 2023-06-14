using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;
using System;
using System.Collections.Generic;
using MdsCommon;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Microsoft.AspNetCore.Components.Web;

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
        public static Var<List<SearchableInfrastructureService>> TransformServicesAsSearchable(this BlockBuilder b, Var<EditConfigurationPage> editConfigurationPage)
        {
            Var<List<SearchableInfrastructureService>> transformedInfrastructureServices = b.NewCollection<SearchableInfrastructureService>();

            var services = b.Get(editConfigurationPage, x => x.Configuration.InfrastructureServices);
        
            b.Foreach(services, (b, service) =>
            {
                Var<SearchableInfrastructureService> transformedInfrastructureService = b.NewObj<SearchableInfrastructureService>();
               
                var applicationId = b.Get(service, x => x.ApplicationId);
                var application = b.Get(editConfigurationPage, applicationId, (page, applicationId) => page.Configuration.Applications.Where(x => x.Id == applicationId).FirstOrDefault(new Application() { Name = "(not set)" }));
                var applicationName = b.Get(application, x => x.Name);
               
                b.Set(transformedInfrastructureService, x => x.ApplicationId, applicationName);
                b.Set(transformedInfrastructureService, x => x.Id, b.Get(service, x => x.Id));

                var projectId = b.Get(service, x => x.ProjectId);
                var project = b.Get(editConfigurationPage, projectId, (page, projectId) => page.AllProjects.Where(x => x.Id == projectId).FirstOrDefault(new Project() { Name = "(not set)" }));
                var projectName = b.Get(project, x => x.Name);
                b.Set(transformedInfrastructureService, x => x.ProjectId, projectName);

                var nodeId = b.Get(service, x => x.InfrastructureNodeId);
                var node = b.Get(editConfigurationPage, nodeId, (page, nodeId) => page.InfrastructureNodes.Where(x => x.Id == nodeId).FirstOrDefault(new InfrastructureNode() { NodeName = "(not set)" }));
                b.If(b.HasObject(node), b =>
                {
                    var nodeName = b.Get(node, x => x.NodeName);
                    b.Set(transformedInfrastructureService, x => x.InfrastructureNodeId, nodeName);
                }, b => b.Set(transformedInfrastructureService, x => x.InfrastructureNodeId, b.Const("")));

                b.Set(transformedInfrastructureService, x => x.Enabled, b.Get(service, x => x.Enabled));
                b.Set(transformedInfrastructureService, x => x.ConfigurationHeaderId, b.Get(service, x => x.ConfigurationHeaderId).As<string>());
                b.Set(transformedInfrastructureService, x => x.ServiceName, b.Get(service, x => x.ServiceName));

                var versionId = b.Get(service, x => x.ProjectVersionId);
                var version = b.Get(editConfigurationPage, versionId, (x, versionId) => x.AllProjects.SelectMany(x => x.Versions).SingleOrDefault(x => x.Id == versionId));
              
                b.If(b.HasObject(version), b =>
                {
                    var tag = b.Get(version, x => x.VersionTag);
                 
                    b.Set(transformedInfrastructureService, x => x.ProjectVersionId, tag);

                }, b => b.Set(transformedInfrastructureService, x => x.ProjectVersionId, b.Const("")));
         
                b.Push(transformedInfrastructureServices, transformedInfrastructureService);
            });
 
            return transformedInfrastructureServices;
        }
        public static void ApplyServicesFilter(this BlockBuilder b, Var<EditConfigurationPage> clientModel)
        {
            b.Set(
                 clientModel,
                 x => x.SearchableFilteredServices,
                 b.ContainingText(
                     b.TransformServicesAsSearchable(clientModel),
                     b.Get(clientModel, x => x.ServicesFilterValue))
                 );

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
                          b.Set(x => x.CssClass, "w-96 border py-2 pl-10 pr-2 rounded-lg text-gray-600");//hyper-input 
                      }
                   );
            var clearButton = b.Node(
               "button",
               "",
               b => b.Svg(Icon.XCircle, "w-8 h-8 flex-none text-gray-400"));

            b.SetOnClick(clearButton, b.MakeAction((BlockBuilder b, Var<EditConfigurationPage> page) =>
            {
                b.Set(page, filterProperty, b.Const(string.Empty));
                b.Call(applyFilter, page);

                return b.Clone(page);
            }));

            //return b.Div(
            //    "flex flex-row items-center",
            //    b => b.Svg(Icon.MagnifyingGlass, "w-8 h-8 text-gray-400"),
            //    b => filterInput,
            //    b => b.Optional(b.HasValue(b.Get(page, filterProperty)), b => b.Div("right-2", b => clearButton)));
            return b.Div(
               "flex flex-row items-center relative",
               b => b.Svg(Icon.MagnifyingGlass, "absolute left-2 w-6 h-6 text-blue-600 z-10"),
               b => filterInput,
               b => b.Optional(b.HasValue(b.Get(page, filterProperty)), b => b.Div("flex flex-row right-2 absolute items-center", b => clearButton)));

        }
        public static Var<HyperNode> TabServices(
           BlockBuilder b,
           Var<EditConfigurationPage> clientModel)
        {
            var rows = b.Get(clientModel, x => x.SearchableFilteredServices.OrderBy(x => x.ServiceName).ToList());

            var onAddService = b.MakeAction((BlockBuilder b, Var<EditConfigurationPage> clientModel) =>
            {
                var configHeaderId = b.Get(clientModel, x => x.Configuration.Id);
                var newId = b.NewId();

                var newService = b.NewObj<InfrastructureService>(b =>
                {
                    b.Set(x => x.Id, newId);
                    b.Set(x => x.ConfigurationHeaderId, configHeaderId);
                    b.Set(x => x.Enabled, true);
                });
                b.Comment("onAddService");
                b.Push(b.Get(clientModel, x => x.Configuration.InfrastructureServices), newService);

                b.Call(ApplyServicesFilter, clientModel);

                return b.GoTo(clientModel, EditService, newService);
            });

            var rc = b.RenderCell((BlockBuilder b, Var<SearchableInfrastructureService> row, Var<DataTable.Column> col) =>
            {
                var columnName = b.Get(col, x => x.Name);
                Var<SearchableInfrastructureService> service = row.As<SearchableInfrastructureService>();
                Var<string> applicationId = b.Get(service, x => x.ApplicationId);
                Var<string> nodeId = b.Get(service, x => x.InfrastructureNodeId); 
                Var<string> serviceName = b.Get(service, x => x.ServiceName);
                Var<string> projectName = b.Get(service, x => x.ProjectId);

                Var<string> projectNameAndVersion = b.Concat(projectName, b.Const(" "), b.Get(service, x => x.ProjectVersionId));
               
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
             (nameof(SearchableInfrastructureService.ServiceName), serviceNameCellBuilder),
             (nameof(SearchableInfrastructureService.ProjectId), b => b.Text(projectNameAndVersion)),
             (nameof(SearchableInfrastructureService.ApplicationId), b => b.Text(applicationId)),
             (nameof(SearchableInfrastructureService.InfrastructureNodeId), b => b.Text(nodeId))

                    ));
            });


            return b.DataGrid<SearchableInfrastructureService>(
                new()
                {
                    b=>b.CommandButton<EditConfigurationPage>(b=>
                    {
                        b.Set(x=>x.Label, "Add service");
                        b.Set(x => x.OnClick, onAddService);
                    }),
                      b => b.Filter<EditConfigurationPage>(
                                clientModel,
                                x => x.ServicesFilterValue,
                                ApplyServicesFilter,
                                "")
                },
                b =>
                {
                    b.AddColumn(nameof(SearchableInfrastructureService.ServiceName), "Name");
                    b.AddColumn(nameof(SearchableInfrastructureService.ProjectId), "Project");
                    b.AddColumn(nameof(SearchableInfrastructureService.ApplicationId), "Application");
                    b.AddColumn(nameof(SearchableInfrastructureService.InfrastructureNodeId), "Node");
                    b.SetRows(rows);
                    b.SetRenderCell(rc);
                },
                (b, actions, item) =>
                {
                    var removeIcon = Icon.Remove;

                    var onRemove = b.Def((BlockBuilder b, Var<SearchableInfrastructureService> item) =>
                        {
                            var serviceId = b.Get(item, x => x.Id);
                            var serviceRemoved = b.Get(clientModel, serviceId, (x, serviceId) => x.Configuration.InfrastructureServices.Where(x => x.Id != serviceId).ToList());
                            
                            b.Set(b.Get(clientModel, x => x.Configuration), x => x.InfrastructureServices, serviceRemoved);
                            b.Call(ApplyServicesFilter, clientModel);
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
    }
}
