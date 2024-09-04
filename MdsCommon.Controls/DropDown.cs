using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.TomSelect;
using System;
using System.Collections.Generic;
using System.Linq;
using Metapsi.Shoelace;
using Metapsi.Html;
using Metapsi;

namespace MdsCommon.Controls;


public static partial class Control
{
    public static Var<IVNode> MdsDropDown(this LayoutBuilder b, Action<PropsBuilder<TomSelect>> buildProps)
    {
        StaticFiles.Add(typeof(Control).Assembly, "MdsCommon.TomSelect.css");
        return b.TomSelect(
            b =>
            {
                b.AddStyle("--sl-input-font-family", "poppins");
                b.Set(b.Props, x => x.cssPaths, new List<string>()
                {
                    "/MdsCommon.TomSelect.css"
                });

                b.SetRenderFunction("option", (SyntaxBuilder b, Var<TomSelectOption> option, Var<Func<string, string>> escape) =>
                {
                    return b.Concat(
                        b.Const(
                            "<div class='item' style='display: flex; flex-direction:row; align-items: baseline;'>"),
                        b.Const("<sl-icon library='system' name='check' style='padding-inline-end: var(--sl-spacing-2x-small);'></sl-icon>"),
                        b.Const("<div>"),
                        b.Call(escape, b.Get(option, x => x.text)),
                        b.Const("</div>"),
                        b.Const("</div>;"));
                });

                b.Configure(x => x.editTsControl, b.Def((SyntaxBuilder b, Var<DomElement> tsControl) =>
                {
                    var svg = b.Const("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-chevron-down\" viewBox=\"0 0 16 16\" part=\"svg\">\r\n      <path fill-rule=\"evenodd\" d=\"M1.646 4.646a.5.5 0 0 1 .708 0L8 10.293l5.646-5.647a.5.5 0 0 1 .708.708l-6 6a.5.5 0 0 1-.708 0l-6-6a.5.5 0 0 1 0-.708z\"></path>\r\n    </svg>");
                    var container = b.CreateElement(b.Const("div"));
                    b.SetAttribute(container, b.Const("class"), b.Const("chevron"));
                    b.SetProperty(container, b.Const("innerHTML"), svg);
                    b.AppendChild(tsControl, container);
                }));

                buildProps(b);
            });
    }

    public static Var<IVNode> MdsInputText(this LayoutBuilder b, Action<PropsBuilder<SlInput>> buildProps)
    {
        return b.SlInput(
            b =>
            {
                b.AddStyle("--sl-input-font-family", "poppins");
                buildProps(b);
            });
    }

    public static Var<IVNode> MdsMainPanel(this LayoutBuilder b, Action<PropsBuilder<HtmlDiv>> buildProps, params Var<IVNode>[] children)
    {
        return b.HtmlDiv(b =>
        {
            b.SetClass("p-4 bg-white rounded drop-shadow");
            buildProps(b);
        }, children);
    }

    //public static Var<IVNode> DropDown(this LayoutBuilder b, Var<List<Metapsi.ChoicesJs.Choice>> choices)
    //{
    //    b.AddStylesheet("MdsCommon.Controls.Choices.css");

