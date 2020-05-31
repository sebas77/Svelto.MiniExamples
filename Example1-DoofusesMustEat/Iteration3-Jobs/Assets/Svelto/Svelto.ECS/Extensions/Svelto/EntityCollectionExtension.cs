using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;

namespace Svelto.ECS
{
    public static class EntityCollectionExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BT<NB<T1>> ToBuffer<T1>(in this EntityCollection<T1> ec) where T1 : unmanaged, IEntityComponent
        {
            return new BT<NB<T1>>(ec.ToBuffer().ToFast(), ec.count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BT<NB<T1>, NB<T2>> ToBuffers<T1, T2>
            (in this EntityCollection<T1, T2> ec)
            where T2 : unmanaged, IEntityComponent where T1 : unmanaged, IEntityComponent
        {
            return new BT<NB<T1>, NB<T2>>(ec.Item1.ToBuffer().ToFast(), ec.Item2.ToBuffer().ToFast(), ec.count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BT<NB<T1>, NB<T2>, NB<T3>> ToBuffers<T1, T2, T3>
            (in this EntityCollection<T1, T2, T3> ec)
            where T2 : unmanaged, IEntityComponent
            where T1 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
        {
            return new BT<NB<T1>, NB<T2>, NB<T3>>(ec.Item1.ToBuffer().ToFast(), ec.Item2.ToBuffer().ToFast()
                                                , ec.Item3.ToBuffer().ToFast(), ec.count);
        }
    }

    public static class EntityCollectionExtensionB
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BT<MB<T1>> ToBuffer<T1>(in this EntityCollection<T1> ec) where T1 : struct, IEntityViewComponent
        {
            return new BT<MB<T1>>(ec.ToBuffer().ToFast(), ec.count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (MB<T1> buffer1, MB<T2> buffer2, uint count) ToBuffers<T1, T2>
            (in this EntityCollection<T1, T2> ec)
            where T2 : struct, IEntityViewComponent where T1 : struct, IEntityViewComponent
        {
            return (ec.Item1.ToBuffer().ToFast(), ec.Item2.ToBuffer().ToFast(), ec.count);
        }

        public static (MB<T1> buffer1, MB<T2> buffer2, MB<T3> buffer3, uint count) ToBuffers<T1, T2, T3>
            (in this EntityCollection<T1, T2, T3> ec)
            where T2 : struct, IEntityViewComponent
            where T1 : struct, IEntityViewComponent
            where T3 : struct, IEntityViewComponent
        {
            return (ec.Item1.ToBuffer().ToFast(), ec.Item2.ToBuffer().ToFast(), ec.Item3.ToBuffer().ToFast(), ec.count);
        }
    }

    public static class EntityCollectionExtensionC
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (NB<T1> buffer1, MB<T2> buffer2, uint count) ToBuffers<T1, T2>
            (in this EntityCollection<T1, T2> ec)
            where T1 : unmanaged, IEntityComponent where T2 : struct, IEntityViewComponent
        {
            return (ec.Item1.ToBuffer().ToFast(), ec.Item2.ToBuffer().ToFast(), ec.count);
        }

        public static (NB<T1> buffer1, MB<T2> buffer2, MB<T3> buffer3, uint count) ToBuffers<T1, T2, T3>
            (in this EntityCollection<T1, T2, T3> ec)
            where T1 : unmanaged, IEntityComponent
            where T2 : struct, IEntityViewComponent
            where T3 : struct, IEntityViewComponent
        {
            return (ec.Item1.ToBuffer().ToFast(), ec.Item2.ToBuffer().ToFast(), ec.Item3.ToBuffer().ToFast(), ec.count);
        }
    }
    

    public static class BTExtensionsA
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BT<NB<T1>> ToFast<T1>(in this BT<IBuffer<T1>> bt)
            where T1 : unmanaged, IEntityComponent
        {
            return new BT<NB<T1>>(bt.buffer.ToFast(), bt.count);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BT<NB<T1>, NB<T2>> ToFast<T1, T2>(in this BT<IBuffer<T1>, IBuffer<T2>> bt)
            where T2 : unmanaged, IEntityComponent
            where T1 : unmanaged, IEntityComponent
        {
            return new BT<NB<T1>, NB<T2>>(bt.buffer1.ToFast(), bt.buffer2.ToFast(), bt.count);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BT<NB<T1>, NB<T2>, NB<T3>> ToFast<T1, T2, T3>
            (in this BT<IBuffer<T1>, IBuffer<T2>, IBuffer<T3>> bt)
            where T2 : unmanaged, IEntityComponent
            where T1 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
        {
            return new BT<NB<T1>, NB<T2>, NB<T3>>(bt.buffer1.ToFast(), bt.buffer2.ToFast(), bt.buffer3.ToFast()
                                                , bt.count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BT<NB<T1>, NB<T2>, NB<T3>, NB<T4>> ToFast<T1, T2, T3, T4>
            (in this BT<IBuffer<T1>, IBuffer<T2>, IBuffer<T3>, IBuffer<T4>> bt)
            where T2 : unmanaged, IEntityComponent
            where T1 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
            where T4 : unmanaged, IEntityComponent
        {
            return new BT<NB<T1>, NB<T2>, NB<T3>, NB<T4>>(bt.buffer1.ToFast(), bt.buffer2.ToFast(), bt.buffer3.ToFast()
                                                        , bt.buffer4.ToFast(), bt.count);
        }
    }
    
    public static class BTExtensionsB
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BT<NB<T1>, NB<T2>, MB<T3>> ToFast<T1, T2, T3>
            (in this BT<IBuffer<T1>, IBuffer<T2>, IBuffer<T3>> bt)
            where T2 : unmanaged, IEntityComponent
            where T1 : unmanaged, IEntityComponent
            where T3 : struct, IEntityViewComponent
        {
            return new BT<NB<T1>, NB<T2>, MB<T3>>(bt.buffer1.ToFast(), bt.buffer2.ToFast(), bt.buffer3.ToFast()
                                                , bt.count);
        }
    }
}