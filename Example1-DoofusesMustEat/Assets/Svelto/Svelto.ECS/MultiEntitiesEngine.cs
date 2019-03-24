using System.Runtime.CompilerServices;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class MultiEntitiesEngine<T, U> : SingleEntityEngine<T>, IHandleEntityStructEngine<U>
        where U : IEntityStruct where T : IEntityStruct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddInternal(in U entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        { Add(in entityView, previousGroup); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveInternal(in U entityView, bool itsaSwap)
        { Remove(in entityView, itsaSwap); }
        
        protected abstract void Add(in U entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup);
        protected abstract void Remove(in U entityView, bool itsaSwap);
    }
    
    public abstract class MultiEntitiesEngine<T, U, V> : MultiEntitiesEngine<T, U>, IHandleEntityStructEngine<V>
        where V :  IEntityStruct where U :  IEntityStruct where T :  IEntityStruct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddInternal(in V entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        { Add(in entityView, previousGroup); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveInternal(in V entityView, bool itsaSwap)
        { Remove(in  entityView, itsaSwap); }
        
        protected abstract void Add(in V entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup);
        protected abstract void Remove(in V entityView, bool itsaSwap);
    }

    /// <summary>
    ///     Please do not add more MultiEntityViewsEngine if you use more than 4 nodes, your engine has
    ///     already too many responsibilities.
    /// </summary>
    public abstract class MultiEntitiesEngine<T, U, V, W> : MultiEntitiesEngine<T, U, V>, IHandleEntityStructEngine<W>
        where W :  IEntityStruct where V :  IEntityStruct where U :  IEntityStruct where T : IEntityStruct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddInternal(in W entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        { Add(in  entityView, previousGroup); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveInternal(in W entityView, bool itsaSwap)
        { Remove(in  entityView, itsaSwap); }
        
        protected abstract void Add(in W entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup);
        protected abstract void Remove(in W entityView, bool itsaSwap);
    }
}