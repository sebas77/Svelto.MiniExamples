using System.Runtime.CompilerServices;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class SingleEntityEngine<T> : EngineInfo, IHandleEntityStructEngine<T> where T : IEntityStruct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddInternal(ref T entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        { Add(ref entityView, previousGroup); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveInternal(ref T entityView, bool itsaSwap)
        { Remove(ref entityView, itsaSwap); }

        protected abstract void Add(ref T entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup);
        protected abstract void Remove(ref T entityView, bool itsaSwap);
    }
}