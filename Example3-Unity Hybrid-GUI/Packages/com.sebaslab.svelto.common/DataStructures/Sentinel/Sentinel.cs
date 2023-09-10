#if DEBUG && !PROFILE_SVELTO
//#define ENABLE_THREAD_SAFE_CHECKS
#endif
using System;

namespace Svelto.DataStructures
{
#if ENABLE_THREAD_SAFE_CHECKS
//A sentinel field must never be readonly and must always use the ENABLE_DEBUG_CHECKS
    public struct Sentinel
    {
#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
        SharedDictionaryStruct sharedMultithreadTest => SharedDictonary.test.Data;
#else
        SharedDictionaryStruct sharedMultithreadTest => SharedDictonary.test;
#endif
        public static readonly int readFlag  = 1;
        public static readonly int writeFlag = 2;

        internal void Use()
        {
            if (_flag == writeFlag)
            {
                //if the state is found in NOT_USED is fine, all the other cases are not
                ref var threadSentinel = ref sharedMultithreadTest.GetValueByRef(_ptr)._threadSentinel;
                if (Interlocked.CompareExchange(ref threadSentinel, USED_WRITE, NOT_USED) != NOT_USED)
                    throw new Exception(
                        "This datastructure is not thread safe, reading and writing operations can happen" +
                        "on different threads, but not simultaneously");
            }
            else
                //if the state is found in NOT_USED or USED_READ, read is allowed
            if (_flag == readFlag)
            {
                ref var threadSentinel = ref sharedMultithreadTest.GetValueByRef(_ptr)._threadSentinel;
                if (Interlocked.CompareExchange(ref threadSentinel, USED_READ, NOT_USED) > USED_READ)
                    throw new Exception(
                        "This datastructure is not thread safe, reading and writing operations can happen" +
                        "on different threads, but not simultaneously");
            }
        }

        /// <summary>
        /// warning the constructor is not thread safe, so i.e.: it must be used always from mainthread
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="flag"></param>
        public Sentinel(IntPtr ptr, int flag) : this()
        {
            _ptr  = ptr.ToInt64();
            _flag = flag;
            if (flag != 0)
            {
                if (sharedMultithreadTest.Exists(ptr) == false)
                    sharedMultithreadTest.Add(_ptr, this);
            }
        }

        internal void Release()
        {
            if (_flag >= 1)
                Volatile.Write(ref sharedMultithreadTest.GetValueByRef(_ptr)._threadSentinel, NOT_USED);
        }

        /// <summary>
        /// This method instead is thread safe, as long as Sentinels are not being created by other threads
        /// </summary>
        /// <returns></returns>
        public TestThreadSafety TestThreadSafety()
        {
            return new TestThreadSafety(this);
        }

        int           _threadSentinel;
        Allocator     _allocator;
        readonly long _ptr;
        readonly int  _flag;

        const  int         NOT_USED   = 0;
        const  int         USED_READ  = 1;
        const  int         USED_WRITE = 2;

        //this must return something that decrease the count
        public Sentinel AsWriter(IntPtr ptr)
        {
            return new Sentinel(ptr, writeFlag); //this must count
        }
    }

    public struct TestThreadSafety : IDisposable
    {
        Sentinel _sentinel;

        public TestThreadSafety(Sentinel sentinel)
        {
            _sentinel = sentinel;
            _sentinel.Use();
        }

        public void Dispose()
        {
            _sentinel.Release();
        }
    }
#else
    //A sentinel field must never be readonly and must always use the ENABLE_DEBUG_CHECKS. Using may prevent inline
    public struct Sentinel
    {
        public Sentinel(IntPtr ptr, uint readFlag)
        {
        }

        public TestThreadSafety TestThreadSafety()
        {
            return default;
        }

        public static uint readFlag  { get; set; }
        public static uint writeFlag { get; set; }
    }

    public struct TestThreadSafety : IDisposable
    {
        public void Dispose()
        {
        }
    }
#endif
}