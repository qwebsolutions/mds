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
using static MdsCommon.MessagingApi;

namespace MdsCommon;

public static class MessagingApi
{
    public static string TypeUrl(Type type)
    {
        return System.Web.HttpUtility.HtmlEncode(type.CSharpTypeName());
    }

    public static void OnMessage<T>(this IEndpointRouteBuilder builder, Func<CommandContext, T, Task> onMessage)
    {
        builder.MapPost(TypeUrl(typeof(T)), async (CommandContext commandContext, HttpContext httpContext, [FromBody] T message) =>
        {
            await onMessage(commandContext, message);
        }).AllowAnonymous();
    }

    public static async Task<HttpResponseMessage> PostMessage<T>(this HttpClient httpClient, string baseUrl, T message)
    {
        var messageApiPath = baseUrl.TrimEnd('/') + "/" + TypeUrl(message.GetType());
        var response = await httpClient.PostAsJsonAsync(messageApiPath, message);
        return response;
    }

    public class HttpEventPoster
    {
        public HttpClient HttpClient { get; set; } = new();

    }

    public class HttpAliasMapper
    {
        public Dictionary<string, string> DestinationMappings { get; set; } = new();

        public static Command<string, string> MapAlias { get; set; } = new Command<string, string>(nameof(MapAlias));
        public static Request<string, string> GetAlias { get; set; } = new Request<string, string>(nameof(GetAlias));
    }

    public class MessageEvent : IData
    {
        public string ToUrl { get; set; }
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
        var httpAliasMapper = setup.AddBusinessState(new HttpAliasMapper());
        ig.MapCommand(HttpAliasMapper.MapAlias, async (cc, alias, url) =>
        {
            await cc.Using(httpAliasMapper, ig).EnqueueCommand(async (cc, state) =>
            {
                state.DestinationMappings[alias] = url;
            });
        });

        ig.MapRequest(HttpAliasMapper.GetAlias, async (cc, alias) =>
        {
            return await cc.Using(httpAliasMapper, ig).EnqueueRequest(async (cc, state) =>
            {
                if (state.DestinationMappings.ContainsKey(alias))
                {
                    return state.DestinationMappings[alias];
                }
                return null;
            });
        });

        setup.MapEvent<MessageEvent>(e =>
        {
            e.Using(httpEventPoster, ig).EnqueueCommand(async (cc, state) =>
            {
                if (!string.IsNullOrEmpty(e.EventData.ToUrl))
                {
                    var response = await state.HttpClient.PostMessage(e.EventData.ToUrl, e.EventData.Message);
                    response.EnsureSuccessStatusCode();
#if DEBUG
                    await DebugTo.File(
                        "c:\\github\\qwebsolutions\\mds\\debug\\notify.txt",
                        $"{response.RequestMessage.RequestUri.ToString()} {response.StatusCode}");
                    //await DebugTo.File("c:\\github\\qwebsolutions\\mds\\debug\\notify.txt", response.StatusCode.ToString());
#endif
                }
            });
        });

        setup.MapEvent<MappingEvent>(e =>
        {
            e.Using(httpAliasMapper, ig).EnqueueCommand(async (cc, state) =>
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

    public static void NotifyUrl(this CommandContext commandContext, string baseUrl, object message)
    {
        var notAwaited = Task.Run(async () =>
        {
            try
            {
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    commandContext.PostEvent(new MessageEvent()
                    {
                        ToUrl = baseUrl,
                        Message = message
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        });
    }

    public static void NotifyGlobal(this CommandContext commandContext, object message)
    {
        var notAwaited = Task.Run(async () =>
        {
            try
            {
                var url = await commandContext.Do(HttpAliasMapper.GetAlias, "global");
                commandContext.NotifyUrl(url, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        });
    }

    public static void NotifyNode(this CommandContext commandContext, string nodeName, object message)
    {
        var notAwaited = Task.Run(async () =>
        {
            try
            {
                var url = await commandContext.Do(HttpAliasMapper.GetAlias, nodeName);
                commandContext.NotifyUrl(url, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        });
    }
}