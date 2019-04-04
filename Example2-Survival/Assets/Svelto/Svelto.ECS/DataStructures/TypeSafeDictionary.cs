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

        void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId);

        void AddEntitiesToEngines(Dictionary<Type,FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDb, ITypeSafeDictionary realDic, ref PlatformProfiler profiler);

        void AddCapacity(uint size);
        void Trim();
        void Clear();
        bool Has(uint entityIdEntityId);
    }

    class TypeSafeDictionary<TValue> : FasterDictionary<uint, TValue>, ITypeSafeDictionary where TValue : IEntityStruct
    {
        static readonly Type   _type     = typeof(TValue);
        static readonly string _typeName = _type.Name;
        static readonly bool HasEgid = typeof(INeedEGID).IsAssignableFrom(_type);
        
        public TypeSafeDictionary(uint size) : base((uint) size) { }
        public TypeSafeDictionary() {}
        
        public void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId)
        {
            var typeSafeDictionary = entitiesToSubmit as TypeSafeDictionary<TValue>;
            
            foreach (var tuple in typeSafeDictionary)
            {
                try
                {
                    if (HasEgid)
                    {
                        var needEgid = (INeedEGID)tuple.Value;
                        needEgid.ID = new EGID(tuple.Key, groupId);
                        Add(tuple.Key, (TValue) needEgid);
                    }
                    else
                        Add(tuple.Key, ref tuple.Value);
                }
                catch (Exception e)
                {
                    throw new TypeSafeDictionaryException(
                        "trying to add an EntityView with the same ID more than once Entity: "
                           .FastConcat(typeof(TValue).ToString()).FastConcat("id ").FastConcat(tuple.Key), e);
                }
            }
        }

        public void AddEntitiesToEngines(
            Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB,
            ITypeSafeDictionary realDic, ref PlatformProfiler profiler)
        {
            foreach (var value in this)
            {
                var typeSafeDictionary = realDic as TypeSafeDictionary<TValue>;
                var i = typeSafeDictionary.GetValueIndex(value.Key);
               
                AddEntityViewToEngines(entityViewEnginesDB, ref typeSafeDictionary._values[i], null, ref profiler);
            }
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
                var previousGroup = fromEntityGid.groupID;
                
                ///
                /// NOTE I WOULD EVENTUALLY NEED TO REUSE THE REAL ID OF THE REMOVING ELEMENT
                /// SO THAT I CAN DECREASE THE GLOBAL GROUP COUNT
                /// 
                
          //      entity.ID = EGID.UPDATE_REAL_ID_AND_GROUP(entity.ID, toEntityID.groupID, entityCount);
                  if (HasEgid)
                  {
                      var needEgid = (INeedEGID)entity;
                      needEgid.ID = toEntityID.Value;
                      entity = (TValue) needEgid;
                  }
                
                var index = toGroupCasted.Add(fromEntityGid.entityID, ref entity);

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
                        (entityViewsEngines[i] as IHandleEntityStructEngine<TValue>).AddInternal(ref entity, previousGroup);
                    }
                }
                catch (Exception e)
                {
                    throw new ECSException(
                        "Code crashed inside Add callback ".FastConcat(typeof(TValue).ToString()), e);
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
                                                                                                    ref entity, itsaswap);
                }
                catch (Exception e)
                {
                    throw new ECSException(
                        "Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString()), e);
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