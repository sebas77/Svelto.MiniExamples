using System;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public class GlobalTypeID
    {
        internal static uint NextID<T>()
        {
            return (uint) (Interlocked.Increment(ref value) - 1);
        }

        static GlobalTypeID()
        {
            value = 0;
        }

        static int value;
    }

    interface IFiller 
    {
        void FillFromByteArray(EntityComponentInitializer init, NativeBag buffer);
    }

    delegate void ForceUnmanagedCast<T>(EntityComponentInitializer init, NativeBag buffer) where T : struct, IEntityComponent;

    class Filler<T>: IFiller where T : struct, IEntityComponent
    {
        static readonly ForceUnmanagedCast<T> _action;

        static Filler()
        {
            var method = typeof(Trick).GetMethod(nameof(Trick.ForceUnmanaged)).MakeGenericMethod(typeof(T));
            _action = (ForceUnmanagedCast<T>) Delegate.CreateDelegate(typeof(ForceUnmanagedCast<T>), method);
        }
        
        //it's an internal interface
        void IFiller.FillFromByteArray(EntityComponentInitializer init, NativeBag buffer)
        {
            DBC.ECS.Check.Require(UnmanagedTypeExtensions.IsUnmanaged<T>() == true, "invalid type used");

            _action(init, buffer);
        }
        
        static class Trick
        {    
            public static void ForceUnmanaged<U>(EntityComponentInitializer init, NativeBag buffer) where U : unmanaged, IEntityComponent
            {
                var component = buffer.Dequeue<U>();

                init.Init(component);
            }
        }
    }

    static class EntityComponentID<T>
    {
#if UNITY_ECS        
        internal static readonly Unity.Burst.SharedStatic<uint> ID = Unity.Burst.SharedStatic<uint>.GetOrCreate<GlobalTypeID, T>();
#else
        internal struct SharedStatic
        {
            public uint Data;
        }

        internal static SharedStatic ID;
#endif
    }

    static class EntityComponentIDMap
    {
        static readonly FasterList<IFiller> TYPE_IDS = new FasterList<IFiller>();

        internal static void Register<T>(IFiller entityBuilder) where T : struct, IEntityComponent
        {
            var location = EntityComponentID<T>.ID.Data = GlobalTypeID.NextID<T>();
            TYPE_IDS.AddAt(location, entityBuilder);
        }
        
        internal static IFiller GetTypeFromID(uint typeId)
        {
            return TYPE_IDS[typeId];
        }
    }
}
