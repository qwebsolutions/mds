using Metapsi.Hyperapp;
using Metapsi;
using Metapsi.Syntax;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Web;

namespace MdsCommon
{
    public static partial class SignIn
    {
        public static async Task<IResult> Credentials(CommandContext commandContext, HttpContext requestData)
        {
            var payload = await requestData.Payload();
            if (!string.IsNullOrEmpty(payload))
            {
                var p = Metapsi.Serialize.FromJson<InputCredentials>(payload);
                var credentials = await commandContext.Do(MdsCommon.Api.GetAdminCredentials);

                if (p.UserName != credentials.AdminUserName || p.Password != credentials.AdminPassword)
                {
                    var queryString = requestData.Request.QueryString.Value;

                    return Page.Result(new SignInPage()
                    {
                        ErrorMessage = "User name or password not valid",
                        Credentials = new(),
                        ReturnUrl = WebServer.Url(Credentials)
                    });
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
                return Results.Redirect(HttpUtility.UrlDecode(requestData.Request.Query["ReturnUrl"]));
            }
            else
            {
                var queryString = requestData.Request.QueryString.Value;// requestData.Request.Query["ReturnUrl"].ToString();
                return Page.Result(new SignInPage());
            }
        }

    }
}