using System.Runtime.CompilerServices;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class SingleEntityEngine<T> : EngineInfo, IHandleEntityStructEngine<T> where T : IEntityStruct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddInternal(in T entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        { Add(in entityView, previousGroup); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveInternal(in T entityView, bool itsaSwap)
        { Remove(in entityView, itsaSwap); }

        protected abstract void Add(in T entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup);
        protected abstract void Remove(in T entityView, bool itsaSwap);
    }
}