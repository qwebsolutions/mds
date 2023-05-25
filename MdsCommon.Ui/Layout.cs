using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MdsCommon
{
    public static partial class Common
    {
        public static Var<HyperNode> Layout(
            this BlockBuilder b,
            Var<HyperNode> menu,
            Var<HyperNode> header,
            Var<HyperNode> page)
        {
            b.AddStylesheet("/static/tw.css");

            var rootNode = b.Node("div", "flex flex-row w-full h-screen");
            //b.AddLoadingPanel(rootNode);

            var layoutFlex = b.Add(rootNode, b.Div());
            b.Add(layoutFlex, menu);
            var rightArea = b.Add(rootNode, b.Div("w-full h-full flex flex-col"));
            var fixedHeader = b.Add(rightArea, b.Div("w-full bg-white drop-shadow px-8 py-4 z-40"));
            b.Add(fixedHeader, header);
            var pageScoller = b.Add(rightArea, b.Div("w-full h-full space-4 px-4 bg-gray-50 overflow-auto"));
            b.Add(pageScoller, b.Div("bg-gray-50 w-full h-4"));
            b.Add(pageScoller, page);
            return rootNode;
        }
    }
}