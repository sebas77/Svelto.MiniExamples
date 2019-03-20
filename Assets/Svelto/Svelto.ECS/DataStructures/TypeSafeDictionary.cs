using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        void MoveEntityFromDictionaryAndEngines(EGID fromEntityGid, EGID? toEntityID, ITypeSafeDictionary toGroup,
            Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> engines,
            ref PlatformProfiler profiler);

        void FillWithIndexedEntities(ITypeSafeDictionary entitiesToSubmit);

        void AddEntitiesToEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB,
            ref PlatformProfiler profiler);

        void AddCapacity(uint size);
        void Trim();
        void Clear();
        bool Has(uint entityIdEntityId);
    }

    class TypeSafeDictionary<TValue> : FasterDictionary<uint, TValue>, ITypeSafeDictionary where TValue : IEntityStruct
    {
        static readonly Type   _type     = typeof(TValue);
        static readonly string _typeName = _type.Name;
        
        public TypeSafeDictionary(uint size) : base(size) { }
        public TypeSafeDictionary() {}

        public void FillWithIndexedEntities(ITypeSafeDictionary entitiesToSubmit)
        {
            var buffer = (entitiesToSubmit as TypeSafeDictionary<TValue>).GetValuesArray(out var count);

            for (var i = 0; i < count; i++)
            {
                try
                {
//                    buffer[i].ID = EGID.UPDATE_REAL_ID(buffer[i].ID, entityCount);

                    Add(buffer[i].ID.entityID, ref buffer[i]);
                }
                catch (Exception e)
                {
                    throw new TypeSafeDictionaryException(
                        "trying to add an EntityView with the same ID more than once Entity: "
                           .FastConcat(typeof(TValue).ToString()).FastConcat("id ").FastConcat(buffer[i].ID.entityID), e);
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

        public bool Has(uint entityIdEntityId) { return ContainsKey(entityIdEntityId); }

        public void MoveEntityFromDictionaryAndEngines(EGID fromEntityGid, EGID? toEntityID,
            ITypeSafeDictionary toGroup,
            Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> engines,
            ref PlatformProfiler profiler)
        {
            var valueIndex = GetValueIndex(fromEntityGid.entityID);

            if (engines != null)
                RemoveEntityViewFromEngines(engines, ref _values[valueIndex], ref profiler, toGroup != null);

            if (toGroup != null)
            {
                var toGroupCasted = toGroup as TypeSafeDictionary<TValue>;
                ref var entity = ref _values[valueIndex];
                var previousGroup = entity.ID.groupID;
                
                ///
                /// NOTE I WOULD EVENTUALLY NEED TO REUSE THE REAL ID OF THE REMOVING ELEMENT
                /// SO THAT I CAN DECREASE THE GLOBAL GROUP COUNT
                /// 
                
          //      entity.ID = EGID.UPDATE_REAL_ID_AND_GROUP(entity.ID, toEntityID.groupID, entityCount);
                entity.ID = toEntityID.Value;
                
                var index = toGroupCasted.Add(entity.ID.entityID, ref entity);

                if (engines != null)
                    AddEntityViewToEngines(engines, ref toGroupCasted._values[index], previousGroup,
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
                RemoveEntityViewFromEngines(entityViewEnginesDB, ref values[i], ref profiler, false);
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
                        (entityViewsEngines[i] as IHandleEntityStructEngine<TValue>).AddInternal(in entity, previousGroup);
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
            ref PlatformProfiler profiler, bool itsaswap)
        {
            if (!entityViewEnginesDB.TryGetValue(_type, out var entityViewsEngines)) return;
            
            for (var i = 0; i < entityViewsEngines.Count; i++)
                try
                {
                    using (profiler.Sample((entityViewsEngines[i] as EngineInfo).name, _typeName))
                        (entityViewsEngines[i] as IHandleEntityStructEngine<TValue>).RemoveInternal(
                            in entity, itsaswap);
                }
                catch (Exception e)
                {
                    throw new ECSException(
                        "Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString())
                           .FastConcat("id ").FastConcat(entity.ID.entityID), e);
                }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref TValue FindElement(uint entityGidEntityId)
        {
#if DEBUG && !PROFILER         
            if (FindIndex(entityGidEntityId, out var findIndex) == false)
                throw new Exception("Entity not found in this group ".FastConcat(typeof(TValue).ToString()));
#else
            FindIndex(entityGidEntityId, out var findIndex);
#endif
            return ref _values[findIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TryFindElementIndex(uint entityGidEntityId, out uint index)
        {
            return FindIndex(entityGidEntityId, out index);
        }
    }
}