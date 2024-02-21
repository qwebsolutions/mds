using Metapsi.Syntax;
using Metapsi.Hyperapp;
using MdsCommon.Controls;
using Metapsi.Dom;

namespace MdsCommon
{
    public static class SignIn
    {
        public static Var<HyperNode> Render(
            this LayoutBuilder b,
            Var<SignInPage> clientModel)
        {
            b.AddModuleStylesheet();

            var page = b.Div("flex flex-col justify-center items-center w-full h-screen bg-gray-100");
            var center = b.Add(page, b.Div("flex flex-row justify-center items-center"));
            var container = b.Add(center, b.Div("flex flex-col items-center gap-4 shadow p-8 rounded bg-white"));
            b.If(b.HasValue(b.Get(clientModel, x => x.ErrorMessage)), b =>
            {
                var error = b.Add(container, b.Text(b.Get(clientModel, x => x.ErrorMessage)));
                b.AddClass(error, "text-red-600");
            });
            var loginMessage = b.Add(container, b.Text(b.Get(clientModel, x => x.LoginMessage)));
            b.AddClass(loginMessage, "p-8 text-gray-800");
            b.Add(container, b.BoundInput(clientModel, x => x.Credentials, x => x.UserName, "User name"));
            var password = b.Add(container, b.BoundInput(clientModel, x => x.Credentials, x => x.Password, "Password"));
            b.SetAttr(password, Html.type, "password");
            var url = b.Get(clientModel, x=>x.ReturnUrl); // TODO: Something was concatenated here, figure out why
            var buttonToRight = b.Add(container, b.Div("flex flex-row justify-end w-full pt-8"));
            var credentials = b.Get(clientModel, x => x.Credentials);
            var submit = b.Add(buttonToRight, b.SubmitButton(b.NewObj<SubmitButton.Props<InputCredentials>>(b =>
            {
                b.Set(x => x.Label, "Sign in");
                b.Set(x => x.Href, url);
                b.Set(x => x.Payload, credentials);
                b.Set(x => x.ButtonClass, "rounded text-white py-2 px-4 shadow bg-sky-500");
            })));
            b.SetAttr(submit, Html.id, "credentials-form");

            b.SetOnEnter(password, b.MakeAction((SyntaxBuilder b, Var<SignInPage> state) =>
            {
                return b.MakeStateWithEffects(state, b.MakeEffect(b.Def((SyntaxBuilder b, Var<HyperType.Dispatcher<SignInPage>> dispatcher) =>
                {
                    var form = b.GetElementById(b.Const("credentials-form"));
                    b.CallExternal("form", "Submit", form);
                })));
            }));

            return page;
        }
    }


    public static partial class Common
    {
        public static Var<HyperNode> Layout(
            this LayoutBuilder b,
            Var<HyperNode> menu,
            Var<HyperNode> header,
            Var<HyperNode> page)
        {
            b.AddModuleStylesheet();

            var rootNode = b.Node("div", "flex flex-row w-full h-screen");
            //b.AddLoadingPanel(rootNode);

            var layoutFlex = b.Add(rootNode, b.Div());
            b.Add(layoutFlex, menu);
            var rightArea = b.Add(rootNode, b.Div("w-full h-full flex flex-col"));
            var fixedHeader = b.Add(rightArea, b.Div("w-full bg-white drop-shadow px-8 py-4 z-40"));
            b.Add(fixedHeader, header);
            var pageScoller = b.Add(rightArea, b.Div("w-full h-full space-4 px-4 bg-gray-50 overflow-auto"));
            b.Add(pageScoller, b.Div("bg-gray-50 w-full h-4"));
            b.Add(pageScoller, page);
            return rootNode;
        }
    }
}
