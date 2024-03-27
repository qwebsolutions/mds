using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;

namespace MdsCommon.Controls
{
    public static class Alert
    {
        public class State
        {
            public string AlertCode { get; set; }
            public string Reference { get; set; }
            public string Text { get; set; }
            public int RemainingTicks { get; set; }
        }

        public static Var<IVNode> RenderNotification(this LayoutBuilder b, Var<State> props)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("grid fixed top-5 left-0 w-screen place-items-center");
                },
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("border border-solid border-red-300 bg-red-100 text-red-300 rounded w-1/2 p-2 drop-shadow");
                    },
                    b.T(b.Get(props, x => x.Text))));
        }

        public static Var<IVNode> RenderValidation(this LayoutBuilder b, Var<State> props)
        {
            return b.HtmlDiv(
                b=>
                {
                    b.SetClass("border border-solid border-red-300 bg-red-100 text-red-300 rounded w-full p-2 drop-shadow");
                },
                b.T(b.Get(props, x => x.Text)));
        }
    }

    public static partial class Controls
    {
        public static Var<IVNode> Notification(this LayoutBuilder b, string text)
        {
            var t = b.Const(text);
            return b.Call(MdsCommon.Controls.Alert.RenderNotification, b.NewObj<MdsCommon.Controls.Alert.State>(b=> b.Set(x => x.Text, t)));
        }

        public static Var<IVNode> Notification(this LayoutBuilder b, Var<string> text)
        {
            return b.Call(MdsCommon.Controls.Alert.RenderNotification, b.NewObj<MdsCommon.Controls.Alert.State>(b => b.Set(x => x.Text, text)));
        }

        public static Var<IVNode> ValidationPanel<TPage>(this LayoutBuilder b, Var<TPage> page)
            where TPage : Metapsi.Ui.IHasValidationPanel
        {
            Var<string> validationMessage = b.Get(page, x => x.ValidationMessage);
            return b.If(
                b.HasValue(validationMessage),
                b => b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("border border-solid border-red-300 bg-red-100 text-red-300 rounded w-full p-2 mb-4 drop-shadow transition-all");
                        },
                        b.T(validationMessage)),
                b => b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("w-0 h-0 transition-all");
                    }));
        }
    }
}