    //    return b.Choices(b =>
    //    {
    //        b.Configure(x => x.classNames, b.CustomClassNames());
    //        b.Configure(x => x.shouldSort, false);
    //        b.Configure(x => x.choices, choices);
    //    });
    //}


//    public static Var<IVNode> BoundDropDown<TState,TItem, TOption, TId>(
//        this LayoutBuilder b, 
//        Var<TState> state,
//        Var<Func<TState, TItem>> getBoundItem,
//        System.Linq.Expressions.Expression<Func<TItem, TId>> getBoundProperty,
//        Var<Func<TState, List<TOption>>> getOptionsSource,
//        Var<Func<TOption, TId>> getValue,
//        Var<Func<TOption, string>> getLabel)
//    {
//        var item = b.Call(getBoundItem, state);
//        var options = b.Call(getOptionsSource, state);
//        var selectedValue = b.Get(item, getBoundProperty);

//        var choices = b.MapChoices(options, getValue, getLabel, selectedValue);

//        return b.Choices(b =>
//        {
//            b.Configure(x => x.choices, choices);
//            b.OnChange(b.MakeAction((SyntaxBuilder b, Var<TState> model, Var<string> choice) =>
//            {
//                var item = b.Call(getBoundItem, state);
//                b.Set(item, getBoundProperty, b.ParseScalar<TId>(choice));

//                return b.Clone(state);
//            }));
//        });
//    }

//    private static Var<ClassNames> CustomClassNames(this SyntaxBuilder b)
//    {
//        var customClassNames = new ClassNames()
//        {
//            containerOuter = "MdsDropDown",
//            containerInner = "MdsDropDown__inner",
//            input = "MdsDropDown__input",
//            inputCloned = "MdsDropDown__input--cloned",
//            list = "MdsDropDown__list",
//            listItems = "MdsDropDown__list--multiple",
//            listSingle = "MdsDropDown__list--single",
//            listDropdown = "MdsDropDown__list--dropdown",
//            item = "MdsDropDown__item",
//            itemSelectable = "MdsDropDown__item--selectable",
//            itemDisabled = "MdsDropDown__item--disabled",
//            itemOption = "MdsDropDown__item--choice",
//            group = "MdsDropDown__group",
//            groupHeading = "MdsDropDown__heading",
//            button = "MdsDropDown__button",
//            activeState = "is-active",
//            focusState = "is-focused",
//            openState = "is-open",
//            disabledState = "is-disabled",
//            highlightedState = "is-highlighted",
//            selectedState = "is-selected",
//            flippedState = "is-flipped"
//        };

//        return b.Const(customClassNames);
//    }
//}

//public static class DropDown
//{
//    public class Props
//    {
//        public string Id  = string.Empty; // The ID is mandatory
//        public string Value  = string.Empty;
//        public string Placeholder  = string.Empty;
//        public List<Option> Options 
//        public HyperType.Action<object, string> OnChanged 
//        public bool Enabled  = true;
//    }

//    public class Option
//    {
//        public string value 
//        public string label 
//        public bool selected 
//        public bool disabled 
//    }

//    /// <summary>
//    /// Needed in the forward of event
//    /// </summary>
//    private class ChoiceData
//    {
//        public List<Option> Options 
//        public HyperType.Action<object, string> OnChanged 
//        public bool Enabled 
//    }

//    internal static Var<HyperNode> Render(BlockBuilder b, Var<Props> props)
//    {
//        b.AddScript("/choices.min.js");
//        b.AddStylesheet("/choices.min.css");
//        b.AddModuleStylesheet();

//        const string import = "choices";

//        var renderAction = b.ModuleBuilder.AddImport(import, "renderDropDown").As<HyperType.Action<object, string>>();
//        var choiceAction = b.ModuleBuilder.AddImport(import, "onChoiceAction").As<HyperType.Action<object, string>>();

//        b.AddSubscription<object>(
//            "onDropDownRender",
//            (BlockBuilder b, Var<object> state) => b.Listen(b.Const("afterRender"), renderAction));

//        b.AddSubscription<object>(
//            "onDropDownChoice",
//            (BlockBuilder b, Var<object> state) => b.Listen(b.Const("onChoice"), choiceAction));

//        var selectId = b.Get(props, props => props.Id);

//        var container = b.Span();
//        b.SetAttr(container, Html.id, selectId);
//        b.If(b.Get(props, x => x.Enabled), b => b.AddClass(container, "cursor-pointer"));
//        var options = b.Get(props, x => x.Options);

//        var selectedValue = b.Get(props, x => x.Value);
//        var selectedOption = b.Get(options, selectedValue, (options, selectedValue) => options.SingleOrDefault(x => x.value == selectedValue));
//        b.If(b.HasObject(selectedOption), b => b.Set(selectedOption, x => x.selected, b.Const(true)));

//        var choiceData = b.NewObj<ChoiceData>();
//        b.Set(choiceData, x => x.Enabled, b.Get(props, x => x.Enabled));
//        b.Set(choiceData, x => x.Options, options);
//        b.Set(choiceData, x => x.OnChanged, b.Get(props, props => props.OnChanged));
//        b.CallExternal(import, "addDropDown", selectId, choiceData);

//        return container;
//    }

//    internal static Var<List<Option>> WithNotSelected(this BlockBuilder b, Var<List<Option>> options, Var<string> selectedValue)
//    {
//        var shouldAddNotSelected =
//            b.If(b.IsEmpty(selectedValue),
//            b => b.Const(true),
//            b => b.If(b.IsEmpty(selectedValue.As<System.Guid>()), b => b.Const(true), b => b.Const(false)));

//        return b.If(shouldAddNotSelected,
//            b =>
//            {
//                var outOptions = b.NewCollection<Option>();
//                b.Push(outOptions, b.Const(new Option() { disabled = true, label = "(not selected)", selected = true, value = string.Empty }));
//                b.Foreach(options, (b, option) => b.Push(outOptions, option));
//                return outOptions;
//            },
//            b =>
//            {
//                var outOptions = b.NewCollection<Option>();
//                b.Foreach(options, (b, option) => {
//                    b.If(b.AreEqual(selectedValue, b.Get(option, x => x.value)),
//                        b =>
//                        {
//                            b.Set(option, x => x.selected, b.Const(true));
//                        },
//                        b =>
//                        {
//                            b.Set(option, x => x.selected, b.Const(false));
//                        });
//                    b.Push(outOptions, option);
//                });
//                return outOptions;
//            });
//    }

//    public static Var<string> ReplaceIfEmptyGuid(this BlockBuilder b, Var<string> selectedId)
//    {
//        return b.If<string>(b.IsEmpty(selectedId.As<System.Guid>()), b => b.Const(string.Empty), b => selectedId);
//    }

//    public static Var<System.Func<TSource, DropDown.Option>> TransformBy<TSource, TProp>(
//        this BlockBuilder b,
//        System.Linq.Expressions.Expression<System.Func<TSource, TProp>> valueSelector,
//        System.Linq.Expressions.Expression<System.Func<TSource, string>> labelSelector)
//    {
//        return b.Def((BlockBuilder b, Var<TSource> source) =>
//        {
//            var label = b.Get(source, labelSelector);
//            var value = b.Get(source, valueSelector);
//            return b.NewObj<DropDown.Option>(b =>
//            {
//                b.Set(x => x.label, label);
//                b.Set(x => x.value, value.As<string>());
//            });
//        });
//    }
//}

//public static partial class Controls
//{
//    public static Var<HyperNode> DropDown(
//       this BlockBuilder b,
//       Var<string> dropDownId,
//       Var<string> value,
//       Var<List<DropDown.Option>> options,
//       Var<System.Action<string>> onInput,
//       Var<string> placeholder,
//       System.Action<Modifier<DropDown.Props>> optional = null)
//    {
//        var withNotSelected = b.WithNotSelected(options, value);
//        var replacedValue = b.ReplaceIfEmptyGuid(value);

//        var onChangedAction = b.MakeAction((BlockBuilder b, Var<object> page, Var<string> payload) =>
//        {
//            b.Call(onInput, payload);
//            return b.Clone(page);
//        });

//        var props = b.NewObj<DropDown.Props>(b =>
//        {
//            b.Set(x => x.Id, dropDownId);
//            b.Set(x => x.Value, replacedValue);
//            b.Set(x => x.OnChanged, onChangedAction);
//            b.Set(x => x.Options, withNotSelected);
//            b.Set(x => x.Placeholder, placeholder);
//          });

//        b.Modify(props, optional);

//        return b.Call(MdsCommon.Controls.DropDown.Render, props);
//    }

//    public static Var<HyperNode> DropDown<TState>(
//       this BlockBuilder b,
//       Var<string> dropDownId,
//       Var<string> value,
//       Var<List<DropDown.Option>> options,
//       Var<HyperType.Action<TState, string>> onInput,
//       Var<string> placeholder,
//       System.Action<Modifier<DropDown.Props>> optional = null)
//    {
//        var withNotSelected = b.WithNotSelected(options, value);
//        var replacedValue = b.ReplaceIfEmptyGuid(value);

//        var props = b.NewObj<DropDown.Props>(b =>
//        {
//            b.Set(x => x.Id, dropDownId);
//            b.Set(x => x.Value, replacedValue);
//            b.Set(x => x.OnChanged, onInput.As<HyperType.Action<object, string>>());
//            b.Set(x => x.Options, withNotSelected);
//            b.Set(x => x.Placeholder, placeholder);
//        });

//        b.Modify(props, optional);

//        return b.Call(MdsCommon.Controls.DropDown.Render, props);
//    }

//    public static Var<HyperNode> BoundDropDown<TObject, TOption, TProp>(
//        this BlockBuilder b,
//        Var<string> dropDownName,
//        Var<TObject> obj,
//        Expression<System.Func<TObject, TProp>> onProperty,
//        Var<List<TOption>> optionsSource,
//        Var<System.Func<TOption, DropDown.Option>> transform,
//        System.Action<Modifier<DropDown.Props>> optional = null)
//    {
//        var selectedValue = b.Get(obj, onProperty).As<string>();
//        var dropDownOptions = b.Get(optionsSource, transform, (x, transform) => x.Select(y => transform(y)).ToList());

//        var onInputConverted = b.Def(
//            (BlockBuilder b, Var<string> stringValue) =>
//            {
//                b.Set(obj, onProperty, stringValue.As<TProp>());
//            });

//        return b.DropDown(dropDownName, selectedValue, dropDownOptions, onInputConverted, b.Const(string.Empty), optional);
//    }

//    public static Var<HyperNode> BoundDropDown<TObject, TOption, TProp>(
//        this BlockBuilder b,
//        Var<string> dropDownName,
//        Var<TObject> obj,
//        Expression<System.Func<TObject, TProp>> onProperty,
//        Var<List<TOption>> optionsSource,
//        Expression<System.Func<TOption, TProp>> valueSelector,
//        Expression<System.Func<TOption, string>> labelSelector,
//        System.Action<Modifier<DropDown.Props>> optional = null)
//    {
//        return b.BoundDropDown(
//            dropDownName, 
//            obj, 
//            onProperty, 
//            optionsSource, 
//            b.TransformBy(valueSelector, labelSelector),
//            optional);
//    }
//}
}