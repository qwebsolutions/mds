using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Syntax;

namespace MdsCommon.Controls;

public static partial class HyperAppEffects
{
    public static Var<HyperType.Effect> RefreshModelEffect<TModel>(
        this SyntaxBuilder b,
        Var<HyperType.Action<TModel, TModel>> onNewModel = null)
    {
        if (onNewModel == null)
        {
            onNewModel = b.MakeAction((SyntaxBuilder b, Var<TModel> model, Var<TModel> newModel) => newModel);
        }

        return MakeRefreshModelEffect<TModel>(b, b.Const($"/api/model/{typeof(TModel).Name}"), onNewModel);
    }

    public static Var<HyperType.Effect> RefreshModelEffect<TModel>(
        this SyntaxBuilder b, Var<string> entityId,
        Var<HyperType.Action<TModel, TModel>> onNewModel = null)
    {
        if (onNewModel == null)
        {
            onNewModel = b.MakeAction((SyntaxBuilder b, Var<TModel> model, Var<TModel> newModel) => newModel);
        }

        return MakeRefreshModelEffect<TModel>(b,
            b.Concat(b.Const($"/api/model/{typeof(TModel).Name}/"), entityId),
            onNewModel);
    }

    private static Var<HyperType.Effect> MakeRefreshModelEffect<TModel>(
        SyntaxBuilder b, 
        Var<string> getModelUrl,
        Var<HyperType.Action<TModel, TModel>> onNewModel)
    {
        return b.MakeEffect((SyntaxBuilder b, Var<HyperType.Dispatcher> dispatch) =>
        {
            //var getModelUrl = $"/api/model/{typeof(TModel).Name}";
            b.GetJson(
                getModelUrl,
                b.Def((SyntaxBuilder b, Var<TModel> newModel) =>
                {
                    //b.Dispatch(dispatch, b.MakeAction((SyntaxBuilder b, Var<TModel> model) =>
                    //{
                    //    b.Log("dispatch new model", newModel);
                    //    return newModel;
                    //}));

                    b.Dispatch(dispatch, onNewModel, newModel);
                }),
                b.Def((SyntaxBuilder b, Var<ClientSideException> ex) =>
                {
                    b.Log(b.Get(ex, x => x.message));
                    //b.CallOnObject(b.Window(), "alert", b.Get(ex, x => x.message));
                }));
        });
    }
}

