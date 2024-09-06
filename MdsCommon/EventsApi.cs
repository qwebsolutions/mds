using Metapsi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MdsCommon;

public static class MessagingApi
{
    public static string TypeUrl(string typeName)
    {
        return typeName.Replace(".", "_").Replace("`", "_").Replace("+", "_");
    }

    public static string TypeUrl<T>()
    {
        return TypeUrl(typeof(T).Name);
    }

    public static void OnMessage<T>(this IEndpointRouteBuilder builder, Func<CommandContext, T, Task> onMessage)
    {
        builder.MapPost(TypeUrl<T>(), async (CommandContext commandContext, HttpContext httpContext, [FromBody] T message) =>
        {
            await onMessage(commandContext, message);
        }).AllowAnonymous();
    }

    public static async Task<HttpResponseMessage> PostMessage<T>(this HttpClient httpClient, string baseUrl, T message)
    {
        var messageApiPath = baseUrl.TrimEnd('/') + "/" + TypeUrl(message.GetType().Name);
        var response = await httpClient.PostAsJsonAsync(messageApiPath, message);
        return response;
    }

    public class HttpEventPoster
    {
        public HttpClient HttpClient { get; set; } = new();
        public Dictionary<string, string> DestinationMappings { get; set; } = new();
    }

    public class MessageEvent : IData
    {
        public string DestinationName { get; set; }
        public object Message { get; set; }
    }

    public class MappingEvent : IData
    {
        public string DestinationName { get; set; }
        public string Url { get; set; }
    }

    public static HttpEventPoster SetupMessagingApi(this ApplicationSetup setup, ImplementationGroup ig)
    {
        var httpEventPoster = setup.AddBusinessState(new HttpEventPoster());

        setup.MapEvent<MessageEvent>(e =>
        {
            e.Using(httpEventPoster, ig).EnqueueCommand(async (cc, state) =>
            {
                var url = httpEventPoster.DestinationMappings.GetValueOrDefault(e.EventData.DestinationName);
                if (!string.IsNullOrEmpty(url))
                {
                    var response = await state.HttpClient.PostMessage(url, e.EventData.Message);
                    response.EnsureSuccessStatusCode();
                }
            });
        });

        setup.MapEvent<MappingEvent>(e =>
        {
            e.Using(httpEventPoster, ig).EnqueueCommand(async (cc, state) =>
            {
                state.DestinationMappings[e.EventData.DestinationName] = e.EventData.Url;
            });
        });

        return httpEventPoster;
    }

    public static void MapMessaging(this CommandContext commandContext, string destinationName, string url)
    {
        commandContext.PostEvent(new MappingEvent()
        {
            DestinationName = destinationName,
            Url = url
        });
    }

    public static void NotifyGlobal(this CommandContext commandContext, object message)
    {
        commandContext.PostEvent(new MessageEvent()
        {
            DestinationName = "global",
            Message = message
        });
    }

    public static void NotifyNode(this CommandContext commandContext, string nodeName, object message)
    {
        commandContext.PostEvent(new MessageEvent()
        {
            DestinationName = nodeName,
            Message = message
        });
    }
}