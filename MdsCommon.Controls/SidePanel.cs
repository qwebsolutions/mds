using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;

namespace MdsCommon.Controls
{
    public static partial class SidePanelControl
    {
        private const string X = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\" fill=\"currentColor\">\r\n\t<path fill-rule=\"evenodd\" d=\"M5.47 5.47a.75.75 0 011.06 0L12 10.94l5.47-5.47a.75.75 0 111.06 1.06L13.06 12l5.47 5.47a.75.75 0 11-1.06 1.06L12 13.06l-5.47 5.47a.75.75 0 01-1.06-1.06L10.94 12 5.47 6.53a.75.75 0 010-1.06z\" clip-rule=\"evenodd\" />\r\n</svg>\r\n";

        private static Reference<bool> ShowSidePanelRef = new Reference<bool>() { Value = false };

        public static void ShowSidePanel(this SyntaxBuilder b)
        {
            b.SetRef(b.GlobalRef(ShowSidePanelRef), b.Const(true));
        }

        public static void HideSidePanel(this SyntaxBuilder b)
        {
            b.SetRef(b.GlobalRef(ShowSidePanelRef), b.Const(false));
        }

        public static Var<IVNode> SidePanel(
            this LayoutBuilder b,
            System.Func<LayoutBuilder, Var<IVNode>> renderContent)
        {
            var isShown = b.GlobalRef(ShowSidePanelRef);

            return b.If(
                b.GetRef(isShown),
                b =>
                {
                    var onClick = b.MakeAction((SyntaxBuilder b, Var<object> state) =>
                    {
                        b.HideSidePanel();
                        return b.Clone(state);
                    });

                    var close = b.HtmlButton(
                        b =>
                        {
                            b.SetClass("rounded text-white bg-rose-600 p-1 shadow");
                            b.OnClickAction(onClick);
                        },
                        b.Svg(X, "w-4 h-4"));

                    var verticalLayout = b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-col p-4 items-start space-y-4 h-full");
                        },
                        close,
                        b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("w-full h-full overflow-auto");
                            },
                            b.Call(renderContent)));

                    return b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("fixed top-0 right-0 bottom-0 w-1/3 bg-gray-50 h-screen transition-all duration-500 z-50");
                        },
                        verticalLayout);
                },
                b => b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("fixed top-0 right-0 bottom-0 w-0 transition-all bg-gray-50 h-screen duration-500");
                    }));
        }
    }
}

