using Metapsi.Hyperapp;
using Metapsi.Html;
using Metapsi.Syntax;
using System.Collections.Generic;

public static class StyleExtensions
{
    public static void UnderlineBlue(this PropsBuilder<HtmlA> b)
    {
        b.AddClass("underline text-sky-500");
    }

    public static Var<IVNode> StyledDiv(this LayoutBuilder b, string classes, params Var<IVNode>[] children)
    {
        return b.HtmlDiv(b => b.SetClass(classes), children);
    }

    public static Var<IVNode> StyledDiv(this LayoutBuilder b, string classes, Var<List<IVNode>> children)
    {
        return b.HtmlDiv(b => b.SetClass(classes), children);
    }

    public static Var<IVNode> StyledSpan(this LayoutBuilder b, string classes, params Var<IVNode>[] children)
    {
        return b.HtmlSpan(b => b.SetClass(classes), children);
    }

    public static Var<IVNode> Bold(this LayoutBuilder b, string text)
    {
        return b.HtmlSpanText(b => b.SetClass("font-bold"), text);
    }

    public static Var<IVNode> TextSpan(this LayoutBuilder b, Var<string> text)
    {
        return b.StyledSpan("", b.Text(text));
    }

    public static Var<IVNode> TextSpan(this LayoutBuilder b, string text)
    {
        return b.TextSpan(b.Const(text));
    }
}