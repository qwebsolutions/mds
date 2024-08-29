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
            StaticFiles.Add(typeof(HeaderRenderer).Assembly, "Poppins-Regular.ttf");

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
                    b.TextSpan(mainOperation)),
                b.Optional(
                    b.HasValue(mainEntity),
                    b => b.HtmlSpan(
                        b =>
                        {
                            b.SetClass("font-medium text-gray-500 px-2");
                        },
                        b.TextSpan(mainEntity))));

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
                                b.TextSpan(userName)),
                            b.Optional(
                                b.HasValue(b.Get(props, x => x.User.AuthType)),
                                b =>
                                {
                                    return b.HtmlA(
                                        b =>
                                        {
                                            b.AddClass("font-thin text-xs");
                                            b.UnderlineBlue();
                                            b.SetHref(b.Concat(b.RootPath(), b.Const("/signout")));
                                        },
                                        b.TextSpan("Sign out"));
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
                        b.TextSpan("Sign in"));
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

        public static Var<MdsCommon.Header.Props> GetHeaderProps(
            this LayoutBuilder b,
            Var<string> operation,
            Var<string> entity,
            Var<User> user)
        {
            var headerTitle = b.NewObj<Header.Title>();
            b.Set(headerTitle, x => x.Operation, operation);
            b.Set(headerTitle, x => x.Entity, entity);

            var headerProps = b.NewObj<Header.Props>();
            b.Set(headerProps, x => x.Main, headerTitle);
            b.Set(headerProps, x => x.User, user);
            return headerProps;
        }

        public static Var<bool> IsSignedIn(this SyntaxBuilder b, Var<MdsCommon.User> user)
        {
            return b.If(
                b.Not(b.HasObject(user)),
                b => b.Const(false),
                b => b.HasValue(b.Get(user, x => x.AuthType)));
        }
    }
}