using Metapsi;
using System;

namespace MdsInfrastructure;

[DocDescription("HTTP webhook to be called when an infrastructure event occurs. Matches contracts in Mds.Webhook<br><br>",0)]
[DocDescription("<i>Name</i> - user defined identifier. Multiple webhooks can be configured for the same event<br>", 1)]
[DocDescription("<i>Type</i> - Type of web hook.<br>", 2)]
[DocDescription("<i>Url</i> - Url to post event to", 3)]
public class WebHook
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Url { get; set; }
}
