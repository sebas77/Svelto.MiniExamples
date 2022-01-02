using System;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeDictionary<TValue> : ITypeSafeDictionary where TValue : IEntityComponent
    {
        void Add(uint egidEntityId, in TValue entityComponent);
        
        bool       TryGetValue(uint entityId, out TValue item);
        ref TValue GetOrCreate(uint idEntityId);

        IBuffer<TValue> GetValues(out uint count);
        ref TValue      GetDirectValueByRef(uint key);
        ref TValue      GetValueByRef(uint key);
    }

    public interface ITypeSafeDictionary : IDisposable
    {
        int                 count { get; }
        
        ITypeSafeDictionary Create();

        void AddEntitiesToDictionary
        (ITypeSafeDictionary toDictionary, ExclusiveGroupStruct groupId, in EnginesRoot.LocatorMap entityLocator);

        void ExecuteEnginesAddCallbacks
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>> entityComponentEnginesDb
       , ITypeSafeDictionary destinationDatabase, ExclusiveGroupStruct toGroup, in PlatformProfiler profiler);

        void SwapEntitiesBetweenDictionaries
        (FasterList<(uint, string)> infosToProcess, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup
       , ITypeSafeDictionary toComponentsDictionary);

        void ExecuteEnginesSwapCallbacks
        (FasterList<(uint, string)> infosToProcess
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>> reactiveEnginesSwap
       , ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup, in PlatformProfiler sampler);

        void ExecuteEnginesSwapGroupCallbacks
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>> reactiveEnginesSwap
       , ITypeSafeDictionary toEntitiesDictionary, ExclusiveGroupStruct fromGroupId, ExclusiveGroupStruct toGroupId
       , in PlatformProfiler platformProfiler);

        void RemoveEntitiesFromDictionary(FasterList<(uint, string)> infosToProcess);

        void ExecuteEnginesRemoveCallbacks
        (FasterList<(uint, string)> infosToProcess
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>> reactiveEnginesRemove
       , ExclusiveGroupStruct fromGroup, in PlatformProfiler sampler);

        void ExecuteEnginesRemoveGroupCallbacks
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>> engines
       , ExclusiveGroupStruct group, in PlatformProfiler profiler);

        void IncreaseCapacityBy(uint size);
        void EnsureCapacity(uint size);
        void Trim();
        void Clear();
        bool Has(uint key);
        bool ContainsKey(uint egidEntityId);
        uint GetIndex(uint valueEntityId);
        bool TryFindIndex(uint entityGidEntityId, out uint index);

        void KeysEvaluator(System.Action<uint> action);
    }
}