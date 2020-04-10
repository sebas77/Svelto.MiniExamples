using System;
using System.Runtime.CompilerServices;
#if !UNITY_COLLECTIONS
using System.Runtime.InteropServices;
#endif
namespace Svelto.Common
{
#if !UNITY_COLLECTIONS
    public enum Allocator
    {
        Invalid ,
        None,
        Temp,
        TempJob,
        Persistent
    }
#else    
    public enum Allocator
    {
        /// <summary>
        ///   <para>Invalid allocation.</para>
        /// </summary>
        Invalid = Unity.Collections.Allocator.Invalid,
        /// <summary>
        ///   <para>No allocation.</para>
        /// </summary>
        None = Unity.Collections.Allocator.None,
        /// <summary>
        ///   <para>Temporary allocation.</para>
        /// </summary>
        Temp = Unity.Collections.Allocator.Temp,
        /// <summary>
        ///   <para>Temporary job allocation.</para>
        /// </summary>
        TempJob = Unity.Collections.Allocator.TempJob,
        /// <summary>
        ///   <para>Persistent allocation.</para>
        /// </summary>
        Persistent = Unity.Collections.Allocator.Persistent
    }
#endif

    public static class MemoryUtilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(IntPtr ptr, Allocator allocator)
        {
            unsafe
            {
#if UNITY_COLLECTIONS
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Free((void*) ptr, (Unity.Collections.Allocator) allocator);
#else
                Marshal.FreeHGlobal((IntPtr) ptr);
#endif
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemCpy(IntPtr newPointer, IntPtr head, uint currentSize)
        {
            unsafe 
            {
                Unsafe.CopyBlock((void*) newPointer, (void*) head, currentSize);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Alloc(uint newCapacity, uint alignOf, Allocator allocator)
        {
            unsafe
            {
#if UNITY_COLLECTIONS
                var newPointer =
                    (void*) Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(newCapacity, (int) alignOf, (Unity.Collections.Allocator) allocator);
#else
                var newPointer = Marshal.AllocHGlobal((int) newCapacity);
#endif
                return (IntPtr) newPointer;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemClear(IntPtr listData, uint sizeOf)
        {
            unsafe 
            {
#if UNITY_COLLECTIONS
               Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemClear((void*) listData, sizeOf);
#else
               Unsafe.InitBlock((void*) listData, 0, sizeOf);
#endif
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>() where T : struct
        {
            return Unsafe.SizeOf<T>();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AlignOf<T>() { return 4; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyStructureToPtr<T>(ref T buffer, IntPtr bufferPtr) where T : struct
        {
            unsafe 
            {
                Unsafe.Write((void*) bufferPtr, buffer);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T ArrayElementAsRef<T>(IntPtr data, int threadIndex) where T : struct
        {
            unsafe
            {
                return ref Unsafe.AsRef<T>(Unsafe.Add<T>((void*) data, threadIndex));
            }
        }
    }
}