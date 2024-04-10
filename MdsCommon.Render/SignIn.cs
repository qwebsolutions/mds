using Metapsi.Syntax;
using Metapsi.Hyperapp;
using MdsCommon.Controls;
using Metapsi.Dom;
using Metapsi.Html;
using System.ComponentModel;
using Metapsi.Shoelace;
using Metapsi;
using System;

namespace MdsCommon
{
    public static class SignIn
    {
        public static Var<IVNode> Render(
            this LayoutBuilder b,
            Var<SignInPage> clientModel)
        {
            b.AddModuleStylesheet();
            return b.HtmlDiv(
                b =>
                {
                    // page
                    b.SetClass("flex flex-col justify-center items-center w-full h-screen bg-gray-100");
                },
                b.HtmlDiv(
                    b =>
                    {
                        // center
                        b.SetClass("flex flex-row justify-center items-center");
                    },
                    b.HtmlForm(
                        b =>
                        {
                            b.SetId("credentials-form");
                            b.SetAttribute("method", "POST");
                            b.SetAttribute("action", b.Concat(b.Const("/signin/credentials"), b.Const("?ReturnUrl="), b.Get(clientModel, x => x.ReturnUrl)));
                            b.SetClass("flex flex-col items-center gap-4 shadow p-8 rounded bg-white w-96");
                        },
                        b.Optional(
                            b.HasValue(b.Get(clientModel, x => x.ErrorMessage)),
                            b =>
                            {
                                return b.HtmlSpanText(
                                    b =>
                                    {
                                        b.SetClass("text-red-600");
                                    },
                                    b.Get(clientModel, x => x.ErrorMessage));
                            }),
                        b.HtmlSpanText(
                            b =>
                            {
                                b.SetClass("p-8 text-gray-600 font-semibold");
                            },
                            b.Get(clientModel, x => x.LoginMessage)),
                        b.SignInField(
                            "User name",
                            b =>
                            {
                                b.SetAttribute("autocomplete", "username");
                                b.SetId("name");
                                b.BindTo(clientModel, x => x.Credentials, x => x.UserName);
                            }),
                        b.SignInField(
                            "Password",
                            b =>
                            {
                                b.SetType("password");
                                b.BindTo(clientModel, x => x.Credentials, x => x.Password);
                            }),
                        b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("flex flex-row justify-end w-full pt-8");
                            },
                            b.HtmlButton(
                                b =>
                                {
                                    b.SetClass("rounded text-white py-2 px-4 shadow bg-sky-500");
                                },
                                b.TextSpan("Sign in")),
                                b.HiddenPayload(b.Get(clientModel, x => x.Credentials))))
                        ));
        }

        private static Var<IVNode> SignInField(this LayoutBuilder b, string label, Action<PropsBuilder<HtmlInput>> buildInput)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col w-full gap-2");
                },
                b.HtmlSpanText(
                    b =>
                    {
                        b.SetClass("font-sm text-gray-600");
                    },
                    label),
                b.HtmlInput(b =>
                {
                    b.SetClass("h-10 w-full rounded outline-none px-4 focus:ring-4 ring-sky-200 border border-gray-400");
                    buildInput(b);
                }));
        }
    }


    public static partial class Common
    {
        public static Var<IVNode> Layout(
            this LayoutBuilder b,
            Var<IVNode> menu,
            Var<IVNode> header,
            Var<IVNode> page)
        {
            b.AddModuleStylesheet();

            var rightArea = b.HtmlDiv(
                b =>
                {
                    b.SetClass("w-full h-full flex flex-col");
                },
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("w-full bg-white drop-shadow px-8 py-4 z-40");
                    }, header),
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("w-full h-full space-4 px-4 bg-gray-50 overflow-auto");
                    },
                    b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("bg-gray-50 w-full h-4");
                        }),
                    page));

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-row w-full h-screen");
                },
                b.HtmlDiv(b => { }, menu),
                rightArea);
        }
    }
}
