using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsCommon.Controls
{
    public static partial class Controls
    {
        public static Var<IVNode> Breadcrumb(
            this LayoutBuilder b,
            Var<List<Breadcrumb.Link>> links,
            Var<string> currentPage,
            System.Action<Modifier<Breadcrumb.Props>> optional = null)
        {
            var props = b.NewObj<Breadcrumb.Props>(optional);
            b.Set(props, x => x.Links, links);
            b.Set(props, x => x.CurrentPage, currentPage);

            return MdsCommon.Controls.Breadcrumb.RenderBreadcrumb(b, props);
        }
    }

    public static partial class Breadcrumb
    {
        public class Link
        {
            public string Href { get; set; } = string.Empty;
            public string Label { get; set; } = string.Empty;
        }

        public class Props
        {
            public IVNode Separator { get; set; }
            public List<Link> Links { get; set; } = new();
            public string CurrentPage { get; set; } = string.Empty;
            public string LinkCss { get; set; }
            public string CurrentPageCss { get; set; }
        }

        public static Var<IVNode> RenderBreadcrumb(this LayoutBuilder b, Var<Props> props)
        {
            b.SetIfEmpty<Props, string>(props, x => x.LinkCss, b.Const(""));
            b.SetIfEmpty<Props, string>(props, x => x.CurrentPageCss, b.Const(""));
            b.SetIfEmpty(props, x => x.Separator, b.TextSpan("»"));

            var nodes = b.NewCollection<IVNode>();



            b.Foreach(b.Get(props, x => x.Links), (b, item) =>
            {
                var link = b.HtmlA(
                    b =>
                    {
                        b.SetClass(b.Get(props, x => x.LinkCss));
                        b.SetHref(b.Get(item, x => x.Href));
                    },
                    b.TextSpan(b.Get(item, x => x.Label)));

                b.Push(nodes, link);
                b.Push(nodes, b.Clone(b.Get(props, x => x.Separator)));
            });

            b.Push(nodes,
                b.HtmlSpan(
                    b =>
                    {
                        b.SetClass(b.Get(props, x => x.CurrentPageCss));
                    },
                    b.TextSpan(b.Get(props, x => x.CurrentPage))));

            var container = b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-row flex-wrap gap-4 items-center");
                },
                nodes);

            return container;
        }
    }
}

