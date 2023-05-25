using System.Collections.Generic;

namespace MdsCommon
{
    public static class AlertTag
    {
        public static HashSet<string> AlertTags()
        {
            HashSet<string> alertTags = new HashSet<string>
            {
                "critical",
                "fatal",
                "error"
            };

            return alertTags;
        }

        public static bool IsAlertTag(string tag)
        {
            return AlertTags().Contains(tag.ToLower());
        }
    }
}