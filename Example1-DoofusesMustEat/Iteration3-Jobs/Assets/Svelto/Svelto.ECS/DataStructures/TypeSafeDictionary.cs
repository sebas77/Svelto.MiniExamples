using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    sealed class TypeSafeDictionary<TValue> : ITypeSafeDictionary<TValue> where TValue : struct, IEntityComponent
    {
        static readonly Type   _type     = typeof(TValue);
        static readonly string _typeName = _type.Name;
        static readonly bool   _hasEgid  = typeof(INeedEGID).IsAssignableFrom(_type);

        public TypeSafeDictionary(uint size, bool IsUnmanaged)
        {
            if (IsUnmanaged == false)
                implementation = new SveltoDictionary<uint, TValue>(size, new ManagedStrategy<TValue>());
            else
                implementation = new SveltoDictionary<uint, TValue>(size, new NativeStrategy<TValue>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint egidEntityId, in TValue entityComponent)
        {
            implementation.Add(egidEntityId, entityComponent);
        }

        /// <summary>
        ///     Add entities from external typeSafeDictionary
        /// </summary>
        /// <param name="entitiesToSubmit"></param>
        /// <param name="groupId"></param>
        /// <exception cref="TypeSafeDictionaryException"></exception>
        public void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId)
        {
            var typeSafeDictionary = entitiesToSubmit as TypeSafeDictionary<TValue>;

            foreach (var tuple in typeSafeDictionary)
                try
                {
                    if (_hasEgid)
                        SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref tuple.Value, new EGID(tuple.Key, groupId));

                    implementation.Add(tuple.Key, tuple.Value);
                }
                catch (Exception e)
                {
                    Console.LogException(
                        e, "trying to add an EntityComponent with the same ID more than once Entity: ".
                           FastConcat(typeof(TValue).ToString()).FastConcat(", group ").
                           FastConcat(groupId).FastConcat(", id ").FastConcat(tuple.Key));

                    throw;
                }
        }

        public void AddEntitiesToEngines
        (FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityComponentEnginesDB
       , ITypeSafeDictionary realDic, ExclusiveGroupStruct group, in PlatformProfiler profiler)
        {
            var typeSafeDictionary = realDic as ITypeSafeDictionary<TValue>;

            //this can be optimized, should pass all the entities and not restart the process for each one
            foreach (var value in implementation)
                AddEntityComponentToEngines(entityComponentEnginesDB, ref typeSafeDictionary.GetValueByRef(value.Key)
                                          , null, in profiler, new EGID(value.Key, group));
        }

        public void AddEntityToDictionary(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup)
        {
            var valueIndex = implementation.GetIndex(fromEntityGid.entityID);

            if (toGroup != null)
            {
                var     toGroupCasted = toGroup as ITypeSafeDictionary<TValue>;
                ref var entity        = ref implementation.unsafeValues[(int) valueIndex];

                if (_hasEgid)
                    SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityID);

                toGroupCasted.Add(fromEntityGid.entityID, entity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() { implementation.Clear(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(uint egidEntityId) { return implementation.ContainsKey(egidEntityId); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITypeSafeDictionary Create() { return TypeSafeDictionaryFactory<TValue>.Create(1); }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear() { implementation.FastClear(); }

        public object GenerateSentinel() { return default; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SveltoDictionary<uint, TValue>.SveltoDictionaryKeyValueEnumerator GetEnumerator()
        {
            return implementation.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(uint valueEntityId) { return implementation.GetIndex(valueEntityId); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrCreate(uint idEntityId) { return ref implementation.GetOrCreate(idEntityId); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueByRef(uint key) { return ref implementation.GetValueByRef(key); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBuffer<TValue> GetValuesArray(out uint count)
        {
            var managedBuffer = implementation.unsafeValues;
            count = this.count;
            return managedBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(uint key) { return implementation.ContainsKey(key); }

        public void MoveEntityFromEngines
        (EGID fromEntityGid, EGID? toEntityID, ITypeSafeDictionary toGroup
       , FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines, in PlatformProfiler profiler)
        {
            var valueIndex = implementation.GetIndex(fromEntityGid.entityID);

            ref var entity = ref implementation.unsafeValues[(int) valueIndex];

            if (toGroup != null)
            {
                RemoveEntityComponentFromEngines(engines, ref entity, fromEntityGid.groupID, in profiler
                                               , fromEntityGid);

                var toGroupCasted = toGroup as ITypeSafeDictionary<TValue>;
                var previousGroup = fromEntityGid.groupID;

                if (_hasEgid)
                    SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityID.Value);

                var index = toGroupCasted.GetIndex(toEntityID.Value.entityID);

                AddEntityComponentToEngines(engines, ref toGroupCasted.unsafeValues[(int) index], previousGroup
                                          , in profiler, toEntityID.Value);
            }
            else
            {
                RemoveEntityComponentFromEngines(engines, ref entity, null, in profiler, fromEntityGid);
            }
        }

        public void RemoveEntitiesFromEngines
        (FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines
       , in PlatformProfiler profiler, ExclusiveGroupStruct group)
        {
            foreach (var value in implementation)
                RemoveEntityComponentFromEngines(engines, ref implementation.GetValueByRef(value.Key)
                                               , null, in profiler, new EGID(value.Key, group));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveEntityFromDictionary(EGID fromEntityGid) { implementation.Remove(fromEntityGid.entityID); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(uint size) { implementation.SetCapacity(size); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim() { implementation.Trim(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindIndex(uint entityId, out uint index)
        {
            return implementation.TryFindIndex(entityId, out index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(uint entityId, out TValue item)
        {
            return implementation.TryGetValue(entityId, out item);
        }

        internal SveltoDictionary<uint, TValue> implementation { get; }

        public uint count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => implementation.count;
        }

        public ref TValue this[uint idEntityId]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref implementation.GetValueByRef(idEntityId);
        }

        public IBuffer<TValue> unsafeValues
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => implementation.unsafeValues;
        }

        static void RemoveEntityComponentFromEngines
        (FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines, ref TValue entity, uint? previousGroup
       , in PlatformProfiler profiler, EGID egid)
        {
            if (!engines.TryGetValue(new RefWrapper<Type>(_type), out var entityComponentsEngines))
                return;

            if (previousGroup == null)
                for (var i = 0; i < entityComponentsEngines.count; i++)
                    try
                    {
                        using (profiler.Sample(entityComponentsEngines[i], _typeName))
                        {
                            (entityComponentsEngines[i] as IReactOnAddAndRemove<TValue>).Remove(ref entity, egid);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ECSException(
                            "Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
        }

        void AddEntityComponentToEngines
        (FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines, ref TValue entity
       , ExclusiveGroupStruct? previousGroup, in PlatformProfiler profiler, EGID egid)
        {
            //get all the engines linked to TValue
            if (!engines.TryGetValue(new RefWrapper<Type>(_type), out var entityComponentsEngines))
                return;

            if (previousGroup == null)
                for (var i = 0; i < entityComponentsEngines.count; i++)
                    try
                    {
                        using (profiler.Sample(entityComponentsEngines[i], _typeName))
                        {
                            (entityComponentsEngines[i] as IReactOnAddAndRemove<TValue>).Add(ref entity, egid);
                        }
                    }
                    catch (Exception)
                    {
                        Svelto.Console.LogError("Code crashed inside Add callback ".FastConcat(typeof(TValue).ToString()));

                        throw;
                    }
            else
                for (var i = 0; i < entityComponentsEngines.count; i++)
                    try
                    {
                        using (profiler.Sample(entityComponentsEngines[i], _typeName))
                        {
                            (entityComponentsEngines[i] as IReactOnSwap<TValue>).MovedTo(
                                ref entity, previousGroup.Value, egid);
                        }
                    }
                    catch (Exception)
                    {
                        Svelto.Console.LogError("Code crashed inside MoveTo callback ".FastConcat(typeof(TValue).ToString()));

                        throw;
                    }
        }

        void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }

        void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
                implementation?.Dispose();
        }

        ~TypeSafeDictionary() { Dispose(false); }
    }
}