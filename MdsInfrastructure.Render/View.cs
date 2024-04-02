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
            this LayoutBuilder b,
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
                    b.SetRef(currentViewRef, b.GetViewName(renderers.First()));
                });

            var currentViewName = b.GetRef(currentViewRef);

            var contentRenderer = b.Ref(b.Def((LayoutBuilder b, Var<TModel> model) => b.TextSpan("View error")));

            foreach (var renderer in renderers)
            {
                b.If(
                    b.AreEqual(
                        b.GetViewName(renderer),
                        currentViewName),
                    b =>
                    {
                        b.Log(nameof(currentViewName), currentViewName);
                        b.SetRef(contentRenderer, b.Def(renderer));
                    });
            }

            return b.Call(b.GetRef(contentRenderer), clientModel);
        }

        public static void SwapView<TModel>(this SyntaxBuilder b, Func<LayoutBuilder, Var<TModel>, Var<IVNode>> renderer)
        {
            //b.Log("viewRenderer", viewRenderer);
            b.SetRef(b.GlobalRef(RendererName), b.GetViewName(renderer));
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
