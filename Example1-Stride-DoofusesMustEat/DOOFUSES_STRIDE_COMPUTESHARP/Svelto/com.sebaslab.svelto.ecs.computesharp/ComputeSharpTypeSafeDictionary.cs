#if DEBUG && !PROFILE_SVELTO
//#define PARANOID_CHECK
#endif

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;

namespace Svelto.ECS.Internal
{
    public sealed class ComputeSharpTypeSafeDictionary<TValue> : ITypeSafeDictionary<TValue>
        where TValue : unmanaged, IEntityComputeSharpComponent
    {
        internal static readonly Type _type = typeof(TValue);
#if SLOW_SVELTO_SUBMISSION
        static readonly bool _hasEgid = typeof(INeedEGID).IsAssignableFrom(_type);
        static readonly bool _hasReference = typeof(INeedEntityReference).IsAssignableFrom(_type);
#endif
        static readonly ThreadLocal<IEntityIDs> cachedEntityIDN =
            new ThreadLocal<IEntityIDs>(() => new NativeEntityIDs());

        ComputeSharpTypeSafeDictionary(uint size)
        {
            computeBufferDictionary =
                new SveltoDictionary<uint, TValue, NativeStrategy<SveltoDictionaryNode<uint>>,
                    ComputeSharpStrategy<TValue>, NativeStrategy<int>>(size, Allocator.Persistent);
        }

        public IEntityIDs entityIDs
        {
            get
            {
                ref var unboxed = ref Unsafe.Unbox<NativeEntityIDs>(cachedEntityIDN.Value);

                unboxed.Update(computeBufferDictionary.unsafeKeys.ToRealBuffer());

                return cachedEntityIDN.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(uint egidEntityId)
        {
            return computeBufferDictionary.ContainsKey(egidEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(uint valueEntityId)
        {
            return computeBufferDictionary.GetIndex(valueEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrAdd(uint idEntityId)
        {
            return ref computeBufferDictionary.GetOrAdd(idEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBuffer<TValue> GetValues(out uint count)
        {
            return computeBufferDictionary.UnsafeGetValues(out count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetDirectValueByRef(uint key)
        {
            return ref computeBufferDictionary.GetDirectValueByRef(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueByRef(uint key)
        {
            return ref computeBufferDictionary.GetValueByRef(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(uint key)
        {
            return computeBufferDictionary.ContainsKey(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindIndex(uint entityId, out uint index)
        {
            return computeBufferDictionary.TryFindIndex(entityId, out index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(uint entityId, out TValue item)
        {
            return computeBufferDictionary.TryGetValue(entityId, out item);
        }

        public int count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => computeBufferDictionary.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITypeSafeDictionary Create()
        {
            return new ComputeSharpTypeSafeDictionary<TValue>(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITypeSafeDictionary Create(uint size)
        {
            return new ComputeSharpTypeSafeDictionary<TValue>(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            computeBufferDictionary.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(uint size)
        {
            computeBufferDictionary.EnsureCapacity(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncreaseCapacityBy(uint size)
        {
            computeBufferDictionary.IncreaseCapacityBy(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            computeBufferDictionary.Trim();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void KeysEvaluator(Action<uint> action)
        {
            foreach (var key in computeBufferDictionary.keys)
                action(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add(uint egidEntityId, in TValue entityComponent)
        {
            if (computeBufferDictionary.TryAdd(egidEntityId, entityComponent, out uint index) == false)
                throw new TypeSafeDictionaryException("Key already present");

            return index;
        }

        public void Dispose()
        {
            computeBufferDictionary.Dispose();

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
            TypeSafeDictionaryMethods.AddEntitiesToDictionary(computeBufferDictionary
                                                            , toDictionary as ITypeSafeDictionary<TValue>
#if SLOW_SVELTO_SUBMISSION
, entityLocator
#endif
                                                            , groupId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveEntitiesFromDictionary(FasterList<(uint, string)> infosToProcess, FasterDictionary<uint, uint> entityIDsAffectedByRemoval)
        {
            TypeSafeDictionaryMethods.RemoveEntitiesFromDictionary(infosToProcess, ref computeBufferDictionary
                                                                 , entityIDsAffectedByRemoval);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapEntitiesBetweenDictionaries(in FasterDictionary<uint, SwapInfo> infosToProcess, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup,
            ITypeSafeDictionary toComponentsDictionary, FasterDictionary<uint, uint> entityIDsAffectedByRemoveAtSwapBack)
        {
            TypeSafeDictionaryMethods.SwapEntitiesBetweenDictionaries(infosToProcess, ref computeBufferDictionary
                                                                    , toComponentsDictionary as
                                                                          ITypeSafeDictionary<TValue>, fromGroup
                                                                    , toGroup, entityIDsAffectedByRemoveAtSwapBack);
        }

        /// <summary>
        ///     Execute all the engine IReactOnAdd callbacks linked to components added this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesAddCallbacks
        (FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnAdd>>> entityComponentEnginesDB
       , ITypeSafeDictionary toDic, ExclusiveGroupStruct toGroup, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesAddCallbacks(ref computeBufferDictionary
                                                               , (ITypeSafeDictionary<TValue>)toDic, toGroup
                                                               , entityComponentEnginesDB, in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnSwap callbacks linked to components swapped this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesSwapCallbacksFast(FasterList<ReactEngineContainer<IReactOnSwapEx>> reactiveEnginesSwap,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup,
            (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler sampler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesSwapCallbacksFast(
                reactiveEnginesSwap, fromGroup, toGroup, entityIDs, this, rangeOfSubmittedEntitiesIndicies, sampler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnRemove callbacks linked to components removed this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesRemoveCallbacks
        (FasterList<(uint, string)> infosToProcess
       , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveEnginesRemove
       , ExclusiveGroupStruct fromGroup, in PlatformProfiler sampler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesRemoveCallbacks(infosToProcess, ref computeBufferDictionary
                                                                  , reactiveEnginesRemove, fromGroup, in sampler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnAddEx callbacks linked to components added this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesAddEntityCallbacksFast
        (FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnAddEx>>> reactiveEnginesAdd
       , ExclusiveGroupStruct groupID, (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesAddEntityCallbacksFast(
                reactiveEnginesAdd, groupID, rangeOfSubmittedEntitiesIndicies, entityIDs, this, profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnSwapEx callbacks linked to components swapped this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesSwapCallbacks(FasterDictionary<uint, SwapInfo> rangeOfSubmittedEntitiesIndicies,
            FasterList<ReactEngineContainer<IReactOnSwap>> reactiveEnginesSwap, ExclusiveGroupStruct fromGroup,
            ExclusiveGroupStruct toGroup, in PlatformProfiler sampler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesSwapCallbacks(rangeOfSubmittedEntitiesIndicies, ref computeBufferDictionary
                                                                 , reactiveEnginesSwap, fromGroup, toGroup, in sampler);
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
        (FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnSwap>>> reactiveEnginesSwap
       , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnSwapEx>>> reactiveEnginesSwapEx
       , ITypeSafeDictionary toDictionary, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup
       , in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesSwapCallbacks_Group(ref computeBufferDictionary
                                                                      , (ITypeSafeDictionary<TValue>)toDictionary
                                                                      , toGroup, fromGroup, reactiveEnginesSwap
                                                                      , reactiveEnginesSwapEx, entityIDs
                                                                      , in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnRemove and IReactOnRemoveEx callbacks linked to components remove from
        ///     whole groups removed during this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesRemoveCallbacks_Group
        (FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveEnginesRemove
       , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemoveEx>>> reactiveEnginesRemoveEx
       , ExclusiveGroupStruct group, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesRemoveCallbacks_Group(
                ref computeBufferDictionary, reactiveEnginesRemove, reactiveEnginesRemoveEx, entityIDs
              , group, in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnDispose for eahc component registered in the DB when it's disposed of
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesDisposeCallbacks_Group
        (FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnDispose>>> reactiveEnginesDispose
          , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnDisposeEx>>> reactiveEnginesDisposeEx
       , ExclusiveGroupStruct group, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesDisposeCallbacks_Group(
                ref computeBufferDictionary, reactiveEnginesDispose, reactiveEnginesDisposeEx, entityIDs, group, in profiler);
        }

        SveltoDictionary<uint, TValue, NativeStrategy<SveltoDictionaryNode<uint>>, ComputeSharpStrategy<TValue>,
            NativeStrategy<int>> computeBufferDictionary;
    }
}