#if DEBUG && !PROFILE_SVELTO
//#define ENABLE_THREAD_SAFE_CHECKS
#endif
using System;
using System.Threading;
using Volatile = System.Threading.Volatile;

namespace Svelto.Common.DataStructures
{
#if ENABLE_THREAD_SAFE_CHECKS
    public struct Sentinel
    {
#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
        SharedDictionaryStruct sharedMultithreadTest => SharedDictonary.test.Data;
#else
            SharedDictionaryStruct sharedMultithreadTest => SharedDictonary.test;
#endif
        public static int ReadFlag  = 1;
        public static int WriteFlag = 2;

        internal void Use()
        {
            if (_flag == WriteFlag)
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
            if (_flag == ReadFlag)
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
#if ENABLE_THREAD_SAFE_CHECKS
            _ptr  = ptr.ToInt64();
            _flag = flag;
            if (flag != 0)
            {
                if (sharedMultithreadTest.Exists(ptr) == false)
                    sharedMultithreadTest.Add(_ptr, this);
            }
#endif
        }

        internal void Release()
        {
#if ENABLE_THREAD_SAFE_CHECKS
            if (_flag >= 1)
                Volatile.Write(ref sharedMultithreadTest.GetValueByRef(_ptr)._threadSentinel, NOT_USED);
#endif
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

        const int NOT_USED   = 0;
        const int USED_READ  = 1;
        const int USED_WRITE = 2;
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
    public struct Sentinel
    {
        public Sentinel(IntPtr ptr, uint readFlag)
        {
        }

        public TestThreadSafety TestThreadSafety()
        {
            return default;
        }

        public static uint ReadFlag  { get; set; }
        public static uint WriteFlag { get; set; }
    }

    public struct TestThreadSafety : IDisposable
    {
        public void Dispose()
        {
        }
    }
#endif
}