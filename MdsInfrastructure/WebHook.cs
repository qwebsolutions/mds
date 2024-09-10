using Metapsi;
using System;

namespace MdsInfrastructure;

[DocDescription("HTTP web hook to be called when an infrastructure event occurs<br>",0)]
[DocDescription("&nbsp;&nbsp;<i>Name</i> - user defined identifier of the configured web hook<br>", 1)]
[DocDescription("&nbsp;&nbsp;<i>Type</i> - Type of web hook. At the moment, the supported type is <i>ServiceCrash</i>", 2)]
public class WebHook
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Url { get; set; }

    public static class WebHookType
    {
        public const string ServiceCrash = nameof(ServiceCrash);
    }
}
