using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsCommon.Controls
{
    //public static class Toolbar
    //{
    //    internal static Var<HyperNode> Render(BlockBuilder b, params System.Func<BlockBuilder, Var<HyperNode>>[] children)
    //    {
    //        return b.Div("flex flex-row space-x-4 items-center p-4", children);
    //    }
    //}

    public static partial class Controls
    {

        public static Var<IVNode> Toolbar(this LayoutBuilder b, Action<PropsBuilder<HtmlDiv>> buildProps, params Func<LayoutBuilder, Var<IVNode>>[] children)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-row space-x-4 items-center p-4");
                    buildProps(b);
                },
                b.List(children.Select(x => x(b))));
        }

        public static Var<IVNode> Toolbar(this LayoutBuilder b, Action<PropsBuilder<HtmlDiv>> buildProps, params Var<IVNode>[] children)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-row space-x-4 items-center p-4");
                    buildProps(b);
                },
                children);
        }
    }
}

