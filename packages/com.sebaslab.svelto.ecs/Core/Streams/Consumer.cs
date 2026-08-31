using System;
using System.Collections.Concurrent;
using Svelto.Common;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    /// <summary>
    ///     Thread-safe, unbounded FIFO consumer for entity stream notifications. Messages are stored as the strongly
    ///     typed ValueTuple payload, so publishing and consuming unmanaged components does not box.
    /// </summary>
    public struct Consumer<T> : IDisposable where T : unmanaged, _IInternalEntityComponent
    {
        internal Consumer(string name) : this()
        {
            unsafe
            {
                _queue = new ConcurrentQueue<ValueTuple<T, EGID>>();
                mustBeDisposed          = MemoryUtilities.NativeAlloc<bool>(1, Allocator.Persistent);
                *(bool*) mustBeDisposed = false;

                isActive          = MemoryUtilities.NativeAlloc<bool>(1, Allocator.Persistent);
                *(bool*) isActive = true;
            }
        }

        internal Consumer(ExclusiveGroupStruct group, string name) : this(name)
        {
            this.group = group;
            hasGroup   = true;
        }

        internal void Enqueue(in T entity, in EGID egid)
        {
            unsafe
            {
                if (*(bool*)isActive)
                    _queue.Enqueue((entity, egid));
            }
        }

        public bool TryDequeue(out T entity)
        {
            var tryDequeue = _queue.TryDequeue(out var values);

            entity = values.Item1;

            return tryDequeue;
        }

        //Note: it is correct to publish the EGID at the moment of the publishing, as the responsibility of 
        //the publisher consumer is not tracking the real state of the entity in the database at the 
        //moment of the consumption, but it's instead to store a copy of the entity at the moment of the publishing
        public bool TryDequeue(out T entity, out EGID id)
        {
            var tryDequeue = _queue.TryDequeue(out var values);

            entity = values.Item1;
            id     = values.Item2;

            return tryDequeue;
        }

        public void Flush() { _queue.Clear(); }

        public void Dispose()
        {
            unsafe
            {
                *(bool*) mustBeDisposed = true;
            }
        }

        public uint Count() { return (uint) _queue.Count; }

        public void Free()
        {
            MemoryUtilities.NativeFree(mustBeDisposed, Allocator.Persistent);
            MemoryUtilities.NativeFree(isActive,       Allocator.Persistent);
        }

        public void Pause()
        {
            unsafe
            {
                *(bool*) isActive = false;
            }
        }

        public void Resume()
        {
            unsafe
            {
                *(bool*) isActive = true;
            }
        }

        readonly ConcurrentQueue<ValueTuple<T, EGID>> _queue;

        internal readonly ExclusiveGroupStruct group;
        internal readonly bool                 hasGroup;
        internal          IntPtr               isActive;
        internal          IntPtr               mustBeDisposed;

    }
}
