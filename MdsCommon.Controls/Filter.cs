using MdsCommon.Controls;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Ui;
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

        public static Var<HyperNode> Filter<TPageModel, TContextModel>(
            this BlockBuilder b,
            Var<DataContext<TPageModel, TContextModel>> dataContext,
            Action<DataContextControlBuilder<TPageModel, TContextModel, Filter>> builder)
        {
            return b.BuildControl(dataContext, builder, (b, props) =>
            {
                var container = b.Div("flex flex-row items-center");
                var filterInputText = b.Add(container, b.InputText(dataContext, b =>
                {
                    b.ControlBuilderActions.Add((b, control) =>
                    {
                        b.AddClass(control, "border rounded-full px-4 py-2");
                    });
                }));


                var clearButton = b.Add(
                    container, b.Node(
                        "button",
                        "-mx-10 w-6 h-6 text-gray-300",
                        b => b.Svg(Metapsi.Heroicons.Outline.XCircle)));

                return container;
            });
        }

        public static void BindFilterValue<TPageModel, TContext, TControlProps>(
            this DataContextControlBuilder<TPageModel, TContext, TControlProps> builder,
            System.Linq.Expressions.Expression<Func<TContext, string>> onProperty)
            where TControlProps : IHasStringValue, IHasOnInputEvent
        {
            builder.BindInput(x => x.Value, onProperty);
            builder.BindEvent(Metapsi.Hyperapp.EventExtensions.SetOnInput, onProperty);
        }

        public static Var<HyperNode> InputText<TPageModel, TContextModel>(
            this BlockBuilder b,
            Var<DataContext<TPageModel, TContextModel>> dataContext,
            Action<DataContextControlBuilder<TPageModel, TContextModel, InputText>> builder)
        {
            return b.BuildControl(dataContext, builder, (b, props) =>
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

