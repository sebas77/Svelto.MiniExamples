#if UNITY_ECS
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS
{
    class GlobalTypeID
    {
        internal static uint NextID<T>()
        {
            //todo: this is not guaranteed to be unique, I must swap back to the increment!
            return (uint) Unity.Burst.BurstRuntime.GetHashCode32<T>();
#pragma warning disable 162
            return (uint) (Interlocked.Increment(ref value.Data) - 1);
#pragma warning restore 162
        }

        static GlobalTypeID()
        {
            value.Data = 0;
        }

        static readonly Unity.Burst.SharedStatic<int> value =
            Unity.Burst.SharedStatic<int>.GetOrCreate<int, GlobalTypeID>();
    }
    
    static class EntityComponentID<T>
    {
        public static readonly uint ID;

        static EntityComponentID()
        {
            ID = GlobalTypeID.NextID<T>();
        }
    }
    
    interface IFiller 
    {
        void FillFromByteArray(EntityComponentInitializer init, SimpleNativeBag buffer);
    }

    class Filler<T>: IFiller where T : struct, IEntityComponent
    {
        void IFiller.FillFromByteArray(EntityComponentInitializer init, SimpleNativeBag buffer)
        {
            var component = buffer.Dequeue<T>();

            init.Init(component);
        }
    }

    static class EntityComponentIDMap
    {
        static readonly FasterDictionary<uint, IFiller> TYPE_IDS = new FasterDictionary<uint, IFiller>();

        internal static void Register<T>(IFiller entityBuilder) where T : struct, IEntityComponent
        {
            var location2 = EntityComponentID<T>.ID;
            TYPE_IDS.Add(location2, entityBuilder);
        }
        
        internal static uint GetIDFromType<T>()
        {
            return EntityComponentID<T>.ID;
        }

        internal static IFiller GetTypeFromID(uint typeId)
        {
            return TYPE_IDS[typeId];
        }
    }
}
#endif