using Metapsi.Hyperapp;
using Metapsi;
using Metapsi.Syntax;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using static Metapsi.WebServer;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Web;

namespace MdsCommon
{
    public static partial class SignIn
    {
        public class InputCredentials
        {
            public string UserName { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class PageModel
        {
            public string ErrorMessage { get; set; }
            public string LoginMessage { get; set; } = "Sign in to your account";

            public InputCredentials Credentials { get; set; } = new();
        }

        public static async Task<IResponse> Credentials(CommandContext commandContext, HttpContext requestData)
        {
            var payload = await requestData.Payload();
            if (!string.IsNullOrEmpty(payload))
            {
                var p = Metapsi.Serialize.FromJson<InputCredentials>(payload);
                var credentials = await commandContext.Do(MdsCommon.Api.GetAdminCredentials);

                if (p.UserName != credentials.AdminUserName || p.Password != credentials.AdminPassword)
                {
                    var queryString = requestData.Request.QueryString.Value;

                    //= "User name or password not valid";
                    return Page.Response(new PageModel()
                    {
                        ErrorMessage = "User name or password not valid",
                        Credentials = new()
                    },
                    (b, clientModel) => b.Render(clientModel, b.Const(queryString)),
                    string.Empty);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, p.UserName),
                    new Claim(ClaimTypes.Name, p.UserName)
                };

                //var identity = new ClaimsIdentity(claims, "LDAP");
                var identity = new ClaimsIdentity(claims, "OIDC");

                var plm = new System.Security.Claims.ClaimsPrincipal(identity);
                await requestData.SignInAsync(plm);
                return Page.Redirect(HttpUtility.UrlDecode(requestData.Request.Query["ReturnUrl"]));
            }
            else
            {
                var queryString = requestData.Request.QueryString.Value;// requestData.Request.Query["ReturnUrl"].ToString();
                return Page.Response(new PageModel(), (b, clientModel) => b.Render(clientModel, b.Const(queryString)), string.Empty);
            }
        }

        public static Var<HyperNode> Render(
            this BlockBuilder b, 
            Var<PageModel> clientModel,
            Var<string> returnUrl)
        {
            b.AddStylesheet("/static/tw_infra.css");
            b.AddStylesheet("tw_framework.css");

            var page = b.Div("flex flex-col justify-center items-center w-full h-screen bg-gray-100");
            var center = b.Add(page, b.Div("flex flex-row justify-center items-center"));
            var container = b.Add(center, b.Div("flex flex-col items-center gap-4 shadow p-8 rounded bg-white"));
            b.If(b.HasValue(b.Get(clientModel, x => x.ErrorMessage)), b =>
            {
                var error = b.Add(container, b.Text(b.Get(clientModel, x => x.ErrorMessage)));
                b.AddClass(error, "text-red-600");
            });
            var loginMessage = b.Add(container, b.Text(b.Get(clientModel, x=>x.LoginMessage)));
            b.AddClass(loginMessage, "p-8 text-gray-800");
            b.Add(container, b.BoundInput(clientModel, x => x.Credentials, x => x.UserName, "User name"));
            var password = b.Add(container, b.BoundInput(clientModel, x => x.Credentials, x => x.Password, "Password"));
            b.SetAttr(password, Html.type, "password");
            var url = b.Concat(b.Url(Credentials), returnUrl);
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

            b.SetOnEnter(password, b.MakeAction((BlockBuilder b, Var<PageModel> state) =>
            {
                return b.MakeStateWithEffects(state, b.MakeEffect(b.MakeEffecter<PageModel>((b, dispatcher) =>
                {
                    var form = b.GetElementById(b.Const("credentials-form"));
                    b.Log("form", form);
                    b.CallExternal("form", "Submit", form);
                })));
            }));

            return page;
        }
    }
}