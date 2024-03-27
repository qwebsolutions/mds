using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;

namespace MdsCommon.Controls
{
    public static partial class Controls
    {
        //public static Var<HyperNode> Image(this LayoutBuilder b, Var<string> src, string stylingClasses = null)
        //{
        //    var image = b.Node("img", stylingClasses);
        //    b.SetAttr(image, Html.src, src);
        //    return image;
        //}

        public static Var<IVNode> Link(this LayoutBuilder b, Var<string> text, Var<string> absoluteUrl)
        {
            return b.HtmlA(
                b =>
                {
                    b.SetClass("underline text-sky-500");
                    b.SetHref(absoluteUrl);
                },
                b.T(text));
        }


        //public static Var<HyperNode> Link(this LayoutBuilder b, Var<string> url, Var<HyperNode> content)
        //{
        //    var a = b.Node("a", "");
        //    var props = b.Get(a, x => x.props);
        //    b.SetDynamic(props, Html.href, url);
        //    b.Add(a, content);
        //    return a;
        //}

        //public static Var<HyperNode> Link(this LayoutBuilder b, string classes, Var<string> url, params Func<LayoutBuilder, Var<HyperNode>>[] children)
        //{
        //    var a = b.Node("a", classes, children);
        //    b.SetAttr(a, Html.href, url);
        //    return a;
        //}

        public static Var<IVNode> Link<TState>(this LayoutBuilder b,Var<string> text, Var<HyperType.Action<TState>> onClick)
        {
            return b.HtmlA(
                b =>
                {
                    b.SetClass("underline text-sky-500");
                    b.SetHref(b.Const("javascript:void(0);"));
                    b.OnClickAction(onClick);
                },
                b.T(text));
        }
    }
}

