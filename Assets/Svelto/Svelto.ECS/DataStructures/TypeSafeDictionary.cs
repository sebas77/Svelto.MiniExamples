using System;
using System.Collections.Generic;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeDictionary
    {
        int                 Count { get; }
        ITypeSafeDictionary Create();

        void RemoveEntitiesFromEngines(
            Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB,
            ref PlatformProfiler profiler);

        void MoveEntityFromDictionaryAndEngines(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup,
            Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB,
            ref PlatformProfiler profiler);

        void FillWithIndexedEntities(ITypeSafeDictionary entities);

        void AddEntitiesToEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB,
            ref PlatformProfiler profiler);

        void AddCapacity(int size);
        void Trim();
        void Clear();
        bool Has(int entityIdEntityId);
    }

    class TypeSafeDictionary<TValue> : FasterDictionary<int, TValue>, ITypeSafeDictionary where TValue : IEntityStruct
    {
        static readonly Type   _type     = typeof(TValue);
        static readonly string _typeName = _type.Name;

        public TypeSafeDictionary(int size) : base(size) { }

        public TypeSafeDictionary() { }

        public void FillWithIndexedEntities(ITypeSafeDictionary entities)
        {
            int count;
            var buffer = (entities as TypeSafeDictionary<TValue>).GetValuesArray(out count);

            for (var i = 0; i < count; i++)
            {
                var idEntityId = 0;
                try
                {
                    idEntityId = buffer[i].ID.entityID;

                    Add(idEntityId, ref buffer[i]);
                }
                catch (Exception e)
                {
                    throw new TypeSafeDictionaryException(
                        "trying to add an EntityView with the same ID more than once Entity: "
                           .FastConcat(typeof(TValue).ToString()).FastConcat("id ").FastConcat(idEntityId), e);
                }
            }
        }

        public void AddEntitiesToEngines(
            Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB,
            ref PlatformProfiler profiler)
        {
            var values = GetValuesArray(out var count);

            for (var i = 0; i < count; i++)
                AddEntityViewToEngines(entityViewEnginesDB, ref values[i], null, ref profiler);
        }

        public bool Has(int entityIdEntityId) { return ContainsKey(entityIdEntityId); }

        public void MoveEntityFromDictionaryAndEngines(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup,
            Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB,
            ref PlatformProfiler profiler)
        {
            var fasterValuesBuffer = GetValuesArray(out _);
            var valueIndex = GetValueIndex(fromEntityGid.entityID);

            if (entityViewEnginesDB != null)
                RemoveEntityViewFromEngines(entityViewEnginesDB, ref fasterValuesBuffer[valueIndex], ref profiler);

            if (toGroup != null)
            {
                var toGroupCasted = toGroup as TypeSafeDictionary<TValue>;
                var previousGroup = fasterValuesBuffer[valueIndex].ID.groupID; 
                fasterValuesBuffer[valueIndex].ID = toEntityID;
                toGroupCasted.Add(toEntityID.entityID, ref fasterValuesBuffer[valueIndex]);

                if (entityViewEnginesDB != null)
                    AddEntityViewToEngines(entityViewEnginesDB,
                                           ref toGroupCasted.GetValuesArray(out _)[
                                               toGroupCasted.GetValueIndex(toEntityID.entityID)], previousGroup,
                                           ref profiler);
            }

            Remove(fromEntityGid.entityID);
        }

        public void RemoveEntitiesFromEngines(
            Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB,
            ref PlatformProfiler profiler)
        {
            var values = GetValuesArray(out var count);

            for (var i = 0; i < count; i++)
                RemoveEntityViewFromEngines(entityViewEnginesDB, ref values[i], ref profiler);
        }

        public ITypeSafeDictionary Create() { return new TypeSafeDictionary<TValue>(); }

        void AddEntityViewToEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB,
                                    ref TValue                                                      entity,
                                    ExclusiveGroup.ExclusiveGroupStruct?                            previousGroup,
                                    ref PlatformProfiler                                            profiler)
        {
            //get all the engines linked to TValue
            if (!entityViewEnginesDB.TryGetValue(_type, out var entityViewsEngines)) return;
            
            for (var i = 0; i < entityViewsEngines.Count; i++)
                try
                {
                    using (profiler.Sample((entityViewsEngines[i] as EngineInfo).name))
                    {
                        (entityViewsEngines[i] as IHandleEntityStructEngine<TValue>).AddInternal(ref entity, previousGroup);
                    }
                }
                catch (Exception e)
                {
                    throw new ECSException(
                        "Code crashed inside Add callback ".FastConcat(typeof(TValue).ToString()).FastConcat("id ")
                           .FastConcat(entity.ID.entityID), e);
                }
        }

        static void RemoveEntityViewFromEngines(
            Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB, ref TValue entity,
            ref PlatformProfiler profiler)
        {
            if (!entityViewEnginesDB.TryGetValue(_type, out var entityViewsEngines)) return;
            
            for (var i = 0; i < entityViewsEngines.Count; i++)
                try
                {
                    using (profiler.Sample((entityViewsEngines[i] as EngineInfo).name, _typeName))
                    {
                        (entityViewsEngines[i] as IHandleEntityStructEngine<TValue>).RemoveInternal(ref entity);
                    }
                }
                catch (Exception e)
                {
                    throw new ECSException(
                        "Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString())
                           .FastConcat("id ").FastConcat(entity.ID.entityID), e);
                }
        }

        public bool ExecuteOnEntityView<W>(int entityGidEntityId, ref W value, EntityAction<TValue, W> action)
        {
            if (!FindIndex(entityGidEntityId, out var findIndex)) return false;
            
            action(ref _values[findIndex], ref value);

            return true;
        }

        public bool ExecuteOnEntityView(int entityGidEntityId, EntityAction<TValue> action)
        {
            if (!FindIndex(entityGidEntityId, out var findIndex)) return false;
            
            action(ref _values[findIndex]);

            return true;

        }

        public uint FindElementIndex(int entityGidEntityId)
        {
            if (FindIndex(entityGidEntityId, out var findIndex) == false)
                throw new Exception("Entity not found in this group");

            return findIndex;
        }

        public bool TryFindElementIndex(int entityGidEntityId, out uint index)
        {
            return FindIndex(entityGidEntityId, out index);
        }
    }
}