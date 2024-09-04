using MdsCommon.Controls;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Collections.Generic;

namespace MdsCommon.Controls;

public static class TabDefinitionExtensions
{

    //public static void AddTab(
    //    this TabDefinition tabDefinition,
    //    string name,
    //    Func<LayoutBuilder, Var<IVNode>> renderHeader,
    //    Func<LayoutBuilder, Var<IVNode>> renderPanel)
    //{
    //    tabDefinition.TabHeaders.AddChild((b, data) =>
    //    {
    //        b.If(
    //            b.Not(b.HasValue(b.Get(data, x => x.SelectedTabName))),
    //            b =>
    //            {
    //                b.Set(data, x => x.SelectedTabName, b.Const(name));
    //            });

    //        return b.HtmlDiv(
    //            b =>
    //            {
    //                b.AddClass("cursor-pointer py-4");

    //                b.If(
    //                    b.AreEqual(b.Get(data, x => x.SelectedTabName), b.Const(name)),
    //                    b => b.AddClass("border-b-2 border-sky-400"),
    //                    b => b.AddClass("border-white hover:border-sky-400 hover:border-b-2"));

    //                b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<object> state) =>
    //                {
    //                    b.Set(data, x => x.SelectedTabName, b.Const(name));
    //                    return b.Clone(state);
    //                }));
    //            },
    //            b.H("div", (b, props) => b.AddClass(props, "px-2"), renderHeader(b)));
    //    });

    //    tabDefinition.TabPanels.AddChild((b, data) =>
    //    {
    //        return b.Optional(
    //            b.AreEqual(b.Get(data, x => x.SelectedTabName), b.Const(name)),
    //            renderPanel);
    //    });
    //}
}

public static partial class Control
{
    //public static TabDefinition DefaultTabs()
    //{
    //    TabDefinition tabDefinition = new TabDefinition();
    //    tabDefinition.Container = ControlDefinition.New<TabData>(
    //        "div",
    //        (b, data, props) =>
    //        {
    //            b.SetClass(props, "p-8 flex flex-col w-full h-full gap-8");
    //        },
    //        (b, data) => b.H(
    //            "div",
    //            (b, props) =>
    //            {
    //                b.SetClass(props, "flex flex-row justify-between items-center");
    //            },
    //            b.Render(tabDefinition.TabHeaders, data),
    //            b.Render(tabDefinition.Toolbar, data)),

    //        (b, data) => b.Render(tabDefinition.TabPanels, data));

    //    tabDefinition.TabHeaders = ControlDefinition.New<TabData>(
    //        "div",
    //        (b, data, props) =>
    //        {
    //            b.SetClass(props, "flex flex-row w-full h-full gap-8");
    //        });

    //    tabDefinition.TabPanels = ControlDefinition.New<TabData>(
    //        "div",
    //        (b, data, props) =>
    //        {
    //            b.SetClass(props, "flex flex-col w-full h-full");
    //        });

    //    tabDefinition.Toolbar = ControlDefinition.New<TabData>(
    //        "div",
    //        (b, data, props) =>
    //        {
    //            b.AddClass(props, "flex flex-row gap-2 items-center");
    //        });

    //    return tabDefinition;
    //}

    //public static Var<IVNode> Tabs(
    //    this LayoutBuilder b,
    //    Action<ControlBuilder<TabDefinition, TabData>> custom)
    //{
    //    return b.FromDefinition(DefaultTabs, custom, b => b.Const(new TabData()));
    //}
}

public static partial class TabControl
{
    public static Var<IVNode> Tabs(
        this LayoutBuilder b,
        Var<IVNode> toolbar,
        params Var<TabPair>[] tabPairs)
    {
        return b.Tabs(toolbar, b.List(tabPairs));
    }

    public static Var<IVNode> Tabs(
        this LayoutBuilder b,
        Var<IVNode> toolbar,
        Var<List<TabPair>> tabs)
    {
        var reference = b.GlobalRef(-1);
        b.If(
            b.AreEqual(
                b.GetRef(reference), b.Const(-1)),
            b =>
            {
                b.SetRef(reference, b.Const(0));
            });

        var selectedIndex = b.GetRef(reference);

        Var<List<IVNode>> tabLabels = b.NewCollection<IVNode>();

        var selectedTabContent = b.Ref(b.HtmlDiv(b => { }));

        b.Foreach(tabs, (b, tab, index) =>
        {
            var isSelected = b.AreEqual(index, selectedIndex);

            var tabLabel = b.HtmlDiv(
                b =>
                {
                    b.SetClass("p-4 cursor-pointer border-b rounded-t bg-white");

                    b.If(
                        isSelected, b =>
                        {
                            b.AddClass("border-b-2 border-sky-400");
                            b.SetRef(selectedTabContent, b.Get(tab, x => x.TabContent));
                        },
                        b =>
                        {
                            b.AddClass("border-white hover:border-sky-400 hover:border-b-2");
                        });

                    b.OnClickAction((SyntaxBuilder b, Var<object> state) =>
                    {
                        b.SetRef(reference, index);
                        return b.Clone(state);
                    });
                },
                b.Get(tab, x=>x.TabHeader));

            b.Push(tabLabels, tabLabel);
        });

        var topArea = b.HtmlDiv(
            b =>
            b.SetClass("flex flex-row justify-between p-4"),
            b.HtmlDiv(
                b =>
                {
                    // labels
                    b.SetClass("flex flex-row gap-4");
                },
                tabLabels),
            toolbar);

        return b.HtmlDiv(
                b =>
                {
                    b.SetClass("bg-white rounded drop-shadow");
                },
                topArea,
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-column p-8");
                    },
                    b.GetRef(selectedTabContent)));
    }

    public class TabPair
    {
        public IVNode TabHeader { get; set; }
        public IVNode TabContent { get; set; }
    }
}

