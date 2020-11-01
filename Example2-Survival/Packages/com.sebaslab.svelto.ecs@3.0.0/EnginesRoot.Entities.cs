using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DBC.ECS;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot : IDisposable, IUnitTestingInterface
    {
        ///--------------------------------------------
        ///
        public IEntityStreamConsumerFactory GenerateConsumerFactory()
        {
            return new GenericEntityStreamConsumerFactory(this);
        }

        public IEntityFactory GenerateEntityFactory()
        {
            return new GenericEntityFactory(this);
        }

        public IEntityFunctions GenerateEntityFunctions()
        {
            return new GenericEntityFunctions(this);
        }

        ///--------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        EntityComponentInitializer BuildEntity
        (EGID entityID, IComponentBuilder[] componentsToBuild, Type descriptorType,
            IEnumerable<object> implementors = null)
        {
            CheckAddEntityID(entityID, descriptorType);
            Check.Require(entityID.groupID != 0, "invalid group detected");

            var dic = EntityFactory.BuildGroupedEntities(entityID, _groupedEntityToAdd, componentsToBuild
              , implementors, descriptorType);

            return new EntityComponentInitializer(entityID, dic);
        }

        ///--------------------------------------------
        void Preallocate<T>(ExclusiveGroupStruct groupID, uint size) where T : IEntityDescriptor, new()
        {
            using (var profiler = new PlatformProfiler("Preallocate"))
            {
                var entityComponentsToBuild  = EntityDescriptorTemplate<T>.descriptor.componentsToBuild;
                var numberOfEntityComponents = entityComponentsToBuild.Length;

                FasterDictionary<RefWrapperType, ITypeSafeDictionary> group = GetOrCreateGroup(groupID, profiler);

                for (var index = 0; index < numberOfEntityComponents; index++)
                {
                    var entityComponentBuilder = entityComponentsToBuild[index];
                    var entityComponentType    = entityComponentBuilder.GetEntityComponentType();

                    var refWrapper = new RefWrapperType(entityComponentType);
                    if (group.TryGetValue(refWrapper, out var dbList) == false)
                        group[refWrapper] = entityComponentBuilder.Preallocate(ref dbList, size);
                    else
                        dbList.SetCapacity(size);

                    if (_groupsPerEntity.TryGetValue(refWrapper, out var groupedGroup) == false)
                        groupedGroup = _groupsPerEntity[refWrapper] = new FasterDictionary<uint, ITypeSafeDictionary>();

                    groupedGroup[groupID] = dbList;
                }
            }
        }

        ///--------------------------------------------
        ///
        void MoveEntityFromAndToEngines(IComponentBuilder[] componentBuilders, EGID fromEntityGID, EGID? toEntityGID)
        {
            using (var sampler = new PlatformProfiler("Move Entity From Engines"))
            {
                var fromGroup = GetGroup(fromEntityGID.groupID);

                //Check if there is an EntityInfo linked to this entity, if so it's a DynamicEntityDescriptor!
                if (fromGroup.TryGetValue(new RefWrapperType(ComponentBuilderUtilities.ENTITY_INFO_COMPONENT)
                      , out var entityInfoDic)
                 && (entityInfoDic as ITypeSafeDictionary<EntityInfoComponent>).TryGetValue(
                        fromEntityGID.entityID, out var entityInfo))
                    MoveEntityComponents(fromEntityGID, toEntityGID, entityInfo.componentsToBuild, fromGroup
                      , sampler);
                //otherwise it's a normal static entity descriptor
                else
                    MoveEntityComponents(fromEntityGID, toEntityGID, componentBuilders, fromGroup, sampler);
            }
        }

        void MoveEntityComponents(EGID fromEntityGID, EGID? toEntityGID, IComponentBuilder[] entitiesToMove
          , FasterDictionary<RefWrapperType, ITypeSafeDictionary> fromGroup, in PlatformProfiler sampler)
        {
            using (sampler.Sample("MoveEntityComponents"))
            {
                var length = entitiesToMove.Length;

                FasterDictionary<RefWrapperType, ITypeSafeDictionary> toGroup = null;

                if (toEntityGID != null)
                {
                    var toGroupID = toEntityGID.Value.groupID;

                    toGroup = GetOrCreateGroup(toGroupID, sampler);

                    //Add all the entities to the dictionary
                    for (var i = 0; i < length; i++)
                        CopyEntityToDictionary(fromEntityGID, toEntityGID.Value, fromGroup, toGroup
                          , entitiesToMove[i].GetEntityComponentType(), sampler);
                }

                //call all the callbacks
                for (var i = 0; i < length; i++)
                    MoveEntityComponentFromAndToEngines(fromEntityGID, toEntityGID, fromGroup, toGroup
                      , entitiesToMove[i].GetEntityComponentType(), sampler);

                //then remove all the entities from the dictionary
                for (var i = 0; i < length; i++)
                    RemoveEntityFromDictionary(fromEntityGID, fromGroup, entitiesToMove[i].GetEntityComponentType(),
                        sampler);
            }
        }

        void CopyEntityToDictionary
        (EGID entityGID, EGID toEntityGID, FasterDictionary<RefWrapperType, ITypeSafeDictionary> fromGroup
          , FasterDictionary<RefWrapperType, ITypeSafeDictionary> toGroup, Type entityComponentType,
            in PlatformProfiler sampler)
        {
            using (sampler.Sample("CopyEntityToDictionary"))
            {
                var wrapper = new RefWrapperType(entityComponentType);

                ITypeSafeDictionary fromTypeSafeDictionary =
                    GetTypeSafeDictionary(entityGID.groupID, fromGroup, wrapper);

#if DEBUG && !PROFILE_SVELTO
                if (fromTypeSafeDictionary.Has(entityGID.entityID) == false)
                {
                    throw new EntityNotFoundException(entityGID, entityComponentType);
                }
#endif
                ITypeSafeDictionary toEntitiesDictionary =
                    GetOrCreateTypeSafeDictionary(toEntityGID.groupID, toGroup, wrapper, fromTypeSafeDictionary);

                fromTypeSafeDictionary.AddEntityToDictionary(entityGID, toEntityGID, toEntitiesDictionary);
            }
        }

        void MoveEntityComponentFromAndToEngines
        (EGID entityGID, EGID? toEntityGID, FasterDictionary<RefWrapperType, ITypeSafeDictionary> fromGroup
          , FasterDictionary<RefWrapperType, ITypeSafeDictionary> toGroup, Type entityComponentType
          , in PlatformProfiler profiler)
        {
            using (profiler.Sample("MoveEntityComponentFromAndToEngines"))
            {
                //add all the entities
                var refWrapper             = new RefWrapperType(entityComponentType);
                var fromTypeSafeDictionary = GetTypeSafeDictionary(entityGID.groupID, fromGroup, refWrapper);

                ITypeSafeDictionary toEntitiesDictionary = null;
                if (toGroup != null)
                    toEntitiesDictionary = toGroup[refWrapper]; //this is guaranteed to exist by AddEntityToDictionary

#if DEBUG && !PROFILE_SVELTO
                if (fromTypeSafeDictionary.Has(entityGID.entityID) == false)
                    throw new EntityNotFoundException(entityGID, entityComponentType);
#endif
                fromTypeSafeDictionary.MoveEntityFromEngines(entityGID, toEntityGID, toEntitiesDictionary
                  , toEntityGID == null
                        ? _reactiveEnginesAddRemove
                        : _reactiveEnginesSwap, in profiler);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RemoveEntityFromDictionary
        (EGID entityGID, FasterDictionary<RefWrapperType, ITypeSafeDictionary> fromGroup, Type entityComponentType
          , in PlatformProfiler sampler)
        {
            using (sampler.Sample("RemoveEntityFromDictionary"))
            {
                var refWrapper             = new RefWrapperType(entityComponentType);
                var fromTypeSafeDictionary = GetTypeSafeDictionary(entityGID.groupID, fromGroup, refWrapper);

                fromTypeSafeDictionary.RemoveEntityFromDictionary(entityGID);
            }
        }

        /// <summary>
        /// Swap all the entities from one group to another
        /// </summary>
        /// <param name="fromIdGroupId"></param>
        /// <param name="toGroupId"></param>
        /// <param name="profiler"></param>
        void SwapEntitiesBetweenGroups(uint fromIdGroupId, uint toGroupId, in PlatformProfiler profiler)
        {
            using (profiler.Sample("SwapEntitiesBetweenGroups"))
            {
                FasterDictionary<RefWrapperType, ITypeSafeDictionary> fromGroup = GetGroup(fromIdGroupId);
                FasterDictionary<RefWrapperType, ITypeSafeDictionary> toGroup = GetOrCreateGroup(toGroupId, profiler);

                foreach (FasterDictionary<RefWrapperType, ITypeSafeDictionary>.KeyValuePairFast dictionaryOfEntities
                    in fromGroup)
                {
                    //call all the MoveTo callbacks
                    dictionaryOfEntities.Value.AddEntitiesToEngines(_reactiveEnginesAddRemove
                      , dictionaryOfEntities.Value
                      , new ExclusiveGroupStruct(toGroupId), profiler);

                    ITypeSafeDictionary toEntitiesDictionary =
                        GetOrCreateTypeSafeDictionary(toGroupId, toGroup, dictionaryOfEntities.Key
                          , dictionaryOfEntities.Value);

                    FasterDictionary<uint, ITypeSafeDictionary> groupsOfEntityType =
                        _groupsPerEntity[dictionaryOfEntities.Key];

                    ITypeSafeDictionary typeSafeDictionary = groupsOfEntityType[fromIdGroupId];
                    toEntitiesDictionary.AddEntitiesFromDictionary(typeSafeDictionary, toGroupId);

                    typeSafeDictionary.FastClear();
                }
            }
        }

        FasterDictionary<RefWrapperType, ITypeSafeDictionary> GetGroup(uint fromIdGroupId)
        {
            if (_groupEntityComponentsDB.TryGetValue(fromIdGroupId
              , out FasterDictionary<RefWrapperType, ITypeSafeDictionary>
                    fromGroup) == false)
                throw new ECSException("Group doesn't exist: ".FastConcat(fromIdGroupId));

            return fromGroup;
        }

        FasterDictionary<RefWrapperType, ITypeSafeDictionary> GetOrCreateGroup(uint toGroupId,
            in PlatformProfiler profiler)
        {
            using (profiler.Sample("GetOrCreateGroup"))
            {
                if (_groupEntityComponentsDB.TryGetValue(
                    toGroupId, out FasterDictionary<RefWrapperType, ITypeSafeDictionary> toGroup) == false)
                    toGroup = _groupEntityComponentsDB[toGroupId] =
                        new FasterDictionary<RefWrapperType, ITypeSafeDictionary>();

                return toGroup;
            }
        }

        ITypeSafeDictionary GetOrCreateTypeSafeDictionary
        (uint groupId, FasterDictionary<RefWrapperType, ITypeSafeDictionary> toGroup, RefWrapperType type
          , ITypeSafeDictionary fromTypeSafeDictionary)
        {
            //be sure that the TypeSafeDictionary for the entity Type exists
            if (toGroup.TryGetValue(type, out ITypeSafeDictionary toEntitiesDictionary) == false)
            {
                toEntitiesDictionary = fromTypeSafeDictionary.Create();
                toGroup.Add(type, toEntitiesDictionary);
            }

            //update GroupsPerEntity
            if (_groupsPerEntity.TryGetValue(type, out var groupedGroup) == false)
                groupedGroup = _groupsPerEntity[type] = new FasterDictionary<uint, ITypeSafeDictionary>();

            groupedGroup[groupId] = toEntitiesDictionary;
            return toEntitiesDictionary;
        }

        static ITypeSafeDictionary GetTypeSafeDictionary
            (uint groupID, FasterDictionary<RefWrapperType, ITypeSafeDictionary> @group, RefWrapperType refWrapper)
        {
            if (@group.TryGetValue(refWrapper, out ITypeSafeDictionary fromTypeSafeDictionary) == false)
            {
                throw new ECSException("no group found: ".FastConcat(groupID));
            }

            return fromTypeSafeDictionary;
        }

        void RemoveEntitiesFromGroup(uint groupID, in PlatformProfiler profiler)
        {
            if (_groupEntityComponentsDB.TryGetValue(groupID, out var dictionariesOfEntities))
            {
                foreach (FasterDictionary<RefWrapperType, ITypeSafeDictionary>.KeyValuePairFast dictionaryOfEntities
                    in dictionariesOfEntities)
                {
                    dictionaryOfEntities.Value.RemoveEntitiesFromEngines(_reactiveEnginesAddRemove, profiler
                      , new ExclusiveGroupStruct(groupID));
                    dictionaryOfEntities.Value.FastClear();

                    FasterDictionary<uint, ITypeSafeDictionary> groupsOfEntityType =
                        _groupsPerEntity[dictionaryOfEntities.Key];
                    groupsOfEntityType[groupID].FastClear();
                }
            }
        }

        //one datastructure rule them all:
        //split by group
        //split by type per group. It's possible to get all the entities of a give type T per group thanks
        //to the FasterDictionary capabilities OR it's possible to get a specific entityComponent indexed by
        //ID. This ID doesn't need to be the EGID, it can be just the entityID
        //for each group id, save a dictionary indexed by entity type of entities indexed by id
        //                         group                EntityComponentType   entityID, EntityComponent
        internal readonly FasterDictionary<uint, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>
            _groupEntityComponentsDB;

        //for each entity view type, return the groups (dictionary of entities indexed by entity id) where they are
        //found indexed by group id. TypeSafeDictionary are never created, they instead point to the ones hold
        //by _groupEntityComponentsDB
        //                        EntityComponentType                    groupID  entityID, EntityComponent
        internal readonly FasterDictionary<RefWrapperType, FasterDictionary<uint, ITypeSafeDictionary>>
            _groupsPerEntity;

        //The filters stored for each component and group
        internal readonly FasterDictionary<RefWrapperType, FasterDictionary<ExclusiveGroupStruct, GroupFilters>>
            _groupFilters;

        readonly EntitiesDB _entitiesDB;

        EntitiesDB IUnitTestingInterface.entitiesForTesting => _entitiesDB;
    }

    public interface IUnitTestingInterface
    {
        EntitiesDB entitiesForTesting { get; }
    }
}