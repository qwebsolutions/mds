using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        public static Var<HyperNode> MainPage(
            this BlockBuilder b,
            Var<EditConfigurationPage> clientModel,
            Var<bool> hasClientSideChanges)
        {
            var isSaved = b.Not(hasClientSideChanges);
            var configId = b.Get(clientModel, x => x.Configuration.Id);
            var configuration = b.Get(clientModel, x => x.Configuration);
            b.Log("MainPage");
            b.Log(configuration);

            var deploymentReportUrl = b.Url(DeploymentReport, configId);
            var saveUrl = b.Url(Save);

            var container = b.Div("flex flex-col w-full");
            b.Add(container, b.ValidationPanel(clientModel));
            var toolbar = b.Toolbar(
                b => b.FromDefault<NavigateButton.Props>(NavigateButton.Render, b =>
                {
                    b.Set(x => x.Label, "Deploy");
                    b.Set(x => x.Href, deploymentReportUrl);
                    b.Set(x => x.Enabled, isSaved);
                }),
                //Href or validate. Return Id + Property + validation message. On blur, remove validation (this is still not done. this sucks.)
                b=> b.SubmitButton<InfrastructureConfiguration>(b =>
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
                    TabContent = b => b.Call(TabConfiguration, clientModel)
                },
                new Metapsi.Hyperapp.Controls.TabRenderer()
                {
                    TabPageCode = "Services",
                    TabHeader = b => b.Text("Services"),
                    TabContent = b => b.Call(TabServices, clientModel)
                },
                new Metapsi.Hyperapp.Controls.TabRenderer()
                {
                    TabPageCode = "Applications",
                    TabHeader = b => b.Text("Applications"),
                    TabContent = b => b.Call(TabApplications, clientModel)
                },
                new Metapsi.Hyperapp.Controls.TabRenderer()
                {
                    TabPageCode = "Variables",
                    TabHeader = b => b.Text("Variables"),
                    TabContent = b => b.Call(TabVariables, clientModel)
                }));

            return container;
        }
    }
}
