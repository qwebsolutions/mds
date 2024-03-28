using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsCommon.Controls
{
    public static partial class Panel
    {
        public enum Style
        {
            Info,
            Ok,
            Warning,
            Error
        }
    }

    public static partial class Controls
    {
        public static Var<IVNode> InfoPanel(
            this LayoutBuilder b,
            Var<Panel.Style> style,
            Func<LayoutBuilder, Var<IVNode>> header,
            Func<LayoutBuilder, Var<IVNode>> body)
        {
            string basePanelStyle = "flex flex-col rounded-md p-4 shadow-md border border-gray-100 ";
            var infoPanelStyle = b.Const(basePanelStyle + "bg-white text-gray-800");
            var errorPanelStyle = b.Const(basePanelStyle + "bg-red-500 text-white");
            var warningPanelStyle = b.Const(basePanelStyle + "bg-yellow-500 text-white");
            var okPanelStyle = b.Const(basePanelStyle + "bg-green-500 text-white");

            Var<bool> isOk = b.AreEqual(style, b.Const(Panel.Style.Ok));
            Var<bool> isError = b.AreEqual(style, b.Const(Panel.Style.Error));
            Var<bool> isWarning = b.AreEqual(style, b.Const(Panel.Style.Warning));

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass(infoPanelStyle);
                    b.If(isOk, b => b.SetClass(okPanelStyle));
                    b.If(isError, b => b.SetClass(errorPanelStyle));
                    b.If(isWarning, b => b.SetClass(warningPanelStyle));
                },
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("pb-2");
                    },
                    header(b)),
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("justify-self-end");
                    },
                    body(b)));
        }

        public static Var<IVNode> InfoPanel(
            this LayoutBuilder b,
            Panel.Style style,
            string header,
            string body,
            Var<string> infoLink = null)
        {
            string info = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\" fill=\"currentColor\">\r\n  <path fill-rule=\"evenodd\" d=\"M2.25 12c0-5.385 4.365-9.75 9.75-9.75s9.75 4.365 9.75 9.75-4.365 9.75-9.75 9.75S2.25 17.385 2.25 12zm8.706-1.442c1.146-.573 2.437.463 2.126 1.706l-.709 2.836.042-.02a.75.75 0 01.67 1.34l-.04.022c-1.147.573-2.438-.463-2.127-1.706l.71-2.836-.042.02a.75.75 0 11-.671-1.34l.041-.022zM12 9a.75.75 0 100-1.5.75.75 0 000 1.5z\" clip-rule=\"evenodd\" />\r\n</svg>\r\n";

            return b.InfoPanel(
                b.Const(style),
                b =>
                {
                    if (infoLink == null)
                    {
                        return b.HtmlSpanText(b => b.AddClass("font-bold"), header);
                    }
                    else
                    {
                        return b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("flex flex-row");
                            },
                            b.HtmlSpanText(b => b.AddClass("font-bold w-full"), header),
                            b.HtmlA(
                                b =>
                                {
                                    b.SetClass("flex flex-row justify-end text-gray-100 w-6 h-6 hover:text-white");
                                    b.SetHref(infoLink);
                                },
                                b.Svg(info, "w-full h-full")));
                    }
                },
                b => b.TextSpan(b.Const(body)));
        }

        public static Var<IVNode> PanelsContainer(this LayoutBuilder b, int columns, IEnumerable<Var<IVNode>> panels)
        {
            // tw import
            // lg:grid-cols-4
            return b.HtmlDiv(
                b =>
                {
                    b.SetClass($"grid grid-cols-1 lg:grid-cols-{columns} gap-4");
                },
                panels.ToArray());
        }
    }
}

