using System;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeDictionary<TValue> : ITypeSafeDictionary where TValue : IEntityComponent
    {
        void Add(uint egidEntityId, in TValue entityComponent);
        ref TValue this[uint idEntityId] { get; }
        bool       TryGetValue(uint entityId, out TValue item);
        ref TValue GetOrCreate(uint idEntityId);

        IBuffer<TValue> GetValues(out uint count);
        ref TValue      GetDirectValueByRef(uint key);
    }

    public interface ITypeSafeDictionary : IDisposable
    {
        int                count { get; }
        ITypeSafeDictionary Create();

        void ExecuteEnginesAddCallbacks
        (uint startIndex, uint count
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>> entityComponentEnginesDb
       , ITypeSafeDictionary destinationDatabase, ExclusiveGroupStruct toGroup, in PlatformProfiler profiler);
        void ExecuteEnginesSwapCallbacks
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>> entityComponentEnginesDb
       , ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup, in PlatformProfiler profiler);
        void ExecuteEnginesRemoveCallbacks
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>> entityComponentEnginesDB
       , in PlatformProfiler profiler, ExclusiveGroupStruct @group);

        void ExecuteEnginesSwapOrRemoveCallbacks
        (EGID fromEntityGid, EGID? toEntityID, ITypeSafeDictionary toGroup
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>> engines, in PlatformProfiler profiler);

        void AddEntitiesFromDictionary
            (ITypeSafeDictionary entitiesToSubmit, ExclusiveGroupStruct groupId, EnginesRoot enginesRoot);

        void SwapEntityInDictionary(EGID fromEntityGid, EGID toEntityEGID, ITypeSafeDictionary toGroup);
        void RemoveEntityFromDictionary(EGID fromEntityGid);

        void ResizeTo(uint size);
        void Trim();
        void Clear();
        void FastClear();
        bool Has(uint key);
        bool ContainsKey(uint egidEntityId);
        uint GetIndex(uint valueEntityId);
        bool TryFindIndex(uint entityGidEntityId, out uint index);

        void KeysEvaluator(System.Action<uint> action);
    }
}