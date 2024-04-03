using MdsCommon;
using Metapsi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace MdsInfrastructure.Flow;

public static class SignIn
{
    public class Form : Metapsi.Http.Get<Routes.SignIn>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
        {
            var returnUrl = HttpUtility.UrlDecode(httpContext.Request.Query["ReturnUrl"]);

            return Page.Result(new SignInPage() { ReturnUrl = returnUrl });
        }
    }

    public class Credentials : Metapsi.Http.Post<Routes.SignIn.Credentials, MdsCommon.InputCredentials>
    {
        public override async Task<IResult> OnPost(CommandContext commandContext, HttpContext httpContext, InputCredentials p)
        {
            var returnUrl = HttpUtility.UrlDecode(httpContext.Request.Query["ReturnUrl"]);

            var credentials = await commandContext.Do(MdsCommon.Api.GetAdminCredentials);
            
            if (p.UserName != credentials.AdminUserName || p.Password != credentials.AdminPassword)
            {
                var queryString = httpContext.Request.QueryString.Value;

                return Page.Result(new SignInPage()
                {
                    ErrorMessage = "User name or password not valid",
                    Credentials = p,
                    ReturnUrl = returnUrl
                });
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, p.UserName),
                    new Claim(ClaimTypes.Name, p.UserName)
                };

            //var identity = new ClaimsIdentity(claims, "LDAP");
            var identity = new ClaimsIdentity(claims, "OIDC");

            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            await httpContext.SignInAsync(principal);
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = "/";
            return Results.Redirect(returnUrl);
        }
    }
}