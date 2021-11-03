using System;
using System.Collections.Generic;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        /// <summary>
        /// Todo: it would be probably better to split even further the logic between submission and callbacks
        /// Something to do when I will optimize the callbacks
        /// </summary>
        /// <param name="profiler"></param>
        /// <param name="maxNumberOfOperations"></param>
        IEnumerator<bool> SingleSubmission(PlatformProfiler profiler)
        {
            while (true)
            {
                DBC.ECS.Check.Require(_maxNumberOfOperationsPerFrame > 0);

                ClearChecks();

                uint numberOfOperations = 0;

                if (_entitiesOperations.count > 0)
                {
                    using (var sample = profiler.Sample("Remove and Swap operations"))
                    {
                        _transientEntitiesOperations.FastClear();
                        _entitiesOperations.CopyValuesTo(_transientEntitiesOperations);
                        _entitiesOperations.FastClear();

                        EntitySubmitOperation[] entitiesOperations =
                            _transientEntitiesOperations.ToArrayFast(out var count);

                        for (var i = 0; i < count; i++)
                        {
                            try
                            {
                                switch (entitiesOperations[i].type)
                                {
                                    case EntitySubmitOperationType.Swap:
                                        MoveEntityFromAndToEngines(entitiesOperations[i].builders
                                                                 , entitiesOperations[i].fromID
                                                                 , entitiesOperations[i].toID);
                                        break;
                                    case EntitySubmitOperationType.Remove:
                                        MoveEntityFromAndToEngines(entitiesOperations[i].builders
                                                                 , entitiesOperations[i].fromID, null);
                                        break;
                                    case EntitySubmitOperationType.RemoveGroup:
                                        RemoveEntitiesFromGroup(entitiesOperations[i].fromID.groupID, profiler);
                                        break;
                                    case EntitySubmitOperationType.SwapGroup:
                                        SwapEntitiesBetweenGroups(entitiesOperations[i].fromID.groupID
                                                                , entitiesOperations[i].toID.groupID, profiler);
                                        break;
                                }
                            }
                            catch
                            {
                                var str = "Crash while executing Entity Operation ".FastConcat(
                                    entitiesOperations[i].type.ToString());

                                Svelto.Console.LogError(str.FastConcat(" ")
#if DEBUG && !PROFILE_SVELTO
                                                           .FastConcat(entitiesOperations[i].trace.ToString())
#endif
                                );

                                throw;
                            }

                            ++numberOfOperations;

                            if (numberOfOperations >= _maxNumberOfOperationsPerFrame)
                            {
                                using (sample.Yield())
                                    yield return true;

                                numberOfOperations = 0;
                            }
                        }
                    }
                }

                _groupedEntityToAdd.Swap();

                if (_groupedEntityToAdd.AnyOtherEntityCreated())
                {
                    using (var outerSampler = profiler.Sample("Add operations"))
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
                                        var type                     = entityComponentsToSubmit.Key;
                                        var targetTypeSafeDictionary = entityComponentsToSubmit.Value;
                                        var wrapper                  = new RefWrapperType(type);

                                        var dbDic = GetOrCreateTypeSafeDictionary(
                                            groupID, groupDB, wrapper, targetTypeSafeDictionary);

                                        //Fill the DB with the entity components generated this frame.
                                        dbDic.AddEntitiesFromDictionary(targetTypeSafeDictionary, groupID, this);
                                    }
                                }
                            }

                            //then submit everything in the engines, so that the DB is up to date with all the entity components
                            //created by the entity built
                            using (var sampler = profiler.Sample("Add entities to engines"))
                            {
                                uint totalCountSoFar = 0;
                                foreach (GroupInfo groupToSubmit in _groupedEntityToAdd)
                                {
                                    var groupID = groupToSubmit.group;
                                    var groupDB = GetDBGroup(groupID);

                                    //This loop iterates again all the entity components that have been just submitted to call
                                    //the Add Callbacks on them. Note that I am iterating the transient buffer of the just
                                    //added components, but calling the callback on the entities just added in the real buffer
                                    //Note: it's OK to add new entities while this happens because of the double buffer
                                    //design of the transient buffer of added entities.
                                    foreach (var componentsArrayToSubmit in groupToSubmit.components)
                                    {
                                        ITypeSafeDictionary databaseDictionaryOfComponents =
                                            groupDB[new RefWrapperType(componentsArrayToSubmit.Key)];

                                        uint startIndex                  = 0;

                                        do 
                                        {
                                            totalCountSoFar += (uint)componentsArrayToSubmit.Value.count;

                                            uint count;
                                            if (totalCountSoFar >= _maxNumberOfOperationsPerFrame)
                                            {
                                                count           = totalCountSoFar - _maxNumberOfOperationsPerFrame;
                                                totalCountSoFar = _maxNumberOfOperationsPerFrame;
                                            }
                                            else
                                            {
                                                count = (uint)componentsArrayToSubmit.Value.count;
                                            }

                                            if (count > 0)
                                            {
                                                componentsArrayToSubmit.Value.ExecuteEnginesAddCallbacks(
                                                    startIndex, count, _reactiveEnginesAddRemove
                                                  , databaseDictionaryOfComponents, groupID, in profiler);
                                            }

                                            if (totalCountSoFar == _maxNumberOfOperationsPerFrame)
                                            {
                                                using (outerSampler.Yield())
                                                using (sampler.Yield())
                                                {
                                                    yield return true;
                                                }

                                                totalCountSoFar = 0;
                                            }

                                            if (startIndex + count < componentsArrayToSubmit.Value.count)
                                                startIndex += count;
                                            else
                                                break;
                                        } while (true);
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

                yield return false;
            }
        }

        bool HasMadeNewStructuralChangesInThisIteration()
        {
            return _groupedEntityToAdd.AnyEntityCreated() || _entitiesOperations.count > 0;
        }

        readonly DoubleBufferedEntitiesToAdd                    _groupedEntityToAdd;
        readonly FasterDictionary<ulong, EntitySubmitOperation> _entitiesOperations;
        readonly FasterList<EntitySubmitOperation>              _transientEntitiesOperations;
        uint                                                    _maxNumberOfOperationsPerFrame;
    }
}