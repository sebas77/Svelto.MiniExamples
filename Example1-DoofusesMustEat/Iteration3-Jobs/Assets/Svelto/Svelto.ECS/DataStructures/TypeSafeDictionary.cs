using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeDictionary
    {
        uint                Count { get; }
        ITypeSafeDictionary Create();

        void AddEntitiesToEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDb,
                                  ITypeSafeDictionary                                     realDic,
                                  ExclusiveGroup.ExclusiveGroupStruct                     @group,
                                  in PlatformProfiler profiler);

        void RemoveEntitiesFromEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB,
                                       in PlatformProfiler                                     profiler,
                                       ExclusiveGroup.ExclusiveGroupStruct                     @group);

        void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId);

        void MoveEntityFromEngines(EGID                                                    fromEntityGid,
                                   EGID?                                                   toEntityID, ITypeSafeDictionary toGroup,
                                   FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines,
                                   in PlatformProfiler                                     profiler);

        void AddEntityToDictionary(EGID fromEntityGid, EGID toEntityID,
                                   ITypeSafeDictionary toGroup);

        void RemoveEntityFromDictionary(EGID fromEntityGid, in PlatformProfiler profiler);

        void SetCapacity(uint size);
        void Trim();
        void Clear();
        void FastClear();
        bool Has(uint key);
        bool       ContainsKey(uint    egidEntityId);
        uint       GetIndex(uint       valueEntityId);
        bool       TryFindIndex(uint   entityGidEntityId, out uint   index);
    }
    
    public interface ITypeSafeDictionary<TValue> : ITypeSafeDictionary where TValue : IEntityStruct
    {
        void       Add(uint            egidEntityId, in TValue entityView);
        ref TValue GetValueByRef(uint  valueKey);
        TValue this[uint idEntityId] { get; set; }
        bool       TryGetValue(uint    entityId,          out TValue item);
        ref TValue GetDirectValue(uint findElementIndex);
        ref TValue GetOrCreate(uint    idEntityId);

        TValue[] GetValuesArray(out uint count);
        TValue[] unsafeValues { get; }
        IFasterDictionary<uint, TValue> implementation { get; }
    }

    sealed class TypeSafeDictionary<TValue> : ITypeSafeDictionary<TValue> where TValue : struct, IEntityStruct
    {
        static readonly Type   _type     = typeof(TValue);
        static readonly string _typeName = _type.Name;
        static readonly bool   _hasEgid  = typeof(INeedEGID).IsAssignableFrom(_type);

        public TypeSafeDictionary(uint size)
        {
            _implementation = new FasterDictionary<uint, TValue>(size);
        }

        public TypeSafeDictionary()
        {
            _implementation = new FasterDictionary<uint, TValue>(1);
        }

        /// <summary>
        /// Add entities from external typeSafeDictionary
        /// </summary>
        /// <param name="entitiesToSubmit"></param>
        /// <param name="groupId"></param>
        /// <exception cref="TypeSafeDictionaryException"></exception>
        public void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId) 
        {
            var typeSafeDictionary = entitiesToSubmit as TypeSafeDictionary<TValue>;

            foreach (var tuple in typeSafeDictionary)
            {
                try
                {
                    if (_hasEgid)
                        SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref tuple.Value, new EGID(tuple.Key, groupId));

                    _implementation.Add(tuple.Key, tuple.Value);
                }
                catch (Exception e)
                {
                    throw new
                        TypeSafeDictionaryException("trying to add an EntityView with the same ID more than once Entity: ".FastConcat(typeof(TValue).ToString()).FastConcat(", group ").FastConcat(groupId).FastConcat(", id ").FastConcat(tuple.Key),
                                                    e);
                }
            }
        }
        
        public void AddEntityToDictionary(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup)
        {
            var valueIndex = _implementation.GetIndex(fromEntityGid.entityID);

            if (toGroup != null)
            {
                var     toGroupCasted = toGroup as ITypeSafeDictionary<TValue>;
                ref var entity        = ref _implementation.unsafeValues[(int) valueIndex];

                if (_hasEgid) SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityID);

                toGroupCasted.Add(fromEntityGid.entityID, entity);
            }
        }

        public void AddEntitiesToEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB,
                                         ITypeSafeDictionary                                     realDic,
                                         ExclusiveGroup.ExclusiveGroupStruct                     @group,
                                         in PlatformProfiler profiler)
        {
            var typeSafeDictionary = realDic as ITypeSafeDictionary<TValue>;

            //this can be optimized, should pass all the entities and not restart the process for each one
            foreach (var value in _implementation)
                AddEntityViewToEngines(entityViewEnginesDB, ref typeSafeDictionary.GetValueByRef(value.Key), null,
                                       in profiler, new EGID(value.Key, group));
        }

        public void RemoveEntitiesFromEngines(
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB, in PlatformProfiler profiler,
            ExclusiveGroup.ExclusiveGroupStruct                     @group)
        {
            foreach (var value in _implementation)
                RemoveEntityViewFromEngines(entityViewEnginesDB, ref _implementation.GetValueByRef(value.Key), null,
                                            in profiler, new EGID(value.Key, group));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear() { _implementation.FastClear(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(uint key) { return _implementation.ContainsKey(key); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveEntityFromDictionary(EGID fromEntityGid, in PlatformProfiler profiler)
        {
            _implementation.Remove(fromEntityGid.entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(uint size) { _implementation.SetCapacity(size); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim() { _implementation.Trim(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() { _implementation.Clear(); }

        public void MoveEntityFromEngines(EGID                                                    fromEntityGid,
                                          EGID?                                                   toEntityID, ITypeSafeDictionary toGroup,
                                          FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines,
                                          in PlatformProfiler                                     profiler)
        {
            var valueIndex = _implementation.GetIndex(fromEntityGid.entityID);

            ref var entity = ref _implementation.unsafeValues[(int) valueIndex];

            if (toGroup != null)
            {
                RemoveEntityViewFromEngines(engines, ref entity, fromEntityGid.groupID, in profiler, fromEntityGid);

                var toGroupCasted = toGroup as ITypeSafeDictionary<TValue>;
                var previousGroup = fromEntityGid.groupID;

                if (_hasEgid) SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityID.Value);

                var index = toGroupCasted.GetIndex(toEntityID.Value.entityID);

                AddEntityViewToEngines(engines, ref toGroupCasted.unsafeValues[(int) index], previousGroup, in profiler,
                                       toEntityID.Value);
            }
            else
                RemoveEntityViewFromEngines(engines, ref entity, null, in profiler, fromEntityGid);
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _implementation.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITypeSafeDictionary Create() { return new TypeSafeDictionary<TValue>(); }

        void AddEntityViewToEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB,
                                    ref TValue                                              entity,
                                    ExclusiveGroup.ExclusiveGroupStruct?                    previousGroup,
                                    in PlatformProfiler                                     profiler,
                                    EGID                                                    egid)
        {
            //get all the engines linked to TValue
            if (!entityViewEnginesDB.TryGetValue(new RefWrapper<Type>(_type), out var entityViewsEngines)) return;

            if (previousGroup == null)
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                        {
                            (entityViewsEngines[i] as IReactOnAddAndRemove<TValue>).Add(ref entity, egid);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new
                            ECSException("Code crashed inside Add callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
            else
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                        {
                            (entityViewsEngines[i] as IReactOnSwap<TValue>).MovedTo(ref entity, previousGroup.Value,
                                                                                    egid);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new
                            ECSException("Code crashed inside MovedTo callback ".FastConcat(typeof(TValue).ToString()),
                                         e);
                    }
            }
        }

        static void RemoveEntityViewFromEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> @group,
                                                ref TValue                                              entity,
                                                ExclusiveGroup.ExclusiveGroupStruct?                    previousGroup,
                                                in PlatformProfiler                                     profiler,
                                                EGID                                                    egid)
        {
            if (!@group.TryGetValue(new RefWrapper<Type>(_type), out var entityViewsEngines)) return;

            if (previousGroup == null)
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                            (entityViewsEngines[i] as IReactOnAddAndRemove<TValue>).Remove(ref entity, egid);
                    }
                    catch (Exception e)
                    {
                        throw new
                            ECSException("Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString()),
                                         e);
                    }
            }
#if SEEMS_UNNECESSARY
            else
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                            (entityViewsEngines[i] as IReactOnSwap<TValue>).MovedFrom(ref entity, egid);
                    }
                    catch (Exception e)
                    {
                        throw new ECSException(
                            "Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue[] GetValuesArray(out uint count)
        {
            var managedBuffer = _implementation.GetValuesArray(out count);
            return managedBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(uint egidEntityId) { return _implementation.ContainsKey(egidEntityId); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint egidEntityId, in TValue entityView) { _implementation.Add(egidEntityId, entityView); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterDictionary<uint, TValue>.FasterDictionaryKeyValueEnumerator GetEnumerator()
        {
            return _implementation.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueByRef(uint valueKey) { return ref _implementation.GetValueByRef(valueKey); }

        public TValue this[uint idEntityId]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _implementation[idEntityId];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _implementation[idEntityId] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(uint valueEntityId) { return _implementation.GetIndex(valueEntityId); }

        public TValue[] unsafeValues
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _implementation.unsafeValues;
        }

        public IFasterDictionary<uint, TValue> implementation => _implementation;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(uint entityId, out TValue item) { return _implementation.TryGetValue(entityId, out item); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrCreate(uint idEntityId) { return ref _implementation.GetOrCreate(idEntityId); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindIndex(uint entityId, out uint index) { return _implementation.TryFindIndex(entityId, out index); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetDirectValue(uint findElementIndex)
        {
            return ref _implementation.GetDirectValue(findElementIndex);
        }
        
        readonly FasterDictionary<uint, TValue> _implementation;
    }
    
    internal static class TypeSafeDictionaryUtilities
    {
        internal static EGIDMapper<T> ToEGIDMapper<T>(this ITypeSafeDictionary<T> dic) where T:struct, IEntityStruct
        {
            var mapper = new EGIDMapper<T> {map = (FasterDictionary<uint, T>) dic.implementation};

            return mapper;
        }

        internal static NativeEGIDMapper<T> ToNativeEGIDMapper<T>(this ITypeSafeDictionary<T> dic) where T : unmanaged, IEntityStruct
        {
            var mapper = new NativeEGIDMapper<T> {map = dic.implementation.ToNative<uint, T>()};

            return mapper;
        }
    }
}