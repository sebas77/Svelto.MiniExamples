//https: //github.com/dave-hillier/disruptor-unity3d/blob/master/DisruptorUnity3d/Assets/RingBuffer.cs

using System.Threading;
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
        /// <returns>The next available item</returns>
        public ref T Dequeue()
        {
            var next = _consumerCursor.ReadAcquireFence() + 1;
            while (_producerCursor.ReadAcquireFence() < next) // makes sure we read the data from _entries after we have read the producer cursor
            {
                Thread.SpinWait(1);
            }
            
            _consumerCursor.WriteReleaseFence(next); // makes sure we read the data from _entries before we update the consumer cursor
            return ref this[next];
        }

        /// <summary>
        /// Attempts to remove an items from the queue
        /// </summary>
        /// <param name="obj">the items</param>
        /// <returns>True if successful</returns>
        public bool TryDequeue(out T obj)
        {
            var next = _consumerCursor.ReadAcquireFence() + 1;

            if (_producerCursor.ReadAcquireFence() < next)
            {
                obj = default(T);
                return false;
            }
            obj = Dequeue();
            return true;
        }
        
        /// <summary>
        /// The number of items in the buffer
        /// </summary>
        /// <remarks>for indicative purposes only, may contain stale data</remarks>
        public int Count { get { return (int)(_producerCursor.ReadFullFence() - _consumerCursor.ReadFullFence()); } }

        public void Reset() { _consumerCursor.WriteReleaseFence(_producerCursor.ReadFullFence());}

        /// <summary>
        /// Add an item to the buffer
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            var next = _producerCursor.ReadAcquireFence() + 1;

            long wrapPoint = next - _entries.Length;
            long min = _consumerCursor.ReadAcquireFence(); 

            while (wrapPoint > min)
            {
                min = _consumerCursor.ReadAcquireFence();
                Thread.SpinWait(1);
            }

            this[next] = item;
            _producerCursor.WriteReleaseFence(next); // makes sure we write the data in _entries before we update the producer cursor
        }
        
        public void Enqueue(ref T item)
        {
            var next = _producerCursor.ReadAcquireFence() + 1;

            long wrapPoint = next - _entries.Length;
            long min = _consumerCursor.ReadAcquireFence(); 

            while (wrapPoint > min)
            {
                min = _consumerCursor.ReadAcquireFence();
                Thread.SpinWait(1);
            }

            this[next] = item;
            _producerCursor.WriteReleaseFence(next); // makes sure we write the data in _entries before we update the producer cursor
        }

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
                Thread.MemoryBarrier();
                var value = _value;
                return value;
            }

            /// <summary>
            /// Read the value applying full fence semantic
            /// </summary>
            /// <returns>The current value</returns>
            public long ReadFullFence()
            {
                Thread.MemoryBarrier();
                return _value;
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