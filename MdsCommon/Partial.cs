using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsCommon
{

    //public partial class InfrastructureEvent : MetapsiRuntime.IData
    //{
    //    //public string NodeName { get; set; }
    //}

    public static class Debug
    {
        public static void Log(string message)
        {
#if DEBUG
            System.Console.WriteLine(message);
#endif
        }

        public static void Log(string tag, object o)
        {
            Log($"{tag}: {Metapsi.Serialize.ToTypedJson(o)}");
        }
    }


    public static class PathParameter
    {
        public static void SetRelativeToFolder(string folder, Dictionary<string, string> parameters, string key)
        {
            if (parameters.ContainsKey(key))
            {
                string inputPath = parameters[key];

                if (!string.IsNullOrWhiteSpace(inputPath))
                {
                    if (!System.IO.Path.IsPathFullyQualified(inputPath))
                    {
                        string qualifiedPath = new System.IO.DirectoryInfo(System.IO.Path.Combine(folder, parameters[key])).FullName;
                        parameters[key] = qualifiedPath;
                    }
                }
            }
        }
    }
}