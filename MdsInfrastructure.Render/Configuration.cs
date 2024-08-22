using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Linq;
using MdsCommon.Controls;
using Metapsi.Html;

namespace MdsInfrastructure.Render
{
    public static class Configuration
    {
        public class List 
        {
            public static Var<IVNode> Render(LayoutBuilder b, ListConfigurationsPage serverModel, Var<ListConfigurationsPage> clientModel)
            {
                b.AddModuleStylesheet();

                return b.Layout(
                        b.InfraMenu(nameof(MdsInfrastructure.Routes.Configuration),
                        serverModel.User.IsSignedIn()),
                        b.Render(b.Const(new Header.Props()
                        {
                            Main = new Header.Title() { Operation = "Configurations" },
                            User = serverModel.User,
                        })),
                        Render(b, b.Get(clientModel, x => x.ConfigurationHeadersList)));
            }

            private static Var<IVNode> Render(LayoutBuilder b, Var<ConfigurationHeadersList> clientModel)
            {
                var rows = b.Get(clientModel, x => x.ConfigurationHeaders.OrderBy(x => x.Name).ToList());
                return b.StyledDiv(
                    "rounded bg-white drop-shadow p-4",
                    b.ListConfigurationsGrid(rows));
            }
        }

        public static Var<IVNode> ListConfigurationsGrid(this LayoutBuilder b, Var<System.Collections.Generic.List<InfrastructureConfiguration>> rows)
        {
            var gridBuilder = MdsDefaultBuilder.DataGrid<InfrastructureConfiguration>();
            gridBuilder.CreateToolbarActions = (b) =>
            {
                return b.HtmlDiv(
                    b =>
                    {

                    },
                    b.HtmlButton(
                        b =>
                        {
                            b.SetClass("py-2 px-4 rounded shadow bg-sky-500 text-white");
                        },
                        b.Text("Add configuration")));
            };

            gridBuilder.DataTableBuilder.OverrideHeaderCell(
                nameof(InfrastructureConfiguration.InfrastructureServices),
                b =>
                {
                    return b.Text("Services");
                });

            gridBuilder.DataTableBuilder.OverrideDataCell(
                nameof(InfrastructureConfiguration.Name),
                (b, row) =>
                {
                    return b.HtmlA(
                        b =>
                        {
                            b.AddClass("py-4");
                            b.UnderlineBlue();
                            b.SetHref(b.Url<Routes.Configuration.Edit, Guid>(b.Get(row, x => x.Id)));
                        },
                        b.Text(b.Get(row, x => x.Name)));
                });

            gridBuilder.DataTableBuilder.OverrideDataCell(
                nameof(InfrastructureConfiguration.InfrastructureServices),
                (b, row) =>
                {
                    var count = b.AsString(b.Get(row, x => x.InfrastructureServices.Count()));
                    return b.StyledSpan("py-4", b.Text(count));
                });

            var dataGrid = b.DataGrid(
                gridBuilder,
                rows,
                b.Const(
                    new System.Collections.Generic.List<string>()
                    {
                        nameof(InfrastructureConfiguration.Name),
                        nameof(InfrastructureConfiguration.InfrastructureServices),
                    }));
            return dataGrid;
        }

        public class Edit
        {
            public static Var<IVNode> Render(LayoutBuilder b, EditConfigurationPage serverModel, Var<EditConfigurationPage> clientModel)
            {
                b.AddModuleStylesheet();


                var headerProps = b.GetHeaderProps(
                    b.Const("Edit configuration"),
                    b.Const(string.Empty),
                    b.Get(clientModel, x => x.User));

                return b.Layout(
                        b.InfraMenu(nameof(MdsInfrastructure.Routes.Configuration),
                        serverModel.User.IsSignedIn()),
                        b.Render(headerProps),
                        Render(b, clientModel)).As<IVNode>();
            }

            public static Var<HyperType.StateWithEffects> OnInit(SyntaxBuilder b, Var<EditConfigurationPage> model)
            {
                var serializedConfiguration = b.Serialize(b.Get(model, x => x.Configuration));
                b.Set(model, x => x.InitialConfiguration, serializedConfiguration);
                return b.MakeStateWithEffects(model);
            }

