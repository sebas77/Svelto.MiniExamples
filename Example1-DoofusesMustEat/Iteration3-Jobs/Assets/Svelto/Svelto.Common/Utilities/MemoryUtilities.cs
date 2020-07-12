#if DEBUG && !PROFILE_SVELTO
#define DEBUG_MEMORY
#endif

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

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
#if UNITY_EDITOR && !UNITY_COLLECTIONS        
        static MemoryUtilities()
        {
            #warning Svelto.Common is depending on the Unity Collection package. Alternatively you can import System.Runtime.CompilerServices.Unsafe.dll
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Alloc(uint newCapacity, Allocator allocator)
        {
            unsafe
            {
                var signedCapacity = (int) SignedCapacity(newCapacity);
#if UNITY_COLLECTIONS
                var newPointer =
                    Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(signedCapacity, (int) OptimalAlignment.alignment, (Unity.Collections.Allocator) allocator);
#else
                var newPointer = System.Runtime.InteropServices.Marshal.AllocHGlobal(signedCapacity);
#endif
                //Note MemClear is actually necessary
                MemClear((IntPtr) newPointer, (uint) signedCapacity);
          
                var signedPointer = SignedPointer(newCapacity, (IntPtr) newPointer);

                CheckBoundaries((IntPtr) newPointer);

                return signedPointer;
            }
        }

        public static IntPtr Realloc(IntPtr realBuffer, uint oldCapacity , uint newCapacity, Allocator allocator)
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO            
                if (newCapacity <= 0)
                    throw new Exception("new size must be greater than 0");
                if (newCapacity <= oldCapacity)
                    throw new Exception("new size must be greater than oldsize");
#endif          
                //Alloc returns the corret Signed Pointer already
                IntPtr signedPointer = Alloc(newCapacity, allocator);

                //Copy only the real data
                Unsafe.CopyBlock((void*) signedPointer, (void*) realBuffer, oldCapacity);
                
                //Free unsigns the pointer itself
                Free(realBuffer, allocator);
                return signedPointer;
#if NOT_USED_BUT_IF_WANT_TO_I_HAVE_TO_CHECK_THE_MEMORY_BOUNDARIES_BEFORE_TO_REALLOC
                //find the real pointer
                var realPointer = UnsignPointer(realBuffer);
                //find the real capacity with extra info
                var realCapacity = (IntPtr) SignedCapacity(newCapacity);
                //realloc with the new size
                var newPointer = System.Runtime.InteropServices.Marshal.ReAllocHGlobal(realPointer, realCapacity);
                
                var signedPointer = SignedPointer(newCapacity, newPointer);
#if DEBUG && !PROFILE_SVELTO
                //clear from the end of the old buffer to the end of the new buffer
                MemClear((IntPtr) signedPointer + (int) oldCapacity, newCapacity - oldCapacity);
#endif                
                //return the pointer with signature
                return signedPointer;
#endif
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(IntPtr ptr, Allocator allocator)
        {
            unsafe
            {
                ptr = CheckAndReturnPointerToFree(ptr);

#if UNITY_COLLECTIONS
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Free((void*) ptr, (Unity.Collections.Allocator) allocator);
#else
                System.Runtime.InteropServices.Marshal.FreeHGlobal((IntPtr) ptr);
#endif
            }
        }
       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemClear(IntPtr destination, uint sizeOf)
        {
            unsafe 
            {
#if UNITY_COLLECTIONS
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemClear((void*) destination, sizeOf);
#else
               Unsafe.InitBlock((void*) destination, 0, sizeOf);
#endif
            }
        }

        static class OptimalAlignment
        {
            internal static readonly uint alignment;

            static OptimalAlignment()
            {
                alignment = (uint) (Environment.Is64BitProcess ? 16 : 8);
            }
        }

        static class CachedSize<T> where T : struct
        {
            public static readonly uint cachedSize = (uint) Unsafe.SizeOf<T>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //THIS MUST STAY INT. THE REASON WHY EVERYTHING IS INT AND NOT UINT IS BECAUSE YOU CAN END UP
        //DOING SUBTRACT OPERATION EXPECTING TO BE < 0 AND THEY WON'T BE
        public static int SizeOf<T>() where T : struct
        {
            return (int) CachedSize<T>.cachedSize;
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

        public static int GetFieldOffset(FieldInfo field)
        {
#if UNITY_COLLECTIONS
            return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.GetFieldOffset(field);
#else
            int GetFieldOffset(RuntimeFieldHandle h) => 
                System.Runtime.InteropServices.Marshal.ReadInt32(h.Value + (4 + IntPtr.Size)) & 0xFFFFFF;

            return GetFieldOffset(field.FieldHandle);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Align4(uint input) { return (uint) (Math.Ceiling(input / 4.0) * 4); }
        
        static long SignedCapacity(uint newCapacity)
        {
#if DEBUG_MEMORY            
            return newCapacity + 128;
#else
            return newCapacity;
#endif            
        }

        static IntPtr SignedPointer(uint capacityWithoutSignature, IntPtr pointerToSign)
        {
            unsafe {
#if DEBUG_MEMORY            
                uint value = 0xDEADBEEF;
                for (int i = 0; i < 60; i += 4)
                {
                    Unsafe.Write((void*) pointerToSign, value); //4 bytes signature
                    pointerToSign += 4;
                }

                Unsafe.Write((void*) pointerToSign, capacityWithoutSignature); //4 bytes size allocated
                pointerToSign += 4;
                
                for (int i = 0; i < 64; i += 4)
                    Unsafe.Write( (void*) (pointerToSign+ (int) (capacityWithoutSignature) + i), value); //4 bytes size allocated

                return (IntPtr) ((byte*) pointerToSign);
#else
                return (IntPtr) pointerToSign;
#endif
            }
        }

        static IntPtr UnsignPointer(IntPtr ptr)
        {
#if DEBUG_MEMORY            
            return ptr - 64;
#else
            return ptr;
#endif
        }

        static IntPtr CheckAndReturnPointerToFree(IntPtr ptr)
        {
            ptr = UnsignPointer(ptr);
            
            CheckBoundaries(ptr);
            return ptr;
        }

        static unsafe void CheckBoundaries(IntPtr ptr)
        {
#if DEBUG_MEMORY
            var debugPtr = ptr;

            for (int i = 0; i < 60; i += 4)
            {
                var u = Unsafe.Read<uint>((void*) (debugPtr));
                if (u != 0xDEADBEEF)
                    throw new Exception();

                debugPtr += 4;
            }

            uint size = Unsafe.Read<uint>((void*) (debugPtr));
            debugPtr = debugPtr + (int) (4 + size);

            for (int i = 0; i < 64; i += 4)
            {
                var u = Unsafe.Read<uint>((void*) (debugPtr + i));
                if (u != 0xDEADBEEF)
                    throw new Exception();
            }
#endif
        }
    }
}