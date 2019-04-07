using System.Runtime.CompilerServices;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class MultiEntitiesReactiveEngine<T, U> : SingleEntityReactiveEngine<T>, IHandleEntityStructEngine<U>
        where U : IEntityStruct where T : IEntityStruct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IHandleEntityStructEngine<U>.AddInternal(ref U entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        { Add(ref entityView, previousGroup); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IHandleEntityStructEngine<U>.RemoveInternal(ref U entityView, bool itsaSwap)
        { Remove(ref entityView, itsaSwap); }
        
        protected abstract void Add(ref U entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup);
        protected abstract void Remove(ref U entityView, bool itsaSwap);
    }
    
    public abstract class MultiEntitiesReactiveEngine<T, U, V> : MultiEntitiesReactiveEngine<T, U>, IHandleEntityStructEngine<V>
        where V :  IEntityStruct where U :  IEntityStruct where T :  IEntityStruct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IHandleEntityStructEngine<V>.AddInternal(ref V entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        { Add(ref entityView, previousGroup); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IHandleEntityStructEngine<V>.RemoveInternal(ref V entityView, bool itsaSwap)
        { Remove(ref  entityView, itsaSwap); }
        
        protected abstract void Add(ref V entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup);
        protected abstract void Remove(ref V entityView, bool itsaSwap);
    }

    /// <summary>
    ///     Please do not add more MultiEntityViewsEngine if you use more than 4 nodes, your engine has
    ///     already too many responsibilities.
    /// </summary>
    public abstract class MultiEntitiesReactiveEngine<T, U, V, W> : MultiEntitiesReactiveEngine<T, U, V>, IHandleEntityStructEngine<W>
        where W :  IEntityStruct where V :  IEntityStruct where U :  IEntityStruct where T : IEntityStruct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IHandleEntityStructEngine<W>.AddInternal(ref W entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        { Add(ref  entityView, previousGroup); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IHandleEntityStructEngine<W>.RemoveInternal(ref W entityView, bool itsaSwap)
        { Remove(ref  entityView, itsaSwap); }
        
        protected abstract void Add(ref W entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup);
        protected abstract void Remove(ref W entityView, bool itsaSwap);
    }
}