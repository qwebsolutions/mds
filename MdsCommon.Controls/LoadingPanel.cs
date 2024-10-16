﻿using Metapsi.Syntax;

namespace MdsCommon.Controls
{
    public static class LoadingPanelControl
    {
        //public static Var<IVNode> LoadingPanel<TPage>(this LayoutBuilder b, Var<TPage> page)
        //    where TPage : IHasLoadingPanel
        //{
        //    return b.If(
        //        b.Get(page, x => x.IsLoading),
        //        b => b.Div("fixed top-0 left-0 right-0 bottom-0 w-full h-screen z-50 overflow-hidden bg-gray-700 opacity-10 flex flex-col items-center justify-center cursor-wait"),
        //        b => b.Div("hidden"));
        //}

        public static Var<TPage> ShowLoading<TPage>(this SyntaxBuilder b, Var<TPage> page)
            where TPage: IHasLoadingPanel
        {
            b.Set(page, x => x.IsLoading, b.Const(true));
            return page;
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        /// <param name="b"></param>
        public static Var<TPage> HideLoading<TPage>(this SyntaxBuilder b, Var<TPage> page)
            where TPage : IHasLoadingPanel
        {
            b.Set(page, x => x.IsLoading, b.Const(false));
            return page;
        }
    }
}
