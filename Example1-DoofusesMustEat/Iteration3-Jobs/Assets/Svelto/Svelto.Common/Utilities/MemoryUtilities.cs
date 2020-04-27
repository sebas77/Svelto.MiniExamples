using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if !UNITY_COLLECTIONS
using System.Runtime.InteropServices;
#else
using Unity.Collections.LowLevel.Unsafe;
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
#if UNITY_5_3_OR_NEWER && !UNITY_COLLECTIONS        
        static MemoryUtilities()
        {
            throw new Exception("Svelto.Common MemoryUtilities needs the Unity Collection package");      
        }
#endif
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(IntPtr ptr, Allocator allocator)
        {
            unsafe
            {
#if UNITY_COLLECTIONS
                UnsafeUtility.Free((void*) ptr, (Unity.Collections.Allocator) allocator);
#else
                Marshal.FreeHGlobal((IntPtr) ptr);
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Alloc<T>(uint newCapacity, Allocator allocator) where T : struct
        {
            unsafe
            {
#if UNITY_COLLECTIONS
                var newPointer =
                    UnsafeUtility.Malloc(newCapacity, (int) UnsafeUtility.AlignOf<T>(), (Unity.Collections.Allocator) allocator);
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
               UnsafeUtility.MemClear((void*) listData, sizeOf);
#else
               Unsafe.InitBlock((void*) listData, 0, sizeOf);
#endif
            }
        }

        static class CachedSize<T> where T : struct
        {
            public static readonly int cachedSize = Unsafe.SizeOf<T>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>() where T : struct
        {
            return CachedSize<T>.cachedSize;
        }

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
        
        public static int GetFieldOffset(RuntimeFieldHandle h) => 
            Marshal.ReadInt32(h.Value + (4 + IntPtr.Size)) & 0xFFFFFF;

        public static int GetFieldOffset(FieldInfo field)
        {
#if UNITY_COLLECTIONS
            return UnsafeUtility.GetFieldOffset(field);
#else
            return GetFieldOffset(field.name);
#endif
        }
    }
}