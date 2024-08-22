using Metapsi.Syntax;
using Metapsi.Hyperapp;
using Metapsi.Html;
using System;
using Microsoft.AspNetCore.Builder;
using Metapsi;

namespace MdsCommon
{
    public static class RendererExtensions
    {
        public static void UseRenderer<TModel>(this WebApplication webApp, Action<HtmlBuilder, TModel> buildDocument)
        {
            webApp.UseRenderer<TModel>(model =>
            {
                return HtmlBuilder.FromDefault(b =>
                {
                    buildDocument(b, model);
                }).ToString();
            });
        }

        public static void UseRenderer<TModel>(this WebApplication webApp, Func<LayoutBuilder, TModel, Var<TModel>, Var<IVNode>> renderHyperapp)
        {
            webApp.UseRenderer((HtmlBuilder b, TModel serverModel) =>
            {
                b.HeadAppend(b.Hyperapp(
                    serverModel,
                    (b, clientModel) =>
                    {
                        return renderHyperapp(b, serverModel, clientModel);
                    }));
            });
        }

        public static void UseRenderer<TModel>(this WebApplication webApp, Func<LayoutBuilder, Var<TModel>, Var<IVNode>> renderHyperapp)
        {
            webApp.UseRenderer((HtmlBuilder b, TModel serverModel) =>
            {
                b.HeadAppend(b.Hyperapp(
                    serverModel,
                    (b, clientModel) =>
                    {
                        return renderHyperapp(b, clientModel);
                    }));
            });
        }
    }
}
