using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        void SingleSubmission(PlatformProfiler profiler)
        {
            ClearChecks();

            _entitiesOperations.ExecuteOperations(_swapEntities, _removeEntities, _removeGroup, _swapGroup, this);

            AddEntities(profiler);
        }

        static void RemoveGroup(ExclusiveGroupStruct groupID, EnginesRoot enginesRoot)
        {
            using (var sampler = new PlatformProfiler("remove whole group"))
            {
                enginesRoot.RemoveEntitiesFromGroup(groupID, sampler);
            }
        }

        static void SwapGroup(ExclusiveGroupStruct fromGroupID, ExclusiveGroupStruct toGroupID, EnginesRoot enginesRoot)
        {
            using (var sampler = new PlatformProfiler("swap whole group"))
            {
                enginesRoot.SwapEntitiesBetweenGroups(fromGroupID, toGroupID, sampler);
            }
        }

        static void RemoveEntities
        (FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, FasterList<(uint, string)>>>
             removeOperations, FasterList<EGID> entitiesRemoved, EnginesRoot enginesRoot)
        {
            using (var sampler = new PlatformProfiler("remove Entities"))
            {
                using (sampler.Sample("remove Entity References"))
                {
                    var count = entitiesRemoved.count;
                    for (int i = 0; i < count; i++)
                    {
                        var fromEntityGid = entitiesRemoved[i];

                        enginesRoot._entityLocator.RemoveEntityReference(fromEntityGid);
                    }
                }

                using (sampler.Sample("execute remove callbacks and remove entities"))
                {
                    foreach (var entitiesToSwap in removeOperations)
                    {
                        ExclusiveGroupStruct group           = entitiesToSwap.key;
                        var                  groupDictionary = enginesRoot.GetDBGroup(group);

                        foreach (var groupedEntitiesToSwap in entitiesToSwap.value)
                        {
                            var                 componentType        = groupedEntitiesToSwap.key;
                            ITypeSafeDictionary componentsDictionary = groupDictionary[componentType];

                            FasterList<(uint, string)> infosToProcess = groupedEntitiesToSwap.value;

                            componentsDictionary.ExecuteEnginesRemoveCallbacks(
                                infosToProcess, enginesRoot._reactiveEnginesRemove, group, in sampler);
                        }
                    }

                    using (sampler.Sample("Remove Entities and References"))
                    {
                        foreach (var entitiesToSwap in removeOperations)
                        {
                            ExclusiveGroupStruct fromGroup           = entitiesToSwap.key;
                            var                  fromGroupDictionary = enginesRoot.GetDBGroup(fromGroup);

                            foreach (var groupedEntitiesToSwap in entitiesToSwap.value)
                            {
                                var                 componentType        = groupedEntitiesToSwap.key;
                                ITypeSafeDictionary componentsDictionary = fromGroupDictionary[componentType];

                                FasterList<(uint, string)> infosToProcess = groupedEntitiesToSwap.value;

                                componentsDictionary.RemoveEntitiesFromDictionary(infosToProcess);
                            }
                        }
                    }
                }
            }
        }

        static void SwapEntities
        (FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType,
             FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, string)>>>> swapEntitiesOperations
       , FasterList<(EGID, EGID)> entitiesIDSwaps, EnginesRoot enginesRoot)
        {
            using (var sampler = new PlatformProfiler("Swap entities between groups"))
            {
                using (sampler.Sample("Update Entity References"))
                {
                    var count = entitiesIDSwaps.count;
                    for (int i = 0; i < count; i++)
                    {
                        var (fromEntityGid, toEntityGid) = entitiesIDSwaps[i];

                        enginesRoot._entityLocator.UpdateEntityReference(fromEntityGid, toEntityGid);
                    }
                }

                using (sampler.Sample("Swap Entities"))
                {
                    foreach (var entitiesToSwap in swapEntitiesOperations)
                    {
                        ExclusiveGroupStruct fromGroup           = entitiesToSwap.key;
                        var                  fromGroupDictionary = enginesRoot.GetDBGroup(fromGroup);

                        foreach (var groupedEntitiesToSwap in entitiesToSwap.value)
                        {
                            var                 componentType            = groupedEntitiesToSwap.key;
                            ITypeSafeDictionary fromComponentsDictionary = fromGroupDictionary[componentType];

                            foreach (var entitiesInfoToSwap in groupedEntitiesToSwap.value)
                            {
                                ExclusiveGroupStruct toGroup = entitiesInfoToSwap.key;
                                ITypeSafeDictionary toComponentsDictionary =
                                    enginesRoot.GetOrCreateTypeSafeDictionary(
                                        toGroup, enginesRoot.GetOrCreateDBGroup(toGroup), componentType
                                      , fromComponentsDictionary);

                                FasterList<(uint, string)> infosToProcess = entitiesInfoToSwap.value;
                                
                                toComponentsDictionary.EnsureCapacity((uint)(toComponentsDictionary.count + (uint)infosToProcess.count));

                                fromComponentsDictionary.SwapEntitiesBetweenDictionaries(
                                    infosToProcess, fromGroup, toGroup, toComponentsDictionary);
                            }
                        }
                    }
                }

                using (sampler.Sample("Execute Swap Callbacks"))
                {
                    foreach (var entitiesToSwap in swapEntitiesOperations)
                    {
                        ExclusiveGroupStruct fromGroup = entitiesToSwap.key;

                        foreach (var groupedEntitiesToSwap in entitiesToSwap.value)
                        {
                            var componentType = groupedEntitiesToSwap.key;

                            foreach (var entitiesInfoToSwap in groupedEntitiesToSwap.value)
                            {
                                ExclusiveGroupStruct toGroup = entitiesInfoToSwap.key;
                                ITypeSafeDictionary toComponentsDictionary =
                                    GetTypeSafeDictionary(toGroup, enginesRoot.GetOrCreateDBGroup(toGroup)
                                                        , componentType);

                                FasterList<(uint, string)> infosToProcess = entitiesInfoToSwap.value;

                                toComponentsDictionary.ExecuteEnginesSwapCallbacks(
                                    infosToProcess, enginesRoot._reactiveEnginesSwap, fromGroup, toGroup, in sampler);
                            }
                        }
                    }
                }
            }
        }

        void AddEntities(PlatformProfiler profiler)
        {
            _groupedEntityToAdd.Swap();

            if (_groupedEntityToAdd.AnyOtherEntityCreated())
            {
                using (profiler.Sample("Add operations"))
                {
                    try
                    {
                        using (profiler.Sample("Add entities to database"))
                        {
                            //each group is indexed by entity view type. for each type there is a dictionary indexed by entityID
                            foreach (var groupToSubmit in _groupedEntityToAdd)
                            {
                                var groupID = groupToSubmit.@group;
                                var groupDB = GetOrCreateDBGroup(groupID);

                                //add the entityComponents in the group
                                foreach (var entityComponentsToSubmit in groupToSubmit.components)
                                {
                                    var type           = entityComponentsToSubmit.key;
                                    var fromDictionary = entityComponentsToSubmit.value;
                                    var wrapper        = new RefWrapperType(type);

                                    var toDictionary = GetOrCreateTypeSafeDictionary(groupID, groupDB, wrapper
                                      , fromDictionary);

                                    //Fill the DB with the entity components generated this frame.
                                    fromDictionary.AddEntitiesToDictionary(toDictionary, groupID, entityLocator);
                                }
                            }
                        }

                        //then submit everything in the engines, so that the DB is up to date with all the entity components
                        //created by the entity built
                        using (profiler.Sample("Add entities to engines"))
                        {
                            foreach (GroupInfo groupToSubmit in _groupedEntityToAdd)
                            {
                                var groupID = groupToSubmit.@group;
                                var groupDB = GetDBGroup(groupID);

                                //This loop iterates again all the entity components that have been just submitted to call
                                //the Add Callbacks on them. Note that I am iterating the transient buffer of the just
                                //added components, but calling the callback on the entities just added in the real buffer
                                //Note: it's OK to add new entities while this happens because of the double buffer
                                //design of the transient buffer of added entities.
                                foreach (var entityComponentsToSubmit in groupToSubmit.components)
                                {
                                    var type           = entityComponentsToSubmit.key;
                                    var fromDictionary = entityComponentsToSubmit.value;

                                    //this contains the total number of components ever submitted in the DB
                                    ITypeSafeDictionary toDictionary = GetTypeSafeDictionary(groupID, groupDB, type);

                                    fromDictionary.ExecuteEnginesAddCallbacks(
                                        _reactiveEnginesAdd, toDictionary, groupID, in profiler);
                                }
                            }
                        }
                    }
                    finally
                    {
                        using (profiler.Sample("clear double buffering"))
                        {
                            //other can be cleared now, but let's avoid deleting the dictionary every time
                            _groupedEntityToAdd.ClearOther();
                        }
                    }
                }
            }
        }

        bool HasMadeNewStructuralChangesInThisIteration()
        {
            return _groupedEntityToAdd.AnyEntityCreated() || _entitiesOperations.AnyOperationCreated();
        }

        void RemoveEntitiesFromGroup(ExclusiveGroupStruct groupID, in PlatformProfiler profiler)
        {
            _entityLocator.RemoveAllGroupReferenceLocators(groupID);

            if (_groupEntityComponentsDB.TryGetValue(groupID, out var dictionariesOfEntities))
            {
                foreach (var dictionaryOfEntities in dictionariesOfEntities)
                {
                    dictionaryOfEntities.value.ExecuteEnginesRemoveGroupCallbacks(
                        _reactiveEnginesRemove, groupID, profiler);
                }

                foreach (var dictionaryOfEntities in dictionariesOfEntities)
                {
                    ClearDictionary(groupID, dictionaryOfEntities);
                }
            }
        }

        void SwapEntitiesBetweenGroups
            (ExclusiveGroupStruct fromGroupId, ExclusiveGroupStruct toGroupId, PlatformProfiler platformProfiler)
        {
            FasterDictionary<RefWrapperType, ITypeSafeDictionary> fromGroup = GetDBGroup(fromGroupId);
            FasterDictionary<RefWrapperType, ITypeSafeDictionary> toGroup   = GetOrCreateDBGroup(toGroupId);

            _entityLocator.UpdateAllGroupReferenceLocators(fromGroupId, toGroupId);

            foreach (var dictionaryOfEntities in fromGroup)
            {
                RefWrapperType refWrapperType = dictionaryOfEntities.key;

                ITypeSafeDictionary fromDictionary = dictionaryOfEntities.value;
                ITypeSafeDictionary toDictionary =
                    GetOrCreateTypeSafeDictionary(toGroupId, toGroup, refWrapperType, fromDictionary);

                fromDictionary.AddEntitiesToDictionary(toDictionary, toGroupId, this.entityLocator);

                ClearDictionary(fromGroupId, dictionaryOfEntities);
            }

            foreach (var dictionaryOfEntities in fromGroup)
            {
                RefWrapperType refWrapperType = dictionaryOfEntities.key;

                ITypeSafeDictionary fromDictionary = dictionaryOfEntities.value;
                ITypeSafeDictionary toDictionary   = GetTypeSafeDictionary(toGroupId, toGroup, refWrapperType);

                if (_reactiveEnginesSwap != null)
                    fromDictionary.ExecuteEnginesSwapGroupCallbacks(_reactiveEnginesSwap, toDictionary, fromGroupId
                                                                  , toGroupId, platformProfiler);

                fromDictionary.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ITypeSafeDictionary GetOrCreateTypeSafeDictionary
        (ExclusiveGroupStruct groupId, FasterDictionary<RefWrapperType, ITypeSafeDictionary> @group
       , RefWrapperType type, ITypeSafeDictionary fromDictionary)
        {
            //be sure that the TypeSafeDictionary for the entity Type exists
            if (@group.TryGetValue(type, out ITypeSafeDictionary toEntitiesDictionary) == false)
            {
                toEntitiesDictionary = fromDictionary.Create();
                @group.Add(type, toEntitiesDictionary);
            }

            //update GroupsPerEntity
            if (_groupsPerEntity.TryGetValue(type, out var groupedGroup) == false)
                groupedGroup = _groupsPerEntity[type] =
                    new FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>();
            groupedGroup[groupId] = toEntitiesDictionary;

            return toEntitiesDictionary;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ITypeSafeDictionary GetTypeSafeDictionary
        (ExclusiveGroupStruct groupID, FasterDictionary<RefWrapperType, ITypeSafeDictionary> @group
       , RefWrapperType refWrapper)
        {
            if (@group.TryGetValue(refWrapper, out ITypeSafeDictionary fromTypeSafeDictionary) == false)
            {
                throw new ECSException("no group found: ".FastConcat(groupID.ToName()));
            }

            return fromTypeSafeDictionary;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ClearDictionary
        (ExclusiveGroupStruct groupID
       , KeyValuePairFast<RefWrapperType, ITypeSafeDictionary, ManagedStrategy<ITypeSafeDictionary>>
             dictionaryOfEntities)
        {
            dictionaryOfEntities.value.Clear();

            _groupsPerEntity[dictionaryOfEntities.key][groupID].Clear();
        }

        readonly DoubleBufferedEntitiesToAdd _groupedEntityToAdd;
        readonly EntitiesOperations          _entitiesOperations;

        static readonly Action<
            FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType,
                FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, string)>>>>, FasterList<(EGID, EGID)>,
            EnginesRoot> _swapEntities;

        static readonly Action<
            FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, FasterList<(uint, string)>>>,
            FasterList<EGID>, EnginesRoot> _removeEntities;

        static readonly Action<ExclusiveGroupStruct, EnginesRoot>                       _removeGroup;
        static readonly Action<ExclusiveGroupStruct, ExclusiveGroupStruct, EnginesRoot> _swapGroup;
    }
}