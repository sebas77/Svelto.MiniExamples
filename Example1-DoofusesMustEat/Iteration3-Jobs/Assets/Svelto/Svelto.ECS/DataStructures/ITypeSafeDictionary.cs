using System;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeDictionary<TValue> : ITypeSafeDictionary where TValue : IEntityStruct
    {
        void       Add(uint            egidEntityId, in TValue entityView);
        ref TValue GetValueByRef(uint  key);
        TValue this[uint               idEntityId] { get; set; }
        bool       TryGetValue(uint    entityId, out TValue item);
        ref TValue GetDirectValue(uint findElementIndex);
        ref TValue GetOrCreate(uint    idEntityId);

        TValue[]                        GetValuesArray(out uint count);
        TValue[]                        unsafeValues   { get; }
    }

    public interface ITypeSafeDictionary
    {
        uint                Count { get; }
        ITypeSafeDictionary Create();

        void AddEntitiesToEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDb,
                                  ITypeSafeDictionary                                     realDic,
                                  ExclusiveGroup.ExclusiveGroupStruct                     @group,
                                  in PlatformProfiler                                     profiler);

        void RemoveEntitiesFromEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB,
                                       in PlatformProfiler                                     profiler,
                                       ExclusiveGroup.ExclusiveGroupStruct                     @group);

        void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId);

        void MoveEntityFromEngines(EGID                                                    fromEntityGid,
                                   EGID?                                                   toEntityID, ITypeSafeDictionary toGroup,
                                   FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines,
                                   in PlatformProfiler                                     profiler);

        void AddEntityToDictionary(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup);

        void RemoveEntityFromDictionary(EGID fromEntityGid, in PlatformProfiler profiler);

        void SetCapacity(uint size);
        void Trim();
        void Clear();
        void FastClear();
        bool Has(uint          key);
        bool ContainsKey(uint  egidEntityId);
        uint GetIndex(uint     valueEntityId);
        bool TryFindIndex(uint entityGidEntityId, out uint index);
    }
}