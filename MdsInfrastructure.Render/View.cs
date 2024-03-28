using Metapsi.Syntax;
using Metapsi.Hyperapp;
using Metapsi.JavaScript;
using Metapsi.Ui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdsInfrastructure
{
    public static class View
    {
        private const string FeatureName = "views";

        public static Var<IVNode> Render<TModel>(
            LayoutBuilder b,
            Var<TModel> clientModel,
            string areaName,
            string defaultViewName,
            params Func<LayoutBuilder, Var<TModel>, Var<IVNode>>[] renderers)
        {

            var currentViewName = b.GetVar<TModel>(clientModel, FeatureName, b.Const(areaName), b.Const(defaultViewName));

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
            b.SetVar<TModel>(model, FeatureName, areaName, viewRenderer);
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

    public static partial class Control
    {
        /// <summary>
        /// Render views
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="b"></param>
        /// <param name="clientModel"></param>
        /// <param name="areaName"></param>
        /// <param name="defaultViewName"></param>
        /// <param name="renderers"></param>
        /// <returns></returns>
        public static Var<IVNode> View<TModel>(
            this LayoutBuilder b,
            Var<TModel> clientModel,
            string areaName,
            string defaultViewName,
            params Func<LayoutBuilder, Var<TModel>, Var<IVNode>>[] renderers)
        {
            return MdsInfrastructure.View.Render(b, clientModel, areaName, defaultViewName, renderers);
        }

        /// <summary>
        /// Render views using the first one as default
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="b"></param>
        /// <param name="clientModel"></param>
        /// <param name="areaName"></param>
        /// <param name="renderers"></param>
        /// <returns></returns>
        public static Var<IVNode> View<TModel>(
            this LayoutBuilder b,
            Var<TModel> clientModel,
            string areaName,
            params Func<LayoutBuilder, Var<TModel>, Var<IVNode>>[] renderers)
        {
            return MdsInfrastructure.View.Render(
                b, 
                clientModel, 
                areaName, 
                MdsInfrastructure.View.GetName(renderers.First()), 
                renderers);
        }
    }
}
