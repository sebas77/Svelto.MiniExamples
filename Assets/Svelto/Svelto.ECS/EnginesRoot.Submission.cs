using System;
using System.Collections.Generic;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Internal;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        void SubmitEntityViews()
        {
            var profiler = new PlatformProfiler();
            using (profiler.StartNewSession("Svelto.ECS - Entities Submission"))
            {
                if (_entitiesOperations.Count > 0)
                {
                    using (profiler.Sample("Remove and Swap operations"))
                    {
#if DEBUG && !PROFILER
                        _entitiesOperationsDebug.Clear();
#endif
                        _transientEntitiesOperations.FastClear();
                        _transientEntitiesOperations.AddRange(_entitiesOperations);
                        _entitiesOperations.FastClear();

                        var entitiesOperations = _transientEntitiesOperations.ToArrayFast();
                        for (var i = 0; i < _transientEntitiesOperations.Count; i++)
                        {
                            try
                            {
                                switch (entitiesOperations[i].type)
                                {
                                    case EntitySubmitOperationType.Swap:
                                        SwapEntityGroup(entitiesOperations[i].builders,
                                                        entitiesOperations[i].entityDescriptor,
                                                        entitiesOperations[i].fromID,
                                                        entitiesOperations[i].toID);
                                        break;
                                    case EntitySubmitOperationType.Remove:
                                        MoveEntity(entitiesOperations[i].builders,
                                                   entitiesOperations[i].fromID,
                                                   entitiesOperations[i].entityDescriptor, new EGID());
                                        break;
                                    case EntitySubmitOperationType.RemoveGroup:
                                        if (entitiesOperations[i].entityDescriptor == null)
                                            RemoveGroupAndEntitiesFromDB(entitiesOperations[i].fromID.groupID);
                                        else
                                            RemoveGroupAndEntitiesFromDB(entitiesOperations[i].fromID.groupID,
                                                                         entitiesOperations[i].entityDescriptor);

                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                var str = "Crash while executing Entity Operation"
                                   .FastConcat(entitiesOperations[i].type.ToString());
#if RELAXED_ECS
                                Console.LogException(str.FastConcat(" ", entitiesOperations[i].trace), e);
#else
                                throw new ECSException(str.FastConcat(" ")
#if DEBUG && !PROFILER                                                           
                                                          .FastConcat(entitiesOperations[i].trace)
#endif
                                                     , e);
#endif
                            }
                        }
                    }
                }

                if (_groupedEntityToAdd.entitiesCreatedPerGroup.Count > 0)
                {
                    using (profiler.Sample("Add operations"))
                    {
                        //use other as source from now on current will be use to write new entityViews
                        _groupedEntityToAdd.Swap();

                        try
                        {
                            //Note: if N entity of the same type are added on the same frame the Add callback is called
                            //N times on the same frame. if the Add callback builds a new entity, that entity will not
                            //be available in the database until the N callbacks are done. Solving this could be
                            //complicated as callback and database update must be interleaved.
                            AddEntityViewsToTheDBAndSuitableEngines(_groupedEntityToAdd, profiler);
                        }
                        finally
                        {
                            //other can be cleared now, but let's avoid deleting the dictionary every time
                            _groupedEntityToAdd.ClearOther();
                        }
                    }
                }
            }
        }

        void AddEntityViewsToTheDBAndSuitableEngines(
            DoubleBufferedEntitiesToAdd dbgroupsOfEntitiesToSubmit,
            PlatformProfiler profiler)
        {
            //each group is indexed by entity view type. for each type there is a dictionary indexed by entityID
            var groupsOfEntitiesToSubmit = dbgroupsOfEntitiesToSubmit.other;
            foreach (var groupOfEntitiesToSubmit in groupsOfEntitiesToSubmit)
            { 
                var groupID = groupOfEntitiesToSubmit.Key;
                
                if (dbgroupsOfEntitiesToSubmit.entitiesCreatedPerGroup.ContainsKey(groupID) == false) continue;
                
                //if the group doesn't exist in the current DB let's create it first
                if (_groupEntityDB.TryGetValue(groupID, out var groupDB) == false)
                    groupDB = _groupEntityDB[groupID] = new Dictionary<Type, ITypeSafeDictionary>();
                
                //add the entityViews in the group
                foreach (var entityViewsToSubmit in groupOfEntitiesToSubmit.Value)
                {
                    if (groupDB.TryGetValue(entityViewsToSubmit.Key, out var dbDic) == false)
                        dbDic = groupDB[entityViewsToSubmit.Key] = entityViewsToSubmit.Value.Create();

                    if (_groupsPerEntity.TryGetValue(entityViewsToSubmit.Key, out var groupedGroup) == false)
                        groupedGroup = _groupsPerEntity[entityViewsToSubmit.Key] =
                            new FasterDictionary<uint, ITypeSafeDictionary>();

                    //Fill the DB with the entity views generate this frame.
                    dbDic.FillWithIndexedEntities(entityViewsToSubmit.Value);
                    groupedGroup[groupID] = dbDic;
                }
            }

            //then submit everything in the engines, so that the DB is up to date
            //with all the entity views and struct created by the entity built
            foreach (var groupToSubmit in groupsOfEntitiesToSubmit)
            {
                foreach (var entityViewsPerType in groupToSubmit.Value)
                {
                    using (profiler.Sample("Add entities to engines"))
                    {
                        entityViewsPerType.Value.AddEntitiesToEngines(_entityEngines, ref profiler);
                    }
                }
            }
        }

        readonly DoubleBufferedEntitiesToAdd _groupedEntityToAdd;

        readonly IEntitySubmissionScheduler        _scheduler;
        readonly FasterList<EntitySubmitOperation> _transientEntitiesOperations;
        readonly FasterList<EntitySubmitOperation> _entitiesOperations;
    }
}