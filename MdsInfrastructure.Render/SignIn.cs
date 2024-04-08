using MdsCommon;
using MdsCommon.Controls;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Dom;
using Metapsi;
using Metapsi.Ui;

namespace MdsInfrastructure.Render
{
    public static class SignIn
    {
        public class Credentials : HtmlPage<SignInPage>
        {
            public override void FillHtml(SignInPage dataModel, DocumentTag document)
            {
                document.Body.AddChild(new HyperAppNode<SignInPage>()
                {
                    Model = dataModel,
                    Render = MdsCommon.SignIn.Render
                });
            }
        }
    }
}