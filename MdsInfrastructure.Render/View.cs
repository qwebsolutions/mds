using Metapsi.Syntax;
using Metapsi.Hyperapp;
using Metapsi.JavaScript;
using Metapsi.Ui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsInfrastructure
{
    public static class ViewControl
    {
        public static Reference<string> RendererName = new Reference<string>() { Value = string.Empty };

        public static Var<IVNode> View<TModel>(
            LayoutBuilder b,
            Var<TModel> clientModel,
            Var<List<Func<TModel, IVNode>>> renderers)
        {
            var currentViewRef = b.GlobalRef(RendererName);
            b.If(
                b.Not(
                    b.HasValue(
                        b.GetRef(currentViewRef))),
                b =>
                {
                    b.SetRef(currentViewRef, b.Get();
                });

            var currentViewName = b.GetRef(currentViewRef);

            var outNode = b.Ref<IVNode>(b.TextSpan("View error"));

            foreach (var renderer in renderers)
            {
                b.If(
                    b.AreEqual(
                        b.Const(renderer.Method.Name),
                        currentViewName),
                    b =>
                    {
                        b.SetRef(outNode, renderer(b, clientModel));
                    });
            }

            return b.GetRef(outNode);
        }

        public static Var<IVNode> View<TModel>(
            LayoutBuilder b,
            Var<TModel> clientModel,
            params Func<LayoutBuilder, Var<TModel>, Var<IVNode>>[] renderers)
        {

            var currentViewRef = b.GlobalRef(RendererName);
            b.If(
                b.Not(
                    b.HasValue(
                        b.GetRef(currentViewRef))),
                b =>
                {
                    b.SetRef(currentViewRef, b.Const(defaultViewName));
                });

            var currentViewName = b.GetRef(currentViewRef);

            var outNode = b.Ref<IVNode>(b.TextSpan("View error"));

            foreach (var renderer in renderers)
            {
                b.If(
                    b.AreEqual(
                        b.Const(renderer.Method.Name),
                        currentViewName),
                    b =>
                    {
                        b.SetRef(outNode, renderer(b, clientModel));
                    });
            }

            return b.GetRef(outNode);
        }

        public static void SwapView<TModel>(this SyntaxBuilder b, Var<TModel> model, Var<string> areaName, Var<string> viewRenderer)
        {
            b.Log("viewRenderer", viewRenderer);
            b.SetRef(b.GlobalRef(RendererName), viewRenderer);
        }

        public static Var<string> GetViewName<TModel>(this SyntaxBuilder b, Func<LayoutBuilder, Var<TModel>, Var<IVNode>> renderer)
        {
            return b.Const(renderer.Method.Name);
        }

        public static string GetName<TModel>(Func<LayoutBuilder, Var<TModel>, Var<IVNode>> renderer)
        {
            return renderer.Method.Name;
        }
    }
}
