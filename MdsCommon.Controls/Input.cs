using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Collections.Generic;

namespace MdsCommon.Controls
{
    public static class Input
    {
        public class Props<TState>
        {
            public bool IsPassword { get; set; }
            public string Value { get; set; } = string.Empty;
            public string Placeholder { get; set; } = string.Empty;
            public HyperType.Action<TState, string> OnInput { get; set; }
            public string CssClass { get; set; } = "hyper-input";
            public bool Enabled { get; set; } = true;
        }

        public static Var<IVNode> Render<TState>(LayoutBuilder b, Var<Props<TState>> props)
        {
            b.AddModuleStylesheet();

            return b.HtmlInput(
                b =>
                {
                    b.SetClass(b.Concat(b.Get(props, x => x.CssClass), b.Const(" w-full")));
                    var isPassword = b.Get(props, x => x.IsPassword);
                    b.If(isPassword, b =>
                    {
                        b.SetType("password");
                    },
                    b =>
                    {
                        b.SetType("text");
                    });

                    var placeholder = b.Get(props, x => x.Placeholder);
                    b.If(b.HasValue(placeholder), b => b.SetPlaceholder(placeholder));

                    b.SetValue(b.Get(props, x => x.Value));
                    b.OnInputAction(b.Get(props, x => x.OnInput));
                    b.SetDisabled(b.Not(b.Get(props, x => x.Enabled)));
                });
        }
    }

    public static partial class Controls
    {
        public static Var<IVNode> Input<TState>(
            this LayoutBuilder b,
            Var<string> value,
            Var<HyperType.Action<TState, string>> onInput,
            Var<string> placeholder,
            System.Action<Modifier<Input.Props<TState>>> optional = null)
        {
            var props = b.NewObj<MdsCommon.Controls.Input.Props<TState>>(b =>
                {
                    b.Set(x => x.OnInput, onInput);
                    b.Set(x => x.Value, value);
                    b.Set(x => x.Placeholder, placeholder);
                });
            b.Modify(props, optional);
            return b.Call(MdsCommon.Controls.Input.Render, props);
        }

        public static Var<IVNode> BoundInput<TEntity, TProp>(
            this LayoutBuilder b,
            Var<TEntity> entity,
            System.Linq.Expressions.Expression<System.Func<TEntity, TProp>> onProperty,
            Var<string> placeholder,
            System.Action<Modifier<Input.Props<object>>> optional = null)
        {
            Var<TProp> value = b.Get(entity, onProperty);

            var setProperty = b.MakeAction<object, string>((SyntaxBuilder b, Var<object> state, Var<string> inputValue) =>
            {
                b.Set(entity, onProperty, inputValue.As<TProp>());
                return b.Clone(state);
            });

            var props = b.NewObj<MdsCommon.Controls.Input.Props<object>>(b =>
            {
                b.Set(x => x.OnInput, setProperty);
                b.Set(x => x.Value, value.As<string>());
                b.Set(x => x.Placeholder, placeholder);
            });

            b.Modify(props, optional);

            return b.Call(MdsCommon.Controls.Input.Render, props);
        }

        public static Var<IVNode> BoundInput<TState, TEntity>(
            this LayoutBuilder b,
            Var<TState> state,
            System.Linq.Expressions.Expression<System.Func<TState, TEntity>> onEntity,
            System.Linq.Expressions.Expression<System.Func<TEntity, string>> onProperty,
            Var<string> placeholder)
        {
            Var<TEntity> entity = b.Get(state, onEntity);
            Var<string> value = b.Get(entity, onProperty);

            var setProperty = b.MakeAction<TState, string>((SyntaxBuilder b, Var<TState> state, Var<string> inputValue) =>
            {
                var entity = b.Get(state, onEntity);
                b.Set(entity, onProperty, inputValue);
                return b.Clone(state);
            });

            var props = b.NewObj<MdsCommon.Controls.Input.Props<TState>>(b =>
            {
                b.Set(x => x.OnInput, setProperty);
                b.Set(x => x.Value, value);
                b.Set(x => x.Placeholder, placeholder);
            });

            return b.Call(MdsCommon.Controls.Input.Render, props);
        }

