using System;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public interface _Internal_IReactEngine : IEngine
    {
    }

    /// <summary>
    /// This is now considered legacy and it will be deprecated in future
    /// </summary>
    public interface _Internal_IReactOnAdd : _Internal_IReactEngine
    {
    }
    
    /// <summary>
    /// This is now considered legacy and it will be deprecated in future
    /// </summary>
    public interface _Internal_IReactOnRemove : _Internal_IReactEngine
    {
    }
    
    /// <summary>
    /// This is now considered legacy and it will be deprecated in future
    /// </summary>
    public interface _Internal_IReactOnSwap : _Internal_IReactEngine
    {
    }
    
    /// <summary>
    /// This is now considered legacy and it will be deprecated in future
    /// </summary>
    public interface _Internal_IReactOnDispose : _Internal_IReactEngine
    {
    }

    public interface _Internal_IReactOnAddEx : _Internal_IReactEngine
    {
    }

    public interface _Internal_IReactOnRemoveEx : _Internal_IReactEngine
    {
    }

    public interface _Internal_IReactOnSwapEx : _Internal_IReactEngine
    {
    }
    
    public interface _Internal_IReactOnDisposeEx : _Internal_IReactEngine
    {
    }
}

namespace Svelto.ECS
{
    public interface IEngine
    {
    }

    public interface IGetReadyEngine : IEngine
    {
        void Ready();
    }

    public interface IQueryingEntitiesEngine : IGetReadyEngine
    {
        EntitiesDB entitiesDB { set; }
    }

    /// <summary>
    /// Interface to mark an Engine as reacting on entities added
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete]
    public interface IReactOnAdd<T> : _Internal_IReactOnAdd where T : _IInternalEntityComponent
    {
        void Add(ref T entityComponent, EGID egid);
    }

    public interface IReactOnAddEx<T> : _Internal_IReactOnAddEx where T : struct, _IInternalEntityComponent
    {
        void Add((uint start, uint end) rangeOfEntities, in EntityCollection<T> entities,
            ExclusiveGroupStruct groupID);
    }

    /// <summary>
    /// Interface to mark an Engine as reacting on entities removed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete]
    public interface IReactOnRemove<T> : _Internal_IReactOnRemove where T : _IInternalEntityComponent
    {
        void Remove(ref T entityComponent, EGID egid);
    }
    
    public interface IReactOnAddAndRemoveEx<T> : IReactOnAddEx<T>, IReactOnRemoveEx<T> where T : struct, _IInternalEntityComponent 
    {
    }

    public interface IReactOnRemoveEx<T> : _Internal_IReactOnRemoveEx where T : struct, _IInternalEntityComponent
    {
        void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<T> entities,
            ExclusiveGroupStruct groupID);
    }

    [Obsolete("Use IReactOnAddEx<T> and IReactOnRemoveEx<T> or IReactOnAddAndRemoveEx<T> instead")]
    public interface IReactOnAddAndRemove<T> : IReactOnAdd<T>, IReactOnRemove<T> where T : _IInternalEntityComponent
    {
    }

    /// <summary>
    /// Interface to mark an Engine as reacting on engines root disposed.
    /// It can work together with IReactOnRemove which normally is not called on enginesroot disposed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete("Use IReactOnDisposeEx<T> instead")]
    public interface IReactOnDispose<T> : _Internal_IReactOnDispose where T : _IInternalEntityComponent
    {
        void Remove(ref T entityComponent, EGID egid);
    }

    /// <summary>
    /// Interface to mark an Engine as reacting to entities swapping group
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete("Use IReactOnSwapEx<T> instead")]
    public interface IReactOnSwap<T> : _Internal_IReactOnSwap where T : _IInternalEntityComponent
    {
        void MovedTo(ref T entityComponent, ExclusiveGroupStruct previousGroup, EGID egid);
    }

    /// <summary>
    /// All the entities have been already submitted in the database (swapped) when this callback is triggered
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactOnSwapEx<T> : _Internal_IReactOnSwapEx where T : struct, _IInternalEntityComponent
    {
        void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<T> entities,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup);
    }
    
    public interface IReactOnDisposeEx<T> : _Internal_IReactOnDisposeEx where T : struct, _IInternalEntityComponent
    {
        void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<T> entities,
            ExclusiveGroupStruct groupID);
    }

    /// <summary>
    /// Interface to mark an Engine as reacting after each entities submission phase
    /// </summary>
    public interface IReactOnSubmission : _Internal_IReactEngine
    {
        void EntitiesSubmitted();
    }
    
    public interface IReactOnSubmissionStarted : _Internal_IReactEngine
    {
        void EntitiesSubmissionStarting();
    }
}