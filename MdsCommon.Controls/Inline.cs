using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;

namespace MdsCommon.Controls
{
    public static partial class Inline
    {
        public static Var<IVNode> Svg(this LayoutBuilder b, string content, string classNames = "")
        {
            return b.HtmlDiv(
                b =>
                {
                    b.AddClass(classNames);
                    b.SetInnerHtml(b.Const(content));
                });
        }
    }
}

