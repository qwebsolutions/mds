﻿using Metapsi.Syntax;
using Metapsi.Hyperapp;
using System.Collections.Generic;
using System.Linq;
using Metapsi;
using MdsCommon.Controls;
using MdsCommon;

namespace MdsLocal
{
    public static partial class MdsLocalMenu
    {
        public static Var<IVNode> LocalMenu(
            this LayoutBuilder b,
            string selectedCode)
        {
            var menuEntries = new List<MdsCommon.Menu.Entry>() {
                new ()
                {
                    Code = nameof(Overview),
                    Label = "Overview",
                    Href = Route.Path<Overview.ListProcesses>(),
                    SvgIcon = MdsCommon.Icon.Status
                },
                new ()
                {
                    Code = nameof(SyncHistory),
                    Label = "Sync history",
                    Href = Route.Path<SyncHistory.List>(),
                    SvgIcon = MdsCommon.Icon.ArrowPathRounded
                },
                new ()
                {
                    Code = nameof(MdsCommon.Routes.EventsLog) ,
                    Label = "Events log",
                    Href = Route.Path<MdsCommon.Routes.EventsLog.List>(),
                    SvgIcon = MdsCommon.Icon.History
                }
            };

            return b.Menu(b.Const(new MdsCommon.Controls.Menu.Props()
            {
                ActiveCode = selectedCode,
                Entries = menuEntries.ToList()
            }));
        }
    }
}
