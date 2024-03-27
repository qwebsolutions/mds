using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Collections.Generic;
using System.Web;
using MdsCommon.Controls;
using Metapsi.Html;

namespace MdsCommon
{
    public static partial class HeaderRenderer
    {
        public static Var<IVNode> Render(this LayoutBuilder b, Var<MdsCommon.Header.Props> props)
        {
            b.AddModuleStylesheet();

            var mainOperation =  b.Get(props, x => x.Main.Operation);
            var mainEntity = b.Get(props, x => x.Main.Entity);

            var titleArea = b.HtmlDiv(
                b =>
                {

                },
                b.HtmlSpan(
                    b =>
                    {
                        b.AddClass("font-bold text-gray-800 px-2");
                    },
                    b.T(mainOperation)),
                b.Optional(
                    b.HasValue(mainEntity),
                    b => b.HtmlSpan(
                        b =>
                        {
                            b.SetClass("font-medium text-gray-500 px-2");
                        },
                        b.T(mainEntity))));

            var signInArea = b.Optional(
                b.Get(props, x => x.UseSignIn),
                b =>
                {
                    b.Log("Header props", props);
                    var userName = b.Get(props, x => x.User.Name);
                    return b.If(b.HasValue(userName), b =>
                    {
                        return b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("flex flex-col items-end");
                            },
                            b.HtmlSpan(
                                b =>
                                {
                                    b.SetClass("font-semibold text-sm text-gray-800");
                                },
                                b.T(userName)),
                            b.Optional(
                                b.Get(props, x => x.User.AuthType == Metapsi.Ui.AuthType.Oidc),
                                b =>
                                {
                                    return b.HtmlA(
                                        b =>
                                        {
                                            b.AddClass("font-thin text-xs");
                                            b.UnderlineBlue();
                                            b.SetHref(b.Concat(b.RootPath(), b.Const("/signout")));
                                        },
                                        b.T("Sign out"));
                                }));

                    },
                    b =>
                    {
                        return b.HtmlA(b =>
                        {
                            b.AddClass("font-thin text-sm");
                            b.UnderlineBlue();
                            b.SetHref(b.Concat(b.RootPath(), b.Const("/signin-redirect")));
                        },
                        b.T("Sign in"));
                    });
                });

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-row justify-between items-center");
                },
                titleArea,
                signInArea);
        }
    }
}