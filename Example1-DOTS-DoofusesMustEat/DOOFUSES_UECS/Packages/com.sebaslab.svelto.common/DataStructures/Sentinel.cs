using System;

// #define ENABLE_THREAD_SAFE_CHECKS
// using System.Threading;
// using Volatile = System.Threading.Volatile;
// using System.Runtime.CompilerServices;
// using System.Runtime.InteropServices;
// using Svelto.DataStructures;
// using System;
// using Unity.Burst;
//
namespace Svelto.Common.DataStructures
{
    // #if ENABLE_THREAD_SAFE_CHECKS
    //     /// <summary>
    //     /// internal structure just for the sake of creating the sentinel dictionary
    //     /// </summary>
    //     /// <typeparam name="T"></typeparam>
    //     public struct SentinelNB<T> : IBuffer<T> where T : struct
    //     {
    //         public SentinelNB(IntPtr array, uint capacity) : this()
    //         {
    //             _ptr      = array;
    //             _capacity = capacity;
    //         }
    //
    //         public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count)
    //         {
    //             for (int i = 0; i < count; i++)
    //             {
    //                 destination[i] = this[i];
    //             }
    //         }
    //
    //         public void Clear()
    //         {
    //             MemoryUtilities.MemClear<T>(_ptr, _capacity);
    //         }
    //
    //         public T[] ToManagedArray()
    //         {
    //             throw new NotImplementedException();
    //         }
    //
    //         [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //         public IntPtr ToNativeArray(out int capacity)
    //         {
    //             capacity = (int)_capacity;
    //             return _ptr;
    //         }
    //
    //         public int capacity
    //         {
    //             [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //             get => (int)_capacity;
    //         }
    //
    //         public bool isValid => _ptr != IntPtr.Zero;
    //
    //         public ref T this[uint index]
    //         {
    //             [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //             get
    //             {
    //                 unsafe
    //                 {
    // #if ENABLE_DEBUG_CHECKS
    //                     if (index >= _capacity)
    //                         throw new Exception($"NativeBuffer - out of bound access: index {index} - capacity {capacity}");
    // #endif
    //                     {
    //                         var     size  = MemoryUtilities.SizeOf<T>();
    //                         ref var asRef = ref Unsafe.AsRef<T>((void*)(_ptr + (int)(index * size)));
    //                         return ref asRef;
    //                     }
    //                 }
    //             }
    //         }
    //
    //         public ref T this[int index]
    //         {
    //             [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //             get
    //             {
    //                 unsafe
    //                 {
    // #if ENABLE_DEBUG_CHECKS
    //                     if (index < 0 || index >= _capacity)
    //                         throw new Exception($"NativeBuffer - out of bound access: index {index} - capacity {capacity}");
    // #endif
    //                     {
    //                         var     size  = MemoryUtilities.SizeOf<T>();
    //                         ref var asRef = ref Unsafe.AsRef<T>((void*)(_ptr + index * size));
    //                         return ref asRef;
    //                     }
    //                 }
    //             }
    //         }
    //
    //         readonly uint _capacity;
    //
    // #if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
    // #if UNITY_BURST
    //         [Unity.Burst.NoAlias]
    // #endif
    //         [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
    // #endif
    //         readonly IntPtr _ptr;
    //
    //         //I can't make the _threadSentinel shareable, so I have to assume that this datastructure is used in
    //         //parallel only with mechanism similar to the DOTS job strategy
    //     }
    //
    //     /// <summary>
    //     /// internal structure just for the sake of creating the sentinel dictionary
    //     /// </summary>
    //     /// <typeparam name="T"></typeparam>
    //     public struct SentinelNativeStrategy<T> : IBufferStrategy<T> where T : struct
    //     {
    // #if DEBUG && !PROFILE_SVELTO
    //         static SentinelNativeStrategy()
    //         {
    //             if (TypeType.isUnmanaged<T>() == false)
    //                 throw new DBC.Common.PreconditionException("Only unmanaged data can be stored natively");
    //         }
    // #endif
    //         public SentinelNativeStrategy(uint size, Allocator allocator, bool clear = true) : this()
    //         {
    //             Alloc(size, allocator, clear);
    //         }
    //
    //         public int       capacity           => _realBuffer.capacity;
    //         public Allocator allocationStrategy => _nativeAllocator;
    //
    //         public void Alloc(uint newCapacity, Allocator allocator, bool clear)
    //         {
    // #if DEBUG && !PROFILE_SVELTO
    //             if (!(this._realBuffer.ToNativeArray(out _) == IntPtr.Zero))
    //                 throw new DBC.Common.PreconditionException("can't alloc an already allocated buffer");
    // #endif
    //             _nativeAllocator = allocator;
    //
    //             IntPtr        realBuffer = MemoryUtilities.Alloc<T>(newCapacity, _nativeAllocator, clear);
    //             SentinelNB<T> b          = new SentinelNB<T>(realBuffer, newCapacity);
    //             _invalidHandle = true;
    //             _realBuffer    = b;
    //         }
    //
    //         public void Resize(uint newSize, bool copyContent = true)
    //         {
    //             if (newSize != capacity)
    //             {
    //                 IntPtr pointer = _realBuffer.ToNativeArray(out _);
    //                 pointer = MemoryUtilities.Realloc<T>(pointer, newSize, _nativeAllocator,
    //                     (uint)newSize > capacity ? (uint)capacity : newSize, copyContent);
    //                 SentinelNB<T> b = new SentinelNB<T>(pointer, newSize);
    //                 _realBuffer    = b;
    //                 _invalidHandle = true;
    //             }
    //         }
    //
    //         public IntPtr AsBytesPointer()
    //         {
    //             throw new NotImplementedException();
    //         }
    //
    //         public void SerialiseFrom(IntPtr bytesPointer)
    //         {
    //             throw new NotImplementedException();
    //         }
    //
    //         public void ShiftLeft(uint index, uint count)
    //         {
    //             DBC.Common.Check.Require(index < capacity, "out of bounds index");
    //             DBC.Common.Check.Require(count < capacity, "out of bounds count");
    //
    //             if (count == index)
    //                 return;
    //
    //             DBC.Common.Check.Require(count > index, "wrong parameters used");
    //
    //             var array = _realBuffer.ToNativeArray(out _);
    //
    //             MemoryUtilities.MemMove<T>(array, index + 1, index, count - index);
    //         }
    //
    //         public void ShiftRight(uint index, uint count)
    //         {
    //             DBC.Common.Check.Require(index < capacity, "out of bounds index");
    //             DBC.Common.Check.Require(count < capacity, "out of bounds count");
    //
    //             if (count == index)
    //                 return;
    //
    //             DBC.Common.Check.Require(count > index, "wrong parameters used");
    //
    //             var array = _realBuffer.ToNativeArray(out _);
    //
    //             MemoryUtilities.MemMove<T>(array, index, index + 1, count - index);
    //         }
    //
    //         public bool isValid => _realBuffer.isValid;
    //
    //         public void Clear() => _realBuffer.Clear();
    //
    //         public ref T this[uint index]
    //         {
    //             [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //             get => ref _realBuffer[index];
    //         }
    //
    //         public ref T this[int index]
    //         {
    //             [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //             get => ref _realBuffer[index];
    //         }
    //
    //         /// <summary>
    //         /// Note on the code of this method. Interfaces cannot be held by this structure as it must be used by Burst.
    //         /// This method could return directly _realBuffer, but this would cost of a boxing allocation.
    //         /// Using the GCHandle.Alloc I will occur to the boxing, but only once as long as the native handle is still
    //         /// valid
    //         /// </summary>
    //         /// <returns></returns>
    //         IBuffer<T> IBufferStrategy<T>.ToBuffer()
    //         {
    //             //handle has been invalidated, dispose of the hold GCHandle (if exists)
    //             if (_invalidHandle == true && ((IntPtr)_cachedReference != IntPtr.Zero))
    //             {
    //                 _cachedReference.Free();
    //                 _cachedReference = default;
    //             }
    //
    //             _invalidHandle = false;
    //             if (((IntPtr)_cachedReference == IntPtr.Zero))
    //             {
    //                 _cachedReference = GCHandle.Alloc(_realBuffer, GCHandleType.Normal);
    //             }
    //
    //             return (IBuffer<T>)_cachedReference.Target;
    //         }
    //
    //         public SentinelNB<T> ToRealBuffer()
    //         {
    //             return _realBuffer;
    //         }
    //
    //         public void Dispose()
    //         {
    //             if ((IntPtr)_cachedReference != IntPtr.Zero)
    //                 _cachedReference.Free();
    //
    //             if (_realBuffer.ToNativeArray(out _) != IntPtr.Zero)
    //                 MemoryUtilities.Free(_realBuffer.ToNativeArray(out _), Allocator.Persistent);
    //             else
    //                 throw new Exception("trying to dispose disposed buffer");
    //
    //             _cachedReference = default;
    //             _realBuffer      = default;
    //         }
    //
    //         Allocator     _nativeAllocator;
    //         SentinelNB<T> _realBuffer;
    //         bool          _invalidHandle;
    //
    // #if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
    //         [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
    // #endif
    //         GCHandle _cachedReference;
    //     }
    //
    //     public struct SharedDictionaryStruct
    //     {
    //         internal IntPtr test;
    //
    //         SveltoDictionary<long, Sentinel, SentinelNativeStrategy<SveltoDictionaryNode<long>>,
    //             SentinelNativeStrategy<Sentinel>, SentinelNativeStrategy<int>> cast
    //         {
    //             get
    //             {
    //                 unsafe
    //                 {
    //                     return Unsafe
    //                        .AsRef<SveltoDictionary<long, Sentinel, SentinelNativeStrategy<SveltoDictionaryNode<long>>,
    //                             SentinelNativeStrategy<Sentinel>, SentinelNativeStrategy<int>>>((void*)test);
    //                 }
    //             }
    //         }
    //
    //
    //         public void Add(long ptr, Sentinel sentinel)
    //         {
    //             cast.Add(ptr, sentinel);
    //         }
    //
    //         public ref Sentinel GetValueByRef(long ptr)
    //         {
    //             return ref cast.GetValueByRef(ptr);
    //         }
    //
    //         public bool Exists(IntPtr ptr)
    //         {
    //             return cast.TryFindIndex((long)ptr, out _);
    //         }
    //     }
    //
    //     public static class SharedDictonary
    //     {
    // #if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
    //         [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    // #endif
    //         public static void InitFuncPtr()
    //         {
    //             unsafe
    //             {
    //             }
    //         }
    //
    //         static SharedDictonary()
    //         {
    //             unsafe
    //             {
    // #if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
    //                 SharedStatic<SharedDictionaryStruct> test = Unity.Burst.SharedStatic<SharedDictionaryStruct>
    //                    .GetOrCreate<SharedDictionaryStruct>();
    // #else
    //                 var test = default(SharedDictionaryStruct);
    // #endif
    //                 test.Data.test = MemoryUtilities
    //                    .Alloc<SveltoDictionary<long, Sentinel, SentinelNativeStrategy<SveltoDictionaryNode<long>>,
    //                         SentinelNativeStrategy<Sentinel>, SentinelNativeStrategy<int>>>((uint)1, Allocator.Persistent);
    //
    //                 Unsafe.AsRef<SveltoDictionary<long, Sentinel, SentinelNativeStrategy<SveltoDictionaryNode<long>>,
    //                         SentinelNativeStrategy<Sentinel>, SentinelNativeStrategy<int>>>((void*)test.Data.test) =
    //                     new SveltoDictionary<long, Sentinel, SentinelNativeStrategy<SveltoDictionaryNode<long>>,
    //                         SentinelNativeStrategy<Sentinel>, SentinelNativeStrategy<int>>(1, Allocator.Persistent);
    //
    //                 SharedDictonary.test = test;
    //             }
    //         }
    // #if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
    //         public static readonly Unity.Burst.SharedStatic<SharedDictionaryStruct> test;
    // #else
    //         public static readonly SharedDictionaryStruct test;
    // #endif
    //     }
    // #endif
    //
    //     public struct Sentinel
    //     {
    // #if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
    //         SharedDictionaryStruct sharedMultithreadTest => SharedDictonary.test.Data;
    // #else
    //         SharedDictionaryStruct sharedMultithreadTest => SharedDictonary.test;
    // #endif
    //         public static int ReadFlag  = 1;
    //         public static int WriteFlag = 2;
    //
    //         internal void Use()
    //         {
    // #if ENABLE_THREAD_SAFE_CHECKS
    //             if (_flag == WriteFlag)
    //             {
    //                 //if the state is found in NOT_USED is fine, all the other cases are not
    //                 ref var threadSentinel = ref sharedMultithreadTest.GetValueByRef(_ptr)._threadSentinel;
    //                 if (Interlocked.CompareExchange(ref threadSentinel, USED_WRITE, NOT_USED) != NOT_USED)
    //                     throw new Exception(
    //                         "This datastructure is not thread safe, reading and writing operations can happen" +
    //                         "on different threads, but not simultaneously");
    //             }
    //             else
    //                 //if the state is found in NOT_USED or USED_READ, read is allowed
    //             if (_flag == ReadFlag)
    //             {
    //                 ref var threadSentinel = ref sharedMultithreadTest.GetValueByRef(_ptr)._threadSentinel;
    //                 if (Interlocked.CompareExchange(ref threadSentinel, USED_READ, NOT_USED) > USED_READ)
    //                     throw new Exception(
    //                         "This datastructure is not thread safe, reading and writing operations can happen" +
    //                         "on different threads, but not simultaneously");
    //             }
    // #endif
    //         }
    //
    //         /// <summary>
    //         /// warning the constructor is not thread safe, so i.e.: it must be used always from mainthread
    //         /// </summary>
    //         /// <param name="ptr"></param>
    //         /// <param name="flag"></param>
    //         public Sentinel(IntPtr ptr, int flag) : this()
    //         {
    // #if ENABLE_THREAD_SAFE_CHECKS
    //             _ptr  = ptr.ToInt64();
    //             _flag = flag;
    //             if (flag != 0)
    //             {
    //                 if (sharedMultithreadTest.Exists(ptr) == false)
    //                     sharedMultithreadTest.Add(_ptr, this);
    //             }
    // #endif
    //         }
    //
    //         internal void Release()
    //         {
    // #if ENABLE_THREAD_SAFE_CHECKS
    //             if (_flag >= 1)
    //                 Volatile.Write(ref sharedMultithreadTest.GetValueByRef(_ptr)._threadSentinel, NOT_USED);
    // #endif
    //         }
    //
    //         /// <summary>
    //         /// This method instead is thread safe, as long as Sentinels are not being created by other threads
    //         /// </summary>
    //         /// <returns></returns>
    //         public TestThreadSafety TestThreadSafety()
    //         {
    //             return new TestThreadSafety(this);
    //         }
    //
    // #if ENABLE_THREAD_SAFE_CHECKS
    //         int           _threadSentinel;
    //         Allocator     _allocator;
    //         readonly long _ptr;
    //         readonly int  _flag;
    //
    //         const int NOT_USED   = 0;
    //         const int USED_READ  = 1;
    //         const int USED_WRITE = 2;
    //
    // #endif
    //     }
    //
    //     public struct TestThreadSafety : IDisposable
    //     {
    // #if ENABLE_THREAD_SAFE_CHECKS
    //         Sentinel _sentinel;
    // #endif
    //
    //         public TestThreadSafety(Sentinel sentinel)
    //         {
    // #if ENABLE_THREAD_SAFE_CHECKS
    //             _sentinel = sentinel;
    //             _sentinel.Use();
    // #endif
    //         }
    //
    //         public void Dispose()
    //         {
    // #if ENABLE_THREAD_SAFE_CHECKS
    //             _sentinel.Release();
    // #endif
    //         }
    //     }
    
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
}


