using System.Runtime.CompilerServices;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class MultiEntitiesEngine<T, U> : SingleEntityEngine<T>, IHandleEntityStructEngine<U>
        where U : IEntityStruct where T : IEntityStruct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddInternal(ref U entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        { Add(ref entityView, previousGroup); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveInternal(ref U entityView, bool itsaSwap)
        { Remove(ref entityView, itsaSwap); }
        
        protected abstract void Add(ref U entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup);
        protected abstract void Remove(ref U entityView, bool itsaSwap);
    }
    
    public abstract class MultiEntitiesEngine<T, U, V> : MultiEntitiesEngine<T, U>, IHandleEntityStructEngine<V>
        where V :  IEntityStruct where U :  IEntityStruct where T :  IEntityStruct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddInternal(ref V entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        { Add(ref entityView, previousGroup); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveInternal(ref V entityView, bool itsaSwap)
        { Remove(ref  entityView, itsaSwap); }
        
        protected abstract void Add(ref V entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup);
        protected abstract void Remove(ref V entityView, bool itsaSwap);
    }

    /// <summary>
    ///     Please do not add more MultiEntityViewsEngine if you use more than 4 nodes, your engine has
    ///     already too many responsibilities.
    /// </summary>
    public abstract class MultiEntitiesEngine<T, U, V, W> : MultiEntitiesEngine<T, U, V>, IHandleEntityStructEngine<W>
        where W :  IEntityStruct where V :  IEntityStruct where U :  IEntityStruct where T : IEntityStruct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddInternal(ref W entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        { Add(ref  entityView, previousGroup); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveInternal(ref W entityView, bool itsaSwap)
        { Remove(ref  entityView, itsaSwap); }
        
        protected abstract void Add(ref W entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup);
        protected abstract void Remove(ref W entityView, bool itsaSwap);
    }
}