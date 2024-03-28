using MdsCommon.Controls;
using Metapsi;
using Metapsi.Dom;
using Metapsi.Html;
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

    //public interface IHasStringValue
    //{
    //    string Value { get; set; }
    //}

    //public interface IHasOnInputEvent { }
    //public interface IHasClearEvent { }

    public class Filter
    {
        public string Value { get; set; } = string.Empty;
    }

    public class FilterDefinition : IControlDefinition<Filter>
    {
        public ControlDefinition<Filter> Container { get; set; }
        public ControlDefinition<Filter> Input { get; set; }
        public ControlDefinition<Filter> ClearButton { get; set; }
        public ControlDefinition<Filter> ClearIcon { get; set; }

        public Func<LayoutBuilder, Var<Filter>, Var<IVNode>> GetRenderer() => Container.GetRenderer();
    }

    public static partial class Control
    {
        public static Var<IVNode> Filter<TModel>(this LayoutBuilder b,
            Var<TModel> model,
            System.Linq.Expressions.Expression<Func<TModel, string>> property)
        {
            var filterValue = b.Get(model, property);
            b.Log(filterValue);

            return b.HtmlDiv(
                b =>
                {
                    b.SetClass(b.Const("flex flex-row items-center relative"));
                },
                b.HtmlInput(
                    b =>
                    {
                        b.SetType("text");
                        b.SetClass("border rounded-full px-4 py-2");
                        b.SetValue(filterValue);
                        b.OnInputAction(b.MakeAction((SyntaxBuilder b, Var<TModel> model, Var<string> newValue) =>
                        {
                            b.Set(model, property, newValue);
                            return b.Clone(model);
                        }));
                    }),
                b.Optional(
                    b.HasValue(filterValue),
                    b =>
                    {
                        var clearFilterAction = b.MakeAction((SyntaxBuilder b, Var<TModel> model, Var<DomEvent> @event) =>
                        {
                            b.PreventDefault(@event);
                            b.StopPropagation(@event);
                            b.Set(model, property, string.Empty);
                            return b.MakeStateWithEffects(
                                b.Clone(model),
                                b =>
                                {
                                    var target = b.Get(@event, x => x.currentTarget);
                                    var htmlInput = b.GetProperty<DomElement>(target, b.Const("previousSibling"));
                                    b.CallOnObject(htmlInput, "focus");
                                });
                        });

                        return b.HtmlButton(
                            b =>
                            {
                                b.SetClass(b.Const("absolute right-3 w-6 h-6 text-gray-300"));
                                b.OnEventAction("mousedown", clearFilterAction);
                                b.OnEventAction("click", clearFilterAction);
                            },
                            b.Svg(Metapsi.Heroicons.Outline.XCircle));
                    }));
        }

        public static FilterDefinition DefaultFilter()
        {
            FilterDefinition filterBuilder = new();
            filterBuilder.Container = ControlDefinition.New<Filter>(
                "div",
                (b, data, props) =>
                {
                    b.SetClass(props, b.Const("flex flex-row items-center relative"));
                },
                (b, data) => b.Render(filterBuilder.Input, data),
                (b, data) => b.Optional(
                    b.HasValue(b.Get(data, x => x.Value)),
                    b => b.Render(filterBuilder.ClearButton, data)));

            filterBuilder.Input = ControlDefinition.New<Filter>(
                "input",
                (b, data, props) =>
                {
                    b.SetDynamic(props, Html.type, b.Const("text"));
                    b.SetDynamic(props, Html.value, b.Get(data, x => x.Value));
                    b.SetClass(props, b.Const("border rounded-full px-4 py-2"));
                });

            filterBuilder.ClearButton = ControlDefinition.New<Filter>(
                    "button",
                    (b, data, props) =>
                    {
                        b.SetClass(props, b.Const("absolute right-3 w-6 h-6 text-gray-300"));
                    },
                    (b, data) => b.Render(filterBuilder.ClearIcon, data));

            filterBuilder.ClearIcon = ControlDefinition.New<Filter>(
                "div",
                (b, data, props) =>
                {
                    b.SetDynamic(props, Html.innerHTML, b.Const(Metapsi.Heroicons.Outline.XCircle));
                });

            return filterBuilder;
        }

        public static Var<IVNode> Filter(this LayoutBuilder b, Action<ControlBuilder<FilterDefinition, Filter>> custom)
        {
            return b.FromDefinition(DefaultFilter, custom);
        }

        public static void BindFilter<TPageModel, TSubmodel>(
            this ControlBuilder<FilterDefinition, MdsCommon.HtmlControls.Filter> b,
            Var<DataContext<TPageModel, TSubmodel>> dataContext,
            System.Linq.Expressions.Expression<Func<TSubmodel, string>> property)
        {
            b.SetData((b, data) =>
            {
                b.InBindingContext(dataContext, data, b =>
                {
                    b.BindOneWay(x => x.Value, property);
                });
            });

            b.Control.Input.EditProps((b, props) =>
            {
                b.OnInputAction(
                    props,
                    b.MakeAction((SyntaxBuilder b, Var<TPageModel> pageModel, Var<string> newValue) =>
                    {
                        var accessData = b.Get(dataContext, x => x.AccessData);
                        var data = b.Call(accessData, pageModel);
                        b.Set(data, property, newValue);
                        return b.Clone(pageModel);
                    }));
            });

            b.Control.ClearButton.EditProps((b, props) =>
            {
                b.OnClickAction(props, b.MakeAction((SyntaxBuilder b, Var<TPageModel> pageModel) =>
                {
                    var accessData = b.Get(dataContext, x => x.AccessData);
                    var data = b.Call(accessData, pageModel);
                    b.Set(data, property, b.Const(string.Empty));
                    return b.Clone(pageModel);
                }));
            });
        }
    }

    public static partial class Control
    {
        public static Var<bool> ContainsValue<T>(this SyntaxBuilder b, Var<T> item, Var<string> value)
        {
            return b.Includes(b.ToLowercase(b.ConcatObjectValues(item)), b.ToLowercase(value));
        }

        public static Var<List<TItem>> FilterList<TItem>(
            this SyntaxBuilder b,
            Var<List<TItem>> list,
            Var<string> value)
        {
            var filteredItems = b.Get(
                list,
                value,
                b.Def<SyntaxBuilder, TItem, string, bool>(ContainsValue),
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

