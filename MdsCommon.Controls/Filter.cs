﻿using MdsCommon.Controls;
using Metapsi;
using Metapsi.ChoicesJs;
using Metapsi.Dom;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MdsCommon.HtmlControls
{

    public interface IHasStringValue
    {
        string Value { get; set; }
    }

    public interface IHasOnInputEvent { }
    public interface IHasClearEvent { }

    public class Filter
    {
        public string Value { get; set; } = string.Empty;
        public Action<object, string> SetValue { get; set; }
    }

    public class FilterBuilder : CompoundBuilder<Filter>
    {
        public ControlBuilder Container { get; set; }
        public ControlBuilder Input { get; set; }
        public ControlBuilder ClearButton { get; set; }
        public ControlBuilder ClearIcon { get; set; }

        public override Var<IVNode> GetRoot(BlockBuilder b)
        {
            return Container.GetRoot(b);
        }

        protected override void Setup(BlockBuilder b)
        {
            this.Container = ControlBuilder.New(
                "div", b =>
                {
                    b.SetClass(b.Const("flex flex-row items-center"));
                },
                b => Input.GetRoot(b),
                b => b.Optional(
                    b.HasValue(b.Get(this.Data, x => x.Value)),
                    b => ClearButton.GetRoot(b)));

            this.Input = ControlBuilder.New("input", b =>
            {
                b.SetDynamic(b.Props, Html.type, b.Const("text"));
                b.SetDynamic(b.Props, Html.value, b.Get(this.Data, x => x.Value));
                b.Log("this.Data", this.Data);

                b.SetDynamic(b.Props,
                    new DynamicProperty<object>("oninput"),
                    b.MakeAction<object, DomEvent<InputTarget>>(
                        (BlockBuilder b, Var<object> state, Var<DomEvent<InputTarget>> @event) =>
                        {
                            var target = b.Get(@event, x => x.target);
                            var value = b.Get(target, x => x.value);
                            b.Call(b.Get(this.Data, x => x.SetValue), state, value);
                            b.Log("oninput", value);
                            return b.Clone(state);
                        }).As<object>());

                b.SetClass(b.Const("border rounded-full px-4 py-2"));
            });

            this.ClearButton = ControlBuilder.New(
                    "button",
                    b=>
                    {
                        b.SetClass(b.Const("-mx-10 w-6 h-6 text-gray-300"));
                        b.SetDynamic(b.Props, new DynamicProperty<object>("onclick"),
                            b.MakeAction((BlockBuilder b, Var<object> model) =>
                            {
                                b.Call(b.Get(this.Data, x=>x.SetValue), model, b.Const(string.Empty));
                                return b.Clone(model);
                            }).As<object>());
                    },
                    b=> ClearIcon.GetRoot(b));

            this.ClearIcon = ControlBuilder.New("div", b =>
            {
                b.SetDynamic(b.Props, Html.innerHTML, b.Const(Metapsi.Heroicons.Outline.XCircle));
            });
        }
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

        //public static void Customize<TBuilder>(
        //    this BlockBuilder b,
        //    Var<TBuilder> builder,
        //    System.Linq.Expressions.Expression<Func<TBuilder, HParams>> getParams,
        //    Action<BlockBuilder, Var<HParams>> customize)
        //{
        //    var hParams = b.Get(builder, getParams);
        //    customize(b, hParams);
        //}

        //public static void SetDefault<TBuilder>(
        //    this BlockBuilder b,
        //    Var<TBuilder> builder,
        //    System.Linq.Expressions.Expression<Func<TBuilder, HParams>> getParams,
        //    Func<BlockBuilder, Var<HParams>> setDefault)
        //{
        //    b.Set(builder, getParams, setDefault(b));
        //}

        public static void BindOneWay<TPageModel, TLocalModel, TBoundObject, TProperty>(
            this BindingContext<TPageModel, TLocalModel, TBoundObject> bindingContext,
            System.Linq.Expressions.Expression<Func<TBoundObject, TProperty>> intoProperty,
            System.Linq.Expressions.Expression<Func<TLocalModel, TProperty>> fromModel)
        {
            bindingContext.Add((b, context, bound) =>
            {
                var localModel = b.Get(context, x => x.InputData);
                var value = b.Get(localModel, fromModel);
                b.Set(bound, intoProperty, value);
            });
        }

        //public static void SetOnInput<TState>(this BlockBuilder b, Var<HParams> hParams, Var<HyperType.Action<TState, string>> onInput)
        //{
        //    var extractInputValue = b.MakeAction<TState, DomEvent<InputTarget>, string>((BlockBuilder b, Var<TState> state, Var<DomEvent<InputTarget>> @event) =>
        //    {
        //        var target = b.Get(@event, x => x.target);
        //        var value = b.Get(target, x => x.value);
        //        return b.MakeActionDescriptor<TState, string>(onInput, value);
        //    });

        //    b.SetProp(hParams, new DynamicProperty<HyperType.Action<TState, DomEvent<InputTarget>>>("oninput"), extractInputValue);
        //}

        //public static void SetOnClick<TState>(this BlockBuilder b,Var<HParams> hParams,Var<HyperType.Action<TState>> onClick)
        //{
        //    var clickEvent = b.MakeAction<TState, DomEvent<ClickTarget>>((BlockBuilder b, Var<TState> state, Var<DomEvent<ClickTarget>> @event) =>
        //    {
        //        b.StopPropagation(@event);
        //        return onClick;
        //    });

        //    b.SetProp(hParams, new DynamicProperty<HyperType.Action<TState, DomEvent<ClickTarget>>>("onclick"), clickEvent);
        //}

        public static Var<IVNode> Optional(this BlockBuilder b, Var<bool> ifValue, System.Func<BlockBuilder, Var<IVNode>> buildControl)
        {
            return b.If(
                ifValue,
                b => b.Call(buildControl),
                b => b.H(ViewBuilder.VoidNodeTag, b => { }));
        }


        public static Var<IVNode> Filter(
            this BlockBuilder b,
            Action<BlockBuilder, FilterBuilder> customize = null)
        {
            return b.BuildControl<FilterBuilder, Filter>(customize);
        }

        public static Var<IVNode> BuildControl<TBuilder, TData>(
            this BlockBuilder b,
            Action<BlockBuilder, TBuilder> customize = null)
            where TBuilder : CompoundBuilder<TData>, new()
            where TData : new()
        {
            var builder = ControlBuilder.New<TBuilder, TData>(b);
            if (customize != null)
            {
                customize(b, builder);
            }

            return builder.GetRoot(b);
        }


        //public static void BindFilter<TPageModel, TLocalModel>(
        //    this BindingContext<TPageModel, TLocalModel, Filter> bindingContext,
        //    System.Linq.Expressions.Expression<Func<TLocalModel, string>> fromModel)
        //{
        //    bindingContext.BindingActions.Add((b, context, bound) =>
        //    {
        //        var inputText = b.Get(bound, x => x.InputText);

        //        var localModel = b.Get(context, x => x.InputData);
        //        var value = b.Get(localModel, fromModel);
        //        b.SetProp(inputText, DynamicProperty.String("value"), value);
        //        b.SetOnInput(inputText, b.MakeAction((BlockBuilder b, Var<TPageModel> model, Var<string> newValue) =>
        //        {
        //            var dataItem = b.Call(b.Get(context, x => x.AccessData), model);
        //            b.Set(dataItem, fromModel, newValue);
        //            return b.Clone(model);
        //        }));
        //    });

        //    bindingContext.BindingActions.Add((b, context, bound) =>
        //    {
        //        var clearButton = b.Get(bound, x => x.ClearButton);
        //        b.SetOnClick(clearButton, b.MakeAction((BlockBuilder b, Var<TPageModel> model) =>
        //        {
        //            var dataItem = b.Call(b.Get(context, x => x.AccessData), model);
        //            b.Set(dataItem, fromModel, string.Empty);
        //            return b.Clone(model);
        //        }));
        //    });
        //}

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

        //public static void BindFilterValue<TPageModel, TContext, TControlProps>(
        //    this DataContextControlBuilder<TPageModel, TContext, TControlProps> builder,
        //    System.Linq.Expressions.Expression<Func<TContext, string>> onProperty)
        //    where TControlProps : IHasStringValue, IHasOnInputEvent
        //{
        //    builder.BindInput(x => x.Value, onProperty);
        //    builder.BindEvent(Metapsi.Hyperapp.EventExtensions.SetOnInput, onProperty);
        //}


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

        //public static Var<HyperNode> InputText(
        //    this BlockBuilder b,
        //    Action<ControlBuilder<InputText>> builder)
        //{
        //    return b.BuildControl(builder, (b, props) =>
        //    {
        //        var filterControl = b.Node("input");
        //        b.SetAttr(filterControl, Html.type, "text");
        //        return filterControl;
        //    });
        //}

        //public static void BindTextValue<TPageModel, TContext, TControlProps>(
        //    this DataContextControlBuilder<TPageModel, TContext, TControlProps> builder,
        //    System.Linq.Expressions.Expression<Func<TContext, string>> onProperty)
        //    where TControlProps : IHasStringValue, IHasOnInputEvent
        //{
        //    builder.BindInput(x => x.Value, onProperty);
        //    builder.BindEvent(Metapsi.Hyperapp.EventExtensions.SetOnInput, onProperty);
        //}

        //public static void BindEvent<TPageModel, TContext, TControlProps, TPayload>(
        //    this DataContextControlBuilder<TPageModel, TContext, TControlProps> builder,
        //    Action<BlockBuilder, Var<HyperNode>, Var<HyperType.Action<TPageModel, TPayload>>> eventSetter,
        //    System.Linq.Expressions.Expression<Func<TContext, TPayload>> onProperty)
        //{
        //    builder.ControlBuilderActions.Add((b, control) =>
        //    {
        //        eventSetter(b, control, b.MakeAction((BlockBuilder b, Var<TPageModel> pageModel, Var<TPayload> newValue) =>
        //        {
        //            var dataItem = b.Call(b.Get(builder.Context, x => x.AccessData), pageModel);
        //            b.Set(dataItem, onProperty, newValue);
        //            return b.Clone(pageModel);
        //        }));
        //    });
        //}
    }
}

