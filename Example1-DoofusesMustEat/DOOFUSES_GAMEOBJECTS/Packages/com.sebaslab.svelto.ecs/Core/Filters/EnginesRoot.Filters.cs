using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        void InitFilters()
        {
            _transientEntityFilters  = new SharedSveltoDictionaryNative<long, EntityFilterCollection>(0);
            _persistentEntityFilters = new SharedSveltoDictionaryNative<long, EntityFilterCollection>(0);
            _indicesOfPersistentFiltersUsedByThisComponent =
                new SharedSveltoDictionaryNative<NativeRefWrapperType, NativeDynamicArrayCast<int>>(0);
        }

        void DisposeFilters()
        {
            foreach (var filter in _transientEntityFilters)
            {
                filter.value.Dispose();
            }

            foreach (var filter in _persistentEntityFilters)
            {
                filter.value.Dispose();
            }

            foreach (var filter in _indicesOfPersistentFiltersUsedByThisComponent)
            {
                filter.value.Dispose();
            }

            _transientEntityFilters.Dispose();
            _persistentEntityFilters.Dispose();
            _indicesOfPersistentFiltersUsedByThisComponent.Dispose();
        }

        void ClearTransientFilters()
        {
            foreach (var filter in _transientEntityFilters)
            {
                filter.value.Clear();
            }
        }

        void RemoveEntitiesFromPersistentFilters
        (FasterList<(uint, string)> entityIDsRemoved, ExclusiveGroupStruct fromGroup, RefWrapperType refWrapperType
       , ITypeSafeDictionary fromDic, FasterList<uint> entityIDsLeftAndAffectedByRemoval)
        {
            //is there any filter used by this component?
            if (_indicesOfPersistentFiltersUsedByThisComponent.TryGetValue(
                    new NativeRefWrapperType(refWrapperType), out NativeDynamicArrayCast<int> listOfFilters) == true)
            {
                var numberOfFilters = listOfFilters.count;
                var filters         = _persistentEntityFilters.unsafeValues;
                
                //remove duplicates
                _entityIDsLeftWithoutDuplicates.FastClear();
                var entityAffectedCount = entityIDsLeftAndAffectedByRemoval.count;
                for (int i = 0; i < entityAffectedCount; i++)
                {
                    _entityIDsLeftWithoutDuplicates[entityIDsLeftAndAffectedByRemoval[i]] = -1;
                }

                for (int filterIndex = 0; filterIndex < numberOfFilters; ++filterIndex)
                {
                    //we are going to remove multiple entities, this means that the dictionary count would decrease
                    //for each entity removed from each filter
                    //we need to keep a copy to reset to the original count for each filter
                    var persistentFiltersPerGroup = filters[listOfFilters[filterIndex]]._filtersPerGroup;

                    if (persistentFiltersPerGroup.TryGetValue(fromGroup, out var fromGroupFilter))
                    {
                        var entitiesCount = entityIDsRemoved.count;

                        //foreach entity to remove
                        for (int entityIndex = 0; entityIndex < entitiesCount; ++entityIndex)
                        {
                            //the current entity id to remove
                            uint fromEntityID = entityIDsRemoved[entityIndex].Item1;

                            fromGroupFilter.Remove(
                                fromEntityID); //Remove works even if the ID is not found (just returns false)
                        }

                        foreach (var entity in _entityIDsLeftWithoutDuplicates)
                        {
                            if (fromGroupFilter.Exists(entity.key))
                            {
                                if (entity.value == -1)
                                    entity.value = (int)fromDic.GetIndex(entity.key);
                                
                                fromGroupFilter._entityIDToDenseIndex[entity.key] = (uint) entity.value;
                            }
                        }
                    }
                }
            }
        }

        //this method is called by the framework only if listOfFilters.count > 0
        void SwapEntityBetweenPersistentFilters
        (FasterList<(uint, uint, string)> fromEntityToEntityIDs, ITypeSafeDictionary fromDic
       , ITypeSafeDictionary toDic, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup
       , RefWrapperType refWrapperType, FasterList<uint> entityIDsLeftAndAffectedByRemoval)
        {
            //is there any filter used by this component?
            if (_indicesOfPersistentFiltersUsedByThisComponent.TryGetValue(
                    new NativeRefWrapperType(refWrapperType), out NativeDynamicArrayCast<int> listOfFilters) == true)
            {
                DBC.ECS.Check.Require(listOfFilters.count > 0, "why are you calling this with an empty list?");
                var numberOfFilters = listOfFilters.count;
                
                //remove duplicates
                _entityIDsLeftWithoutDuplicates.FastClear();
                var entityAffectedCount = entityIDsLeftAndAffectedByRemoval.count;
                for (int i = 0; i < entityAffectedCount; i++)
                {
                    _entityIDsLeftWithoutDuplicates[entityIDsLeftAndAffectedByRemoval[i]] = -1;
                }

                /// fromEntityToEntityIDs are the IDs of the entities to swap from the from group to the to group.
                /// for this component type. for each component type, there is only one set of fromEntityToEntityIDs
                /// per from/to group.
                for (int filterIndex = 0; filterIndex < numberOfFilters; ++filterIndex)
                {
                    //if the group has a filter linked:
                    EntityFilterCollection persistentFilter =
                        _persistentEntityFilters.unsafeValues[listOfFilters[filterIndex]];
                    if (persistentFilter._filtersPerGroup.TryGetValue(fromGroup, out var fromGroupFilter))
                    {
                        EntityFilterCollection.GroupFilters groupFilterTo = default;

                        foreach (var (fromEntityID, toEntityID, _) in fromEntityToEntityIDs)
                        {
                            //if there is an entity, it must be moved to the to filter
                            if (fromGroupFilter.Exists(fromEntityID) == true)
                            {
                                var toIndex = toDic.GetIndex(toEntityID);

                                if (groupFilterTo.isValid == false)
                                    groupFilterTo = persistentFilter.GetOrCreateGroupFilter(toGroup);

                                groupFilterTo.Add(toEntityID, toIndex);
                            }
                        }

                        foreach (var (fromEntityID, _, _) in fromEntityToEntityIDs)
                        {
                            fromGroupFilter.Remove(fromEntityID); //Remove works even if the ID is not found (just returns false)
                        }

                        foreach (var entity in _entityIDsLeftWithoutDuplicates)
                        {
                            if (fromGroupFilter.Exists(entity.key))
                            {
                                if (entity.value == -1)
                                    entity.value = (int)fromDic.GetIndex(entity.key);
                                
                                fromGroupFilter._entityIDToDenseIndex[entity.key] = (uint) entity.value;
                            }
                        }
                    }
                }
            }
        }

        internal SharedSveltoDictionaryNative<long, EntityFilterCollection> _transientEntityFilters;
        internal SharedSveltoDictionaryNative<long, EntityFilterCollection> _persistentEntityFilters;

        internal SharedSveltoDictionaryNative<NativeRefWrapperType, NativeDynamicArrayCast<int>>
            _indicesOfPersistentFiltersUsedByThisComponent;
    }
}