using MdsCommon;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using MdsCommon.Controls;

namespace MdsInfrastructure
{
    public static partial class MdsInfra
    {
        public static Var<HyperNode> InfraMenu(
            this LayoutBuilder b,
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
                SvgIcon = MdsCommon.Icon.Status
            });

            if (isSignedIn)
            {
                menuEntries.Add(new()
                {
                    Code = nameof(Routes.Configuration),
                    Label = "Configurations",
                    Href = Metapsi.Route.Path<Routes.Configuration.List>(),
                    SvgIcon =MdsCommon.Icon.Configuration
                });
            }

            menuEntries.Add(new()
            {
                Code = nameof(Routes.Node),
                Label = "Nodes",
                Href = Metapsi.Route.Path<Routes.Node.List>(),
                SvgIcon = MdsCommon.Icon.Computer
            });

            if (isSignedIn)
            {
                menuEntries.Add(new()
                {
                    Code = nameof(Routes.Deployment),
                    Label = "Deployments",
                    Href = Metapsi.Route.Path<Routes.Deployment.List>(),
                    SvgIcon = MdsCommon.Icon.Package
                });
            }

            if (isSignedIn)
            {
                menuEntries.Add(new()
                {
                    Code = nameof(Routes.Project),
                    Label = "Projects",
                    Href = Metapsi.Route.Path<Routes.Project.List>(),
                    SvgIcon = MdsCommon.Icon.DocumentText
                });
            }

            menuEntries.Add(new()
            {
                Code = nameof(MdsCommon.Routes.EventsLog),
                Label = "Events log",
                Href = Metapsi.Route.Path<MdsCommon.Routes.EventsLog.List>(),
                SvgIcon = MdsCommon.Icon.History
            });

            return b.Menu(b.Const(new Menu.Props()
            {
                ActiveCode = selectedCode,
                //ActiveCode = nameof(Routes.Status),
                Entries = menuEntries.ToList()
            }));
        }
    }
}

