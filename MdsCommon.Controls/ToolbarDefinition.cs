using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;

namespace MdsCommon.Controls
{
    public class ToolbarData { }

    public class ToolbarDefinition : IControlDefinition<ToolbarData>
    {
        public ControlDefinition<ToolbarData> Container { get; set; }
        public ControlDefinition<ToolbarData> Left { get; set; }
        public ControlDefinition<ToolbarData> Right { get; set; }

        public Func<LayoutBuilder, Var<ToolbarData>, Var<IVNode>> GetRenderer()
        {
            return Container.GetRenderer();
        }
    }

    public static class ToolbarExtensions
    {
        public static void AddToolbarChild(
            this ControlBuilder<ToolbarDefinition, ToolbarData> b,
            Func<LayoutBuilder, Var<IVNode>> buildChild,
            HorizontalPlacement placement = HorizontalPlacement.Left)
        {
            if (placement == HorizontalPlacement.Left)
            {
                b.OnControl(x => x.Left, x => x, b => b.AddChild((b, data) => buildChild(b)));
            }
            else
            {
                b.OnControl(x => x.Right, x => x, b => b.AddChild((b, data) => buildChild(b)));
            }
        }
    }

    public static partial class Control
    {
        public static ToolbarDefinition DefaultToolbar()
        {
            ToolbarDefinition toolbarDefinition = new ToolbarDefinition();

            toolbarDefinition.Container = ControlDefinition.New<ToolbarData>(
                "div",
                (b, data, props) =>
                {
                    b.SetClass(props, "flex flex-row justify-between");
                },
                (b, data) => b.Render(toolbarDefinition.Left, data),
                (b, data) => b.Render(toolbarDefinition.Right, data));

            toolbarDefinition.Left = ControlDefinition.New<ToolbarData>(
                "div",
                (b, data, props) =>
                {
                    b.SetClass(props, "flex flex-row items-center justify-left gap-2");
                });

            toolbarDefinition.Right = ControlDefinition.New<ToolbarData>(
                "div",
                (b, data, props) =>
                {
                    b.SetClass(props, "flex flex-row items-center gap-2");
                });

            return toolbarDefinition;
        }

        public static Var<IVNode> Toolbar<TRow>(
            this LayoutBuilder b,
            Action<ControlBuilder<ToolbarDefinition, ToolbarData>> custom)
        {
            return b.FromDefinition(DefaultToolbar, custom);
        }
    }
}

