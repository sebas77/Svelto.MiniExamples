using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly struct EGIDMapper<T> where T : struct, IEntityComponent
    {
        readonly ITypeSafeDictionary<T> map;
        public uint Length => map.count;
        public ExclusiveGroupStruct groupID { get; }

        public EGIDMapper(ExclusiveGroupStruct groupStructId, ITypeSafeDictionary<T> dic):this()
        {
            groupID = groupStructId;
            map = dic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(uint entityID)
        {
#if DEBUG && !PROFILE_SVELTO
                if (map.TryFindIndex(entityID, out var findIndex) == false)
                    throw new System.Exception("Entity not found in this group ".FastConcat(typeof(T).ToString()));
#else
                map.TryFindIndex(entityID, out var findIndex);
#endif
                return ref map.GetDirectValueByRef(findIndex);
        }
        
        public bool TryGetEntity(uint entityID, out T value)
        {
            if (map.TryFindIndex(entityID, out var index))
            {
                value = map.GetDirectValueByRef(index);
                return true;
            }

            value = default;
            return false;
        }
        
        public IBuffer<T> GetArrayAndEntityIndex(uint entityID, out uint index)
        {
            if (map.TryFindIndex(entityID, out index))
            {
                return map.GetValues(out _);
            }

            throw new ECSException("Entity not found");
        }
        
        public bool TryGetArrayAndEntityIndex(uint entityID, out uint index, out IBuffer<T> array)
        {
            if (map.TryFindIndex(entityID, out index))
            {
                array = map.GetValues(out _);
                return true;
            }

            array = default;
            return false;
        }
    }
}

