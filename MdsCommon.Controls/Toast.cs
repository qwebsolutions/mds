using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Shoelace;
using Metapsi.Syntax;

namespace MdsCommon.Controls
{

    public static partial class Controls
    {
        public const string IdDeploymentToastsContainer = nameof(IdDeploymentToastsContainer);
        public const string IdDeploymentStartedToast = nameof(IdDeploymentStartedToast);
        public const string IdDeploymentSuccessToast = nameof(IdDeploymentSuccessToast);
        public const string IdDeploymentFailedToast = nameof(IdDeploymentFailedToast);

        private static void HideToast(this SyntaxBuilder b, string toastId)
        {
            var element = b.GetElementById(toastId + "_clone");
            b.If(
                b.HasObject(element),
                b =>
                {
                    b.CallOnObject(element, "hide");
                });
        }

        public static void HideAllDeploymentToasts(this SyntaxBuilder b)
        {
            b.HideToast(IdDeploymentStartedToast);
            b.HideToast(IdDeploymentSuccessToast);
            b.HideToast(IdDeploymentFailedToast);
        }

        public static void ShowDeploymentToast(this SyntaxBuilder b, string toastId)
        {
            b.HideAllDeploymentToasts();
            var template = b.GetElementById(toastId);
            var templateContent = b.GetProperty<DomElement>(template, "content");
            var templateChild = b.GetProperty<DomElement>(templateContent, "firstElementChild");
            var alertClone = b.CallOnObject<DomElement>(templateChild, "cloneNode", b.Const(true));
            var newId = b.Const(toastId + "_clone");
            b.SetProperty(alertClone, b.Const("id"), newId);
            var container = b.GetElementById(IdDeploymentToastsContainer);
            b.AppendChild(container, alertClone);
            b.CallOnObject(alertClone, "toast");
        }

        public static IHtmlNode DeploymentStartedToast(this HtmlBuilder b)
        {
            return b.SlAlert(
                b =>
                {
                    b.SetClosable();
                    b.SetId(IdDeploymentStartedToast);
                },
                b.Text("Deployment in progress"));
        }

        public static IHtmlNode DeploymentSuccessToast(this HtmlBuilder b)
        {
            return b.SlAlert(
                b =>
                {
                    b.SetClosable();
                    b.SetId(IdDeploymentSuccessToast);
                    b.SetVariantSuccess();
                    b.SetDuration("5000");
                },
                b.Text("Deployment complete!"));
        }

        public static IHtmlNode DeploymentFailedToast(this HtmlBuilder b)
        {
            return b.SlAlert(
                b =>
                {
                    b.SetClosable(true);
                    b.SetId(IdDeploymentFailedToast);
                    b.SetVariantDanger();
                    b.SetClosable();
                    b.SetDuration("5000");
                },
                b.Text("Deployment error!"));
        }

        public static IHtmlNode DeploymentToasts(this HtmlBuilder b)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetId(IdDeploymentToastsContainer);
                },
                b.HtmlStyle(b.Text(".sl-toast-stack { left: 50%; transform: translateX(-50%); }")),
                b.HtmlTemplate(
                    b =>
                    {
                        b.SetId(IdDeploymentStartedToast);
                    },
                    b.DeploymentStartedToast()),
                b.HtmlTemplate(
                    b =>
                    {
                        b.SetId(IdDeploymentSuccessToast);
                    },
                    b.DeploymentSuccessToast()),
                b.HtmlTemplate(
                    b =>
                    {
                        b.SetId(IdDeploymentFailedToast);
                    },
                    b.DeploymentFailedToast()));
        }
    }
}

