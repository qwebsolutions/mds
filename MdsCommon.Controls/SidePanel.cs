using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Linq;

namespace MdsCommon.Controls
{
    public static class SidePanel
    {
        public class State
        {
            public string PanelName { get; set; } = string.Empty;
            public string Reference { get; set; } = string.Empty;
        }

        public const string X = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\" fill=\"currentColor\">\r\n\t<path fill-rule=\"evenodd\" d=\"M5.47 5.47a.75.75 0 011.06 0L12 10.94l5.47-5.47a.75.75 0 111.06 1.06L13.06 12l5.47 5.47a.75.75 0 11-1.06 1.06L12 13.06l-5.47 5.47a.75.75 0 01-1.06-1.06L10.94 12 5.47 6.53a.75.75 0 010-1.06z\" clip-rule=\"evenodd\" />\r\n</svg>\r\n";

        public static Var<IVNode> Render(LayoutBuilder b, Var<IVNode> content)
        {
            var onClick = b.MakeAction((SyntaxBuilder b, Var<Metapsi.Ui.IHasSidePanel> state) =>
            {
                b.Set(state, x => x.ShowSidePanel, b.Const(false));
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
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("w-full h-full overflow-auto");
                    },
                    content));

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("fixed top-0 right-0 bottom-0 w-1/3 bg-gray-50 h-screen transition-all duration-500 z-50");
                },
                verticalLayout);
        }

        public static Var<TPage> ShowSidePanel<TPage>(this SyntaxBuilder b, Var<TPage> page)
            where TPage : Metapsi.Ui.IHasSidePanel
        {
            b.Set(page, x => x.ShowSidePanel, b.Const(true));
            return b.Clone(page);
        }

        //public static void ShowSidePanel<T>(this BlockBuilder b, Var<T> entity) where T: IRecord
        //{
        //    var reference = b.Get(entity, x => x.Id).As<string>();
        //    b.UpdateControlState<State>(b.Const(nameof(SidePanel)), b =>
        //    {
        //        b.Set(x => x.PanelName, b.Const(typeof(T).Name));
        //        b.Set(x => x.Reference, reference);
        //    });
        //}
    }

    public static partial class Controls
    {
        public static Var<IVNode> SidePanel<TPage>(
            this LayoutBuilder b,
            Var<TPage> page,
            System.Func<LayoutBuilder, Var<IVNode>> renderContent)
             where TPage : Metapsi.Ui.IHasSidePanel
        {
            return b.If(
                b.Get(page, x => x.ShowSidePanel),
                b =>
                {
                    var content = b.Call(renderContent);
                    return b.Call(MdsCommon.Controls.SidePanel.Render, content);
                },
                b => b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("fixed top-0 right-0 bottom-0 w-0 transition-all bg-gray-50 h-screen duration-500");
                    }));
        }
    }
}

