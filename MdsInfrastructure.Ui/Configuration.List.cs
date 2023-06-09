using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using MdsCommon;
using Microsoft.AspNetCore.Http;

namespace MdsInfrastructure
{
    public static partial class Configuration
    {
        public static async Task<IResult> List(CommandContext commandContext, HttpContext requestData)
        {
            var configurationsList = await MdsInfrastructureFunctions.Configurations(commandContext);

            return Page.Response(configurationsList,
                (b, clientModel) => b.Layout(
                    b.InfraMenu(nameof(Configuration), requestData.User().IsSignedIn()),
                    b.Render(b.Const(new Header.Props()
                    {
                        Main = new Header.Title() { Operation = "Configurations" },
                        User = requestData.User()
                    })),
                    Render(b, clientModel)),
                string.Empty);
        }

        public static Var<HyperNode> Render(BlockBuilder b, Var<ConfigurationHeadersList> clientModel)
        {
            var addConfigurationUrl = b.Url(Configuration.Add);
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
                        var link = b.Add(cell, b.Link(confName, b.Url(EditConfiguration.Edit, confId)));
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

