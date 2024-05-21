using Metapsi;
using Metapsi.Dom;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace MdsCommon;

public static class CallApiExtensions
{
    public static Var<HyperType.Effect> PostRequest<TModel, TResponse, TInput>(
        this SyntaxBuilder b,
        Request<TResponse, TInput> request,
        Var<TInput> input,
        Var<HyperType.Action<TModel, TResponse>> onSuccess)
    {
        var uri = "/api/" + request.Name;
        return b.PostJson(
            b.Const(uri),
            input,
            onSuccess,
            b.ShowAlertOnException<TModel>());
    }

    public static Var<HyperType.Effect> GetRequest<TModel, TResponse>(
        this SyntaxBuilder b,
        Request<TResponse, string> request,
        Var<string> p1,
        Var<HyperType.Action<TModel, TResponse>> onSuccess)
    {
        var uri = b.Concat(b.Const("/api/" + request.Name + "/"), p1);
        return b.GetJson(
            uri,
            onSuccess,
            b.ShowAlertOnException<TModel>());
    }

    public static Var<HyperType.Effect> GetRequest<TModel, TResponse>(
        this SyntaxBuilder b,
        Request<TResponse, Guid> request,
        Var<Guid> p1,
        Var<HyperType.Action<TModel, TResponse>> onSuccess)
    {
        var uri = b.Concat(b.Const("/api/" + request.Name + "/"), b.AsString(p1));
        return b.GetJson(
            uri,
            onSuccess,
            b.ShowAlertOnException<TModel>());
    }

    public static Var<HyperType.Effect> Request<TModel, TResponse>(
        this SyntaxBuilder b,
        Request<TResponse> request,
        Var<HyperType.Action<TModel, TResponse>> onSuccess)
    {
        var uri = "/api/" + request.Name;
        return b.GetJson(
            b.Const(uri),
            onSuccess,
            b.ShowAlertOnException<TModel>());
    }

    public static Var<HyperType.Action<TModel, ClientSideException>> ShowAlertOnException<TModel>(this SyntaxBuilder b)
    {
        return b.MakeAction((SyntaxBuilder b, Var<TModel> model, Var<ClientSideException> error) =>
        {
            b.CallOnObject(b.Window(), "alert", b.Get(error, x => x.message));
            return model;
        });
    }

    public static Var<TModel> ShowPanel<TModel>(this SyntaxBuilder b, Var<TModel> model)
    {
        return b.Clone(model);
    }

    public static RouteHandlerBuilder MapGetRequest<TOut>(this IEndpointRouteBuilder endpoint, Request<TOut> request, System.Func<CommandContext, HttpContext, Task<TOut>> handler)
    {
        return endpoint.MapGet(request.Name, handler);
    }

    public static RouteHandlerBuilder MapGetRequest<TOut>(this IEndpointRouteBuilder endpoint, Request<TOut, string> request, System.Func<CommandContext, HttpContext, string, Task<TOut>> handler)
    {
        return endpoint.MapGet(request.Name + "/" + "{p1}", async (CommandContext commandContext, HttpContext httpContext, string p1) => await handler(commandContext, httpContext, p1));
    }

    public static RouteHandlerBuilder MapGetRequest<TOut>(this IEndpointRouteBuilder endpoint, Request<TOut, Guid> request, System.Func<CommandContext, HttpContext, Guid, Task<TOut>> handler)
    {
        return endpoint.MapGet(request.Name + "/" + "{p1}", async (CommandContext commandContext, HttpContext httpContext, Guid p1) => await handler(commandContext, httpContext, p1));
    }

    public static RouteHandlerBuilder MapPostRequest<TIn, TOut>(this IEndpointRouteBuilder endpoint, Request<TOut, TIn> request, System.Func<CommandContext, HttpContext, TIn, Task<TOut>> handler)
    {
        return endpoint.MapPost(request.Name, handler);
    }

    public static RouteHandlerBuilder MapPostRequest<TOut>(this IEndpointRouteBuilder endpoint, Request<TOut> request, System.Func<CommandContext, HttpContext, Task<TOut>> handler)
    {
        return endpoint.MapPost(request.Name, handler);
    }
}