        public static Var<IVNode> BoundInput<TState, TEntity>(
            this LayoutBuilder b,
            Var<TState> state,
            System.Linq.Expressions.Expression<System.Func<TState, TEntity>> onEntity,
            System.Linq.Expressions.Expression<System.Func<TEntity, int>> onProperty,
            Var<string> placeholder)
        {
            Var<TEntity> entity = b.Get(state, onEntity);
            Var<int> value = b.Get(entity, onProperty);

            var setProperty = b.MakeAction<TState, string>((SyntaxBuilder b, Var<TState> state, Var<string> inputValue) =>
            {
                Var<TEntity> entity = b.Get(state, onEntity);
                b.Set(entity, onProperty, b.ParseInt(inputValue));
                return b.Clone(state);
            });

            var stringValue = b.AsString(value);

            var props = b.NewObj<MdsCommon.Controls.Input.Props<TState>>(b =>
            {
                b.Set(x => x.OnInput, setProperty);
                b.Set(x => x.Value, stringValue);
                b.Set(x => x.Placeholder, placeholder);
            });

            return b.Call(MdsCommon.Controls.Input.Render, props);
        }

        public static Var<IVNode> BoundInput<TState, TEntity, TId>(
            this LayoutBuilder b,
            Var<TState> state,
            Var<TId> id,
            System.Linq.Expressions.Expression<System.Func<TState, TId, TEntity>> onEntity,
            System.Linq.Expressions.Expression<System.Func<TEntity, string>> onProperty,
            Var<string> placeholder)
        {
            Var<TEntity> entity = b.Get(state, id, onEntity);
            Var<string> value = b.Get(entity, onProperty);

            var setProperty = b.MakeAction<TState, string>((SyntaxBuilder b, Var<TState> state, Var<string> inputValue) =>
            {
                var entity = b.Get(state, id, onEntity);
                b.Set(entity, onProperty, inputValue);
                return b.Clone(state);
            });

            var props = b.NewObj<MdsCommon.Controls.Input.Props<TState>>(b =>
            {
                b.Set(x => x.OnInput, setProperty);
                b.Set(x => x.Value, value);
                b.Set(x => x.Placeholder, placeholder);
            });

            return b.Call(MdsCommon.Controls.Input.Render, props);
        }

        public static Var<IVNode> BoundInput<TState, TEntity>(
            this LayoutBuilder b,
            Var<TState> state,
            System.Linq.Expressions.Expression<System.Func<TState, TEntity>> onEntity,
            System.Linq.Expressions.Expression<System.Func<TEntity, string>> onProperty,
            string placeholder = "")
        {
            return b.BoundInput(state, onEntity, onProperty, b.Const(placeholder));
        }

        //public static void BindTo<TControl, TState, TEntity>(
        //    this PropsBuilder<TControl> b,
        //    Var<TState> state,
        //    System.Func<SyntaxBuilder, Var<TState>, Var<TEntity>> onEntity,
        //    System.Linq.Expressions.Expression<System.Func<TEntity, string>> onProperty)
        //    where TControl : IHasInputTextEvent, IHasValueAttribute, new()
        //{
        //    b.BindTo(state, b.Def(onEntity), onProperty);
        //}

        //public static void BindTo<TControl, TState, TEntity>(
        //    this PropsBuilder<TControl> b,
        //    Var<TState> state,
        //    Var<System.Func<TState, TEntity>> onEntity,
        //    System.Linq.Expressions.Expression<System.Func<TEntity, string>> onProperty)
        //    where TControl : IHasInputTextEvent, IHasValueAttribute, new()
        //{
        //    Var<TEntity> entity = b.Call(onEntity, state);
        //    Var<string> value = b.Get(entity, onProperty);

        //    var setProperty = b.MakeAction<TState, string>((SyntaxBuilder b, Var<TState> state, Var<string> inputValue) =>
        //    {
        //        Var<TEntity> entity = b.Call(onEntity, state);
        //        b.Set(entity, onProperty, inputValue);
        //        return b.Clone(state);
        //    });

        //    b.SetValue(value);
        //    b.OnInputAction(setProperty);
        //}

        //public static void BindTo<TControl, TState, TEntity>(this PropsBuilder<TControl> b,
        //    Var<TState> state,
        //    System.Linq.Expressions.Expression<System.Func<TState, TEntity>> onEntity,
        //    System.Linq.Expressions.Expression<System.Func<TEntity, string>> onProperty)
        //    where TControl : IHasInputTextEvent, IHasValueAttribute, new()
        //{
        //    Var<TEntity> entity = b.Get(state, onEntity);
        //    Var<string> value = b.Get(entity, onProperty);

        //    var setProperty = b.MakeAction<TState, string>((SyntaxBuilder b, Var<TState> state, Var<string> inputValue) =>
        //    {
        //        Var<TEntity> entity = b.Get(state, onEntity);
        //        b.Set(entity, onProperty, inputValue);
        //        return b.Clone(state);
        //    });

        //    b.SetValue(value);
        //    b.OnInputAction(setProperty);
        //}
    }
}

