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
        readonly T[]        _entries;
        readonly int        _modMask;
        VolatileHelper.PaddedLong _consumerCursor = new VolatileHelper.PaddedLong();
        VolatileHelper.PaddedLong _producerCursor = new VolatileHelper.PaddedLong();

#if DEBUG && !PROFILE_SVELTO
        readonly string _name;
#endif

        const int _NUMBER_OF_ITERATIONS_BEFORE_STALL_DETECTED = 1024;

        /// <summary>
        /// Creates a new RingBuffer with the given capacity
        /// </summary>
        /// <param name="capacity">The capacity of the buffer</param>
        /// <remarks>Only a single thread may attempt to consume at any one time</remarks>
        public RingBuffer(int capacity, string name)
        {
            capacity = NextPowerOfTwo(capacity);
            _modMask = capacity - 1;
            _entries = new T[capacity];
#if DEBUG && !PROFILE_SVELTO
            _name = name;
#endif
        }

        /// <summary>
        /// The maximum number of items that can be stored
        /// </summary>
        public int Capacity => _entries.Length;

        ref T this[long index] => ref _entries[index & _modMask];

        /// <summary>
        /// Removes an item from the buffer.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The next available item</returns>
        public ref T Dequeue()
        {
            var next = _consumerCursor.ReadAcquireFence() + 1;

            int quickIterations = 0;

            // makes sure we read the data from _entries after we have read the producer cursor
            while (_producerCursor.ReadAcquireFence() < next &&
                   quickIterations < _NUMBER_OF_ITERATIONS_BEFORE_STALL_DETECTED)
            {
                ThreadUtility.Wait(ref quickIterations, 16);
            }
#if DEBUG && !PROFILE_SVELTO
            if (quickIterations >= _NUMBER_OF_ITERATIONS_BEFORE_STALL_DETECTED)
                throw new RingBufferExceptionDequeue<T>(_name, next);
#endif
            _consumerCursor.WriteReleaseFence(next); // makes sure we read the data from _entries before we update the consumer cursor
            return ref this[next];
        }

        /// <summary>
        /// Attempts to remove an items from the queue
        /// </summary>
        /// <param name="obj">the items</param>
        /// <param name="name"></param>
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
        public int Count => (int) (_producerCursor.ReadAcquireFence() - _consumerCursor.ReadAcquireFence());

        public void Reset()
        {
            _consumerCursor.WriteReleaseFence(_producerCursor.ReadAcquireFence());
        }

        public void Enqueue(in T item)
        {
            var next = _producerCursor.ReadAcquireFence() + 1;

            long wrapPoint = next - _entries.Length;
            long min = _consumerCursor.ReadAcquireFence();

            int quickIterations = 0;

            while (wrapPoint > min && quickIterations < _NUMBER_OF_ITERATIONS_BEFORE_STALL_DETECTED)
            {
                min = _consumerCursor.ReadAcquireFence();

                ThreadUtility.Wait(ref quickIterations, 16);
            }

            if (quickIterations >= _NUMBER_OF_ITERATIONS_BEFORE_STALL_DETECTED)
                throw new RingBufferExceptionEnqueue<T>(
#if DEBUG && !PROFILE_SVELTO
                    _name
#else
                    "consumer"
#endif
                    , next);

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

    static class VolatileHelper
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
                var value = System.Threading.Volatile.Read(ref _value);
                return value;
            }

            /// <summary>
            /// Write the value applying release fence semantic
            /// </summary>
            /// <param name="newValue">The new value</param>
            public void WriteReleaseFence(long newValue)
            {
                System.Threading.Volatile.Write(ref _value, newValue);
            }
        }
    }
}