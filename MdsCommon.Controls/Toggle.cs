using Metapsi;
using Metapsi.Dom;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;

namespace MdsCommon.Controls
{
    public static class Toggle
    {
        public class Props
        {
            public bool IsOn { get; set; }
            public string OnLabel { get; set; }
            public string OffLabel { get; set; }
            public bool Enabled { get; set; } = true;
            public string PillClass { get; set; } = "toggle-pill";
            public string DotClass { get; set; } = "toggle-dot";
            public string LabelClass { get; set; } = string.Empty;
            public string ExtraRootCss { get; set; } = string.Empty;
        }

        public class ToggleTarget
        {
            public bool @checked { get; set; }
        }

        internal static Var<IVNode> Render<TState>(LayoutBuilder b, Var<Props> props, Var<HyperType.Action<TState, bool>> onToggle)
        {
            var isOn = b.Get(props, x => x.IsOn);
            var enabled = b.Get(props, x => x.Enabled);

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex items-center justify-left");
                    b.AddClass(b.Get(props, x => x.ExtraRootCss));
                },
                b.HtmlLabel(
                    b =>
                    {
                        b.SetClass("flex items-center");
                        b.If(enabled, b => b.AddClass(" cursor-pointer"));
                    },
                    b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("relative");
                        },
                        b.HtmlInput(
                            b =>
                            {
                                b.SetDisabled(b.Not(enabled));
                                b.SetClass("sr-only peer");
                                b.SetAttribute("type", "checkbox");
                                b.SetAttribute("checked", isOn);
                                b.OnEventAction(
                                    "click",
                                     b.MakeAction((SyntaxBuilder b, Var<TState> state, Var<DomEvent<ToggleTarget>> @event) =>
                                     {
                                         b.StopPropagation(@event);
                                         var target = b.Get(@event, x => x.target);
                                         var value = b.Get(target, x => x.@checked);
                                         return b.MakeActionDescriptor(onToggle, value);
                                     }));
                            }),
                        b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("block");
                                b.AddClass(b.Get(props, x => x.PillClass));
                            }),
                        b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("dot absolute");
                                b.AddClass(b.Get(props, x => x.DotClass));
                            })),
                    b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("ml-3");
                        },
                        b.HtmlSpanText(
                            b =>
                            {
                                b.SetClass(b.Get(props, x => x.LabelClass));
                            },
                            b.Get(props, isOn, (props, isOn) => isOn ? props.OnLabel : props.OffLabel))
                        )
                    ));
        }
    }

    public static partial class Controls
    {
        public static Var<IVNode> Toggle<TState>(
            this LayoutBuilder b,
            Var<bool> isOn,
            Var<HyperType.Action<TState, bool>> onToggle,
            Var<string> onLabel,
            Var<string> offLabel,
            System.Action<PropsBuilder<Toggle.Props>> optional = null)
        {
            var props = b.NewObj<Toggle.Props>(b =>
            {
                b.Set(x => x.IsOn, isOn);
                b.Set(x => x.OnLabel, onLabel);
                b.Set(x => x.OffLabel, offLabel);
                b.AddProps(optional);
            });
            
            return b.Call(MdsCommon.Controls.Toggle.Render, props, onToggle);
        }

        public static Var<IVNode> BoundToggle<TEntity>(
            this LayoutBuilder b,
            Var<TEntity> entity,
            System.Linq.Expressions.Expression<System.Func<TEntity, bool>> onProperty,
            Var<string> onLabel,
            Var<string> offLabel,
            System.Action<PropsBuilder<Toggle.Props>> optional = null)
        {
            var isOn = b.Get(entity, onProperty);

            return b.Toggle(
                isOn,
                b.MakeAction((SyntaxBuilder b, Var<object> state, Var<bool> isOn) =>
                {
                    b.Set(entity, onProperty, isOn);
                    return b.Clone(state);
                }),
                onLabel,
                offLabel,
                optional);
        }
    }
}

