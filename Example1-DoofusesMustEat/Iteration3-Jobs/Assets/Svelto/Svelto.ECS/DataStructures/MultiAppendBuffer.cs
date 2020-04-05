using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Allocator = Svelto.Common.Allocator;

namespace Svelto.ECS.DataStructures.Unity
{
    /// <summary>
    /// A collection of <see cref="SimpleNativeBag"/> intended to allow one buffer per thread.
    /// from: https://github.com/jeffvella/UnityEcsEvents/blob/develop/Runtime/MultiAppendBuffer.cs
    /// </summary>
    unsafe struct MultiAppendBuffer:IDisposable
    {
        public const int DefaultThreadIndex = -1;
        public const int MinThreadIndex = DefaultThreadIndex;

#if ENABLE_BURST_AOT        
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        SimpleNativeBag* _data;
        public readonly Allocator Allocator;
        readonly uint _threadsCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInvalidThreadIndex(int index) => index < MinThreadIndex || index > _threadsCount;

        public MultiAppendBuffer(Common.Allocator allocator, uint threadsCount)
        {
            Allocator = allocator;
            _threadsCount = threadsCount;

            var bufferSize = MemoryUtilities.SizeOf<SimpleNativeBag>();
            var bufferCount = _threadsCount;
            var allocationSize = bufferSize * bufferCount;

            var ptr = (byte*)MemoryUtilities.Alloc(allocationSize, MemoryUtilities.AlignOf<int>(), allocator);
            MemoryUtilities.MemClear((IntPtr) ptr, allocationSize);

            for (int i = 0; i < bufferCount; i++)
            {
                var bufferPtr = (SimpleNativeBag*)(ptr + bufferSize * i);
                var buffer = new SimpleNativeBag(allocator);
                MemoryUtilities.CopyStructureToPtr(ref buffer, (IntPtr) bufferPtr);
            }

            _data = (SimpleNativeBag*)ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref SimpleNativeBag GetBuffer(int index)
        {
            return ref MemoryUtilities.ArrayElementAsRef<SimpleNativeBag>((IntPtr) _data, index);
        }

        public uint count => _threadsCount;

        public void Dispose()
        {
            for (int i = 0; i < _threadsCount; i++)
            {
                GetBuffer(i).Dispose();
            }
            MemoryUtilities.Free((IntPtr) _data, Allocator);
        }

        public void Clear()
        {
            for (int i = 0; i < _threadsCount; i++)
            {
                GetBuffer(i).Clear();
            }
        }
    }
}
