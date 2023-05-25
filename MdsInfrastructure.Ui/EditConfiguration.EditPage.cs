using Metapsi;
using Metapsi.Syntax;
using Metapsi.Hyperapp;
using System;

namespace MdsInfrastructure
{
    public static partial class EditConfiguration
    {
        private static Var<TState> GoTo<TState>(
            this BlockBuilder b,
            Var<TState> state,
            System.Func<BlockBuilder, Var<TState>, Var<Guid>, Var<HyperNode>> render,
            Var<System.Guid> entityId)
            where TState : IEditPage<TState>
        {
            //b.SetEditEntityId(state, entityId);
            b.PushView<TState>(state, x => x.EditStack, b.Def(render), entityId);
            return b.Clone(state);
        }

        private static Var<TState> GoTo<TState, TRecord>(
            this BlockBuilder b,
            Var<TState> state,
            System.Func<BlockBuilder, Var<TState>, Var<Guid>, Var<HyperNode>> render,
            Var<TRecord> entity) 
            where TRecord : IRecord
            where TState : IEditPage<TState>
        {
            return b.GoTo(state, render, b.Get(entity, x => x.Id));
        }
    }
}
