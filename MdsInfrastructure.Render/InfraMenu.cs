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
            bool isSignedIn
            )
        {
            var menuEntries = new List<Metapsi.Ui.Menu.Entry>();

            menuEntries.Add(new()
            {
                Code = nameof(Routes.Status),
                Label = "Status",
                Href = Metapsi.Route.Path<Routes.Status.Infra>(),
                SvgIcon = string.Empty /*Icon.Status*/
            });
            //if (isSignedIn)
            //{
            //    menuEntries.Add(new Menu.Entry() { Code = nameof(Configuration), Label = "Configurations", Href = Path<Configuration.List>(), SvgIcon = string.Empty /*Icon.Configuration*/ });
            //}

            //menuEntries.Add(new Menu.Entry() { Code = nameof(Deployments), Label = "Deployments", Href = Path<Deployments.List>(), SvgIcon = string.Empty /*Icon.Package*/ });
            //if (isSignedIn)
            //{
            //    menuEntries.Add(new Menu.Entry() { Code = nameof(Nodes), Label = "Nodes", Href = Path<Nodes.List>(), SvgIcon = string.Empty /*Icon.Computer*/ });
            //    menuEntries.Add(new Menu.Entry() { Code = nameof(Projects), Label = "Projects", Href = Path<Projects.List>(), SvgIcon = string.Empty /*Icon.DocumentText*/ });
            //}

            //menuEntries.Add(new Menu.Entry() { Code = nameof(EventsLog), Label = "Events log", Href = Path<EventsLog.List>(), SvgIcon = string.Empty /*Icon.History*/ });

            return b.Menu(b.Const(new Menu.Props()
            {
                ActiveCode = selectedCode,
                //ActiveCode = nameof(Routes.Status),
                Entries = menuEntries.ToList()
            }));
        }
    }
}

