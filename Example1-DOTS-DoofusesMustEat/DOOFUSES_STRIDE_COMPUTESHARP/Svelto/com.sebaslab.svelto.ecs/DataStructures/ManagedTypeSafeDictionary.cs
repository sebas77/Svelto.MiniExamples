using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public sealed class ManagedTypeSafeDictionary<TValue> : ITypeSafeDictionary<TValue>
        where TValue : struct, IBaseEntityComponent
    {
        static readonly Type _type = typeof(TValue);
#if SLOW_SVELTO_SUBMISSION
        static readonly bool _hasEgid = typeof(INeedEGID).IsAssignableFrom(_type);
        static readonly bool _hasReference = typeof(INeedEntityReference).IsAssignableFrom(_type);
#endif
        static readonly ThreadLocal<IEntityIDs> cachedEntityIDM =
            new ThreadLocal<IEntityIDs>(() => new ManagedEntityIDs());

        public ManagedTypeSafeDictionary(uint size)
        {
            implMgd =
                new SveltoDictionary<uint, TValue, ManagedStrategy<SveltoDictionaryNode<uint>>, ManagedStrategy<TValue>,
                    ManagedStrategy<int>>(size, Allocator.Managed);
        }

        public IEntityIDs entityIDs
        {
            get
            {
                ref var unboxed = ref Unsafe.Unbox<ManagedEntityIDs>(cachedEntityIDM.Value);

                unboxed.Update(implMgd.unsafeKeys.ToRealBuffer());

                return cachedEntityIDM.Value;
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
            return TypeSafeDictionaryFactory<TValue>.Create(1);
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

        /// *********************************
        /// the following methods are executed during the submission of entities
        /// *********************************
        public void AddEntitiesToDictionary
        (ITypeSafeDictionary toDictionary, ExclusiveGroupStruct groupId
#if SLOW_SVELTO_SUBMISSION
                           , in EnginesRoot.EntityReferenceMap entityLocator
#endif
        )

        {
            var destinationDictionary = toDictionary as ITypeSafeDictionary<TValue>;

            TypeSafeDictionaryMethods.AddEntitiesToDictionary(implMgd, destinationDictionary
#if SLOW_SVELTO_SUBMISSION
  , entityLocator
#endif
                                                                    , groupId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveEntitiesFromDictionary(FasterList<(uint, string)> infosToProcess)
        {
            TypeSafeDictionaryMethods.RemoveEntitiesFromDictionary(infosToProcess, ref implMgd);
        }

        public void SwapEntitiesBetweenDictionaries
        (FasterList<(uint, uint, string)> infosToProcess, ExclusiveGroupStruct fromGroup
       , ExclusiveGroupStruct toGroup, ITypeSafeDictionary toComponentsDictionary)
        {
            var toGroupCasted = toComponentsDictionary as ITypeSafeDictionary<TValue>;

            TypeSafeDictionaryMethods.SwapEntitiesBetweenDictionaries(infosToProcess, ref implMgd
                                                                    , toGroupCasted, fromGroup, toGroup);
        }

        /// <summary>
        ///     Execute all the engine IReactOnAdd callbacks linked to components added this submit
        /// </summary>
        public void ExecuteEnginesAddCallbacks
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAdd>>> entityComponentEnginesDB
       , ITypeSafeDictionary toDic, ExclusiveGroupStruct toGroup, in PlatformProfiler profiler)
        {
            var toDictionary = (ITypeSafeDictionary<TValue>)toDic;

            TypeSafeDictionaryMethods.ExecuteEnginesAddCallbacks(ref implMgd, toDictionary, toGroup, entityComponentEnginesDB, in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnSwap callbacks linked to components swapped this submit
        /// </summary>
        public void ExecuteEnginesSwapCallbacks
        (FasterList<(uint, uint, string)> infosToProcess
       , FasterList<ReactEngineContainer<IReactOnSwap>> reactiveEnginesSwap, ExclusiveGroupStruct fromGroup
       , ExclusiveGroupStruct toGroup, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesSwapCallbacks(infosToProcess, ref implMgd
                                                                , reactiveEnginesSwap, toGroup, fromGroup, in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnREmove callbacks linked to components removed this submit
        /// </summary>
        public void ExecuteEnginesRemoveCallbacks
        (FasterList<(uint, string)> infosToProcess
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveEnginesRemove
       , ExclusiveGroupStruct fromGroup, in PlatformProfiler sampler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesRemoveCallbacks(infosToProcess
                                                                  , ref implMgd, reactiveEnginesRemove, fromGroup, in sampler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnAddEx callbacks linked to components added this submit
        /// </summary>
        public void ExecuteEnginesAddEntityCallbacksFast
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAddEx>>> reactiveEnginesAdd
       , ExclusiveGroupStruct groupID, (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler profiler)
        {
            //get all the engines linked to TValue
            if (!reactiveEnginesAdd.TryGetValue(new RefWrapperType(_type), out var entityComponentsEngines))
                return;

            for (var i = 0; i < entityComponentsEngines.count; i++)
                try
                {
                    using (profiler.Sample(entityComponentsEngines[i].name))
                    {
                        ((IReactOnAddEx<TValue>)entityComponentsEngines[i].engine).Add(
                            rangeOfSubmittedEntitiesIndicies
                          , new EntityCollection<TValue>(GetValues(out var count), count, entityIDs), groupID);
                    }
                }
                catch (Exception e)
                {
                    Console.LogException(
                        e, "Code crashed inside Add callback ".FastConcat(entityComponentsEngines[i].name));

                    throw;
                }
        }

        /// <summary>
        ///     Execute all the engine IReactOnSwapEx callbacks linked to components swapped this submit
        /// </summary>
        public void ExecuteEnginesSwapCallbacksFast
        (FasterList<ReactEngineContainer<IReactOnSwapEx>> reactiveEnginesSwap, ExclusiveGroupStruct fromGroup
       , ExclusiveGroupStruct toGroup, (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler sampler)
        {
            for (var i = 0; i < reactiveEnginesSwap.count; i++)
                try
                {
                    using (sampler.Sample(reactiveEnginesSwap[i].name))
                    {
                        ((IReactOnSwapEx<TValue>)reactiveEnginesSwap[i].engine).MovedTo(
                            rangeOfSubmittedEntitiesIndicies
                          , new EntityCollection<TValue>(GetValues(out var count), count, entityIDs), fromGroup
                          , toGroup);
                    }
                }
                catch (Exception e)
                {
                    Console.LogException(
                        e, "Code crashed inside Add callback ".FastConcat(reactiveEnginesSwap[i].name));

                    throw;
                }
        }

        /// <summary>
        ///     Execute all the engine IReactOnRemoveEx callbacks linked to components removed this submit
        /// </summary>
        public void ExecuteEnginesRemoveCallbacksFast
        (FasterList<ReactEngineContainer<IReactOnRemoveEx>> reactiveEnginesRemoveEx, ExclusiveGroupStruct fromGroup
       , (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler sampler)
        {
            for (var i = 0; i < reactiveEnginesRemoveEx.count; i++)
                try
                {
                    using (sampler.Sample(reactiveEnginesRemoveEx[i].name))
                    {
                        ((IReactOnRemoveEx<TValue>)reactiveEnginesRemoveEx[i].engine).Remove(
                            rangeOfSubmittedEntitiesIndicies
                          , new EntityCollection<TValue>(GetValues(out var count), count, entityIDs), fromGroup);
                    }
                }
                catch (Exception e)
                {
                    Console.LogException(
                        e, "Code crashed inside Add callback ".FastConcat(reactiveEnginesRemoveEx[i].name));

                    throw;
                }
        }

        /// <summary>
        ///     Execute all the engine IReactOnSwap and IReactOnSwapEx callbacks linked to components swapped between
        ///     whole groups swapped during this submit
        /// </summary>
        public void ExecuteEnginesSwapCallbacks_Group
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwap>>> reactiveEnginesSwap
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwapEx>>> reactiveEnginesSwapEx
       , ITypeSafeDictionary toDictionary, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup
       , in PlatformProfiler profiler)
        {
            var toEntitiesDictionary = (ITypeSafeDictionary<TValue>)toDictionary;

            TypeSafeDictionaryMethods.ExecuteEnginesSwapCallbacks_Group(ref implMgd, toEntitiesDictionary, toGroup, fromGroup, this, reactiveEnginesSwap, reactiveEnginesSwapEx, count, entityIDs, in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnRemove and IReactOnRemoveEx callbacks linked to components remove from
        ///     whole groups removed during this submit
        /// </summary>
        public void ExecuteEnginesRemoveCallbacks_Group
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveEnginesRemove
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemoveEx>>> reactiveEnginesRemoveEx
       , ExclusiveGroupStruct group, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesRemoveCallbacks_Group(ref implMgd, this, reactiveEnginesRemove, reactiveEnginesRemoveEx, count, entityIDs, group, in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnDispose for eahc component registered in the DB when it's disposed of
        /// </summary>
        public void ExecuteEnginesDisposeCallbacks_Group
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnDispose>>> engines
       , ExclusiveGroupStruct group, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesDisposeCallbacks_Group(ref implMgd, engines, group, in profiler);
        }

        SveltoDictionary<uint, TValue, ManagedStrategy<SveltoDictionaryNode<uint>>, ManagedStrategy<TValue>,
            ManagedStrategy<int>> implMgd;
    }
}