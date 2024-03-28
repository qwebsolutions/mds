using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using static MdsCommon.Controls.Breadcrumb;

namespace MdsCommon.Controls
{
    public static class NavigateButton
    {
        public class Props
        {
            public string Label { get; set; }
            public string Href { get; set; }
            public bool Enabled { get; set; } = true;
            public Button.Style Style { get; set; } = Button.Style.Primary;
            public string SvgIcon { get; set; }
        }

        public static Var<IVNode> Render(this LayoutBuilder b, Var<Props> props)
        {
            return b.HtmlA(
                b =>
                {
                    b.If(
                        b.Get(props, x => x.Enabled),
                        b =>
                        {
                            b.SetHref(b.Get(props, x => x.Href));
                        });
                },
                b.HtmlButton(
                    b =>
                    {
                        b.SetClass("rounded");
                        b.If(b.HasValue(b.Get(props, x => x.Label)),
                            b =>
                            {
                                b.AddClass("text-white py-2 px-4 shadow");
                            },
                            b =>
                            {
                                b.AddClass("p-1 shadow");
                            });

                        b.If(b.Get(props, x => x.Enabled), b =>
                        {
                            var bgClass = b.Switch(
                                b.Get(props, x => x.Style),
                                b => b.Const(""),
                                (Button.Style.Primary, b => b.Const("bg-sky-500")),
                                (Button.Style.Danger, b => b.Const("bg-red-500")),
                                (Button.Style.Light, b => b.Const("bg-white")));

                            b.If(b.HasValue(bgClass), b =>
                            {
                                b.AddClass(bgClass);
                            });
                        },
                        b =>
                        {
                            b.SetDisabled();
                            b.AddClass("bg-gray-300");
                        });
                    },
                    b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-row space-x-2 items-center");
                        },
                        b.Optional(
                            b.HasValue(b.Get(props, x => x.SvgIcon)),
                            b =>
                            {
                                return b.HtmlDiv(
                                    b =>
                                    {
                                        b.SetClass("h-5 w-5");
                                        b.SetInnerHtml(b.Get(props, x => x.SvgIcon));
                                    });
                            }),
                        b.Optional(
                            b.HasValue(b.Get(props, x => x.Label)),
                            b => b.TextSpan(b.Get(props, x => x.Label))))

                        ));
        }
    }
}
