#if DEBUG && !PROFILE_SVELTO
//#define PARANOID_CHECK
#endif

using System;
using System.Runtime.CompilerServices;
using DBC.ECS;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Internal
{
    public sealed class ComputeSharpTypeSafeDictionary<TValue> : ITypeSafeDictionary<TValue> where TValue : struct, IEntityComponent
    {
        static readonly Type _type = typeof(TValue);
#if SLOW_SVELTO_SUBMISSION
        static readonly bool _hasEgid      = typeof(INeedEGID).IsAssignableFrom(_type);
        static readonly bool _hasReference = typeof(INeedEntityReference).IsAssignableFrom(_type);
#endif
        public ComputeSharpTypeSafeDictionary(uint size)
        {
                implMgd =
                    new SveltoDictionary<uint, TValue, ManagedStrategy<SveltoDictionaryNode<uint>>,
                        ManagedStrategy<TValue>, ManagedStrategy<int>>(size, Allocator.Managed);
        }

        public EntityIDs entityIDs
        {
            get
            {
                return new EntityIDs(implMgd.unsafeKeys);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(uint egidEntityId)
        {
            return implMgd.ContainsKey(egidEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(uint valueEntityId)
        {
            return implMgd.GetIndex(valueEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrAdd(uint idEntityId)
        {
            return ref implMgd.GetOrAdd(idEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBuffer<TValue> GetValues(out uint count)
        {
            return implMgd.UnsafeGetValues(out count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetDirectValueByRef(uint key)
        {
            return ref implMgd.GetDirectValueByRef(key);
        }

        public ref TValue GetValueByRef(uint key)
        {
            return ref implMgd.GetValueByRef(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(uint key)
        {
            return implMgd.ContainsKey(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindIndex(uint entityId, out uint index)
        {
            return implMgd.TryFindIndex(entityId, out index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(uint entityId, out TValue item)
        {
            return implMgd.TryGetValue(entityId, out item);
        }

        public int count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => implMgd.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITypeSafeDictionary Create()
        {
            return new ComputeSharpTypeSafeDictionary<TValue>(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            implMgd.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(uint size)
        {
            implMgd.EnsureCapacity(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncreaseCapacityBy(uint size)
        {
            implMgd.IncreaseCapacityBy(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            implMgd.Trim();
        }

        public void KeysEvaluator(Action<uint> action)
        {
            foreach (var key in implMgd.keys)
                    action(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint egidEntityId, in TValue entityComponent)
        {
            implMgd.Add(egidEntityId, entityComponent);
        }

        public void Dispose()
        {
            implMgd.Dispose();

            GC.SuppressFinalize(this);
        }

        public void AddEntitiesToDictionary(ITypeSafeDictionary toDictionary, ExclusiveGroupStruct groupId,
            in EnginesRoot.LocatorMap entityLocator)
        {
            void SharedAddEntitiesFromDictionary<Strategy1, Strategy2, Strategy3>(
                in SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary,
                ITypeSafeDictionary<TValue> toDic, in EnginesRoot.LocatorMap locator,
                ExclusiveGroupStruct toGroupID) where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
            {
                foreach (var tuple in fromDictionary)
                {
#if SLOW_SVELTO_SUBMISSION
                    var egid = new EGID(tuple.key, toGroupID);

                    if (_hasEgid)
                        SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref tuple.value, egid);

                    //todo: temporary code that will eventually be removed 
                    if (_hasReference)
                        SetEGIDWithoutBoxing<TValue>.SetRefWithoutBoxing(ref tuple.value,
                            locator.GetEntityReference(egid));
#endif
                    try
                    {
                        toDic.Add(tuple.key, tuple.value);
                    }
                    catch (Exception e)
                    {
                        Console.LogException(e,
                            "trying to add an EntityComponent with the same ID more than once Entity: "
                               .FastConcat(typeof(TValue).ToString()).FastConcat(", group ")
                               .FastConcat(toGroupID.ToString()).FastConcat(", id ").FastConcat(tuple.key));

                        throw;
                    }
#if PARANOID_CHECK && SLOW_SVELTO_SUBMISSION
                        DBC.ECS.Check.Ensure(_hasEgid == false || ((INeedEGID)fromDictionary[egid.entityID]).ID == egid, "impossible situation happened during swap");
#endif
                }
            }

            var destinationDictionary = toDictionary as ITypeSafeDictionary<TValue>;

            SharedAddEntitiesFromDictionary(implMgd, destinationDictionary, entityLocator, groupId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveEntitiesFromDictionary(FasterList<(uint, string)> infosToProcess)
        {
            void AgnosticMethod<Strategy1, Strategy2, Strategy3>(
                ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary)
                where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
            {
                var iterations = infosToProcess.count;

                for (var i = 0; i < iterations; i++)
                {
                    var (id, trace) = infosToProcess[i];

                    try
                    {
                        if (fromDictionary.Remove(id, out var value))
                            //Note I am doing this to be able to use a range of values even with the 
                            //remove Ex callbacks. Basically I am copying back the deleted value
                            //at the end of the array, so I can use as range 
                            //count, count + number of deleted entities
                            fromDictionary.GetDirectValueByRef((uint)fromDictionary.count) = value;
                    }
                    catch
                    {
                        var str = "Crash while executing Remove Entity operation on ".FastConcat(TypeCache<TValue>.name)
                           .FastConcat(" from : ", trace);

                        Console.LogError(str);

                        throw;
                    }
                }
            }

            AgnosticMethod(ref implMgd);
        }

        public void SwapEntitiesBetweenDictionaries(FasterList<(uint, uint, string)> infosToProcess,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup, ITypeSafeDictionary toComponentsDictionary)
        {
            void SharedSwapEntityInDictionary<Strategy1, Strategy2, Strategy3>(
                ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary,
                ITypeSafeDictionary<TValue> toDictionary)
                where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
            {
                var iterations = infosToProcess.count;

                for (var i = 0; i < iterations; i++)
                {
                    var (fromID, toID, trace) = infosToProcess[i];

                    try
                    {
                        var fromEntityGid = new EGID(fromID, fromGroup);
                        var toEntityEgid  = new EGID(toID, toGroup);

                        Check.Require(toGroup != null,
                            "Invalid To Group"); //todo check this, if it's right merge GetIndex

                        var isFound = fromDictionary.Remove(fromEntityGid.entityID, out var entity);
                        Check.Assert(isFound, "Swapping an entity that doesn't exist");
#if SLOW_SVELTO_SUBMISSION
                        if (_hasEgid)
                            SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityEgid);
#endif

                        toDictionary.Add(toEntityEgid.entityID, entity);

#if PARANOID_CHECK
                        DBC.ECS.Check.Ensure(_hasEgid == false || ((INeedEGID)toGroupCasted[toEntityEGID.entityID]).ID == toEntityEGID, "impossible situation happened during swap");
#endif
                    }
                    catch
                    {
                        var str = "Crash while executing Swap Entity operation on ".FastConcat(TypeCache<TValue>.name)
                           .FastConcat(" from : ", trace);

                        Console.LogError(str);

                        throw;
                    }
                }
            }

            var toGroupCasted = toComponentsDictionary as ITypeSafeDictionary<TValue>;

            SharedSwapEntityInDictionary(ref implMgd, toGroupCasted);
        }

        public void ExecuteEnginesAddCallbacks(
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAdd>>> entityComponentEnginesDB,
            ITypeSafeDictionary toDic, ExclusiveGroupStruct toGroup, in PlatformProfiler profiler)
        {
            void AgnosticMethod<Strategy1, Strategy2, Strategy3>(
                ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary,
                ITypeSafeDictionary<TValue> todic, in PlatformProfiler sampler)
                where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
            {
                if (entityComponentEnginesDB.TryGetValue(new RefWrapperType(_type), out var entityComponentsEngines))
                {
                    if (entityComponentsEngines.count == 0) return;

                    var dictionaryKeyEnumerator = fromDictionary.unsafeKeys;
                    var count                   = fromDictionary.count;

                    for (var i = 0; i < count; ++i)
                        try
                        {
                            var     key    = dictionaryKeyEnumerator[i].key;
                            ref var entity = ref todic.GetValueByRef(key);
                            var     egid   = new EGID(key, toGroup);
                            //get all the engines linked to TValue
                            for (var j = 0; j < entityComponentsEngines.count; j++)
                                using (sampler.Sample(entityComponentsEngines[j].name))
                                {
                                    ((IReactOnAdd<TValue>)entityComponentsEngines[j].engine).Add(ref entity, egid);
                                }
                        }
                        catch (Exception e)
                        {
                            Console.LogException(e,
                                "Code crashed inside Add callback with Type ".FastConcat(TypeCache<TValue>.name));

                            throw;
                        }
                }
            }

            var toDictionary = (ITypeSafeDictionary<TValue>)toDic;

            AgnosticMethod(ref implMgd, toDictionary, in profiler);
        }

        public void ExecuteEnginesSwapCallbacks(FasterList<(uint, uint, string)> infosToProcess,
            FasterList<ReactEngineContainer<IReactOnSwap>> reactiveEnginesSwap, ExclusiveGroupStruct fromGroup,
            ExclusiveGroupStruct toGroup, in PlatformProfiler profiler)
        {
            void AgnosticMethod<Strategy1, Strategy2, Strategy3>(
                ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary,
                in PlatformProfiler sampler) where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
            {
                if (reactiveEnginesSwap.count == 0) return;

                var iterations = infosToProcess.count;

                for (var i = 0; i < iterations; i++)
                {
                    var (fromEntityID, toEntityID, trace) = infosToProcess[i];

                    try
                    {
                        ref var entityComponent = ref fromDictionary.GetValueByRef(fromEntityID);
                        var     newEgid         = new EGID(toEntityID, toGroup);
                        for (var j = 0; j < reactiveEnginesSwap.count; j++)
                            using (sampler.Sample(reactiveEnginesSwap[j].name))
                            {
                                ((IReactOnSwap<TValue>)reactiveEnginesSwap[j].engine).MovedTo(ref entityComponent,
                                    fromGroup, newEgid);
                            }
                    }
                    catch
                    {
                        var str = "Crash while executing Swap Entity callback on ".FastConcat(TypeCache<TValue>.name)
                           .FastConcat(" from : ", trace);

                        Console.LogError(str);

                        throw;
                    }
                }
            }

            AgnosticMethod(ref implMgd, in profiler);
        }

        public void ExecuteEnginesRemoveCallbacks(FasterList<(uint, string)> infosToProcess,
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveEnginesRemove,
            ExclusiveGroupStruct fromGroup, in PlatformProfiler sampler)
        {
            void AgnosticMethod<Strategy1, Strategy2, Strategy3>(
                ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary,
                in PlatformProfiler profiler) where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
            {
                if (reactiveEnginesRemove.TryGetValue(new RefWrapperType(_type), out var entityComponentsEngines))
                {
                    if (entityComponentsEngines.count == 0) return;

                    var iterations = infosToProcess.count;

                    for (var i = 0; i < iterations; i++)
                    {
                        var (entityID, trace) = infosToProcess[i];
                        try
                        {
                            ref var entity = ref fromDictionary.GetValueByRef(entityID);
                            var     egid   = new EGID(entityID, fromGroup);

                            for (var j = 0; j < entityComponentsEngines.count; j++)
                                using (profiler.Sample(entityComponentsEngines[j].name))
                                {
                                    ((IReactOnRemove<TValue>)entityComponentsEngines[j].engine).Remove(ref entity,
                                        egid);
                                }
                        }
                        catch
                        {
                            var str = "Crash while executing Remove Entity callback on "
                               .FastConcat(TypeCache<TValue>.name).FastConcat(" from : ", trace);

                            Console.LogError(str);

                            throw;
                        }
                    }
                }
            }

            AgnosticMethod(ref implMgd, in sampler);
        }

        public void ExecuteEnginesAddEntityCallbacksFast(
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAddEx>>> reactiveEnginesAdd,
            ExclusiveGroupStruct groupID, (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler profiler)
        {
            //get all the engines linked to TValue
            if (!reactiveEnginesAdd.TryGetValue(new RefWrapperType(_type), out var entityComponentsEngines))
                return;

            for (var i = 0; i < entityComponentsEngines.count; i++)
                try
                {
                    using (profiler.Sample(entityComponentsEngines[i].name))
                    {
                        ((IReactOnAddEx<TValue>)entityComponentsEngines[i].engine).Add(rangeOfSubmittedEntitiesIndicies,
                            new EntityCollection<TValue>(GetValues(out var count), count, entityIDs), groupID);
                    }
                }
                catch (Exception e)
                {
                    Console.LogException(e,
                        "Code crashed inside Add callback ".FastConcat(entityComponentsEngines[i].name));

                    throw;
                }
        }

        public void ExecuteEnginesSwapCallbacksFast(
            FasterList<ReactEngineContainer<IReactOnSwapEx>> reactiveEnginesSwap, ExclusiveGroupStruct fromGroup,
            ExclusiveGroupStruct toGroup, (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler sampler)
        {
            for (var i = 0; i < reactiveEnginesSwap.count; i++)
                try
                {
                    using (sampler.Sample(reactiveEnginesSwap[i].name))
                    {
                        ((IReactOnSwapEx<TValue>)reactiveEnginesSwap[i].engine).MovedTo(
                            rangeOfSubmittedEntitiesIndicies,
                            new EntityCollection<TValue>(GetValues(out var count), count, entityIDs), fromGroup,
                            toGroup);
                    }
                }
                catch (Exception e)
                {
                    Console.LogException(e,
                        "Code crashed inside Add callback ".FastConcat(reactiveEnginesSwap[i].name));

                    throw;
                }
        }

        public void ExecuteEnginesRemoveCallbacksFast(
            FasterList<ReactEngineContainer<IReactOnRemoveEx>> reactiveEnginesRemoveEx, ExclusiveGroupStruct fromGroup,
            (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler sampler)
        {
            for (var i = 0; i < reactiveEnginesRemoveEx.count; i++)
                try
                {
                    using (sampler.Sample(reactiveEnginesRemoveEx[i].name))
                    {
                        ((IReactOnRemoveEx<TValue>)reactiveEnginesRemoveEx[i].engine).Remove(
                            rangeOfSubmittedEntitiesIndicies,
                            new EntityCollection<TValue>(GetValues(out var count), count, entityIDs), fromGroup);
                    }
                }
                catch (Exception e)
                {
                    Console.LogException(e,
                        "Code crashed inside Add callback ".FastConcat(reactiveEnginesRemoveEx[i].name));

                    throw;
                }
        }

        public void ExecuteEnginesSwapCallbacks_Group(
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwap>>> reactiveEnginesSwap,
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwapEx>>> reactiveEnginesSwapEx,
            ITypeSafeDictionary toDictionary, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup,
            in PlatformProfiler profiler)
        {
            void AgnosticMethod<Strategy1, Strategy2, Strategy3>(
                ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary,
                ITypeSafeDictionary<TValue> toDic, in PlatformProfiler sampler)
                where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
            {
                //get all the engines linked to TValue
                if (!reactiveEnginesSwap.TryGetValue(new RefWrapperType(_type), out var reactiveEnginesSwapPerType))
                    return;

                var componentsEnginesCount = reactiveEnginesSwapPerType.count;

                for (var i = 0; i < componentsEnginesCount; i++)
                    try
                    {
                        foreach (var value in fromDictionary)
                        {
                            ref var entityComponent = ref toDic.GetValueByRef(value.key);
                            var     newEgid         = new EGID(value.key, toGroup);


                            using (sampler.Sample(reactiveEnginesSwapPerType[i].name))
                            {
                                ((IReactOnSwap<TValue>)reactiveEnginesSwapPerType[i].engine).MovedTo(
                                    ref entityComponent, fromGroup, newEgid);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Console.LogError(
                            "Code crashed inside MoveTo callback ".FastConcat(reactiveEnginesSwapPerType[i].name));

                        throw;
                    }

                if (reactiveEnginesSwapEx.TryGetValue(new RefWrapperType(_type),
                        out var reactiveEnginesRemoveExPerType))
                {
                    var enginesCount = reactiveEnginesRemoveExPerType.count;

                    for (var i = 0; i < enginesCount; i++)
                        try
                        {
                            using (sampler.Sample(reactiveEnginesRemoveExPerType[i].name))
                            {
                                ((IReactOnSwapEx<TValue>)reactiveEnginesRemoveExPerType[i].engine).MovedTo(
                                    (0, (uint)count),
                                    new EntityCollection<TValue>(GetValues(out _), (uint)count, entityIDs), fromGroup,
                                    toGroup);
                            }
                        }
                        catch
                        {
                            Console.LogError(
                                "Code crashed inside Remove callback ".FastConcat(
                                    reactiveEnginesRemoveExPerType[i].name));

                            throw;
                        }
                }
            }

            var toEntitiesDictionary = (ITypeSafeDictionary<TValue>)toDictionary;

            AgnosticMethod(ref implMgd, toEntitiesDictionary, in profiler);
        }

        public void ExecuteEnginesRemoveCallbacks_Group(
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveEnginesRemove,
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemoveEx>>>
                reactiveEnginesRemoveEx, ExclusiveGroupStruct group, in PlatformProfiler profiler)
        {
            void AgnosticMethod<Strategy1, Strategy2, Strategy3>(
                ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary,
                in PlatformProfiler sampler) where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
            {
                if (reactiveEnginesRemove.TryGetValue(new RefWrapperType(_type), out var reactiveEnginesRemovePerType))
                {
                    var enginesCount = reactiveEnginesRemovePerType.count;

                    for (var i = 0; i < enginesCount; i++)
                        try
                        {
                            foreach (var value in fromDictionary)
                            {
                                ref var entity = ref value.value;
                                var     egid   = new EGID(value.key, group);


                                using (sampler.Sample(reactiveEnginesRemovePerType[i].name))
                                {
                                    ((IReactOnRemove<TValue>)reactiveEnginesRemovePerType[i].engine).Remove(ref entity,
                                        egid);
                                }
                            }
                        }
                        catch
                        {
                            Console.LogError(
                                "Code crashed inside Remove callback ".FastConcat(reactiveEnginesRemovePerType[i]
                                   .name));

                            throw;
                        }
                }

                if (reactiveEnginesRemoveEx.TryGetValue(new RefWrapperType(_type),
                        out var reactiveEnginesRemoveExPerType))
                {
                    var enginesCount = reactiveEnginesRemoveExPerType.count;

                    for (var i = 0; i < enginesCount; i++)
                        try
                        {
                            using (sampler.Sample(reactiveEnginesRemoveExPerType[i].name))
                            {
                                ((IReactOnRemoveEx<TValue>)reactiveEnginesRemoveExPerType[i].engine).Remove(
                                    (0, (uint)count),
                                    new EntityCollection<TValue>(GetValues(out _), (uint)count, entityIDs), group);
                            }
                        }
                        catch
                        {
                            Console.LogError(
                                "Code crashed inside Remove callback ".FastConcat(
                                    reactiveEnginesRemoveExPerType[i].name));

                            throw;
                        }
                }
            }

            AgnosticMethod(ref implMgd, in profiler);
        }

        public void ExecuteEnginesDisposeCallbacks_Group(
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnDispose>>> engines,
            ExclusiveGroupStruct group, in PlatformProfiler profiler)
        {
            void ExecuteEnginesDisposeEntityCallback<Strategy1, Strategy2, Strategy3>(
                ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary,
                FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnDispose>>> allEngines,
                in PlatformProfiler sampler, ExclusiveGroupStruct inGroup)
                where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
            {
                if (allEngines.TryGetValue(new RefWrapperType(_type), out var entityComponentsEngines) == false)
                    return;

                for (var i = 0; i < entityComponentsEngines.count; i++)
                    try
                    {
                        using (sampler.Sample(entityComponentsEngines[i].name))
                        {
                            foreach (var value in fromDictionary)
                            {
                                ref var entity        = ref value.value;
                                var     egid          = new EGID(value.key, inGroup);
                                var     reactOnRemove = ((IReactOnDispose<TValue>)entityComponentsEngines[i].engine);
                                reactOnRemove.Remove(ref entity, egid);
                            }
                        }
                    }
                    catch
                    {
                        Console.LogError(
                            "Code crashed inside Remove callback ".FastConcat(entityComponentsEngines[i].name));

                        throw;
                    }
            }

            ExecuteEnginesDisposeEntityCallback(ref implMgd, engines, in profiler, @group);
        }

        SveltoDictionary<uint, TValue, ManagedStrategy<SveltoDictionaryNode<uint>>, ManagedStrategy<TValue>,
            ManagedStrategy<int>> implMgd;
    }
}