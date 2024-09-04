using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;

namespace MdsCommon.Controls
{
    public static class TextArea
    {
        public class Props
        {
            public string Text { get; set; }
            public int Rows { get; set; } = 5;
            public string CssClass { get; set; } = "hyper-input";
            public bool Enabled { get; set; } = true;
            public string Placeholder { get; set; } = "";
        }

        public static Var<IVNode> Render(this LayoutBuilder b, Var<TextArea.Props> props)
        {
            return b.HtmlTextarea(
                b =>
                {
                    b.SetClass(b.Get(props, x => x.CssClass));
                    b.SetAttribute("rows", b.Get(props, x => x.Rows));
                    b.SetAttribute("placeholder", b.Get(props, x => x.Placeholder));
                    b.SetDisabled(b.Not(b.Get(props, x => x.Enabled)));
                },
                b.TextSpan(b.Get(props, x => x.Text)));
        }
    }
    public static partial class Controls
    {
        public static Var<IVNode> TextArea(this LayoutBuilder b, Var<TextArea.Props> props)
        {
            return MdsCommon.Controls.TextArea.Render(b,props);
        }
        public static Var<IVNode> TextArea(
            this LayoutBuilder b, 
            Var<string> text,
            System.Action<PropsBuilder<TextArea.Props>> optional = null)
        {
            var props = b.NewObj<TextArea.Props>(
                b =>
                {
                    b.Set(x => x.Text, text);
                });
            b.SetProps(props, optional);

            return b.TextArea(props);
        }
    }
}

