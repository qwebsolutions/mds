using Metapsi;
using Metapsi.Dom;
using Metapsi.Html;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Routing;
using Metapsi.Ui;

namespace MdsCommon
{
    public static class FetchExtensions
    {
        public static Var<string> GetApiUrl(this SyntaxBuilder b, ExternalOperation apiRequest)
        {
            return b.Const($"/api/{apiRequest.Name}");
        }

        public static Var<string> GetApiUrl(this SyntaxBuilder b, ExternalOperation apiRequest, Var<string> p1)
        {
            return b.Concat(b.GetApiUrl(apiRequest), b.Const("/"), p1);
        }

        public static void Alert(this SyntaxBuilder b, Var<string> message)
        {
            b.CallOnObject(b.Window(), "alert", message);
        }

        public static void Alert(this SyntaxBuilder b, Var<ClientSideException> ex)
        {
            b.Alert(b.Get(ex, x => x.message));
        }
    }
}