            private static Var<IVNode> Render(LayoutBuilder b, Var<EditConfigurationPage> clientModel)
            {
                return b.View(
                    clientModel,
                    EditConfiguration.MainPage,
                    EditConfiguration.EditApplication,
                    EditConfiguration.EditNote,
                    EditConfiguration.EditParameter,
                    EditConfiguration.EditService,
                    EditConfiguration.EditVariable);
            }
        }

        public class Review
        {
            public static Var<IVNode> Render(LayoutBuilder b, ReviewConfigurationPage serverModel, Var<ReviewConfigurationPage> clientModel)
            {
                b.AddModuleStylesheet();

                var headerProps = b.GetHeaderProps(
                    b.Const("Review configuration"),
                    b.Const(string.Empty),
                    b.Get(clientModel, x => x.User));

                return b.Layout(
                    b.InfraMenu(nameof(Configuration),
                    serverModel.User.IsSignedIn()),
                    b.Render(headerProps),
                    RenderDeploymentConfiguration(b, clientModel, serverModel.Snapshot, serverModel.SavedConfiguration)).As<IVNode>();
            }

            public class ConfigurationRow
            {
                public string ServiceName { get; set; }
                public string Property { get; set; }
                public string Value { get; set; }
            }

            public static Var<IVNode> RenderDeploymentConfiguration(
                LayoutBuilder b,
                Var<ReviewConfigurationPage> clientModel,
                System.Collections.Generic.List<MdsCommon.ServiceConfigurationSnapshot> infrastructureSnapshot,
                InfrastructureConfiguration infrastructureConfiguration)
            {

                System.Collections.Generic.List<ConfigurationRow> configurationRows = new();
                foreach (ServiceConfigurationSnapshot serviceSnapshot in infrastructureSnapshot.OrderBy(x => x.ServiceName))
                {
                    configurationRows.Add(new ConfigurationRow() { ServiceName = serviceSnapshot.ServiceName, Property = "Node", Value = serviceSnapshot.NodeName });
                    configurationRows.Add(new ConfigurationRow() { ServiceName = serviceSnapshot.ServiceName, Property = "Project", Value = serviceSnapshot.ProjectName });
                    configurationRows.Add(new ConfigurationRow() { ServiceName = serviceSnapshot.ServiceName, Property = "Version", Value = serviceSnapshot.ProjectVersionTag });
                    foreach (var param in serviceSnapshot.ServiceConfigurationSnapshotParameters)
                    {
                        configurationRows.Add(new ConfigurationRow() { ServiceName = serviceSnapshot.ServiceName, Property = param.ParameterName, Value = param.DeployedValue });
                    }
                }

                return b.HtmlDiv(
                   b =>
                   {
                       b.SetClass("flex flex-col w-full gap-4");
                   },
                   b.Toolbar(
                       b =>
                       {
                           b.AddClass("justify-end");
                       },
                       b.HtmlA(
                           b =>
                           {
                               b.SetClass("rounded flex flex-row gap-2 items-center py-2 px-4 shadow text-white bg-sky-500");
                               b.SetHref(b.Url<Routes.Deployment.ConfigurationPreview, Guid>(b.Const(infrastructureConfiguration.Id)));
                           },
                           b.Svg(Icon.Swap, "w-5 h-5"),
                           b.Text("Review deployment actions")),
                       b.DeployNowButton<ReviewConfigurationPage>(b.Const(infrastructureConfiguration.Id))),
                   b.HtmlDiv(
                       b =>
                       {
                           b.SetClass("flex flex-col gap-2 w-full bg-white p-8 rounded shadow");
                       },
                       b.HtmlDiv(
                           b =>
                           {
                               b.SetClass("flex flex-row justify-end");
                           },
                           b.Filter(clientModel, x => x.FilterValue)),
                       b.DataTable(
                           MdsDefaultBuilder.DataTable<ConfigurationRow>(),
                           b.FilterList(b.Const(configurationRows), b.Get(clientModel, x => x.FilterValue)))));
            }
        }
    }

    public static class EditConfigurationViews
    {
        public static Var<EditConfigurationPage> EditView<TModel>(this SyntaxBuilder b, Var<EditConfigurationPage> page, Func<LayoutBuilder, Var<TModel>, Var<IVNode>> viewRenderer)
        {
            b.SwapView(viewRenderer);
            return b.Clone(page);
        }
    }
}

