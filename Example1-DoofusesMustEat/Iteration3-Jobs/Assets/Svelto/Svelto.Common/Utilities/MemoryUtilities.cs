using System;
using System.Runtime.CompilerServices;

#if !ENABLE_BURST_AOT
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#endif

namespace Svelto.Common
{
#if !ENABLE_BURST_AOT
    public enum Allocator
    {
        
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
        public static void Free(IntPtr ptr, Allocator allocator)
        {
            unsafe
            {
#if ENABLE_BURST_AOT
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Free((void*) ptr, (Unity.Collections.Allocator) allocator);
#else
                Marshal.FreeHGlobal((IntPtr) ptr);
#endif
            }
        }

        public static void MemCpy(IntPtr newPointer, IntPtr head, uint currentSize)
        {
            unsafe 
            {
                Unsafe.CopyBlock((void*) newPointer, (void*) head, currentSize);
            }
        }

        public static IntPtr Alloc(uint newCapacity, uint alignOf, Allocator allocator)
        {
            unsafe
            {
#if ENABLE_BURST_AOT
                var newPointer =
                    (void*) Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(newCapacity, (int) alignOf, (Unity.Collections.Allocator) allocator);
#else
                var newPointer = Marshal.AllocHGlobal((int) newCapacity);
#endif
                return (IntPtr) newPointer;
            }
        }

        public static void MemClear(IntPtr listData, uint sizeOf)
        {
            unsafe 
            {
#if ENABLE_BURST_AOT
               Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemClear((void*) listData, sizeOf);
#else
               Unsafe.InitBlock((void*) listData, 0, sizeOf);
#endif
            }
        }

        public static uint SizeOf<T>() where T : struct
        {
            return (uint) Unsafe.SizeOf<T>();
        }

        public static uint AlignOf<T>() { return 4; }

        public static void CopyStructureToPtr<T>(ref T buffer, IntPtr bufferPtr) where T : struct
        {
            unsafe 
            {
                Unsafe.Write((void*) bufferPtr, buffer);
            }
        }

        public static ref T ArrayElementAsRef<T>(IntPtr data, int threadIndex) where T : struct
        {
            unsafe
            {
                return ref Unsafe.AsRef<T>(Unsafe.Add<T>((void*) data, threadIndex));
            }
        }
    }
}