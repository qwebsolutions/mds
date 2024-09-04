using Metapsi.Syntax;
using System.Collections.Generic;
using System;
using Metapsi.Hyperapp;
using Metapsi.Html;

namespace MdsCommon.Controls
{
    public static class Button
    {
        public enum Style
        {
            None,
            Primary,
            Light,
            Danger
        }
    }

    public static class SubmitButton
    {
        public class Props<TPayload>
        {
            public string Label { get; set; }
            public string Href { get; set; }
            public bool Enabled { get; set; } = true;
            public TPayload Payload { get; set; }
            public Button.Style Style { get; set; } = Button.Style.Primary;
            public string SvgIcon { get; set; }
            public string ButtonClass { get; set; } = "button-button";
            public string LabelClass { get; set; } = "button-label";
        }

        public static Var<IVNode> Render<TPayload>(this LayoutBuilder b, Var<Props<TPayload>> props)
        {
            var enabled = b.Get(props, x => x.Enabled);

            return b.HtmlForm(
                b =>
                {
                    b.If(enabled,
                        b =>
                        {
                            b.SetAttribute("action", b.Get(props, command => command.Href));
                            b.SetAttribute("method", "POST");
                        });
                },
                b.HtmlButton(
                    b =>
                    {
                        b.SetClass(b.Get(props, x => x.ButtonClass));

                        b.If(b.HasValue(b.Get(props, x => x.Label)),
                            b =>
                            {
                                b.AddClass(b.Get(props, x => x.LabelClass));
                            },
                            b =>
                            {
                                b.AddClass("p-1 shadow");
                            });

                        b.If(enabled,
                            b =>
                            {
                                b.SetAttribute("type", "submit");
                                var bgClass = b.Switch(
                                    b.Get(props, x => x.Style),
                                    b => b.Const(""),
                                    (Button.Style.Primary, b => b.Const("button-primary")),
                                    (Button.Style.Danger, b => b.Const("button-danger")),
                                    (Button.Style.Light, b => b.Const("button-light")));

                                b.If(b.HasValue(bgClass), b =>
                                {
                                    b.AddClass(bgClass);
                                });
                            },
                            b =>
                            {
                                b.SetDisabled();
                                b.AddClass("bg-gray-300");
                            });
                    },
                    b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-row space-x-2 items-center");
                        },
                        b.Optional(
                            b.HasValue(b.Get(props, x => x.SvgIcon)),
                            b =>
                            {
                                return b.HtmlDiv(
                                    b =>
                                    {
                                        b.SetClass("h-5 w-5");
                                        b.SetInnerHtml(b.Get(props, x => x.SvgIcon));
                                    });
                            }),
                        b.Optional(
                            b.HasValue(b.Get(props, x => x.Label)),
                            b =>
                            {
                                return b.TextSpan(b.Get(props, x => x.Label));
                            })
                        )),
                b.Optional(
                    enabled,
                    b =>
                    {
                        return b.HiddenPayload(b.Get(props, x => x.Payload));
                    })
                );
        }
    }

    public static partial class Controls
    {
        public static Var<IVNode> HiddenPayload<TPayload>(this LayoutBuilder b, Var<TPayload> payload)
        {
            return b.HtmlInput(
                b =>
                {
                    b.SetAttribute("name", "payload");
                    b.SetId("payload");
                    b.SetAttribute("type", "hidden");
                    b.If(
                        b.HasObject(payload),
                        b =>
                        {
                            b.SetAttribute("value", b.Serialize(payload));
                        });
                });
        }

        public static Var<IVNode> NavigateButton(this LayoutBuilder b, Var<MdsCommon.Controls.NavigateButton.Props> props)
        {
            return b.Call(MdsCommon.Controls.NavigateButton.Render, props);
        }

        public static Var<IVNode> NavigateButton(this LayoutBuilder b, Action<PropsBuilder<NavigateButton.Props>> updateDefaults)
        {
            return b.FromDefault(MdsCommon.Controls.NavigateButton.Render, updateDefaults);
        }

        public static Var<IVNode> SubmitButton<TPayload>(this LayoutBuilder b, Var<MdsCommon.Controls.SubmitButton.Props<TPayload>> props)
        {
            return b.Call(MdsCommon.Controls.SubmitButton.Render, props);
        }

        public static Var<IVNode> SubmitButton<TPayload>(this LayoutBuilder b, Action<PropsBuilder<SubmitButton.Props<TPayload>>> updateDefaults)
        {
            return b.SubmitButton(b.NewObj(updateDefaults));
        }

        public static Var<IVNode> FromDefault<TProps>(this LayoutBuilder b, Func<LayoutBuilder, Var<TProps>, Var<IVNode>> control, Action<PropsBuilder<TProps>> updateDefaults)
            where TProps : new()
        {
            var modifiedProps = b.NewObj<TProps>(updateDefaults);
            return b.Call(control, modifiedProps);
        }

        public static Var<IVNode> FromProps<TProps>(this LayoutBuilder b, Func<LayoutBuilder, Var<TProps>, Var<IVNode>> control, TProps props)
            where TProps : new()
        {
            return b.Call(control, b.Const(props));
        }
    }
}
