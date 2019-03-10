using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    /// Do not use this class in place of a normal polling.
    /// I eventually realised than in ECS no form of communication other than polling entity components can exist.
    /// Using groups, you can have always an optimal set of entity components to poll, so EntityStreams must be used
    /// only if:
    /// - you want to polling engine to be able to track all the entity changes happening in between polls and not
    /// just the current state
    /// - you want a thread-safe way to read entity states, which includes all the state changes and not the last
    /// one only
    /// - you want to communicate between EnginesRoots  
    /// </summary>
    class EntitiesStream
    {
        internal Consumer<T> GenerateConsumer<T>(string name, int capacity) where T : unmanaged, IEntityStruct
        {
            if (_streams.ContainsKey(typeof(T)) == false) _streams[typeof(T)] = new EntityStream<T>();
            
            return (_streams[typeof(T)] as EntityStream<T>).GenerateConsumer(name, capacity);
        }

        internal void PublishEntity<T>(ref T entity) where T : unmanaged, IEntityStruct
        {
            if (_streams.TryGetValue(typeof(T), out var typeSafeStream)) 
                (typeSafeStream as EntityStream<T>).PublishEntity(ref entity);
            else
                Console.LogWarning("No Consumers are waiting for this entity to change "
                                      .FastConcat(typeof(T).ToString()));
        }

        readonly Dictionary<Type, ITypeSafeStream> _streams = new Dictionary<Type, ITypeSafeStream>();
    }

    interface ITypeSafeStream
    {}

    class EntityStream<T>:ITypeSafeStream where T:unmanaged, IEntityStruct
    {
        public void PublishEntity(ref T entity)
        {
            for (int i = 0; i < _buffers.Count; i++)
                _buffers[i].Enqueue(ref entity);
        }

        public Consumer<T> GenerateConsumer(string name, int capacity)
        {
            var consumer = new Consumer<T>(name, capacity);
            _buffers.Add(consumer);
            return consumer;
        }

        readonly FasterList<Consumer<T>> _buffers = new FasterList<Consumer<T>>();
    }

    public class Consumer<T> where T:unmanaged, IEntityStruct
    {
        public Consumer(string name, int capacity)
        {
            _ringBuffer = new RingBuffer<T>(capacity);
            _capacity = capacity;
            _name = name;
        }

        public void Enqueue(ref T entity)
        {
            if (_ringBuffer.Count >= _capacity)
                throw new Exception(
                    "Entity Stream capacity has been saturated Type: ".FastConcat(typeof(T).ToString(), 
                                                                                  " Consumer Name: ", _name));
                
            _ringBuffer.Enqueue(ref entity);
        }

        public bool TryDequeue(ExclusiveGroup group, out T entity)
        {
            if (_ringBuffer.TryDequeue(out entity) == true)
                return entity.ID.groupID == @group;

            return false;
        }
        
        public bool TryDequeue(out T entity) { return _ringBuffer.TryDequeue(out entity); }

        public void Flush() { _ringBuffer.Reset(); }
        
        readonly RingBuffer<T> _ringBuffer;
        readonly int           _capacity;
        readonly string        _name;
    }
}    