//https: //github.com/dave-hillier/disruptor-unity3d/blob/master/DisruptorUnity3d/Assets/RingBuffer.cs

using System;
using Svelto.Utilities;

namespace Svelto.DataStructures
{
    /// <summary>
    /// Implementation of the Disruptor pattern
    /// </summary>
    /// <typeparam name="T">the type of item to be stored</typeparam>
    public class RingBuffer<T>
    {
        readonly T[] _entries;
        readonly int _modMask;
        Volatile.PaddedLong _consumerCursor = new Volatile.PaddedLong();
        Volatile.PaddedLong _producerCursor = new Volatile.PaddedLong();

        /// <summary>
        /// Creates a new RingBuffer with the given capacity
        /// </summary>
        /// <param name="capacity">The capacity of the buffer</param>
        /// <remarks>Only a single thread may attempt to consume at any one time</remarks>
        public RingBuffer(int capacity)
        {
            capacity = NextPowerOfTwo(capacity);
            _modMask = capacity - 1;
            _entries = new T[capacity];
        }

        /// <summary>
        /// The maximum number of items that can be stored
        /// </summary>
        public int Capacity
        {
            get { return _entries.Length; }
        }

        ref T this[long index] => ref _entries[index & _modMask];

        /// <summary>
        /// Removes an item from the buffer.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The next available item</returns>
        public ref T Dequeue(string name)
        {
            var next = _consumerCursor.ReadAcquireFence() + 1;
            
            int quickIterations = 0;

            // makes sure we read the data from _entries after we have read the producer cursor
            while (_producerCursor.ReadAcquireFence() < next && quickIterations < 1024)
            {
                ThreadUtility.Wait(ref quickIterations, 16);
            }
            
            if (quickIterations >= 1024) throw new RingBufferExceptionDequeue<T>(name, next);

            _consumerCursor.WriteReleaseFence(next); // makes sure we read the data from _entries before we update the consumer cursor
            return ref this[next];
        }

        /// <summary>
        /// Attempts to remove an items from the queue
        /// </summary>
        /// <param name="obj">the items</param>
        /// <param name="name"></param>
        /// <returns>True if successful</returns>
        public bool TryDequeue(out T obj, string name)
        {
            var next = _consumerCursor.ReadAcquireFence() + 1;

            if (_producerCursor.ReadAcquireFence() < next)
            {
                obj = default(T);
                return false;
            }
            
            obj = Dequeue(name);
            return true;
        }
        
        /// <summary>
        /// The number of items in the buffer
        /// </summary>
        /// <remarks>for indicative purposes only, may contain stale data</remarks>
        public int Count { get { return (int)(_producerCursor.ReadAcquireFence() - _consumerCursor.ReadAcquireFence()); } }

        public void Reset() { _consumerCursor.WriteReleaseFence(_producerCursor.ReadAcquireFence());}

        public void Enqueue(ref T item, string name)
        {
            var next = _producerCursor.ReadAcquireFence() + 1;

            long wrapPoint = next - _entries.Length;
            long min = _consumerCursor.ReadAcquireFence();
            
            int quickIterations = 0;

            while (wrapPoint > min && quickIterations < 1024)
            {
                min = _consumerCursor.ReadAcquireFence();
                
                ThreadUtility.Wait(ref quickIterations, 16);
            }
            
            if (quickIterations >= 1024) throw new RingBufferExceptionEnqueue<T>(name, next);

            this[next] = item;
            _producerCursor.WriteReleaseFence(next); // makes sure we write the data in _entries before we update the producer cursor
        }

        //todo: probably better to move to prime and fasts mod
        static int NextPowerOfTwo(int x)
        {
            var result = 2;
            while (result < x)
            {
                result <<= 1;
            }
            return result;
        }
    }

    public class RingBufferExceptionDequeue<T> : Exception
    {
        public RingBufferExceptionDequeue(string name, long count) : base(
            "Consumer is consuming too fast. Type: "
               .FastConcat(typeof(T).ToString(), " Consumer Name: ", name, " count ").FastConcat(count))
        {}
    }
    
    public class RingBufferExceptionEnqueue<T> : Exception
    {
        public RingBufferExceptionEnqueue(string name, long count) : base(
            "Entity Stream capacity has been saturated Type: "
               .FastConcat(typeof(T).ToString(), " Consumer Name: ", name, " count ").FastConcat(count))
        {}
    }

    public static class Volatile
    {
        public struct PaddedLong
        {
            long _value;

            /// <summary>
            /// Read the value applying acquire fence semantic
            /// </summary>
            /// <returns>The current value</returns>
            public long ReadAcquireFence()
            {
                var value = ThreadUtility.VolatileRead(ref _value);
                return value;
            }
            /// <summary>
            /// Write the value applying release fence semantic
            /// </summary>
            /// <param name="newValue">The new value</param>
            public void WriteReleaseFence(long newValue)
            {
                ThreadUtility.VolatileWrite(ref _value, newValue);
            }
        }
    }
}