﻿using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;

namespace MdsCommon.Controls
{
    public static class CommandButton
    {
        public class Props
        {
            public string Label { get; set; }
            //public Command OnClick { get; set; }
            public bool Enabled { get; set; } = true;
            public string SvgIcon { get; set; }
            public Button.Style Style { get; set; } = Button.Style.Primary;
        }

        public class Props<TState> : Props
        {
            public HyperType.Action<TState> OnClick { get; set; }
        }

        public class Props<TState, TPayload> : Props
        {
            public HyperType.Action<TState, TPayload> OnClick { get; set; }
            public TPayload Payload { get; set; }
        }

        public static Var<IVNode> Render<TState, TPayload>(this LayoutBuilder b, Var<Props<TState, TPayload>> props)
        {
            var button = b.RenderBase(props.As<Props>(),
                b =>
                {
                    b.OnClickAction(b.MakeActionDescriptor(
                        b.Get(props, x => x.OnClick),
                        b.Get(props, x => x.Payload)));
                });
            return button;
        }

        public static Var<IVNode> Render<TState>(this LayoutBuilder b, Var<Props<TState>> props)
        {
            var button = b.RenderBase(props.As<Props>(),
                b =>
                {
                    b.OnClickAction(b.Get(props, x => x.OnClick));
                });
            return button;
        }

        private static Var<IVNode> RenderBase(this LayoutBuilder b, Var<Props> props, Action<PropsBuilder<HtmlButton>> setAction)
        {
            var button = b.HtmlButton(
                b =>
                {
                    b.AddClass("rounded");
                    b.If(
                        b.HasValue(b.Get(props, x => x.Label)),
                        b =>
                        {
                            b.AddClass("py-2 px-4 shadow");
                        },
                        b =>
                        {
                            b.AddClass("p-1 shadow");
                        });

                    b.If(b.Get(props, x => x.Enabled), b =>
                    {
                        var bgClass = b.Switch(
                            b.Get(props, x => x.Style),
                            b => b.Const(""),
                            (Button.Style.Primary, b => b.Const("bg-sky-500")),
                            (Button.Style.Danger, b => b.Const("bg-red-500")),
                            (Button.Style.Light, b => b.Const("bg-white")));

                        b.If(b.HasValue(bgClass), b =>
                        {
                            b.AddClass(bgClass);
                        });
                    },
                    b =>
                    {
                        // if disabled
                        b.SetDisabled();
                        b.AddClass("bg-gray-300");
                    });
                    setAction(b);
                },
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-row space-x-2 items-center");
                    },
                    b.Optional(
                        b.HasValue(b.Get(props, x => x.SvgIcon)),
                        b => b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("h-5 w-5");
                                b.SetInnerHtml(b.Get(props, x => x.SvgIcon));
                            })),
                    b.Optional(b.HasValue(b.Get(props, x => x.Label)), b =>
                    {
                        return b.TextSpan(b.Get(props, x => x.Label));
                    })));

            return button;
        }

        public static void AddButtonStyle(this PropsBuilder<HtmlButton> b)
        {
            b.AddClass("rounded flex flex-row items-center py-2 px-4 shadow text-white");
        }

        public static void AddPrimaryButtonStyle(this PropsBuilder<HtmlButton> b)
        {
            b.AddButtonStyle();
            b.AddClass("bg-sky-500");
        }
    }

    public static partial class Controls
    {
        public static Var<IVNode> CommandButton<TState>(this LayoutBuilder b, Var<MdsCommon.Controls.CommandButton.Props<TState>> props)
        {
            return b.Call(MdsCommon.Controls.CommandButton.Render, props);
        }

        public static Var<IVNode> CommandButton<TState>(this LayoutBuilder b, Action<PropsBuilder<CommandButton.Props<TState>>> updateDefaults)
        {
            return b.CommandButton(b.NewObj(updateDefaults));
        }

        public static Var<IVNode> CommandButton<TState, TPayload>(this LayoutBuilder b, Var<MdsCommon.Controls.CommandButton.Props<TState, TPayload>> props)
        {
            return b.Call(MdsCommon.Controls.CommandButton.Render, props);
        }

        public static Var<IVNode> CommandButton<TState, TPayload>(this LayoutBuilder b, Action<PropsBuilder<CommandButton.Props<TState, TPayload>>> updateDefaults)
        {
            return b.CommandButton(b.NewObj(updateDefaults));
        }
    }
}
