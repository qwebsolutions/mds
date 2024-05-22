using Metapsi;
using Metapsi.Dom;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using System;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MdsCommon;

public static class CallApiExtensions
{
    public static Var<HyperType.Effect> GetCommand<TModel, TInput>(
        this SyntaxBuilder b,
        Command<TInput> command,
        Var<TInput> input,
        Var<HyperType.Action<TModel>> onSuccess)
    {
        var uri = b.Concat(b.Const("/api/" + command.Name + "/"), b.AsString(input));
        return b.Fetch(
            uri,
            b => { },
            onSuccess,
            b.ShowAlertOnException<TModel>());
    }

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

    public static Var<HyperType.Effect> PostRequest<TModel, TResponse, TInput>(
        this SyntaxBuilder b,
        Request<TResponse, TInput> request,
        Var<TInput> input,
        System.Func<SyntaxBuilder, Var<TModel>, Var<TResponse>, Var<TModel>> onSuccess)
    {
        var uri = "/api/" + request.Name;
        return b.PostJson(
            b.Const(uri),
            input,
            b.MakeAction(onSuccess),
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

    public static Var<HyperType.Effect> GetRequest<TModel, TResponse>(
        this SyntaxBuilder b,
        Request<TResponse, Guid> request,
        Var<Guid> p1,
        Func<SyntaxBuilder, Var<TModel>, Var<TResponse>, Var<TModel>> onSuccess)
    {
        return b.GetRequest(request, p1, b.MakeAction(onSuccess));
    }

    public static Var<HyperType.Effect> GetRequest<TModel, TResponse>(
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
}
