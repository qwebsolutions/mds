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
using Metapsi.Html;

namespace MdsInfrastructure.Render
{
    public static partial class EditConfiguration
    {
        public const string IdSaveValidationFailedPopup = "id-save-validation-failed";
        public const string IdCurrentJsonPopup = "id-current-json";
        public const string IdSaveConflictPopup = "id-save-conflict";
        public const string IdMergeSuccessPopup = "id-merge-success";
        public const string IdMergeFailedPopup = "if-merge-failed";

        public static Var<IVNode> MainPage(
            LayoutBuilder b,
            Var<EditConfigurationPage> clientModel)
        {
            StaticFiles.Add(typeof(HyperType).Assembly, "fetch.js");

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
                        b.OnSlSelect(b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<SlSelectEventArgs> args) =>
                        {
                            var selectedValue = b.GetProperty<string>(b.Get(args, x => x.item), "value");

                        var showCurrentJson = b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> clientModel) =>
                        {
                        return b.MakeStateWithEffects(
                                clientModel,
                                b.PostJson(
                                    b.GetApiUrl(Frontend.GetConfigurationJson),
                                    b.Get(clientModel, x => x.Configuration),
                                    b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<GetConfigurationJsonResponse> response) =>
                                            {
                                                b.Set(page, x => x.CurrentConfigurationSimplifiedJson, b.Get(response, x => x.Json));
                                                b.ShowDialog(b.Const(IdCurrentJsonPopup));
                                                return b.Clone(page);
                                            }),
                                    b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<ClientSideException> ex) =>
                                    {
                                        b.Alert(ex);
                                        return page;
                                    })));
                            });

                            var downloadWindowsScripts = b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> clientModel) =>
                            {
                                var fetchOptions = b.SetProps<FetchOptions>(
                                    b.NewObj<DynamicObject>(),
                                    b =>
                                    {
                                        b.Set(x => x.method, b.Const("POST"));
                                        b.Set(x => x.body, b.Serialize(b.Get(clientModel, x => x.Configuration)));
                                        b.SetJsonContentTypeHeaders();
                                    });

                                return b.MakeStateWithEffects(
                                    clientModel,
                                    b.MakeEffect((SyntaxBuilder b, Var<HyperType.Dispatcher> dispatch) =>
                                    {
                                        var fetch = b.Fetch(
                                            b.Const("/api/windows-scripts"),
                                            b =>
                                            {
                                                b.SetMethodPost();
                                                b.SetJsonBody(b.Get(clientModel, x => x.Configuration));
                                                b.SetJsonContentTypeHeaders();
                                            });

                                        var then1 = b.Then(fetch, b.Def((SyntaxBuilder b, Var<object> r) =>
                                        {
                                            return b.If(
                                                    b.GetProperty<bool>(r, "ok"),
                                                    b =>
                                                    {
                                                        return r.As<Promise>();
                                                    },
                                                    b =>
                                                    {
                                                        return b.CallOnObject<Promise>(b.Promise(), "reject", r);
                                                    });
                                        }));

                                        var then2 = b.Then(
                                            then1,
                                            b.Def((SyntaxBuilder b, Var<object> r) =>
                                            {
                                                return b.CallOnObject<Promise>(r, "blob");
                                            }));

                                        var onOk = b.Then(then2, b.Def((SyntaxBuilder b, Var<object> file) =>
                                        {
                                            b.CallExternal("fetch", "DownloadBlob", file, b.Concat(b.Get(clientModel, x => x.Configuration.Name), b.Const(".zip")));
                                            return b.Const(true).As<Promise>();
                                        }));
                                        b.Catch(onOk, b.Def((SyntaxBuilder b, Var<ClientSideException> err) =>
                                        {
                                            b.Log(err);
                                        }));

                                    }));
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
                        b.TextSpan("Local configuration")),
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
                        b.TextSpan("Merge tools"))));

            var deploymentReportUrl = b.Url<Routes.Deployment.ConfigurationPreview, Guid>(configId);

            var saveButton = b.HtmlButton(
                b =>
                {
                    b.SetClass("rounded text-white py-2 px-4 shadow");
                    b.If(isSaved,
                        b =>
                        {
                            b.SetDisabled();
                            b.AddClass("bg-gray-300");
                        },
                        b =>
                        {
                            b.AddClass("bg-sky-500");
                        });

                    b.OnClickAction(b.MakeAction<EditConfigurationPage>((b, model) =>
                    {
                        var saveInput = b.NewObj<SaveConfigurationInput>();
                        b.Set(saveInput, x => x.InfrastructureConfiguration, b.Get(model, x => x.Configuration));
                        b.Set(saveInput, x => x.OriginalJson, b.Get(model, x => x.InitialConfiguration));

                        return b.MakeStateWithEffects(
                            model,
                            b.PostJson(
                                b.GetApiUrl(Frontend.SaveConfiguration),
                                saveInput,
                                b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<SaveConfigurationResponse> response) =>
                                        {
                                            b.Set(page, x => x.SaveConfigurationResponse, response);

                                            return b.If(
                                                b.Get(response, x => x.SaveValidationMessages.Any()),
                                                b =>
                                                {
                                                    b.ShowDialog(b.Const(IdSaveValidationFailedPopup));
                                                    return b.Clone(page);
                                                },
                                                b => b.If(
                                                    b.Get(response, x => x.ConflictMessages.Any()), b =>
                                                    {
                                                        b.ShowDialog(b.Const(IdSaveConflictPopup));
                                                        return b.Clone(page);
                                                    },
                                                    b =>
                                                    {
                                                        b.Set(page, x => x.InitialConfiguration, b.Serialize(b.Get(page, x => x.Configuration)));
                                                        b.HideDialog(b.Const(IdSaveConflictPopup));
                                                        return b.Clone(page);
                                                    }));
                                        }),
                                b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<ClientSideException> ex) =>
                                {
                                    b.Alert(ex);
                                    return page;
                                })));
                    }));
                },
                b.TextSpan("Save"));

            var deployLink = b.HtmlA(
                b =>
                {
                    b.SetClass("rounded text-white py-2 px-4 shadow");
                    b.SetHref(deploymentReportUrl);

                    b.If(b.Not(isSaved),
                        b =>
                        {
                            b.SetAttribute("disabled");
                            b.AddClass("bg-gray-300");
                        },
                        b =>
                        {
                            b.AddClass("bg-sky-500");
                        });
                },
                b.TextSpan("Deploy"));

            return b.HtmlDiv(b =>
            {
                b.SetClass("flex flex-col w-full bg-white rounded shadow");
            },
            b.Tabs(
                b.Toolbar(
                    b =>
                    {

                    },
                    menuDropdown,
                    deployLink,
                    saveButton),
                b.TabPair(b.Const("Configuration"), b.Call(EditConfiguration.TabConfiguration, clientModel)),
                b.TabPair(b.Const("Services"), b.Call(EditConfiguration.TabServices, clientModel)),
                b.TabPair(b.Const("Applications"), b.Call(EditConfiguration.TabApplications, clientModel)),
                b.TabPair(b.Const("Variables"), b.Call(EditConfiguration.TabVariables, clientModel))),
            SaveValidationFailedPopup(b, clientModel),
            SaveConflictPopup(b, clientModel),
            MergeFailedPopup(b, clientModel),
            MergeSuccessPopup(b, clientModel),
            CurrentConfigurationJsonPopup(b, clientModel));
        }

        public static Var<TabControl.TabPair> TabPair(this LayoutBuilder b, string headerText, Var<IVNode> content)
        {
            return b.TabPair(b.Const(headerText), content);
        }

        public static Var<TabControl.TabPair> TabPair(this LayoutBuilder b, Var<string> headerText, Var<IVNode> content)
        {
            var tabPair = b.NewObj<TabControl.TabPair>();
            b.Set(tabPair, x => x.TabHeader, b.Text(headerText));
            b.Set(tabPair, x => x.TabContent, content);
            return tabPair;
        }

        private static Var<bool> HasChanges(SyntaxBuilder b, Var<EditConfigurationPage> model)
        {
            var current = b.Serialize(b.Get(model, x => x.Configuration));
            var hasChanges = b.Not(b.AreEqual(current, b.Get(model, x => x.InitialConfiguration)));
            return hasChanges;
        }

        public static Var<IVNode> MessagesList(this LayoutBuilder b, Var<List<string>> messages)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.AddClass("flex flex-col gap-2 text-sm");
                },
                b.Map(messages, (b, message) => b.HtmlSpan(b => { }, b.TextSpan(message))));
        }

        public static Var<IVNode> CurrentConfigurationJsonPopup(
            LayoutBuilder b,
            Var<EditConfigurationPage> model)
        {
            var okButton = (LayoutBuilder b) =>
            b.HtmlButton(
                b =>
                {
                    b.AddPrimaryButtonStyle();
                    b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> model) =>
                    {
                        b.HideDialog(b.Const(IdCurrentJsonPopup));
                        return b.Clone(model);
                    }));
                },
                b.TextSpan("OK"));

            var copyButton = (LayoutBuilder b) =>
            {
                return b.Optional(
                    b.GetProperty<bool>(b.Window(), b.Const("isSecureContext")),
                    b =>
                    b.SlCopyButton(
                        b =>
                        {
                            b.SetCopyLabel(b.Const("Copy configuration JSON"));
                            b.SetValue(b.Get(model, x => x.CurrentConfigurationSimplifiedJson));
                        }));
            };

            return b.SlDialog(
                b =>
                {
                    b.SetId(b.Const(IdCurrentJsonPopup));
                },
                b.DialogHeader("Your configuration file", Metapsi.Heroicons.Outline.Clipboard, "text-sky-500"),
                b.SlTextarea(
                    b =>
                    {
                        b.SetReadonly();
                        b.SetRows(10);
                        b.SetValue(b.Get(model, x => x.CurrentConfigurationSimplifiedJson));
                    }),
                b.DialogFooter(
                    "You can use this JSON to upload configuration by hand",
                    b.Call(copyButton),
                    b.Call(okButton)));
        }


        public static Var<IVNode> SaveValidationFailedPopup(
            LayoutBuilder b,
            Var<EditConfigurationPage> model)
        {
            var okButton = (LayoutBuilder b) =>
            b.HtmlButton(
                b =>
                {
                    b.AddPrimaryButtonStyle();
                    b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> model) =>
                    {
                        b.HideDialog(b.Const(IdSaveValidationFailedPopup));
                        return b.Clone(model);
                    }));
                },
                b.TextSpan("OK"));

            return b.SlDialog(
                b =>
                {
                    b.SetId(b.Const(IdSaveValidationFailedPopup));
                },
                b.DialogHeader("Cannot save", Metapsi.Heroicons.Solid.ExclamationTriangle, "text-yellow-600"),
                b.MessagesList(b.Get(model, x => x.SaveConfigurationResponse.SaveValidationMessages)),
                b.DialogFooter(
                    "",
                    b.Call(okButton)));
        }

        public static Var<IVNode> SaveConflictPopup(
            LayoutBuilder b,
            Var<EditConfigurationPage> model)
        {
            var mergeButton = (LayoutBuilder b) =>
            b.HtmlButton(
                b =>
                {
                    b.AddPrimaryButtonStyle();
                    b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> model) =>
                    {
                        var mergeConfigurationInput = b.NewObj<MergeConfigurationInput>();
                        b.Set(mergeConfigurationInput, x => x.SourceConfigurationJson, b.Get(model, x => x.InitialConfiguration));
                        b.Set(mergeConfigurationInput, x => x.EditedConfiguration, b.Get(model, x => x.Configuration));

                        return b.MakeStateWithEffects(
                            model,
                            b.PostJson(
                                b.GetApiUrl(Frontend.MergeConfiguration),
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
                                        }),
                                        b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> page, Var<ClientSideException> ex) =>
                                        {
                                            b.Alert(ex);
                                            return page;
                                        })));
                    }));
                },
                b.TextSpan("Merge"));

            return b.SlDialog(
                b =>
                {
                    b.SetId(b.Const(IdSaveConflictPopup));
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
                b.SlDialog(
                    b  =>
                    {
                        b.SetId(b.Const(IdMergeSuccessPopup));
                    },
                    b.DialogHeader("Merge success", Metapsi.Heroicons.Solid.CheckCircle, "text-green-600"),
                    b.MessagesList(b.Get(model, x => x.MergeConfigurationResponse.SuccessMessages)),
                    b.DialogFooter(
                        "You can save your changes now",
                        b.HtmlButton(
                            b =>
                            {
                                b.AddPrimaryButtonStyle();

                                b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> model) =>
                                {
                                    b.HideDialog(b.Const(IdMergeSuccessPopup));
                                    b.Set(model, x => x.SaveConfigurationResponse, b.NewObj<SaveConfigurationResponse>());
                                    b.Set(model, x => x.MergeConfigurationResponse, b.NewObj<MergeConfigurationResponse>());
                                    return b.Clone(model);
                                }));
                            },
                            b.TextSpan("OK"))));
        }


        public static Var<IVNode> MergeFailedPopup(
            LayoutBuilder b,
            Var<EditConfigurationPage> model)
        {
            return b.SlDialog(
                b =>
                {
                    b.SetId(IdMergeFailedPopup);
                },
                b.DialogHeader("Configuration cannot be saved", Metapsi.Heroicons.Solid.XCircle, "text-red-600"),
                b.MessagesList(b.Get(model, x => x.MergeConfigurationResponse.ConflictMessages)),
                b.DialogFooter(
                    "You can edit your configuration or you can copy it and merge by hand",
                    b.SlCopyButton(
                        b=>
                        {
                            b.SetCopyLabel(b.Const("Copy configuration JSON"));
                            b.SetValue(b.Get(model, x => x.MergeConfigurationResponse.ConfigurationJson));
                        }),
                    b.HtmlButton(
                        b =>
                        {
                            b.AddPrimaryButtonStyle();
                            b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<EditConfigurationPage> model) =>
                            {
                                b.HideDialog(b.Const(IdMergeFailedPopup));
                                return b.Clone(model);
                            }));
                        },
                        b.TextSpan("OK"))));
        }

        public static Var<IVNode> DialogHeader(this LayoutBuilder b, string label, string svg, string color)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.AddClass("flex flex-row items-center gap-2");
                    b.SetSlot("label");
                },
                b.TextSpan(label),
                b.Svg(svg, $"{color} w-5 h-5"));
        }

        public static Var<IVNode> DialogFooter(this LayoutBuilder b, string text, params Var<IVNode>[] buttons)
        {
            return
                b.HtmlDiv(
                    b =>
                    {
                        b.SetSlot("footer");
                        b.AddClass("flex flex-row items-center justify-between");
                    },
                    b.HtmlDiv(
                        b =>
                        {
                            b.AddClass("text-xs text-gray-600 text-left");
                        },
                        b.TextSpan(text)),
                    b.HtmlDiv(
                        b =>
                        {
                            // To make the buttons smaller at end
                            b.AddClass("flex flex-row gap-2 justify-end");
                        },
                        buttons));
        }
    }
}
