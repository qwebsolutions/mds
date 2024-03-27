using Metapsi.Html;
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

                        b.OnClickAction(props, b.MakeAction((SyntaxBuilder b, Var<object> state) =>
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
                    b.AddClass(props, "flex flex-row gap-2 items-center");
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

        internal static Var<IVNode> RenderTab<TPage>(
            this LayoutBuilder b,
            Var<TPage> page,
            Var<string> tabName,
            Var<IVNode> toolbar,
            params TabRenderer[] tabPages)
        {

            if (!tabPages.Any())
            {
                return b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("bg-white rounded drop-shadow");
                    });
            }

            var selectedTabPageCode = b.GetVar(page, FeatureName, tabName, b.Const(tabPages.First().TabPageCode));

            List<Var<IVNode>> tabLabels = new();

            var selectedTabContent = b.Ref(b.HtmlDiv(b => { }));

            foreach (var tab in tabPages)
            {
                var tabCode = b.Const(tab.TabPageCode);
                var isSelected = b.AreEqual(tabCode, selectedTabPageCode);

                var tabLabel = b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("p-4 cursor-pointer border-b rounded-t bg-white");

                        b.If(
                            isSelected, b =>
                            {
                                b.AddClass("border-b-2 border-sky-400");
                            },
                            b =>
                            {
                                b.AddClass("border-white hover:border-sky-400 hover:border-b-2");
                            });

                        b.OnClickAction((SyntaxBuilder b, Var<TPage> state) =>
                        {
                            b.SetSelectedTabPage(state, tabName, b.Const(tab.TabPageCode));
                            return b.Clone(state);
                        });
                    },
                    b.Call(tab.TabHeader));

                tabLabels.Add(tabLabel);

                b.If(
                    isSelected,
                    b =>
                    {
                        b.SetRef(selectedTabContent, b.Call(tab.TabContent));
                    });
            }

            var topArea = b.HtmlDiv(
                b =>
                b.SetClass("flex flex-row justify-between p-4"),
                b.HtmlDiv(
                    b =>
                    {
                        // labels
                        b.SetClass("flex flex-row gap-4");
                    },
                    tabLabels.ToArray()),
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

        public static void SetSelectedTabPage<TPage>(this SyntaxBuilder b, Var<TPage> page, Var<string> tabName, Var<string> tabPageCode)
        {
            b.SetVar(page, FeatureName, tabName, tabPageCode);
        }
    }
}

