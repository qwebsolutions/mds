using MdsCommon;
using Metapsi;
using Metapsi.Dom;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
using System;
using System.Linq;
using MdsCommon.Controls;

namespace MdsInfrastructure.Render
{
    public static class Configuration
    {
        public class List : MixedHyperPage<ListConfigurationsPage, ConfigurationHeadersList>
        {
            public override ConfigurationHeadersList ExtractClientModel(ListConfigurationsPage serverData)
            {
                return serverData.ConfigurationHeadersList;
            }

            public override Var<HyperNode> OnRender(BlockBuilder b, ListConfigurationsPage serverModel, Var<ConfigurationHeadersList> clientModel)
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
                        Render(b, clientModel));
            }

            private static Var<HyperNode> Render(BlockBuilder b, Var<ConfigurationHeadersList> clientModel)
            {
                var addConfigurationUrl = Route.Path<Routes.Configuration.Add>();
                var rows = b.Get(clientModel, x => x.ConfigurationHeaders.OrderBy(x => x.Name).ToList());

                var rc = b.RenderCell<InfrastructureConfiguration>((b, configurationHeader, column) =>
                {
                    var confId = b.Get(configurationHeader, x => x.Id);

                    var cell = b.Div();

                    var isNameCol = b.AreEqual(b.Get(column, x => x.Name), b.Const("Name"));

                    b.If(isNameCol,
                        b =>
                        {
                            var confName = b.Get(configurationHeader, x => x.Name);
                            var confId = b.Get(configurationHeader, x => x.Id);
                            var link = b.Add(cell, b.Link(confName, b.Url<Routes.Configuration.Edit, Guid>(confId)));
                        },
                        b =>
                        {
                            var servicesCount = b.Get(clientModel, confId, (x, confId) => x.Services.Where(x => x.ConfigurationHeaderId == confId).Count());
                            b.Add(cell, b.AsString(servicesCount));
                        });

                    return b.VPadded4(cell);
                });

                var dataGrid = b.DataGrid<InfrastructureConfiguration>(
                    new()
                    {
                    b => b.AddClass(b.FromDefault<NavigateButton.Props>(NavigateButton.Render, b=>
                    {
                        b.Set(x=>x.Label, "Add configuration");
                        b.Set(x => x.Href, addConfigurationUrl);
                    }), "text-white")
                    },
                    b =>
                    {
                        b.AddColumn("Name");
                        b.AddColumn("Services");
                        b.SetRows(rows);
                        b.SetRenderCell(rc);
                    });
                b.AddClass(dataGrid, "drop-shadow");
                return dataGrid;
            }
        }

        public class Edit : MixedHyperPage<EditConfigurationPage, EditConfigurationPage>
        {
            public override EditConfigurationPage ExtractClientModel(EditConfigurationPage serverModel)
            {
                return serverModel;
            }

            public override Var<HyperNode> OnRender(BlockBuilder b, EditConfigurationPage serverModel, Var<EditConfigurationPage> clientModel)
            {
                b.AddModuleStylesheet();

                return b.Layout(
                        b.InfraMenu(nameof(MdsInfrastructure.Routes.Configuration),
                        serverModel.User.IsSignedIn()),
                        b.Render(b.Const(new Header.Props()
                        {
                            Main = new Header.Title() { Operation = "Edit configuration" },
                            User = serverModel.User,
                        })),
                        Render(b, clientModel));
                //Render(b, clientModel));
            }

            public override Var<HyperType.StateWithEffects> OnInit(BlockBuilder b, Var<EditConfigurationPage> model)
            {
                var serializedConfiguration = b.Serialize(b.Get(model, x => x.Configuration));
                b.Set(model, x => x.InitialConfiguration, serializedConfiguration);
                return b.MakeStateWithEffects(model);
            }

            private Var<HyperNode> Render(BlockBuilder b, Var<EditConfigurationPage> clientModel)
            {
                return b.View(
                    clientModel,
                    EditConfigurationViews.AreaName,
                    EditConfiguration.MainPage,
                    EditConfiguration.EditApplication,
                    EditConfiguration.EditNote,
                    EditConfiguration.EditParameter,
                    EditConfiguration.EditService,
                    EditConfiguration.EditVariable);
            }
        }

        public class Review : MixedHyperPage<ReviewConfigurationPage, ReviewConfigurationPage>
        {
            public override ReviewConfigurationPage ExtractClientModel(ReviewConfigurationPage serverModel)
            {
                return serverModel;
            }

            public override Var<HyperNode> OnRender(BlockBuilder b, ReviewConfigurationPage serverModel, Var<ReviewConfigurationPage> clientModel)
            {
                b.AddModuleStylesheet();

                return b.Layout(
                    b.InfraMenu(nameof(Configuration),
                    serverModel.User.IsSignedIn()),
                    b.Render(
                        b.Const(new Header.Props()
                        {
                            Main = new Header.Title() { Operation = "Review configuration" },
                            User = serverModel.User
                        })),
                    RenderDeploymentConfiguration(b, serverModel.Snapshot, serverModel.SavedConfiguration));
            }

            public class ConfigurationRow
            {
                public string ServiceName { get; set; }
                public string Property { get; set; }
                public string Value { get; set; }
            }

            public Var<HyperNode> RenderDeploymentConfiguration(
                BlockBuilder b,
                System.Collections.Generic.List<MdsCommon.ServiceConfigurationSnapshot> infrastructureSnapshot,
                InfrastructureConfiguration infrastructureConfiguration)
            {
                var view = b.Div();

                var deploymentReportUrl = b.Url<Routes.Deployment.ConfigurationPreview, Guid>(b.Const(infrastructureConfiguration.Id));
                //var confirmDeploymentUrl = b.Url(ConfirmDeployment, b.Const(infrastructureConfiguration.Id));

                var swapIcon = Icon.Swap;

                var deployNowButton = b.Node(
                    "button",
                    "bg-red-500 rounded px-4 py-2 text-white",
                    b => b.Text("Deploy now"));

                b.SetOnClick(deployNowButton, b.MakeAction((BlockBuilder b, Var<ReviewConfigurationPage> model) =>
                {
                    return b.AsyncResult(
                        b.ShowPanel(model),
                        b.Request(
                            Frontend.ConfirmDeployment,
                            b.Get(model, x => x.SavedConfiguration.Id),
                            b.MakeAction((BlockBuilder b, Var<ReviewConfigurationPage> model, Var<Frontend.ConfirmDeploymentResponse> response) =>
                            {
                                b.SetUrl(b.Const("/"));
                                return model;
                            }))
                        );
                }));

                var toolbar = b.Add(view,
                    b.Toolbar(
                        b => b.NavigateButton(b =>
                        {
                            b.Set(x => x.Label, "Review deployment actions");
                            b.Set(x => x.Href, deploymentReportUrl);
                            b.Set(x => x.SvgIcon, swapIcon);
                        }),
                        b => deployNowButton));

                b.AddClass(toolbar, "justify-end");

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

                var rc = b.Def((BlockBuilder b, Var<ConfigurationRow> row, Var<DataTable.Column> column) =>
                {
                    return b.VPadded4(b.Text(b.GetProperty<string>(row, b.Get(column, x => x.Name))));
                });

                var dataTableProps = b.NewObj<DataTable.Props<ConfigurationRow>>(b =>
                {
                    b.SetRows(b.Const(configurationRows.ToList()));
                    b.AddColumn(nameof(ConfigurationRow.ServiceName), "Service name");
                    b.AddColumn(nameof(ConfigurationRow.Property));
                    b.AddColumn(nameof(ConfigurationRow.Value));
                    b.SetRenderCell(rc);
                });

                b.Add(view, b.DataTable(dataTableProps));

                return view;
            }

        }
    }

    public static class EditConfigurationViews
    {
        public const string AreaName = nameof(EditConfigurationViews);

        public static Var<EditConfigurationPage> EditView(this BlockBuilder b, Var<EditConfigurationPage> page, Var<string> viewName)
        {
            b.SwapView(page, b.Const(AreaName), viewName);
            return b.Clone(page);
        }

        public static Var<EditConfigurationPage> EditView<TModel>(this BlockBuilder b, Var<EditConfigurationPage> page, Func<BlockBuilder, Var<TModel>, Var<HyperNode>> viewRenderer)
        {
            b.SwapView(page, b.Const(AreaName), b.GetViewName(viewRenderer));
            return b.Clone(page);
        }
    }
}

