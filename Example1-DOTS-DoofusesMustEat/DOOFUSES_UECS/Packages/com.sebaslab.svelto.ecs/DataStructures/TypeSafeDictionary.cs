#if DEBUG && !PROFILE_SVELTO
//#define PARANOID_CHECK
#endif

using System;
using System.Runtime.CompilerServices;
using DBC.ECS;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Internal
{
    public readonly struct NativeEntityIDs
    {
        public NativeEntityIDs(NB<SveltoDictionaryNode<uint>> native)
        {
            _native = native;
        }

        public uint this[uint index] => _native[index].key;
        public uint this[int index] => _native[index].key;

        readonly NB<SveltoDictionaryNode<uint>> _native;
    }

    public struct ManagedEntityIDs
    {
        public ManagedEntityIDs(MB<SveltoDictionaryNode<uint>> managed)
        {
        }
    }

    public readonly struct EntityIDs
    {
        readonly NB<SveltoDictionaryNode<uint>> _native;
        readonly MB<SveltoDictionaryNode<uint>> _managed;

        public EntityIDs(NativeStrategy<SveltoDictionaryNode<uint>> unmanagedKeys) : this()
        {
            _native = unmanagedKeys.ToRealBuffer();
        }

        public EntityIDs(ManagedStrategy<SveltoDictionaryNode<uint>> managedKeys) : this()
        {
            _managed = managedKeys.ToRealBuffer();
        }

        public NativeEntityIDs  nativeIDs  => new NativeEntityIDs(_native);
        public ManagedEntityIDs managedIDs => new ManagedEntityIDs(_managed);
    }

    public sealed class TypeSafeDictionary<TValue> : ITypeSafeDictionary<TValue> where TValue : struct, IEntityComponent
    {
        static readonly Type _type         = typeof(TValue);
        static readonly bool _hasEgid      = typeof(INeedEGID).IsAssignableFrom(_type);
        static readonly bool _hasReference = typeof(INeedEntityReference).IsAssignableFrom(_type);

        internal static readonly bool isUnmanaged =
            _type.IsUnmanagedEx() && typeof(IEntityViewComponent).IsAssignableFrom(_type) == false;

        public EntityIDs entityIDs
        {
            get
            {
                if (isUnmanaged)
                    return new EntityIDs(implUnmgd.unsafeKeys);

                return new EntityIDs(implMgd.unsafeKeys);
            }
        }

        public TypeSafeDictionary(uint size)
        {
            if (isUnmanaged)
                implUnmgd =
                    new SveltoDictionary<uint, TValue, NativeStrategy<SveltoDictionaryNode<uint>>,
                        NativeStrategy<TValue>, NativeStrategy<int>>(size, Allocator.Persistent);
            else
                implMgd =
                    new SveltoDictionary<uint, TValue, ManagedStrategy<SveltoDictionaryNode<uint>>,
                        ManagedStrategy<TValue>, ManagedStrategy<int>>(size, Allocator.Managed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(uint egidEntityId)
        {
            return isUnmanaged ? implUnmgd.ContainsKey(egidEntityId) : implMgd.ContainsKey(egidEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(uint valueEntityId)
        {
            return isUnmanaged ? implUnmgd.GetIndex(valueEntityId) : implMgd.GetIndex(valueEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrCreate(uint idEntityId)
        {
            return ref isUnmanaged ? ref implUnmgd.GetOrCreate(idEntityId) : ref implMgd.GetOrCreate(idEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBuffer<TValue> GetValues(out uint count)
        {
            return isUnmanaged ? implUnmgd.UnsafeGetValues(out count) : implMgd.UnsafeGetValues(out count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetDirectValueByRef(uint key)
        {
            return ref isUnmanaged ? ref implUnmgd.GetDirectValueByRef(key) : ref implMgd.GetDirectValueByRef(key);
        }

        public ref TValue GetValueByRef(uint key)
        {
            return ref isUnmanaged ? ref implUnmgd.GetValueByRef(key) : ref implMgd.GetValueByRef(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(uint key)
        {
            return isUnmanaged ? implUnmgd.ContainsKey(key) : implMgd.ContainsKey(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindIndex(uint entityId, out uint index)
        {
            return isUnmanaged
                ? implUnmgd.TryFindIndex(entityId, out index)
                : implMgd.TryFindIndex(entityId, out index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(uint entityId, out TValue item)
        {
            return isUnmanaged ? implUnmgd.TryGetValue(entityId, out item) : implMgd.TryGetValue(entityId, out item);
        }

        public int count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => isUnmanaged ? implUnmgd.count : implMgd.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITypeSafeDictionary Create()
        {
            return TypeSafeDictionaryFactory<TValue>.Create(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (isUnmanaged)
                implUnmgd.FastClear();
            else
                implMgd.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(uint size)
        {
            if (isUnmanaged)
                implUnmgd.EnsureCapacity(size);
            else
                implMgd.EnsureCapacity(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncreaseCapacityBy(uint size)
        {
            if (isUnmanaged)
                implUnmgd.IncreaseCapacityBy(size);
            else
                implMgd.IncreaseCapacityBy(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            if (isUnmanaged)
                implUnmgd.Trim();
            else
                implMgd.Trim();
        }

        public void KeysEvaluator(Action<uint> action)
        {
            if (isUnmanaged)
                foreach (var key in implUnmgd.keys)
                    action(key);
            else
                foreach (var key in implMgd.keys)
                    action(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint egidEntityId, in TValue entityComponent)
        {
            if (isUnmanaged)
                implUnmgd.Add(egidEntityId, entityComponent);
            else
                implMgd.Add(egidEntityId, entityComponent);
        }

        public void Dispose()
        {
            if (isUnmanaged)
                implUnmgd.Dispose();
            else
                implMgd.Dispose();

            GC.SuppressFinalize(this);
        }

        public void AddEntitiesToDictionary(ITypeSafeDictionary toDictionary, ExclusiveGroupStruct groupId,
            in EnginesRoot.LocatorMap entityLocator)
        {
            static void SharedAddEntitiesFromDictionary<Strategy1, Strategy2, Strategy3>(
                in SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary,
                ITypeSafeDictionary<TValue> toDictionary, in EnginesRoot.LocatorMap entityLocator,
                ExclusiveGroupStruct toGroupID) where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
            {
                foreach (var tuple in fromDictionary)
                {
                    var egid = new EGID(tuple.key, toGroupID);

                    if (_hasEgid)
                        SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref tuple.value, egid);

                    //todo: temporary code that will eventually be removed
                    if (_hasReference)
                        SetEGIDWithoutBoxing<TValue>.SetRefWithoutBoxing(ref tuple.value,
                            entityLocator.GetEntityReference(egid));
                    try
                    {
                        toDictionary.Add(tuple.key, tuple.value);
                    }
                    catch (Exception e)
                    {
                        Console.LogException(e,
                            "trying to add an EntityComponent with the same ID more than once Entity: "
                               .FastConcat(typeof(TValue).ToString()).FastConcat(", group ")
                               .FastConcat(toGroupID.ToName()).FastConcat(", id ").FastConcat(tuple.key));

                        throw;
                    }
#if PARANOID_CHECK
                        DBC.ECS.Check.Ensure(_hasEgid == false || ((INeedEGID)fromDictionary[egid.entityID]).ID == egid, "impossible situation happened during swap");
#endif
                }
            }

            var destinationDictionary = toDictionary as ITypeSafeDictionary<TValue>;

            if (isUnmanaged)
                SharedAddEntitiesFromDictionary(implUnmgd, destinationDictionary, entityLocator, groupId);
            else
                SharedAddEntitiesFromDictionary(implMgd, destinationDictionary, entityLocator, groupId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveEntitiesFromDictionary(FasterList<(uint, string)> infosToProcess)
        {
            var iterations = infosToProcess.count;

            if (isUnmanaged)
            {
                for (var i = 0; i < iterations; i++)
                {
                    var (id, trace) = infosToProcess[i];

                    try
                    {
                        implUnmgd.Remove(id);
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
            else
            {
                for (var i = 0; i < iterations; i++)
                {
                    var (id, trace) = infosToProcess[i];

                    try
                    {
                        implMgd.Remove(id);
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
                        DBC.ECS.Check.Assert(isFound == true, "Swapping an entity that doesn't exist");

                        if (_hasEgid)
                            SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityEgid);

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

            if (isUnmanaged)
                SharedSwapEntityInDictionary(ref implUnmgd, toGroupCasted);
            else
                SharedSwapEntityInDictionary(ref implMgd, toGroupCasted);
        }

        public void ExecuteEnginesAddCallbacks(
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAdd>>> entityComponentEnginesDB,
            ITypeSafeDictionary toDic, ExclusiveGroupStruct toGroup, in PlatformProfiler profiler)
        {
            ITypeSafeDictionary<TValue> toDictionary = (ITypeSafeDictionary<TValue>)toDic;

            try
            {
                if (isUnmanaged)
                {
                    var dictionaryKeyEnumerator = implUnmgd.unsafeKeys;
                    var count                   = implUnmgd.count;

                    for (int i = 0; i < count; ++i)
                    {
                        var     key    = dictionaryKeyEnumerator[i].key;
                        ref var entity = ref toDictionary.GetValueByRef(key);
                        ExecuteEnginesAddEntityCallbacks(entityComponentEnginesDB, ref entity, new EGID(key, toGroup),
                            in profiler);
                    }
                }
                else
                {
                    var dictionaryKeyEnumerator = implMgd.unsafeKeys;
                    var count                   = implMgd.count;

                    for (int i = 0; i < count; ++i)
                    {
                        var     key    = dictionaryKeyEnumerator[i].key;
                        ref var entity = ref toDictionary.GetValueByRef(key);
                        ExecuteEnginesAddEntityCallbacks(entityComponentEnginesDB, ref entity, new EGID(key, toGroup),
                            in profiler);
                    }
                }
            }
            catch (Exception e)
            {
                Console.LogException(e,
                    "Code crashed inside Add callback with Type ".FastConcat(TypeCache<TValue>.name));

                throw;
            }
        }

        public void ExecuteEnginesSwapCallbacks(FasterList<(uint, uint, string)> infosToProcess,
            FasterList<ReactEngineContainer<IReactOnSwap>> reactiveEnginesSwap, ExclusiveGroupStruct fromGroup,
            ExclusiveGroupStruct toGroup, in PlatformProfiler profiler)
        {
            var iterations = infosToProcess.count;

            if (isUnmanaged)
                for (var i = 0; i < iterations; i++)
                {
                    var (fromEntityID, toEntityID, trace) = infosToProcess[i];

                    try
                    {
                        ExecuteEnginesSwapEntityCallbacks(reactiveEnginesSwap, ref implUnmgd.GetValueByRef(fromEntityID),
                            fromGroup, new EGID(toEntityID, toGroup), in profiler);
                    }
                    catch
                    {
                        var str = "Crash while executing Swap Entity callback on ".FastConcat(TypeCache<TValue>.name)
                           .FastConcat(" from : ", trace);

                        Console.LogError(str);

                        throw;
                    }
                }
            else
                for (var i = 0; i < iterations; i++)
                {
                    var (fromEntityID, toEntityID, trace) = infosToProcess[i];

                    try
                    {
                        ExecuteEnginesSwapEntityCallbacks(reactiveEnginesSwap, ref implMgd.GetValueByRef(fromEntityID),
                            fromGroup, new EGID(toEntityID, toGroup), in profiler);
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

        public void ExecuteEnginesRemoveCallbacks(FasterList<(uint, string)> infosToProcess,
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveEnginesRemove,
            ExclusiveGroupStruct fromGroup, in PlatformProfiler sampler)
        {
            var iterations = infosToProcess.count;

            if (isUnmanaged)
            {
                for (var i = 0; i < iterations; i++)
                {
                    var (entityID, trace) = infosToProcess[i];

                    try
                    {
                        ExecuteEnginesRemoveEntityCallback(reactiveEnginesRemove, ref implUnmgd.GetValueByRef(entityID),
                            new EGID(entityID, fromGroup), sampler);
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
            else
            {
                for (var i = 0; i < iterations; i++)
                {
                    var (entityID, trace) = infosToProcess[i];

                    try
                    {
                        ExecuteEnginesRemoveEntityCallback(reactiveEnginesRemove, ref implMgd.GetValueByRef(entityID),
                            new EGID(entityID, fromGroup), sampler);
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
        }

        public void ExecuteEnginesAddEntityCallbacksFast(
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAddEx>>> reactiveEnginesAdd,
            ExclusiveGroupStruct groupID, (uint, uint) enumeratorCurrent, in PlatformProfiler profiler)
        {
            //get all the engines linked to TValue
            if (!reactiveEnginesAdd.TryGetValue(new RefWrapperType(_type), out var entityComponentsEngines))
                return;

            for (var i = 0; i < entityComponentsEngines.count; i++)
                try
                {
                    using (profiler.Sample(entityComponentsEngines[i].name))
                    {
                        ((IReactOnAddEx<TValue>)entityComponentsEngines[i].engine).Add(enumeratorCurrent,
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
            ExclusiveGroupStruct toGroup, (uint, uint) enumeratorCurrent, in PlatformProfiler sampler)
        {
            for (var i = 0; i < reactiveEnginesSwap.count; i++)
                try
                {
                    using (sampler.Sample(reactiveEnginesSwap[i].name))
                    {
                        ((IReactOnSwapEx<TValue>)reactiveEnginesSwap[i].engine).MovedTo(enumeratorCurrent,
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

        public void ExecuteEnginesSwapCallbacks_Group(
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwap>>> reactiveEnginesSwap,
            ITypeSafeDictionary toDictionary, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup,
            in PlatformProfiler profiler)
        {
            var toEntitiesDictionary = (ITypeSafeDictionary<TValue>)toDictionary;

            //get all the engines linked to TValue
            if (!reactiveEnginesSwap.TryGetValue(new RefWrapperType(_type), out var entityComponentsEngines))
                return;

            try
            {
                if (isUnmanaged)
                    foreach (KeyValuePairFast<uint, TValue, NativeStrategy<TValue>> value in implUnmgd)
                    {
                        ExecuteEnginesSwapEntityCallbacks(entityComponentsEngines,
                            ref toEntitiesDictionary.GetValueByRef(value.key), fromGroup, new EGID(value.key, toGroup),
                            in profiler);
                    }
                else
                    foreach (KeyValuePairFast<uint, TValue, ManagedStrategy<TValue>> value in implMgd)
                    {
                        ExecuteEnginesSwapEntityCallbacks(entityComponentsEngines,
                            ref toEntitiesDictionary.GetValueByRef(value.key), fromGroup, new EGID(value.key, toGroup),
                            in profiler);
                    }
            }
            catch
            {
                Console.LogError("Code crashed Swap callback with Type ".FastConcat(TypeCache<TValue>.name));

                throw;
            }
        }

        public void ExecuteEnginesRemoveCallbacks_Group(
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>> engines,
            ExclusiveGroupStruct group, in PlatformProfiler profiler)
        {
            try
            {
                if (isUnmanaged)
                    foreach (var value in implUnmgd)
                        ExecuteEnginesRemoveEntityCallback(engines, ref value.value, new EGID(value.key, group),
                            in profiler);
                else
                    foreach (var value in implMgd)
                        ExecuteEnginesRemoveEntityCallback(engines, ref value.value, new EGID(value.key, group),
                            in profiler);
            }
            catch
            {
                Console.LogError(
                    "Code crashed inside Swap Group callback with Type ".FastConcat(TypeCache<TValue>.name));

                throw;
            }
        }

        public void ExecuteEnginesDisposeCallbacks_Group(
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnDispose>>> engines,
            ExclusiveGroupStruct group, in PlatformProfiler profiler)
        {
            static void ExecuteEnginesDisposeEntityCallback(
                FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnDispose>>> engines,
                ref TValue entity, EGID egid, in PlatformProfiler profiler)
            {
                if (!engines.TryGetValue(new RefWrapperType(_type), out var entityComponentsEngines))
                    return;

                for (var i = 0; i < entityComponentsEngines.count; i++)
                    try
                    {
                        using (profiler.Sample(entityComponentsEngines[i].name))
                        {
                            ((IReactOnRemove<TValue>)entityComponentsEngines[i].engine).Remove(ref entity, egid);
                        }
                    }
                    catch
                    {
                        Console.LogError(
                            "Code crashed inside Remove callback ".FastConcat(entityComponentsEngines[i].name));

                        throw;
                    }
            }

            try
            {
                if (isUnmanaged)
                    foreach (var value in implUnmgd)
                        ExecuteEnginesDisposeEntityCallback(engines, ref value.value, new EGID(value.key, group),
                            in profiler);
                else
                    foreach (var value in implMgd)
                        ExecuteEnginesDisposeEntityCallback(engines, ref value.value, new EGID(value.key, group),
                            in profiler);
            }
            catch
            {
                Console.LogError(
                    "Code crashed inside Swap Group callback with Type ".FastConcat(TypeCache<TValue>.name));

                throw;
            }
        }

        static void ExecuteEnginesAddEntityCallbacks(
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAdd>>> engines,
            ref TValue entityComponent, EGID egid, in PlatformProfiler profiler)
        {
            //get all the engines linked to TValue
            if (!engines.TryGetValue(new RefWrapperType(_type), out var entityComponentsEngines))
                return;

            for (var i = 0; i < entityComponentsEngines.count; i++)
                try
                {
                    using (profiler.Sample(entityComponentsEngines[i].name))
                    {
                        ((IReactOnAdd<TValue>)entityComponentsEngines[i].engine).Add(ref entityComponent, egid);
                    }
                }
                catch (Exception e)
                {
                    Console.LogException(e,
                        "Code crashed inside Add callback ".FastConcat(entityComponentsEngines[i].name));

                    throw;
                }
        }

        static void ExecuteEnginesRemoveEntityCallback(
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>> engines,
            ref TValue entity, EGID egid, in PlatformProfiler profiler)
        {
            if (!engines.TryGetValue(new RefWrapperType(_type), out var entityComponentsEngines))
                return;

            for (var i = 0; i < entityComponentsEngines.count; i++)
                try
                {
                    using (profiler.Sample(entityComponentsEngines[i].name))
                    {
                        ((IReactOnRemove<TValue>)entityComponentsEngines[i].engine).Remove(ref entity, egid);
                    }
                }
                catch
                {
                    Console.LogError(
                        "Code crashed inside Remove callback ".FastConcat(entityComponentsEngines[i].name));

                    throw;
                }
        }

        static void ExecuteEnginesSwapEntityCallbacks(
            FasterList<ReactEngineContainer<IReactOnSwap>> entityComponentsEngines, ref TValue entityComponent,
            ExclusiveGroupStruct previousGroup, EGID newEGID, in PlatformProfiler profiler)
        {
            for (var i = 0; i < entityComponentsEngines.count; i++)
                try
                {
                    using (profiler.Sample(entityComponentsEngines[i].name))
                    {
                        ((IReactOnSwap<TValue>)entityComponentsEngines[i].engine).MovedTo(ref entityComponent,
                            previousGroup, newEGID);
                    }
                }
                catch (Exception)
                {
                    Console.LogError(
                        "Code crashed inside MoveTo callback ".FastConcat(entityComponentsEngines[i].name));

                    throw;
                }
        }


        SveltoDictionary<uint, TValue, ManagedStrategy<SveltoDictionaryNode<uint>>, ManagedStrategy<TValue>,
            ManagedStrategy<int>> implMgd;

        internal SveltoDictionary<uint, TValue, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<TValue>,
            NativeStrategy<int>> implUnmgd;
    }
}