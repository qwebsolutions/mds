using MdsCommon;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
using System;
using System.Linq;

namespace MdsInfrastructure.Render
{
    public class ListConfigurations : MixedHyperPage<ListConfigurationsPage, ConfigurationHeadersList>
    {
        public override ConfigurationHeadersList ExtractClientModel(ListConfigurationsPage serverData)
        {
            return serverData.ConfigurationHeadersList;
        }

        public override Var<HyperNode> OnRender(BlockBuilder b, ListConfigurationsPage serverModel, Var<ConfigurationHeadersList> clientModel)
        {
            b.AddStylesheet("/static/tw.css");

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
                        var link = b.Add(cell, b.Link(confName, b.Concat(b.Const(Route.Path<Routes.Configuration.Edit>()), b.AsString(confId))));
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
                    b => b.FromDefault<NavigateButton.Props>(NavigateButton.Render, b=>
                    {
                        b.Set(x=>x.Label, "Add configuration");
                        b.Set(x => x.Href, addConfigurationUrl);
                    })
                },
                b => {
                    b.AddColumn("Name");
                    b.AddColumn("Services");
                    b.SetRows(rows);
                    b.SetRenderCell(rc);
                });
            b.AddClass(dataGrid, "drop-shadow");
            return dataGrid;
        }
    }
}
