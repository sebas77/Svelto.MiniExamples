#if DEBUG && !PROFILE_SVELTO
//#define PARANOID_CHECK
#endif

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Internal
{
#if SLOW_SVELTO_SUBMISSION
    static class SlowSubmissionInfo<T>
    {
        internal static readonly bool hasEgid      = typeof(INeedEGID).IsAssignableFrom(TypeCache<T>.type);
        internal static readonly bool hasReference = typeof(INeedEntityReference).IsAssignableFrom(TypeCache<T>.type);
    }
#endif

    public sealed class UnmanagedTypeSafeDictionary<TValue> : ITypeSafeDictionary<TValue>
        where TValue : struct, IBaseEntityComponent
    {
        static readonly ThreadLocal<IEntityIDs> cachedEntityIDN =
            new ThreadLocal<IEntityIDs>(() => new NativeEntityIDs());

        public UnmanagedTypeSafeDictionary(uint size)
        {
            implUnmgd =
                new SharedDisposableNative<SveltoDictionary<uint, TValue, NativeStrategy<SveltoDictionaryNode<uint>>,
                    NativeStrategy<TValue>, NativeStrategy<int>>>(
                    new SveltoDictionary<uint, TValue, NativeStrategy<SveltoDictionaryNode<uint>>,
                        NativeStrategy<TValue>, NativeStrategy<int>>(size, Allocator.Persistent));
        }

        public IEntityIDs entityIDs
        {
            get
            {
                ref var unboxed = ref Unsafe.Unbox<NativeEntityIDs>(cachedEntityIDN.Value);

                unboxed.Update(implUnmgd.value.unsafeKeys.ToRealBuffer());

                return cachedEntityIDN.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(uint egidEntityId)
        {
            return implUnmgd.value.ContainsKey(egidEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(uint valueEntityId)
        {
            return implUnmgd.value.GetIndex(valueEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrAdd(uint idEntityId)
        {
            return ref implUnmgd.value.GetOrAdd(idEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBuffer<TValue> GetValues(out uint count)
        {
            return implUnmgd.value.UnsafeGetValues(out count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetDirectValueByRef(uint key)
        {
            return ref implUnmgd.value.GetDirectValueByRef(key);
        }

        public ref TValue GetValueByRef(uint key)
        {
            return ref implUnmgd.value.GetValueByRef(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(uint key)
        {
            return implUnmgd.value.ContainsKey(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindIndex(uint entityId, out uint index)
        {
            return implUnmgd.value.TryFindIndex(entityId, out index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(uint entityId, out TValue item)
        {
            return implUnmgd.value.TryGetValue(entityId, out item);
        }

        public int count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => implUnmgd.value.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITypeSafeDictionary Create()
        {
            return TypeSafeDictionaryFactory<TValue>.Create(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            implUnmgd.value.FastClear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(uint size)
        {
            implUnmgd.value.EnsureCapacity(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncreaseCapacityBy(uint size)
        {
            implUnmgd.value.IncreaseCapacityBy(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            implUnmgd.value.Trim();
        }

        public void KeysEvaluator(Action<uint> action)
        {
            foreach (var key in implUnmgd.value.keys)
                action(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint egidEntityId, in TValue entityComponent)
        {
            implUnmgd.value.Add(egidEntityId, entityComponent);
        }

        public void Dispose()
        {
            implUnmgd.Dispose(); //SharedDisposableNative already calls the dispose of the underlying value

            GC.SuppressFinalize(this);
        }

        /// *********************************
        /// the following methods are executed during the submission of entities
        /// *********************************
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddEntitiesToDictionary
        (ITypeSafeDictionary toDictionary, ExclusiveGroupStruct groupId
#if SLOW_SVELTO_SUBMISSION
       , in EnginesRoot.EntityReferenceMap entityLocator
#endif
        )

        {
            TypeSafeDictionaryMethods.AddEntitiesToDictionary(implUnmgd.value
                                                            , toDictionary as ITypeSafeDictionary<TValue>
#if SLOW_SVELTO_SUBMISSION
                                                            , entityLocator
#endif
                                                            , groupId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveEntitiesFromDictionary
            (FasterList<(uint, string)> infosToProcess, FasterList<uint> entityIDsAffectedByRemoval)
        {
            TypeSafeDictionaryMethods.RemoveEntitiesFromDictionary(infosToProcess, ref implUnmgd.value
                                                                 , entityIDsAffectedByRemoval);
        }

        public void SwapEntitiesBetweenDictionaries
        (FasterList<(uint, uint, string)> infosToProcess, ExclusiveGroupStruct fromGroup
       , ExclusiveGroupStruct toGroup, ITypeSafeDictionary toComponentsDictionary
       , FasterList<uint> entityIDsAffectedByRemoval)
        {
            TypeSafeDictionaryMethods.SwapEntitiesBetweenDictionaries(infosToProcess, ref implUnmgd.value
                ,toComponentsDictionary as ITypeSafeDictionary<TValue>, fromGroup, toGroup, entityIDsAffectedByRemoval);
        }

        /// <summary>
        ///     Execute all the engine IReactOnAdd callbacks linked to components added this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesAddCallbacks
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAdd>>> entityComponentEnginesDB
       , ITypeSafeDictionary toDic, ExclusiveGroupStruct toGroup, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesAddCallbacks(ref implUnmgd.value, (ITypeSafeDictionary<TValue>)toDic
                                                               , toGroup, entityComponentEnginesDB, in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnSwap callbacks linked to components swapped this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesSwapCallbacks
        (FasterList<(uint, uint, string)> infosToProcess
       , FasterList<ReactEngineContainer<IReactOnSwap>> reactiveEnginesSwap, ExclusiveGroupStruct fromGroup
       , ExclusiveGroupStruct toGroup, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesSwapCallbacks(infosToProcess, ref implUnmgd.value
                                                                , reactiveEnginesSwap, toGroup, fromGroup, in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnREmove callbacks linked to components removed this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesRemoveCallbacks
        (FasterList<(uint, string)> infosToProcess
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveEnginesRemove
       , ExclusiveGroupStruct fromGroup, in PlatformProfiler sampler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesRemoveCallbacks(infosToProcess, ref implUnmgd.value
                                                                  , reactiveEnginesRemove, fromGroup, in sampler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnAddEx callbacks linked to components added this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesAddEntityCallbacksFast
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAddEx>>> reactiveEnginesAdd
       , ExclusiveGroupStruct groupID, (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesAddEntityCallbacksFast(
                reactiveEnginesAdd, groupID, rangeOfSubmittedEntitiesIndicies, entityIDs, this, profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnSwapEx callbacks linked to components swapped this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesSwapCallbacksFast
        (FasterList<ReactEngineContainer<IReactOnSwapEx>> reactiveEnginesSwap, ExclusiveGroupStruct fromGroup
       , ExclusiveGroupStruct toGroup, (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler sampler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesSwapCallbacksFast(reactiveEnginesSwap, fromGroup, toGroup, entityIDs
                                                                    , this, rangeOfSubmittedEntitiesIndicies, sampler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnRemoveEx callbacks linked to components removed this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesRemoveCallbacksFast
        (FasterList<ReactEngineContainer<IReactOnRemoveEx>> reactiveEnginesRemoveEx, ExclusiveGroupStruct fromGroup
       , (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler sampler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesRemoveCallbacksFast(reactiveEnginesRemoveEx, fromGroup
                                                                      , rangeOfSubmittedEntitiesIndicies, entityIDs
                                                                      , this, sampler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnSwap and IReactOnSwapEx callbacks linked to components swapped between
        ///     whole groups swapped during this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesSwapCallbacks_Group
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwap>>> reactiveEnginesSwap
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwapEx>>> reactiveEnginesSwapEx
       , ITypeSafeDictionary toDictionary, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup
       , in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesSwapCallbacks_Group(
                ref implUnmgd.value, (ITypeSafeDictionary<TValue>)toDictionary, toGroup, fromGroup, this
              , reactiveEnginesSwap, reactiveEnginesSwapEx, count, entityIDs, in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnRemove and IReactOnRemoveEx callbacks linked to components remove from
        ///     whole groups removed during this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesRemoveCallbacks_Group
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveEnginesRemove
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemoveEx>>> reactiveEnginesRemoveEx
       , ExclusiveGroupStruct group, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesRemoveCallbacks_Group(
                ref implUnmgd.value, this, reactiveEnginesRemove, reactiveEnginesRemoveEx, count, entityIDs, group
              , in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnDispose for eahc component registered in the DB when it's disposed of
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesDisposeCallbacks_Group
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnDispose>>> engines
       , ExclusiveGroupStruct group, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesDisposeCallbacks_Group(
                ref implUnmgd.value, engines, group, in profiler);
        }

        internal SharedDisposableNative<SveltoDictionary<uint, TValue, NativeStrategy<SveltoDictionaryNode<uint>>,
            NativeStrategy<TValue>, NativeStrategy<int>>> implUnmgd;
    }
}