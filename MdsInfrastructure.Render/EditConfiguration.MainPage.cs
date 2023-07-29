using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {

        public static Var<HyperNode> MainPage(
            BlockBuilder b,
            Var<EditConfigurationPage> clientModel)
        {
            var isSaved = b.Not(HasChanges(b, clientModel));
            var configId = b.Get(clientModel, x => x.Configuration.Id);
            var configuration = b.Get(clientModel, x => x.Configuration);

            var deploymentReportUrl = b.Url<Routes.Deployment.ConfigurationPreview, Guid>(configId);

            // TODO: Change to API
            //var saveUrl = b.Url(Save);
            var saveUrl = "/save";

            var container = b.Div("flex flex-col w-full");
            //b.Add(container, b.ValidationPanel(clientModel));
            var toolbar = b.Toolbar(
                b => b.FromDefault<NavigateButton.Props>(NavigateButton.Render, b =>
                {
                    b.Set(x => x.Label, "Deploy");
                    b.Set(x => x.Href, deploymentReportUrl);
                    b.Set(x => x.Enabled, isSaved);
                }),
                //Href or validate. Return Id + Property + validation message. On blur, remove validation (this is still not done. this sucks.)
                b => b.SubmitButton<InfrastructureConfiguration>(b =>
                {
                    b.Set(x => x.Label, "Save");
                    b.Set(x => x.Href, saveUrl);
                    b.Set(x => x.Payload, configuration);
                    b.Set(x => x.Enabled, b.Not(isSaved));
                    b.Set(x => x.ButtonClass, "rounded text-white py-2 px-4 shadow");
                }));

            b.AddClass(toolbar, "justify-end");

            b.Add(container, b.Tabs(
                clientModel,
                b.Const("configurationTabs"),
                toolbar,
                new Metapsi.Hyperapp.Controls.TabRenderer()
                {
                    TabPageCode = "Configuration",
                    TabHeader = b => b.Text("Configuration"),
                    TabContent = b => b.Call(EditConfiguration.TabConfiguration, clientModel)
                },
                new Metapsi.Hyperapp.Controls.TabRenderer()
                {
                    TabPageCode = "Services",
                    TabHeader = b => b.Text("Services"),
                    TabContent = b => b.Call(EditConfiguration.TabServices, clientModel)
                },
                new Metapsi.Hyperapp.Controls.TabRenderer()
                {
                    TabPageCode = "Applications",
                    TabHeader = b => b.Text("Applications"),
                    TabContent = b => b.Call(EditConfiguration.TabApplications, clientModel)
                },
                new Metapsi.Hyperapp.Controls.TabRenderer()
                {
                    TabPageCode = "Variables",
                    TabHeader = b => b.Text("Variables"),
                    TabContent = b => b.Call(EditConfiguration.TabVariables, clientModel)
                }));

            return container;
        }

        private static Var<bool> HasChanges(BlockBuilder b, Var<EditConfigurationPage> model)
        {
            var current = b.Serialize(b.Get(model, x => x.Configuration));
            var hasChanges = b.Not(b.AreEqual(current, b.Get(model, x => x.InitialConfiguration)));
            return hasChanges;
        }
    }
}
