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

            return b.Menu(b.Const(new Menu.Props()
            {
                ActiveCode = selectedCode,
                //ActiveCode = nameof(Routes.Status),
                Entries = menuEntries.ToList()
            }));
        }
    }
}

