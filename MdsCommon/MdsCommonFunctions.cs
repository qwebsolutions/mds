using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MdsCommon
{
    public static class Keys
    {
        public const string SelectedInfrastructureEventId = "SelectedInfrastructureEventId";

        public static string Persisted(string key)
        {
            return $"Persisted.{key}";
        }
    }

}