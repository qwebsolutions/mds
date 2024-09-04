using MdsCommon;
using Metapsi.Html;

namespace MdsInfrastructure.Render
{
    public static class SignIn
    {
        public class Credentials
        {
            public static void Render(HtmlBuilder b, SignInPage dataModel)
            {
                b.BodyAppend(b.Hyperapp(dataModel, MdsCommon.SignIn.Render));
            }
        }
    }
}