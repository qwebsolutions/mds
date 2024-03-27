using MdsCommon;
using MdsCommon.Controls;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Dom;

namespace MdsInfrastructure.Render
{
    public static class SignIn
    {
        public class Credentials : HyperPage<SignInPage>
        {
            public override Var<IVNode> OnRender(LayoutBuilder b, Var<SignInPage> clientModel)
            {
                return MdsCommon.SignIn.Render(b, clientModel);

                //b.AddModuleStylesheet();

                //var page = b.Div("flex flex-col justify-center items-center w-full h-screen bg-gray-100");
                //var center = b.Add(page, b.Div("flex flex-row justify-center items-center"));
                //var container = b.Add(center, b.Div("flex flex-col items-center gap-4 shadow p-8 rounded bg-white"));
                //b.If(b.HasValue(b.Get(clientModel, x => x.ErrorMessage)), b =>
                //{
                //    var error = b.Add(container, b.Text(b.Get(clientModel, x => x.ErrorMessage)));
                //    b.AddClass(error, "text-red-600");
                //});
                //var loginMessage = b.Add(container, b.Text(b.Get(clientModel, x => x.LoginMessage)));
                //b.AddClass(loginMessage, "p-8 text-gray-800");
                //b.Add(container, b.BoundInput(clientModel, x => x.Credentials, x => x.UserName, "User name"));
                //var password = b.Add(container, b.BoundInput(clientModel, x => x.Credentials, x => x.Password, "Password"));
                //b.SetAttr(password, Html.type, "password");
                //var url = b.Concat(b.Url<Routes.SignIn.Credentials, MdsCommon.InputCredentials>(), b.Const("?ReturnUrl="), b.Get(clientModel, x => x.ReturnUrl));
                //var buttonToRight = b.Add(container, b.Div("flex flex-row justify-end w-full pt-8"));
                //var credentials = b.Get(clientModel, x => x.Credentials);
                //var submit = b.Add(buttonToRight, b.SubmitButton(b.NewObj<SubmitButton.Props<InputCredentials>>(b =>
                //{
                //    b.Set(x => x.Label, "Sign in");
                //    b.Set(x => x.Href, url);
                //    b.Set(x => x.Payload, credentials);
                //    b.Set(x => x.ButtonClass, "rounded text-white py-2 px-4 shadow bg-sky-500");
                //})));
                //b.SetAttr(submit, Html.id, "credentials-form");

                //b.SetOnEnter(password, b.MakeAction((SyntaxBuilder b, Var<SignInPage> state) =>
                //{
                //    return b.MakeStateWithEffects(state, b.MakeEffect(b.Def((SyntaxBuilder b, Var<HyperType.Dispatcher<SignInPage>> dispatcher) =>
                //    {
                //        var form = b.GetElementById(b.Const("credentials-form"));
                //        b.CallExternal("form", "Submit", form);
                //    })));
                //}));

                //return page.As<IVNode>();
            }
        }
    }
}