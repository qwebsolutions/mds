using MdsCommon;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsInfrastructure
{
    public static partial class MdsInfra
    {
        public static Var<HyperNode> InfraMenu(
            this BlockBuilder b,
            string selectedCode,
            bool isSignedIn)
        {
            var menuEntries = new List<Menu.Entry>();

            menuEntries.Add(new Menu.Entry() { Code = nameof(Status), Label = "Status", Href = Path(Status.Infra), SvgIcon = Icon.Status });
            if (isSignedIn)
            {
                menuEntries.Add(new Menu.Entry() { Code = nameof(Configuration), Label = "Configurations", Href = Path(Configuration.List), SvgIcon = Icon.Configuration });
            }

            menuEntries.Add(new Menu.Entry() { Code = nameof(Deployments), Label = "Deployments", Href = Path(Deployments.List), SvgIcon = Icon.Package });
            if (isSignedIn)
            {
                menuEntries.Add(new Menu.Entry() { Code = nameof(Nodes), Label = "Nodes", Href = Path(Nodes.List), SvgIcon = Icon.Computer });
                menuEntries.Add(new Menu.Entry() { Code = nameof(Projects), Label = "Projects", Href = Path(Projects.List), SvgIcon = Icon.DocumentText });
            }

            menuEntries.Add(new Menu.Entry() { Code = nameof(EventsLog), Label = "Events log", Href = Path(EventsLog.List), SvgIcon = Icon.History });

            return b.Menu(b.Const(new Menu.Props()
            {
                ActiveCode = selectedCode,
                Entries = menuEntries.ToList()
            }));
        }

        public static string Path(Delegate d)
        {
            return Metapsi.WebServer.RelativePath(d.Method);
        }
    }
}

