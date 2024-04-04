using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Collections.Generic;
using System.Web;
using MdsCommon.Controls;

namespace MdsCommon
{
    public static partial class HeaderRenderer
    {
        public static Var<HyperNode> Render(this LayoutBuilder b, Var<MdsCommon.Header.Props> props)
        {
            b.AddModuleStylesheet();

            var container = b.Div("flex flex-row justify-between items-center");
            var mainOperation =  b.Get(props, x => x.Main.Operation);
            var mainEntity = b.Get(props, x => x.Main.Entity);

            var titleArea = b.Add(container, b.Div());

            var mainTitle = b.Add(titleArea, b.Text(mainOperation));
            b.AddClass(mainTitle, "font-bold text-gray-800 px-2");

            b.If(b.HasValue(mainEntity), b =>
            {
                var entityLabel = b.Add(titleArea, b.Text(mainEntity));
                b.AddClass(entityLabel, "font-medium text-gray-500 px-2");
            });

            b.If(
                b.Get(props, x => x.UseSignIn),
                b =>
                {
                    var userName = b.Get(props, x => x.User.Name);
                    b.If(b.HasValue(userName), b =>
                    {
                        var rightDiv = b.Add(container, b.Div("flex flex-col items-end"));
                        var name = b.Add(rightDiv, b.Text(userName));
                        b.AddClass(name, "font-semibold text-sm text-gray-800");

                        b.If(b.Get(props, x => x.User.AuthType == Metapsi.Ui.AuthType.Oidc), b =>
                        {
                            var signout = b.Add(rightDiv, b.Link(b.Const("Sign out"), b.Concat(b.RootPath(), b.Const("/signout"))));
                            b.AddClass(signout, "font-thin text-xs");
                        });
                    },
                    b =>
                    {
                        var signin = b.Add(container, b.Link(b.Const("Sign in"), b.Concat(b.RootPath(), b.Const("/signin-redirect"))));
                        b.AddClass(signin, "font-thin text-sm");
                    });
                });

            return container;
        }

        public static Var<MdsCommon.Header.Props> GetHeaderProps(
            this LayoutBuilder b,
            Var<string> operation,
            Var<string> entity,
            Var<Metapsi.Ui.User> user)
        {
            var headerTitle = b.NewObj<Header.Title>();
            b.Set(headerTitle, x => x.Operation, operation);
            b.Set(headerTitle, x => x.Entity, entity);

            var headerProps = b.NewObj<Header.Props>();
            b.Set(headerProps, x => x.Main, headerTitle);
            b.Set(headerProps, x => x.User, user);
            return headerProps;
        }
    }
}