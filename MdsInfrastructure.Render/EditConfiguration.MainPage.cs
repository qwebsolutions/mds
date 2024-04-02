using MdsCommon.Controls;
using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Metapsi.Shoelace;
using MdsCommon;
using Microsoft.Extensions.Options;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public const string IdCurrentJsonPopup = "id-current-json";
        public const string IdSaveConflictPopup = "id-save-conflict";
        public const string IdMergeSuccessPopup = "id-merge-success";
        public const string IdMergeFailedPopup = "if-merge-failed";

        public static Var<HyperNode> MainPage(
            LayoutBuilder b,
            Var<EditConfigurationPage> clientModel)
        {
            var isSaved = b.Not(HasChanges(b, clientModel));
            var configId = b.Get(clientModel, x => x.Configuration.Id);
            var configuration = b.Get(clientModel, x => x.Configuration);
            var menuDropdown = b.SlDropdown(
                b => { },
                b.SlIconButton(b =>
                {
                    b.SetSlot(SlDropdown.Slot.Trigger);
                    b.SetName("gear");
                    b.SetLabel("merge-options");
                }),
                b.SlMenu(
                    b =>
                    {
                        b.OnSlSelect(b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<object> args) =>
                        {
                            var item = b.GetDynamic(args, new DynamicProperty<object>("item"));
                            var selectedValue = b.GetDynamic(item, new DynamicProperty<string>("value"));

                            var showCurrentJson = b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> clientModel) =>
                            {
                                return b.MakeStateWithEffects(
                                    b.ShowPanel(clientModel),
                                    b.MakeEffect(
                                        b.Def(
                                            b.Request(
                                                Frontend.GetConfigurationJson,
                                                b.Get(clientModel, x => x.Configuration),
                                                b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<GetConfigurationJsonResponse> response) =>
                                                {
                                                    b.Set(page, x => x.CurrentConfigurationSimplifiedJson, b.Get(response, x => x.Json));
                                                    b.ShowDialog(b.Const(IdCurrentJsonPopup));
                                                    return b.Clone(page);
                                                })))));
                            });

                            var downloadWindowsScripts = b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> clientModel) =>
                            {
                                var fetchOptions = b.NewObj<FetchOptions>();
                                b.Set(fetchOptions, x => x.method, b.Const("POST"));
                                b.Set(fetchOptions, x => x.body, b.Serialize(b.Get(clientModel, x => x.Configuration)));
                                b.SetDynamic(b.Get(fetchOptions, x => x.headers), DynamicProperty.String("Content-Type"), b.Const("application/json"));
                                return b.MakeStateWithEffects(
                                    b.ShowPanel(clientModel),
                                    b.MakeEffect(
                                        b.Def((SyntaxBuilder b, Var<HyperType.Dispatcher<EditConfigurationPage>> dispatcher) =>
                                            b.CallExternal("fetch", "DownloadFile",
                                                b.Const("/api/windows-scripts"),
                                                fetchOptions,
                                                b.Def((SyntaxBuilder b, Var<object> file) =>
                                                {
                                                    b.CallExternal("fetch", "DownloadBlob", file, b.Concat(b.Get(clientModel, x => x.Configuration.Name), b.Const(".zip")));
                                                }),
                                                b.Def((SyntaxBuilder b, Var<ApiError> not_used) => { b.Log("Error"); b.Log(not_used); })))));
                            });

                            return b.Switch(selectedValue,
                                b => showCurrentJson,
                                ("download-windows-scripts", b => downloadWindowsScripts));
                        }));
                    },
                    b.SlMenuItem(
                        b =>
                        {
                            b.SetValue("show-current-json");                   
                        },
                        b.SlIcon(
                            b =>
                            {
                                b.SetSlot(SlMenuItem.Slot.Prefix);
                                b.SetName(b.Const("clipboard-check"));
                            }),
                        b.T("Local configuration")),
                    b.SlMenuItem(
                        b =>
                        {
                            b.SetValue("download-windows-scripts");
                        },
                        b.SlIcon(
                            b =>
                            {
                                b.SetSlot(SlMenuItem.Slot.Prefix);
                                b.SetName("box-arrow-down");
                            }),
                        b.T("Merge tools"))));

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
                var saveInput = b.NewObj<SaveConfigurationInput>();
                b.Set(saveInput, x => x.InfrastructureConfiguration, b.Get(model, x => x.Configuration));
                b.Set(saveInput, x => x.OriginalJson, b.Get(model, x => x.InitialConfiguration));

                return b.MakeStateWithEffects(
                    b.ShowPanel(model),
                    b.MakeEffect(
                        b.Def(
                            b.Request(
                                Frontend.SaveConfiguration,
                                saveInput,
                                b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<SaveConfigurationResponse> response) =>
                                {
                                    b.Set(page, x => x.SaveConfigurationResponse, response);
                                    return b.If(b.Get(response, x => x.ConflictMessages.Any()), b =>
                                    {
                                        b.ShowDialog(b.Const(IdSaveConflictPopup));
                                        return b.Clone(page);
                                    },
                                    b =>
                                    {
                                        b.Set(page, x => x.InitialConfiguration, b.Serialize(b.Get(page, x => x.Configuration)));
                                        b.HideDialog(b.Const(IdSaveConflictPopup));
                                        return b.Clone(page);
                                    });
                                })))));
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
            b.Add(
                container,
                b.Tabs(
                    b =>
                    {
                        b.AddTab(
                            "Configuration",
                            b => b.Call(EditConfiguration.TabConfiguration, clientModel));

                        b.AddTab(
                            "Services",
                            b => b.Call(EditConfiguration.TabServices, clientModel).As<IVNode>());

                        b.AddTab(
                            "Applications",
                            b => b.Call(EditConfiguration.TabApplications, clientModel).As<IVNode>());

                        b.AddTab(
                            "Variables",
                            b => b.Call(EditConfiguration.TabVariables, clientModel).As<IVNode>());

                        b.AddToolbarCommand(b => menuDropdown);
                        b.AddToolbarCommand(b => deployLink.As<IVNode>());
                        b.AddToolbarCommand(b => saveButton.As<IVNode>());

                    }).As<HyperNode>());

            b.Add(container, SaveConflictPopup(b, clientModel).As<HyperNode>());
            b.Add(container, MergeFailedPopup(b, clientModel).As<HyperNode>());
            b.Add(container, MergeSuccessPopup(b, clientModel).As<HyperNode>());
            b.Add(container, CurrentConfigurationJsonPopup(b, clientModel).As<HyperNode>());

            return container;
        }

        private static Var<bool> HasChanges(SyntaxBuilder b, Var<EditConfigurationPage> model)
        {
            var current = b.Serialize(b.Get(model, x => x.Configuration));
            var hasChanges = b.Not(b.AreEqual(current, b.Get(model, x => x.InitialConfiguration)));
            return hasChanges;
        }

        public static Var<IVNode> MessagesList(this LayoutBuilder b, Var<List<string>> messages)
        {
            return b.H(
                "div",
                (b, props) =>
                {
                    b.AddClass(props, "flex flex-col gap-2 text-sm");
                },
                b.Map(messages, (b, message) => b.H("span", (b, props) => { }, b.T(message))));
        }

        public static Var<IVNode> CurrentConfigurationJsonPopup(
            LayoutBuilder b,
            Var<EditConfigurationPage> model)
        {
            var okButton = (LayoutBuilder b) =>
            b.H(
                "button",
                (b, props) =>
                {
                    b.AddPrimaryButtonStyle(props);
                    b.OnClickAction(props, b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> model) =>
                    {
                        b.HideDialog(b.Const(IdCurrentJsonPopup));
                        return b.Clone(model);
                    }));
                },
                b.T("OK"));

            var copyButton = (LayoutBuilder b) =>
            {
                return b.SlNode(
                    "sl-copy-button",
                    (b, props) =>
                    {
                        b.SetDynamic(props, DynamicProperty.String("copy-label"), b.Const("Copy configuration JSON"));
                        b.SetDynamic(props, DynamicProperty.String("value"), b.Get(model, x => x.CurrentConfigurationSimplifiedJson));
                    });
            };

            return b.SlNode(
                "sl-dialog",
                (b, props) =>
                {
                    b.SetDynamic(props, Html.id, b.Const(IdCurrentJsonPopup));
                },
                b.DialogHeader("Your configuration file", Metapsi.Heroicons.Outline.Clipboard, "text-sky-500"),
                b.SlNode(
                    "sl-textarea",
                    (b, props) =>
                    {
                        b.SetDynamic(props, DynamicProperty.Bool("readonly"), b.Const(true));
                        b.SetDynamic(props, DynamicProperty.Int("rows"), b.Const(10));
                        b.SetDynamic(props, DynamicProperty.String("value"), b.Get(model, x => x.CurrentConfigurationSimplifiedJson));
                    }),
                b.DialogFooter(
                    "You can use this JSON to upload configuration by hand",
                    b.Call(copyButton),
                    b.Call(okButton)));
        }

        public static Var<IVNode> SaveConflictPopup(
            LayoutBuilder b,
            Var<EditConfigurationPage> model)
        {
            var mergeButton = (LayoutBuilder b) =>
            b.H(
                "button",
                (b, props) =>
                {
                    b.AddPrimaryButtonStyle(props);
                    b.OnClickAction(props, b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> model) =>
                    {
                        var mergeConfigurationInput = b.NewObj<MergeConfigurationInput>();
                        b.Set(mergeConfigurationInput, x => x.SourceConfigurationJson, b.Get(model, x => x.InitialConfiguration));
                        b.Set(mergeConfigurationInput, x => x.EditedConfiguration, b.Get(model, x => x.Configuration));

                        return b.MakeStateWithEffects(
                            b.ShowPanel(model),
                            b.MakeEffect(
                                b.Def(
                                    b.Request(
                                        Frontend.MergeConfiguration,
                                        mergeConfigurationInput,
                                        b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<MergeConfigurationResponse> response) =>
                                        {
                                            b.Set(page, x => x.MergeConfigurationResponse, response);
                                            b.HideDialog(b.Const(IdSaveConflictPopup));
                                            return b.If(b.Get(response, x => x.ConflictMessages.Any()),
                                                b =>
                                                {
                                                    b.ShowDialog(b.Const(IdMergeFailedPopup));
                                                    return b.Clone(page);
                                                },
                                                b =>
                                                {
                                                    b.Set(page, x => x.Configuration, b.Get(response, x => x.Configuration));
                                                    b.Set(page, x => x.InitialConfiguration, b.Serialize(b.Get(response, x => x.SourceConfiguration)));
                                                    b.ShowDialog(b.Const(IdMergeSuccessPopup));
                                                    return b.Clone(page);
                                                });
                                        })))));
                    }));
                },
                b.T("Merge"));

            return b.SlNode(
                "sl-dialog",
                (b, props) =>
                {
                    b.SetDynamic(props, Html.id, b.Const(IdSaveConflictPopup));
                },
                b.DialogHeader("Configuration conflict", Metapsi.Heroicons.Solid.ExclamationTriangle, "text-yellow-600"),
                b.MessagesList(b.Get(model, x => x.SaveConfigurationResponse.ConflictMessages)),
                b.DialogFooter(
                    "Merging will import changes into your local configuration",
                    b.Call(mergeButton)));
        }

        public static Var<IVNode> MergeSuccessPopup(
            LayoutBuilder b,
            Var<EditConfigurationPage> model)
        {
            return
                b.SlNode(
                    "sl-dialog",
                    (b, props) =>
                    {
                        b.SetDynamic(props, Html.id, b.Const(IdMergeSuccessPopup));
                    },
                    b.DialogHeader("Merge success", Metapsi.Heroicons.Solid.CheckCircle, "text-green-600"),
                    b.MessagesList(b.Get(model, x => x.MergeConfigurationResponse.SuccessMessages)),
                    b.DialogFooter(
                        "You can save your changes now",
                        b.H(
                            "button",
                            (b, props) =>
                            {
                                b.AddPrimaryButtonStyle(props);

                                b.OnClickAction(props, b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> model) =>
                                {
                                    b.HideDialog(b.Const(IdMergeSuccessPopup));
                                    b.Set(model, x => x.SaveConfigurationResponse, b.NewObj<SaveConfigurationResponse>());
                                    b.Set(model, x => x.MergeConfigurationResponse, b.NewObj<MergeConfigurationResponse>());
                                    return b.Clone(model);
                                }));
                            },
                            b.T("OK"))));
        }


        public static Var<IVNode> MergeFailedPopup(
            LayoutBuilder b,
            Var<EditConfigurationPage> model)
        {
            return b.SlNode(
                "sl-dialog",
                (b, props) =>
                {
                    b.SetDynamic(props, Html.id, b.Const(IdMergeFailedPopup));
                },
                b.DialogHeader("Configuration cannot be saved", Metapsi.Heroicons.Solid.XCircle, "text-red-600"),
                b.MessagesList(b.Get(model, x => x.MergeConfigurationResponse.ConflictMessages)),
                b.DialogFooter(
                    "You can edit your configuration or you can copy it and merge by hand",
                    b.SlNode(
                        "sl-copy-button",
                        (b, props) =>
                        {
                            b.SetDynamic(props, DynamicProperty.String("copy-label"), b.Const("Copy configuration JSON"));
                            b.SetDynamic(props, DynamicProperty.String("value"), b.Get(model, x => x.MergeConfigurationResponse.ConfigurationJson));
                        }),
                    b.H(
                        "button",
                        (b, props) =>
                        {
                            b.AddPrimaryButtonStyle(props);
                            b.OnClickAction(props, b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> model) =>
                            {
                                b.HideDialog(b.Const(IdMergeFailedPopup));
                                return b.Clone(model);
                            }));
                        },
                        b.T("OK"))));
        }

        public static Var<IVNode> DialogHeader(this LayoutBuilder b, string label, string svg, string color)
        {
            return b.H(
                "div",
                (b, props) =>
                {
                    b.AddClass(props, "flex flex-row items-center gap-2");
                    b.SetDynamic(props, DynamicProperty.String("slot"), b.Const("label"));
                },
                b.T(label),
                b.SvgNew($"{color} w-5 h-5", svg));
        }

        public static Var<IVNode> DialogFooter(this LayoutBuilder b, string text, params Var<IVNode>[] buttons)
        {
            return
                b.H(
                    "div",
                    (b, props) =>
                    {
                        b.SetDynamic(props, DynamicProperty.String("slot"), b.Const("footer"));
                        b.AddClass(props, "flex flex-row items-center justify-between");
                    },
                    b.H(
                        "div",
                        (b, props) =>
                        {
                            b.AddClass(props, "text-xs text-gray-600 text-left");
                        },
                        b.T(text)),
                    b.H(
                        "div",
                        (b, props) =>
                        {
                            // To make the buttons smaller at end
                            b.AddClass(props, "flex flex-row gap-2 justify-end");
                        },
                        buttons));
        }
    }
}
