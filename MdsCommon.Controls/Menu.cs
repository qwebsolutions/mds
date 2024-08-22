using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsCommon.Controls
{
    public static class Menu
    {
        public class Props
        {
            public List<MdsCommon.Menu.Entry> Entries { get; set; }
            public string ActiveCode { get; set; }
        }

        public static Var<IVNode> Render(this LayoutBuilder b, Var<Props> props)
        {
            b.AddModuleStylesheet();

            var isEmptyString = b.Def<LayoutBuilder, string,bool>(Core.IsEmpty);
            var missingIcons = b.Get(props, isEmptyString, (props, empty) => props.Entries.Any(x => empty(x.SvgIcon)));
            
            var selectedCode = b.Get(props, x => x.ActiveCode);

            var entries = b.Get(props, x => x.Entries);

            var menuDiv = b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col p-4 h-full border-r shadow-md group");
                },
                b.Map(entries, (b, menuEntry) =>
                {
                    var isActive = b.AreEqual(b.Get(menuEntry, x => x.Code), selectedCode);

                    var link = b.HtmlA(
                        b =>
                        {
                            b.SetClass("flex flex-row items-center p-4 rounded hover:bg-gray-100 text-clip whitespace-nowrap overflow-hidden");
                            b.SetHref(b.Get(menuEntry, x => x.Href));

                            b.If(isActive,
                                b => b.AddClass("text-sky-600"),
                                b => b.AddClass("text-gray-800"));
                        },
                        b.Optional(
                            b.HasValue(
                                b.Get(menuEntry, x => x.SvgIcon)),
                            b =>
                            {
                                return b.HtmlDiv(
                                    b =>
                                    {
                                        b.SetClass("bg-sky-600 rounded-full w-6 h-6 flex items-center justify-center");
                                    },
                                    b.HtmlDiv(b =>
                                    {
                                        b.SetClass("w-4 h-4 text-gray-50");
                                        b.SetInnerHtml(b.Get(menuEntry, x => x.SvgIcon));
                                    }));
                            }),

                        b.HtmlSpanText(
                            b =>
                            // If general hover behavior
                            b.If(missingIcons, b =>
                            {
                                // Layout if this particular item is missing icon
                                b.If(
                                    b.IsEmpty(b.Get(menuEntry, x => x.SvgIcon)),
                                    b =>
                                    {
                                        b.AddClass("w-48 pl-8");
                                    },
                                    b =>
                                {
                                    b.AddClass("w-48 pl-2");
                                });
                            },
                            b =>
                            {
                                b.AddClass("group-hover:pl-4 opacity-0 w-0 group-hover:w-48 group-hover:opacity-100 transition-all delay-1000");
                            }),
                        b.Get(menuEntry, x => x.Label)));
                    return link;
                }));

            return menuDiv;
        }
    }

    public static partial class Controls
    {
        public static Var<IVNode> Menu(this LayoutBuilder v,Var<Menu.Props> entries)
        {
            return v.Call(MdsCommon.Controls.Menu.Render, entries);
        }
    }
}

