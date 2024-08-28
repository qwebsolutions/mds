using Metapsi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
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

    public static async Task PostMessage<T>(this HttpClient httpClient, string baseUrl, T message)
    {
        var messageApiPath = baseUrl.TrimEnd('/') + "/" + TypeUrl<T>();
        var response = await httpClient.PostAsJsonAsync(messageApiPath, message);
        //response.EnsureSuccessStatusCode();
    }
}