using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MdsCommon.Controls
{
    public static class ActionBar
    {
        public class Command<TItem>
        {
            public string IconHtml { get; set; }
            public System.Action<TItem> OnCommand { get; set; }
        }

        public class Props<TItem>
        {
            public List<Command<TItem>> Commands { get; set; } = new();
        }

        public static Var<IVNode> Render<TItem>(LayoutBuilder b, Var<Props<TItem>> props, Var<TItem> entity)
        {
            b.AddModuleStylesheet();

            var commands = b.Get(props, x => x.Commands);

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-row space-x-2 justify-end");
                },
                b.Map(
                    commands,
                    (b, command) => b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex rounded bg-gray-200 w-10 h-10 p-1 cursor-pointer justify-center items-center opacity-50 hover:opacity-100 text-red-500");
                            b.SetInnerHtml(b.Get(command, x => x.IconHtml));
                            b.OnClickAction((SyntaxBuilder b, Var<object> state) =>
                            {
                                var onCommand = b.Get(command, x => x.OnCommand);
                                b.Call(onCommand, entity);
                                return b.Clone(state);
                            });
                        })
                    ));
        }
    }

    public static partial class Controls
    {

        public static Var<IVNode> ActionBar<TItem>(
            this LayoutBuilder b,
            Var<ActionBar.Props<TItem>> props,
            Var<TItem> entity)
        {
            return b.Call(MdsCommon.Controls.ActionBar.Render, props, entity);
        }
    }
}

