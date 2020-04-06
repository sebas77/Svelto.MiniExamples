using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.ECS.DataStructures
{
    public struct UnsafeArrayIndex
    {
        internal uint writerPointer;
        internal uint capacity;
    }

    /// <summary>
    /// Note: this must work inside burst, so it must follow burst restrictions
    /// </summary>
    struct UnsafeArray : IDisposable 
    {    
        /// <summary>
        /// </summary>
#if ENABLE_BURST_AOT        
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        internal unsafe byte* ptr;

        /// <summary>
        /// </summary>
        uint writePointer, readPointer;

        /// <summary>
        /// </summary>
        internal uint capacity;

        internal uint size => writePointer - readPointer;
        internal uint space => capacity - size;

        /// <summary>
        /// </summary>
        internal Allocator allocator;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Write<T>(in T item) where T : struct
        {
            unsafe
            {
                uint structSize = (uint) Align4((uint) Unsafe.SizeOf<T>());
            
                //the idea is, considering the wrap, a read pointer must always be behind a writer pointer
#if DEBUG && !PROFILE_SVELTO                
                if (space - structSize < 0)
                    throw new Exception("no writing authorized");
#endif
                var pointer = writePointer % capacity;

                if (pointer + structSize <= capacity)
                    Unsafe.Write(ptr + pointer, item);
                else
                //copy with wrap
                {
                    var byteCount = capacity - pointer;
                    //need a copy to be sure that the GC won't move the data around
                    T copyItem = item; 
                    void* asPointer = Unsafe.AsPointer(ref copyItem);
                    Unsafe.CopyBlock(ptr + pointer, asPointer, byteCount);
                    var restCount = structSize - byteCount;
                    Unsafe.CopyBlock(ptr, (byte *)asPointer + byteCount, restCount);
                }

                writePointer += structSize;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteUnaligned<T>(in T item) where T : struct
        {
            unsafe
            {
                uint structSize = (uint) Unsafe.SizeOf<T>();
            
                //the idea is, considering the wrap, a read pointer must always be behind a writer pointer
#if DEBUG && !PROFILE_SVELTO                
                if (space - structSize < 0)
                    throw new Exception("no writing authorized");
#endif

                var pointer = writePointer % capacity;

                if (pointer + structSize <= capacity)
                    Unsafe.Write(ptr + pointer, item);
                else
                {
                    var byteCount = capacity - pointer;
                    T copyItem = item;
                    var asPointer = Unsafe.AsPointer(ref copyItem);
                    Unsafe.CopyBlock(ptr + pointer, asPointer, byteCount);
                    var restCount = structSize - byteCount;
                    Unsafe.CopyBlock(ptr, (byte *)asPointer + byteCount, restCount);
                }

                writePointer += structSize;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T Read<T>() where T : struct
        {
            unsafe
            {
                uint structSize = (uint) Align4((uint) Unsafe.SizeOf<T>());
#if DEBUG && !PROFILE_SVELTO            
                if (size < structSize)
                    throw new Exception("dequeuing empty queue or unexpected type dequeued");
#endif

                var pointer = readPointer % capacity;
                byte* addr = ptr + pointer;
                
                readPointer += structSize;
#if DEBUG && !PROFILE_SVELTO                            
                if (readPointer > writePointer)
                    throw new Exception("unexpected read");
#endif               
                
                if (pointer + structSize <= capacity)
                    return Unsafe.Read<T>(addr);
                else
                {
                    T item = default;
                    var byteCount = capacity - pointer;
                    var asPointer = Unsafe.AsPointer(ref item);
                    Unsafe.CopyBlock(asPointer, ptr + pointer, byteCount);
                    var restCount = structSize - byteCount;
                    Unsafe.CopyBlock((byte *)asPointer + byteCount, ptr, restCount);

                    return item;
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T Reserve<T>(out UnsafeArrayIndex index) where T : unmanaged
        {
            unsafe
            {
                var align4 = (uint) Align4((uint) Unsafe.SizeOf<T>());
                
                var head   = writePointer % capacity;
                T*  buffer = (T*) (ptr + head);
#if DEBUG && !PROFILE_SVELTO
                if (head + align4 > capacity)
                    throw new Exception("Reserving wrong index");
#endif                
                index = new UnsafeArrayIndex()
                {
                    capacity      = capacity
                  , writerPointer = writePointer
                };

                writePointer += align4;
            
                return ref buffer[0];
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T AccessReserved<T>(UnsafeArrayIndex index) where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (index.writerPointer >= capacity) throw new Exception($"SimpleNativeArray: out of bound access, index {index} capacity {capacity}");
                if (index.writerPointer < readPointer) throw new Exception($"SimpleNativeArray: out of bound access, index {index} count {readPointer % capacity}");
#endif                

                T* buffer = (T*) (ptr + index.writerPointer);
            
                return ref buffer[0];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Realloc(uint alignOf, uint newCapacity)
        {
            unsafe
            {
                //be sure it's multiple of 4. Assuming that what we write is aligned to 4, then we will always have aligned wrapped heads
                newCapacity = (uint)(Math.Ceiling(newCapacity / 4.0) * 4); 
                
                byte* newPointer = null;
#if DEBUG && !PROFILE_SVELTO            
                if (newCapacity <= capacity)
                    throw new Exception("new capacity must be bigger than current");
#endif                

                if (newCapacity > 0)
                {
                    newPointer = (byte*) MemoryUtilities.Alloc(newCapacity, alignOf, allocator);
                    if (size > 0)
                    {
                        var readerHead = readPointer % capacity;
                        var writerHead = writePointer % capacity;

                        if (readerHead < writerHead)
                        {
                            var currentSize = writePointer - readPointer;
                            //copy to the new pointer, from th reader position
                            MemoryUtilities.MemCpy((IntPtr) newPointer, (IntPtr) (ptr + readPointer), currentSize);
                        }
                        //the assumption is that if size > 0 (so readerPointer and writerPointer are not the same)
                        //writerHead wrapped and reached readerHead. so I have to copy from readerHead to the end
                        //and from 0 to readherHEad
                        else
                        {
                            var sizeToTheEnd = capacity - readerHead;
                            
                            MemoryUtilities.MemCpy((IntPtr) newPointer, (IntPtr) (ptr + readerHead), sizeToTheEnd);
                            MemoryUtilities.MemCpy((IntPtr) (newPointer + sizeToTheEnd), (IntPtr) ptr, writerHead);
                        }
                    }
                }

                if (ptr != null)
                    MemoryUtilities.Free((IntPtr) ptr, allocator);

                writePointer = size;
                readPointer  = 0;
            
                ptr      = newPointer;
                capacity = newCapacity;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            unsafe
            {
                if (ptr != null)
                    MemoryUtilities.Free((IntPtr) ptr, allocator);

                ptr          = null;
                writePointer = 0;
                capacity     = 0;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            writePointer = 0;
            readPointer = 0;
        }

        uint Align4(uint input)
        {
            return (uint)(Math.Ceiling(input / 4.0) * 4);            
        }
    }
}
