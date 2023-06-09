using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;

namespace MdsInfrastructure
{
    public interface IEditPage<TState>
    {
        public Guid EntityId { get; set; }
        public PageStack.Stack<TState> EditStack { get; set; }
    }

    //public static class EditPageExtensions
    //{
    //    //public static Var<Guid> GetEditEntityId<TState>(this BlockBuilder b, Var<TState> state)
    //    //    where TState : IEditPage<TState>
    //    //{
    //    //    return b.Get(state, x => x.EntityId);
    //    //}

    //    //public static void SetEditEntityId<TState>(this BlockBuilder b, Var<TState> state, Var<Guid> id)
    //    //    where TState : IEditPage<TState>
    //    //{
    //    //    b.Set(state, x => x.EntityId, id);
    //    //}
    //}
}
