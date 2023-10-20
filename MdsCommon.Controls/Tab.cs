using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MdsCommon.Controls
{
    public class TabData
    {
        public string SelectedTabName { get; set; }
    }

    public class TabDefinition : IControlDefinition<TabData>
    {
        public ControlDefinition<TabData> Container { get; set; }
        public ControlDefinition<TabData> TabHeaders { get; set; }
        public ControlDefinition<TabData> TabPanels { get; set; }
        public ControlDefinition<TabData> Toolbar { get; set; }

        public Func<LayoutBuilder, Var<TabData>, Var<IVNode>> GetRenderer()
        {
            return Container.GetRenderer();
        }
    }

    public static class TabDefinitionExtensions
    {
        public static void AddTab(
            this ControlBuilder<TabDefinition, TabData> builder,
            string name,
            Func<LayoutBuilder, Var<IVNode>> renderPanel)
        {
            builder.Control.AddTab(name, b => b.T(name), renderPanel);
        }

        public static void AddToolbarCommand(
            this ControlBuilder<TabDefinition, TabData> builder,
            Func<LayoutBuilder, Var<IVNode>> renderCommand)
        {
            builder.Control.Toolbar.AddChild(renderCommand);
        }

        public static void AddTab(
           this TabDefinition tabDefinition,
           string name,
           Func<LayoutBuilder, Var<IVNode>> renderPanel)
        {
            AddTab(tabDefinition, name, b => b.T(name), renderPanel);
        }
            
        public static void AddTab(
            this TabDefinition tabDefinition,
            string name,
            Func<LayoutBuilder, Var<IVNode>> renderHeader,
            Func<LayoutBuilder, Var<IVNode>> renderPanel)
        {
            tabDefinition.TabHeaders.AddChild((b, data) =>
            {
                b.If(
                    b.Not(b.HasValue(b.Get(data, x => x.SelectedTabName))),
                    b =>
                    {
                        b.Set(data, x => x.SelectedTabName, b.Const(name));
                    });

                return b.H(
                    "div",
                    (b, props) =>
                    {
                        b.AddClass(props, "cursor-pointer py-4");

                        b.If(
                            b.AreEqual(b.Get(data, x => x.SelectedTabName), b.Const(name)),
                            b => b.AddClass(props, "border-b-2 border-sky-400"),
                            b => b.AddClass(props, "border-white hover:border-sky-400 hover:border-b-2"));

                        b.OnClickAction(props, b.MakeAction((BlockBuilder b, Var<object> state) =>
                        {
                            b.Set(data, x => x.SelectedTabName, b.Const(name));
                            return b.Clone(state);
                        }));
                    },
                    b.H("div", (b, props) => b.AddClass(props, "px-2"), renderHeader(b)));
            });

            tabDefinition.TabPanels.AddChild((b, data) =>
            {
                return b.Optional(
                    b.AreEqual(b.Get(data, x => x.SelectedTabName), b.Const(name)),
                    renderPanel);
            });
        }
    }

    public static partial class Control
    {
        public static TabDefinition DefaultTabs()
        {
            TabDefinition tabDefinition = new TabDefinition();
            tabDefinition.Container = ControlDefinition.New<TabData>(
                "div",
                (b, data, props) =>
                {
                    b.SetClass(props, "p-8 flex flex-col w-full h-full gap-8");
                },
                (b, data) => b.H(
                    "div",
                    (b, props) =>
                    {
                        b.SetClass(props, "flex flex-row justify-between items-center");
                    },
                    b.Render(tabDefinition.TabHeaders, data),
                    b.Render(tabDefinition.Toolbar, data)),

                (b, data) => b.Render(tabDefinition.TabPanels, data));

            tabDefinition.TabHeaders = ControlDefinition.New<TabData>(
                "div",
                (b, data, props) =>
                {
                    b.SetClass(props, "flex flex-row w-full h-full gap-8");
                });

            tabDefinition.TabPanels = ControlDefinition.New<TabData>(
                "div",
                (b, data, props) =>
                {
                    b.SetClass(props, "flex flex-col w-full h-full");
                });

            tabDefinition.Toolbar = ControlDefinition.New<TabData>(
                "div",
                (b, data, props) =>
                {
                    b.AddClass(props, "flex flex-row gap-2");
                });

            return tabDefinition;
        }

        public static Var<IVNode> Tabs(
            this LayoutBuilder b,
            Action<ControlBuilder<TabDefinition, TabData>> custom)
        {
            return b.FromDefinition(DefaultTabs, custom, b => b.Const(new TabData()));
        }
    }

    
    public static partial class Tab
    {
        private const string FeatureName = "Tabs";
        //private const string SelectedTabKey = "SelectedTab";

        //private static string GetTabKey(string tabName)
        //{
        //    return $"{FeatureName}.{tabName}";
        //}

        internal static Var<HyperNode> RenderTab<TPage>(
            this BlockBuilder b, 
            Var<TPage> page, 
            Var<string> tabName,
            Var<HyperNode> toolbar,
            params TabRenderer[] tabPages)
        {
            var rootContainer = b.Div("bg-white rounded drop-shadow");

            if (tabPages.Any())
            {
                var selectedTabPageCode = b.GetVar(page, FeatureName, tabName, b.Const(tabPages.First().TabPageCode));

                var topArea = b.Add(rootContainer, b.Div("flex flex-row justify-between p-4"));

                var labels = b.Add(topArea, b.Div("flex flex-row gap-4"));

                var commands = b.Add(topArea, toolbar);

                var contentContainer = b.Add(rootContainer, b.Div("flex flex-column p-8"));

                foreach (var tab in tabPages)
                {
                    var tabLabel = b.Div("p-4 cursor-pointer border-b rounded-t bg-white");
                    var tabCode = b.Const(tab.TabPageCode);

                    b.SetOnClick<TPage>(
                        tabLabel,
                        b.MakeAction((BlockBuilder b, Var<TPage> state) =>
                        {
                            b.SetSelectedTabPage(state, tabName, b.Const(tab.TabPageCode));
                            return b.Clone(state);
                        }));

                    b.Add(tabLabel, b.Call(tab.TabHeader));

                    b.Add(labels, tabLabel);

                    var isSelected = b.AreEqual<string>(tabCode, selectedTabPageCode);

                    b.If(isSelected, b =>
                    {
                        b.AddClass(tabLabel, "border-b-2 border-sky-400");
                        b.Add(contentContainer, b.Call(tab.TabContent));
                    }, b =>
                    {
                        b.AddClass(tabLabel, "border-white hover:border-sky-400 hover:border-b-2");
                    });
                }
            }
            return rootContainer;
        }

        public static void SetSelectedTabPage<TPage>(this BlockBuilder b, Var<TPage> page, Var<string> tabName, Var<string> tabPageCode)
        {
            b.SetVar(page, FeatureName, tabName, tabPageCode);
        }
    }
}

