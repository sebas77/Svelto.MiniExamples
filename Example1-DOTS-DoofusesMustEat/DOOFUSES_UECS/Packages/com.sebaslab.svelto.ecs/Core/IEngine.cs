using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public interface IReactEngine: IEngine
    {}
    
    public interface IReactOnAdd : IReactEngine
    {}
    
    public interface IReactOnRemove : IReactEngine
    {}
    
    public interface IReactOnDispose : IReactEngine
    {}

    public interface IReactOnSwap : IReactEngine
    {}
}

namespace Svelto.ECS
{
    public interface IEngine
    {}
    
    /// <summary>
    /// Interface to mark an Engine as reacting on entities added
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactOnAdd<T> : IReactOnAdd where T : IEntityComponent
    {
        void Add(ref T entityComponent, EGID egid);
    }
    
    /// <summary>
    /// Interface to mark an Engine as reacting on entities removed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactOnRemove<T> : IReactOnRemove where T : IEntityComponent
    {
        void Remove(ref T entityComponent, EGID egid);
    }
    
    public interface IReactOnAddAndRemove<T> : IReactOnAdd<T>, IReactOnRemove<T> where T : IEntityComponent
    { }
    
    /// <summary>
    /// Interface to mark an Engine as reacting on engines root disposed.
    /// It can work together with IReactOnRemove which normally is not called on enginesroot disposed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactOnDispose<T> : IReactOnDispose where T : IEntityComponent
    {
        void Remove(ref T entityComponent, EGID egid);
    }
    
    /// <summary>
    /// Interface to mark an Engine as reacting to entities swapping group
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactOnSwap<T> : IReactOnSwap where T : IEntityComponent
    {
        void MovedTo(ref T entityComponent, ExclusiveGroupStruct previousGroup, EGID egid);
    }
    
    /// <summary>
    /// Interface to mark an Engine as reacting after each entities submission phase
    /// </summary>
    public interface IReactOnSubmission:IReactEngine
    {
        void EntitiesSubmitted();
    }
}