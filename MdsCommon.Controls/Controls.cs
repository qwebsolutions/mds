﻿using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MdsCommon.Controls
{
    // This empty class is here for copy-paste
    public static partial class Controls
    {
    }

    public static partial class Control
    {
        public static Var<IVNode> Badge(
            this LayoutBuilder b,
            Var<string> label,
            Var<string> additionalCss)
        {
            return b.HtmlSpan(
                b =>
                {
                    b.SetClass("uppercase text-xs text-white p-1 mx-4 font-medium rounded-md");
                    b.AddClass(additionalCss);
                },
                b.TextSpan(label));
        }



    }
}

