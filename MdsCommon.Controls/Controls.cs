﻿using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MdsCommon.Controls
{
    // This empty class is here for copy-paste
    public static partial class Controls
    {
    }

    public static partial class Control
    {
        public static Var<HyperNode> Badge(
            this BlockBuilder b,
            Var<string> label)
        {
            var badge = b.Text(label);
            b.AddClass(badge, "uppercase text-xs text-white p-1 mx-4 font-medium rounded-md");
            return badge;
        }


        public static Var<HyperNode> Tabs<TPage>(
            this BlockBuilder b,
            Var<TPage> page,
            Var<string> tabName,
            Var<HyperNode> toolbar,
            params TabRenderer[] tabPages)
        {
            return b.RenderTab(page, tabName, toolbar, tabPages);
        }

    }

    public class TabRenderer
    {
        public string TabPageCode { get; set; } = string.Empty;
        public System.Func<BlockBuilder, Var<HyperNode>> TabHeader { get; set; }
        public System.Func<BlockBuilder, Var<HyperNode>> TabContent { get; set; }
    }
}
