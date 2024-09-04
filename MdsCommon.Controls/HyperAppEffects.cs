using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;

namespace MdsCommon.Controls;

public static partial class HyperAppEffects
{
    public static Var<HyperType.Effect> RefreshModelEffect<TModel>(this SyntaxBuilder b)
    {
        return MakeRefreshModelEffect<TModel>(b, b.Const($"/api/model/{typeof(TModel).Name}"));
    }

    public static Var<HyperType.Effect> RefreshModelEffect<TModel>(this SyntaxBuilder b, Var<string> entityId)
    {
        return MakeRefreshModelEffect<TModel>(b, b.Concat(b.Const($"/api/model/{typeof(TModel).Name}/"), entityId));
    }

    private static Var<HyperType.Effect> MakeRefreshModelEffect<TModel>(SyntaxBuilder b, Var<string> getModelUrl)
    {
        return b.MakeEffect((SyntaxBuilder b, Var<HyperType.Dispatcher> dispatch) =>
        {
            //var getModelUrl = $"/api/model/{typeof(TModel).Name}";
            b.GetJson(
                getModelUrl,
                b.Def((SyntaxBuilder b, Var<TModel> newModel) =>
                {
                    b.Log("newModel", newModel);
                    b.Dispatch(dispatch, b.MakeAction((SyntaxBuilder b, Var<TModel> model) =>
                    {
                        b.Log("dispatch new model", newModel);
                        return newModel;
                    }));
                }),
                b.Def((SyntaxBuilder b, Var<ClientSideException> ex) =>
                {
                    b.CallOnObject(b.Window(), "alert", b.Get(ex, x => x.message));
                }));
        });
    }
}

