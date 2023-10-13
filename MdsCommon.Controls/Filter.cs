using MdsCommon.Controls;
using Metapsi;
using Metapsi.ChoicesJs;
using Metapsi.Dom;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsCommon.HtmlControls
{
    public interface IHasStringValue
    {
        string Value { get; set; }
    }

    public interface IHasOnInputEvent { }
    public interface IHasClearEvent { }

    public class InputText : IHasStringValue, IHasOnInputEvent
    {
        public string Value { get; set; }
    }

    public class Filter : IHasStringValue, IHasOnInputEvent, IHasClearEvent
    {
        public string Value { get; set; }
    }

    public class FilterBuilder
    {
        public HParams Container { get; set; }
        public HParams InputText { get; set; }
        public HParams ClearButton { get; set; }
        public HParams ClearButtonContent { get; set; }
    }

    public static partial class Control
    {
        public static Var<bool> ContainsValue<T>(this BlockBuilder b, Var<T> item, Var<string> value)
        {
            return b.Includes(b.ToLowercase(b.ConcatObjectValues(item)), b.ToLowercase(value));
        }

        public static Var<List<TItem>> FilterList<TItem>(
            this BlockBuilder b,
            Var<List<TItem>> list,
            Var<string> value)
        {
            var filteredItems = b.Get(
                list,
                value,
                b.DefineFunc<TItem, string, bool>(ContainsValue),
                (all, value, filterFunc) => all.Where(x => filterFunc(x, value)).ToList());

            return filteredItems;
        }

        public static void Customize<TBuilder>(
            this BlockBuilder b,
            Var<TBuilder> builder,
            System.Linq.Expressions.Expression<Func<TBuilder, HParams>> getParams,
            Action<BlockBuilder, Var<HParams>> customize)
        {
            var hParams = b.Get(builder, getParams);
            customize(b, hParams);
        }

        public static void SetDefault<TBuilder>(
            this BlockBuilder b,
            Var<TBuilder> builder,
            System.Linq.Expressions.Expression<Func<TBuilder, HParams>> getParams,
            Func<BlockBuilder, Var<HParams>> setDefault)
        {
            b.Set(builder, getParams, setDefault(b));
        }

        public static Var<HParams> HNode(this BlockBuilder b, Var<string> tag)
        {
            var p = b.NewObj<HParams>();
            b.Set(p, x => x.Tag, tag);
            return p;
        }

        public static Var<HParams> HNode(this BlockBuilder b, string tag)
        {
            return b.HNode(b.Const(tag));
        }

        public static Var<HParams> HDiv(this BlockBuilder b)
        {
            return b.HNode("div");
        }

        public static Var<HParams> HSvg(this BlockBuilder b, Var<string> svg)
        {
            var container = b.HDiv();
            b.SetProp(container, Html.innerHTML, svg);
            return container;
        }

        public static Var<HParams> HSvg(this BlockBuilder b, string svg)
        {
            return b.HSvg(b.Const(svg));
        }

        public static Var<HParams> SetProp<T>(
            this BlockBuilder b,
            Var<HParams> hParams,
            DynamicProperty<T> property,
            Var<T> value)
        {
            b.SetDynamic(b.Get(hParams, x => x.Props), property, value);
            return hParams;
        }

        public static Var<HParams> SetProp<T>(
            this BlockBuilder b,
            Var<HParams> hParams,
            DynamicProperty<T> property,
            T value)
        {
            return b.SetProp(hParams, property, b.Const(value));
        }

        public static void BindOneWay<TPageModel, TLocalModel, TBoundObject, TProperty>(
            this BindingContext<TPageModel, TLocalModel, TBoundObject> bindingContext,
            System.Linq.Expressions.Expression<Func<TBoundObject, TProperty>> intoProperty,
            System.Linq.Expressions.Expression<Func<TLocalModel, TProperty>> fromModel)
        {
            bindingContext.BindingActions.Add((b, context, bound) =>
            {
                var localModel = b.Get(context, x => x.InputData);
                var value = b.Get(localModel, fromModel);
                b.Set(bound, intoProperty, value);
            });
        }

        public static void SetOnInput<TState>(this BlockBuilder b, Var<HParams> hParams, Var<HyperType.Action<TState, string>> onInput)
        {
            var extractInputValue = b.MakeAction<TState, DomEvent<InputTarget>, string>((BlockBuilder b, Var<TState> state, Var<DomEvent<InputTarget>> @event) =>
            {
                var target = b.Get(@event, x => x.target);
                var value = b.Get(target, x => x.value);
                return b.MakeActionDescriptor<TState, string>(onInput, value);
            });

            b.SetProp(hParams, new DynamicProperty<HyperType.Action<TState, DomEvent<InputTarget>>>("oninput"), extractInputValue);
        }

        public static void SetOnClick<TState>(this BlockBuilder b,Var<HParams> hParams,Var<HyperType.Action<TState>> onClick)
        {
            var clickEvent = b.MakeAction<TState, DomEvent<ClickTarget>>((BlockBuilder b, Var<TState> state, Var<DomEvent<ClickTarget>> @event) =>
            {
                b.StopPropagation(@event);
                return onClick;
            });

            b.SetProp(hParams, new DynamicProperty<HyperType.Action<TState, DomEvent<ClickTarget>>>("onclick"), clickEvent);
        }

        public static void BindFilter<TPageModel, TLocalModel>(
            this BindingContext<TPageModel, TLocalModel, FilterBuilder> bindingContext,
            System.Linq.Expressions.Expression<Func<TLocalModel, string>> fromModel)
        {
            bindingContext.BindingActions.Add((b, context, bound) =>
            {
                var inputText = b.Get(bound, x => x.InputText);

                var localModel = b.Get(context, x => x.InputData);
                var value = b.Get(localModel, fromModel);
                b.SetProp(inputText, DynamicProperty.String("value"), value);
                b.SetOnInput(inputText, b.MakeAction((BlockBuilder b, Var<TPageModel> model, Var<string> newValue) =>
                {
                    var dataItem = b.Call(b.Get(context, x => x.AccessData), model);
                    b.Set(dataItem, fromModel, newValue);
                    return b.Clone(model);
                }));
            });

            bindingContext.BindingActions.Add((b, context, bound) =>
            {
                var clearButton = b.Get(bound, x => x.ClearButton);
                b.SetOnClick(clearButton, b.MakeAction((BlockBuilder b, Var<TPageModel> model) =>
                {
                    var dataItem = b.Call(b.Get(context, x => x.AccessData), model);
                    b.Set(dataItem, fromModel, string.Empty);
                    return b.Clone(model);
                }));
            });
        }

        public static void InBindingContext<TPageModel, TLocalModel, TBoundObject>(
            this BlockBuilder b,
            Var<DataContext<TPageModel, TLocalModel>> dataContext,
            Var<TBoundObject> boundObject,
            Action<BindingContext<TPageModel, TLocalModel, TBoundObject>> bindingAction)
        {
            BindingContext<TPageModel, TLocalModel, TBoundObject> bindingContext = new();
            bindingAction(bindingContext);

            foreach (var builderAction in bindingContext.BindingActions)
            {
                builderAction(b, dataContext, boundObject);
            }
        }

        public static Var<HyperNode> Filter(
            this BlockBuilder b,
            Action<BlockBuilder, Var<FilterBuilder>> customize = null)
        {
            var props = b.NewObj<FilterBuilder>();

            b.SetDefault(props, x => x.Container, b =>
            {
                var container = b.HDiv();
                b.SetProp(container, Html.@class, b.Const("flex flex-row items-center"));
                b.Log("container", container);
                return container;
            });

            b.SetDefault(props, x => x.InputText, b =>
            {
                var input = b.HNode("input");
                b.SetProp(input, Html.type, b.Const("text"));
                b.SetProp(input, Html.@class, b.Const("border rounded-full px-4 py-2"));
                b.Log("input", input);
                return input;
            });

            b.SetDefault(props, x => x.ClearButton, b =>
            {
                var button = b.HNode("button");
                b.SetProp(button, Html.@class, "-mx-10 w-6 h-6 text-gray-300");
                b.Log("button", button);
                return button;
            });

            b.SetDefault(props, x => x.ClearButtonContent, b =>
            {
                var svg = b.HSvg(Metapsi.Heroicons.Outline.XCircle);
                b.Log("svg", svg);
                return svg;
            });

            if (customize != null)
            {
                customize(b, props);
            }

            return b.H(
                b.Get(props, x => x.Container),
                b => b.H(b.Get(props, x => x.InputText)),
                b => b.H(
                    b.Get(props, x => x.ClearButton),
                    b => b.H(b.Get(props, x => x.ClearButtonContent))));
        }

        //public static Var<HyperNode> Filter(
        //    this BlockBuilder b,
        //    Action<BlockBuilder, Var<Filter>> build)
        //{
        //    var props = b.NewObj<Filter>();
        //    build(b, props);

        //    var container = b.Div("flex flex-row items-center");
        //    var inputText = b.Add(container, b.BuildControl<InputText>(
        //        b.Const("input"),
        //        (b, props) =>
        //        {
        //            b.Set(props, x => x.Value, "input in filter");
        //        }));

        //    var clearButton = b.Add(
        //        container, b.Node(
        //            "button",
        //            "-mx-10 w-6 h-6 text-gray-300",
        //            b => b.Svg(Metapsi.Heroicons.Outline.XCircle)));

        //    return container;
        //}

        //public static Var<HyperNode> Filter<TPageModel, TContextModel>(
        //    this BlockBuilder b,
        //    Var<DataContext<TPageModel, TContextModel>> dataContext,
        //    Action<DataContextControlBuilder<TPageModel, TContextModel, Filter>> builder)
        //{
        //    return b.BuildControl(dataContext, builder, (b, props) =>
        //    {
        //        var container = b.Div("flex flex-row items-center");
        //        //var filterInputText = b.Add(container, b.InputText(dataContext, b =>
        //        //{
        //        //    b.ControlBuilderActions.Add((b, control) =>
        //        //    {
        //        //        b.AddClass(control, "border rounded-full px-4 py-2");
        //        //    });
        //        //}));


        //        //var clearButton = b.Add(
        //        //    container, b.Node(
        //        //        "button",
        //        //        "-mx-10 w-6 h-6 text-gray-300",
        //        //        b => b.Svg(Metapsi.Heroicons.Outline.XCircle)));

        //        return container;
        //    });
        //}

        public static void BindFilterValue<TPageModel, TContext, TControlProps>(
            this DataContextControlBuilder<TPageModel, TContext, TControlProps> builder,
            System.Linq.Expressions.Expression<Func<TContext, string>> onProperty)
            where TControlProps : IHasStringValue, IHasOnInputEvent
        {
            builder.BindInput(x => x.Value, onProperty);
            builder.BindEvent(Metapsi.Hyperapp.EventExtensions.SetOnInput, onProperty);
        }

        //public static Var<HyperNode> InputText<TPageModel, TContextModel>(
        //    this BlockBuilder b,
        //    Var<DataContext<TPageModel, TContextModel>> dataContext,
        //    Action<DataContextControlBuilder<TPageModel, TContextModel, InputText>> builder)
        //{
        //    return b.BuildControl(dataContext, builder, (b, props) =>
        //    {
        //        var filterControl = b.Node("input");
        //        b.SetAttr(filterControl, Html.type, "text");
        //        return filterControl;
        //    });
        //}

        public static Var<HyperNode> InputText(
            this BlockBuilder b,
            Action<ControlBuilder<InputText>> builder)
        {
            return b.BuildControl(builder, (b, props) =>
            {
                var filterControl = b.Node("input");
                b.SetAttr(filterControl, Html.type, "text");
                return filterControl;
            });
        }

        public static void BindTextValue<TPageModel, TContext, TControlProps>(
            this DataContextControlBuilder<TPageModel, TContext, TControlProps> builder,
            System.Linq.Expressions.Expression<Func<TContext, string>> onProperty)
            where TControlProps : IHasStringValue, IHasOnInputEvent
        {
            builder.BindInput(x => x.Value, onProperty);
            builder.BindEvent(Metapsi.Hyperapp.EventExtensions.SetOnInput, onProperty);
        }

        public static void BindEvent<TPageModel, TContext, TControlProps, TPayload>(
            this DataContextControlBuilder<TPageModel, TContext, TControlProps> builder,
            Action<BlockBuilder, Var<HyperNode>, Var<HyperType.Action<TPageModel, TPayload>>> eventSetter,
            System.Linq.Expressions.Expression<Func<TContext, TPayload>> onProperty)
        {
            builder.ControlBuilderActions.Add((b, control) =>
            {
                eventSetter(b, control, b.MakeAction((BlockBuilder b, Var<TPageModel> pageModel, Var<TPayload> newValue) =>
                {
                    var dataItem = b.Call(b.Get(builder.Context, x => x.AccessData), pageModel);
                    b.Set(dataItem, onProperty, newValue);
                    return b.Clone(pageModel);
                }));
            });
        }
    }
}

