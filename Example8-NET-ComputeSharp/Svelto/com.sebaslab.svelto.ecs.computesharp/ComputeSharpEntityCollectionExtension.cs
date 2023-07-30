using System.Runtime.CompilerServices;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public static class ComputeSharpEntityCollectionExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1>
            (in this EntityCollection<T1> ec, out ComputeSharpBuffer<T1> buffer, out int count)
            where T1 : unmanaged, IEntityComputeSharpComponent
        {
            buffer = (ComputeSharpBuffer<T1>)ec._buffer;
            count  = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1>
        (in this EntityCollection<T1> ec, out ComputeSharpBuffer<T1> buffer, out NativeEntityIDs entityIDs
       , out int count) where T1 : unmanaged, IEntityComputeSharpComponent
        {
            buffer    = (ComputeSharpBuffer<T1>)ec._buffer;
            count     = (int)ec.count;
            entityIDs = (NativeEntityIDs)ec._entityIDs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>
        (in this EntityCollection<T1, T2> ec, out ComputeSharpBuffer<T1> buffer1, out ComputeSharpBuffer<T2> buffer2
       , out NativeEntityIDs entityIDs, out int count) where T1 : unmanaged, IEntityComputeSharpComponent
                                                       where T2 : unmanaged, IEntityComputeSharpComponent
        {
            buffer1   = (ComputeSharpBuffer<T1>)ec.buffer1._buffer;
            buffer2   = (ComputeSharpBuffer<T2>)ec.buffer2._buffer;
            count     = ec.count;
            entityIDs = (NativeEntityIDs)ec.buffer1._entityIDs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>
        (in this EntityCollection<T1, T2> ec, out ComputeSharpBuffer<T1> buffer1, out ComputeSharpBuffer<T2> buffer2
       , out int count) where T1 : unmanaged, IEntityComputeSharpComponent
                        where T2 : unmanaged, IEntityComputeSharpComponent
        {
            buffer1 = (ComputeSharpBuffer<T1>)ec.buffer1._buffer;
            buffer2 = (ComputeSharpBuffer<T2>)ec.buffer2._buffer;
            count   = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>
        (in this EntityCollection<T1, T2, T3> ec, out ComputeSharpBuffer<T1> buffer1
       , out ComputeSharpBuffer<T2> buffer2, out ComputeSharpBuffer<T3> buffer3, out int count)
            where T1 : unmanaged, IEntityComputeSharpComponent
            where T2 : unmanaged, IEntityComputeSharpComponent
            where T3 : unmanaged, IEntityComputeSharpComponent
        {
            buffer1 = (ComputeSharpBuffer<T1>)ec.buffer1._buffer;
            buffer2 = (ComputeSharpBuffer<T2>)ec.buffer2._buffer;
            buffer3 = (ComputeSharpBuffer<T3>)ec.buffer3._buffer;
            count   = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>
        (in this EntityCollection<T1, T2, T3> ec, out ComputeSharpBuffer<T1> buffer1
       , out ComputeSharpBuffer<T2> buffer2, out ComputeSharpBuffer<T3> buffer3, out NativeEntityIDs entityIDs
       , out int count) where T1 : unmanaged, IEntityComputeSharpComponent
                        where T2 : unmanaged, IEntityComputeSharpComponent
                        where T3 : unmanaged, IEntityComputeSharpComponent
        {
            buffer1   = (ComputeSharpBuffer<T1>)ec.buffer1._buffer;
            buffer2   = (ComputeSharpBuffer<T2>)ec.buffer2._buffer;
            buffer3   = (ComputeSharpBuffer<T3>)ec.buffer3._buffer;
            count     = (int)ec.count;
            entityIDs = (NativeEntityIDs)ec.buffer1._entityIDs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3, T4>
        (in this EntityCollection<T1, T2, T3, T4> ec, out ComputeSharpBuffer<T1> buffer1
       , out ComputeSharpBuffer<T2> buffer2, out ComputeSharpBuffer<T3> buffer3, out ComputeSharpBuffer<T4> buffer4
       , out int count) where T1 : unmanaged, IEntityComputeSharpComponent
                        where T2 : unmanaged, IEntityComputeSharpComponent
                        where T3 : unmanaged, IEntityComputeSharpComponent
                        where T4 : unmanaged, IEntityComputeSharpComponent
        {
            buffer1 = (ComputeSharpBuffer<T1>)ec.buffer1._buffer;
            buffer2 = (ComputeSharpBuffer<T2>)ec.buffer2._buffer;
            buffer3 = (ComputeSharpBuffer<T3>)ec.buffer3._buffer;
            buffer4 = (ComputeSharpBuffer<T4>)ec.buffer4._buffer;
            count   = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3, T4>
        (in this EntityCollection<T1, T2, T3, T4> ec, out ComputeSharpBuffer<T1> buffer1
       , out ComputeSharpBuffer<T2> buffer2, out ComputeSharpBuffer<T3> buffer3, out ComputeSharpBuffer<T4> buffer4
       , out NativeEntityIDs entityIDs, out int count) where T1 : unmanaged, IEntityComputeSharpComponent
                                                       where T2 : unmanaged, IEntityComputeSharpComponent
                                                       where T3 : unmanaged, IEntityComputeSharpComponent
                                                       where T4 : unmanaged, IEntityComputeSharpComponent
        {
            buffer1   = (ComputeSharpBuffer<T1>)ec.buffer1._buffer;
            buffer2   = (ComputeSharpBuffer<T2>)ec.buffer2._buffer;
            buffer3   = (ComputeSharpBuffer<T3>)ec.buffer3._buffer;
            buffer4   = (ComputeSharpBuffer<T4>)ec.buffer4._buffer;
            entityIDs = (NativeEntityIDs)ec.buffer1._entityIDs;
            count     = (int)ec.count;
        }
    }
}