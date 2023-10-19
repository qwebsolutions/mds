using MdsCommon.Controls;
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

            var saveButton = b.Node(
                "button",
                "rounded text-white py-2 px-4 shadow",
                b => b.Text("Save", "text-white"));

            b.If(isSaved,
                b =>
                {
                    b.SetAttr(saveButton, Html.disabled, true);
                    b.AddClass(saveButton, "bg-gray-300");
                },
                b =>
                {
                    b.SetAttr(saveButton, Html.disabled, false);
                    b.AddClass(saveButton, "bg-sky-500");
                });

            b.SetOnClick(saveButton, b.MakeAction<EditConfigurationPage>((b, model) =>
            {
                return b.AsyncResult(
                    b.ShowPanel(model),
                    b.Request(
                        Frontend.SaveConfiguration,
                        b.Get(model, x => x.Configuration),
                        b.MakeAction((BlockBuilder b, Var<EditConfigurationPage> page, Var<Frontend.SaveConfigurationResponse> response) =>
                        {
                            b.Set(page, x => x.InitialConfiguration, b.Serialize(b.Get(page, x => x.Configuration)));
                            b.Log("InitialConfiguration set", page);
                            return b.Clone(page);
                        })));
            }));

            var deployLink = b.Node(
                "a",
                "rounded text-white py-2 px-4 shadow",
                b => b.Text("Deploy", "text-white"));

            b.SetAttr(deployLink, Html.href, deploymentReportUrl);

            b.If(b.Not(isSaved),
                b =>
                {
                    b.SetAttr(deployLink, Html.disabled, true);
                    b.AddClass(deployLink, "bg-gray-300");
                },
                b =>
                {
                    b.SetAttr(deployLink, Html.disabled, false);
                    b.AddClass(deployLink, "bg-sky-500");
                });

            var container = b.Div("flex flex-col w-full bg-white rounded shadow");
            var layoutBuilder = new LayoutBuilder(b);
            b.Add(
                container,
                layoutBuilder.Tabs(
                    b =>
                    {
                        b.AddTab(
                            "Configuration",
                            b => b.Call(EditConfiguration.TabConfiguration, clientModel).As<IVNode>());

                        b.AddTab(
                            "Services",
                            b => b.Call(EditConfiguration.TabServices, clientModel).As<IVNode>());

                        b.AddTab(
                            "Applications",
                            b => b.Call(EditConfiguration.TabApplications, clientModel).As<IVNode>());

                        b.AddTab(
                            "Variables",
                            b => b.Call(EditConfiguration.TabVariables, clientModel).As<IVNode>());

                        b.AddToolbarCommand(b => deployLink.As<IVNode>());
                        b.AddToolbarCommand(b => saveButton.As<IVNode>());

                    }).As<HyperNode>());

            //b.Add(container, b.Tabs(
            //    clientModel,
            //    b.Const("configurationTabs"),
            //    toolbar,
            //    new TabRenderer()
            //    {
            //        TabPageCode = "Configuration",
            //        TabHeader = b => b.Text("Configuration"),
            //        TabContent = b => b.Call(EditConfiguration.TabConfiguration, clientModel)
            //    },
            //    new TabRenderer()
            //    {
            //        TabPageCode = "Services",
            //        TabHeader = b => b.Text("Services"),
            //        TabContent = b => b.Call(EditConfiguration.TabServices, clientModel)
            //    },
            //    new TabRenderer()
            //    {
            //        TabPageCode = "Applications",
            //        TabHeader = b => b.Text("Applications"),
            //        TabContent = b => b.Call(EditConfiguration.TabApplications, clientModel)
            //    },
            //    new TabRenderer()
            //    {
            //        TabPageCode = "Variables",
            //        TabHeader = b => b.Text("Variables"),
            //        TabContent = b => b.Call(EditConfiguration.TabVariables, clientModel)
            //    }));

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
