using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MdsCommon.Controls
{
    public static class Badge
    {
        private static HashSet<string> alertTags = null;

        public static HashSet<string> AlertTags
        {
            get
            {
                if (alertTags == null)
                {
                    alertTags = new HashSet<string>();
                    alertTags.Add("critical");
                    alertTags.Add("fatal");
                    alertTags.Add("error");
                }

                return alertTags;
            }
        }

        public static Var<IVNode> AlertBadge(this LayoutBuilder b, Var<string> currentTag)
        {
            var lowercase = b.ToLowercase(currentTag);
            var isAlert = b.Get(
                b.Const(Badge.AlertTags),
                lowercase,
                (alertTags, lowercase) => alertTags.Select(x => x).Contains(lowercase));

            return b.Optional(
                isAlert,
                b =>
                {
                    return b.Badge(b.Const("ALERT"), b.Const("bg-red-600"));
                });
        }
    }
}

