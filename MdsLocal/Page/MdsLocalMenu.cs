using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsLocal
{
    public static partial class MdsLocalMenu
    {
        public static Var<HyperNode> LocalMenu(
            this BlockBuilder b,
            string selectedCode)
        {
            var menuEntries = new List<Menu.Entry>() {
                new Menu.Entry(){Code = nameof(Overview), Label = "Overview", Href = Path(Overview.ListProcesses)},
                new Menu.Entry(){Code = nameof(SyncHistory), Label = "Sync history", Href = Path(SyncHistory.List)},
                new Menu.Entry(){Code = nameof(EventsLog) , Label = "Events log", Href = Path(EventsLog.List) }
            };

            return b.Menu(b.Const(new Menu.Props()
            {
                ActiveCode = selectedCode,
                Entries = menuEntries.ToList()
            }));
        }

        public static string Path(Delegate d)
        {
            return Metapsi.WebServer.Path(d.Method);
        }
    }
}

