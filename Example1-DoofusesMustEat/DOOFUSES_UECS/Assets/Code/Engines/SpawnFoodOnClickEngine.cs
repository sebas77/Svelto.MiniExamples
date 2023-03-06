using System;
using System.Collections;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Native;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Svelto.ECS.MiniExamples.DoofusesDOTS
{
    /// <summary>
    /// This is a SveltoOnDOTS structural engine. It can be used to create Svelto entities and DOTS entities
    /// It could have been split in two engines, so indeed it has a double responsibility. In fact it is not entirely necessary
    /// to build svelto entities and DOTS entities in the same engine.
    /// DOTS entities can be build by a ISveltoOnDOTSStructuralEngine during the Add callbacks (this is the expected pattern)
    /// </summary>
    [Sequenced(nameof(DoofusesEngineNames.PlaceFoodOnClickEngine))]
    public class SpawnFoodOnClickEngine: ISveltoOnDOTSStructuralEngine, IQueryingEntitiesEngine, IJobifiedEngine,  IReactOnAddEx<DOTSEntityComponent>
    {
        const int MaxMeals = 500;

        public SpawnFoodOnClickEngine(Entity redfood, Entity bluefood, IEntityFactory entityFactory)
        {
            //read why I am using a Native Factory here in the Execute method
            _entityFactory = entityFactory.ToNative<FoodEntityDescriptor>("SpawnFoodOnClickEngine");
            _redfood = redfood;
            _bluefood = bluefood;
        }

        public DOTSOperationsForSvelto DOTSOperations { get; set; }
        public string name => nameof(SpawnFoodOnClickEngine);

        IEnumerator CheckClick()
        {
            _inputDeps = default;

            while (true)
            {
                //note: in a complex project an engine shouldn't ever poll input directly, it should instead poll
                //entity states
                if (Input.GetMouseButton(0) || Input.GetMouseButton(1) == true)
                {
                    //I am cheating a bit with the MouseToPosition function, but for the purposes of this demo
                    //creating a Camera Entity was an overkill
                    if (UnityUtilities.MouseToPosition(out Vector3 position))
                    {
                        if (Input.GetMouseButton(0))
                        {
                            _inputDeps =
                                    new SpawnFoodJob(
                                        position, _entityFactory, GameGroups.RED_FOOD_NOT_EATEN.BuildGroup
                                      , _redfood, _foodPlaced).ScheduleParallel(MaxMeals, _inputDeps);
                        }
                        else
                        {
                            _inputDeps =
                                    new SpawnFoodJob(
                                        position, _entityFactory, GameGroups.BLUE_FOOD_NOT_EATEN.BuildGroup
                                      , _bluefood, _foodPlaced).ScheduleParallel(MaxMeals, _inputDeps);
                        }

                        _foodPlaced += MaxMeals;

                        var now = DateTime.Now;
                        while ((DateTime.Now - now).TotalMilliseconds < 100)
                            yield return null;
                    }
                }

                yield return null;
            }
        }

        public EntitiesDB entitiesDB { private get; set; }

        public void Ready()
        {
            _taskRunner = CheckClick();
        }
        
        //Collect all the FOOD Svelto DOTSEntityComponents submitted this frame and create a DOTS entity for each of them
        //We also initialise the DOTS position after the entities are created
        public void Add((uint start, uint end) rangeOfEntities,
            in EntityCollection<DOTSEntityComponent> entities, ExclusiveGroupStruct groupID)
        {
            if (GameGroups.FOOD.Includes(groupID) == true)
            {
                var (sveltoOnDOTSEntities, ids, _) = entities;
                var (positions, _) = entitiesDB.QueryEntities<PositionEntityComponent>(groupID);

                InitDOTSFoodPositionJob job = default;
                job.spawnPoints = positions;
                job.sveltoStartIndex = rangeOfEntities.start;
                job.entityManager = DOTSOperations;

                using (new PlatformProfiler("CreateDOTSEntityOnSveltoBatched"))
                {
                    //Standard way (SveltoOnDOTS pattern) to create DOTS entities from a Svelto ones. The returning job must be completed by the end of the frame
                    var DOTSEntities = DOTSOperations.CreateDOTSEntityFromSveltoBatched(sveltoOnDOTSEntities[0].dotsEntity, rangeOfEntities, groupID, sveltoOnDOTSEntities, ids, out var jobHandle);

                    job.createdEntities = DOTSEntities;
                    
                    //run custom job to initialise the DOTS position
                    jobHandle = job.ScheduleParallel(job.createdEntities.Length, jobHandle);
                    
                    //don't forget to add the job to the list of jobs to complete at the end of the frame
                    DOTSOperations.AddJobToComplete(jobHandle);
                }
            }
        }

        public void OnOperationsReady()
        {
        }

        public void OnPostSubmission()
        {
        }
        
        public JobHandle Execute(JobHandle inputDeps)
        {
            _inputDeps = inputDeps;

            _taskRunner.MoveNext();

            return _inputDeps;
        }

        static IEnumerator _taskRunner;

        readonly Entity _redfood;
        readonly Entity _bluefood;
        uint _foodPlaced;

        readonly NativeEntityFactory _entityFactory;
        JobHandle _inputDeps;

        [BurstCompile]
        struct SpawnFoodJob: IJobParallelFor
        {
            readonly Vector3 _position;
            readonly NativeEntityFactory _entityFactory;
            readonly ExclusiveBuildGroup _exclusiveBuildGroup;
            readonly Entity _prefabID;
            readonly uint _foodPlaced;
            Random _random;
            [NativeSetThreadIndex] readonly int _threadIndex;

            public SpawnFoodJob(Vector3 position, NativeEntityFactory factory, ExclusiveBuildGroup exclusiveBuildGroup, Entity prefabID
              , uint foodPlaced): this()
            {
                _position = position;
                _entityFactory = factory;
                _exclusiveBuildGroup = exclusiveBuildGroup;
                _prefabID = prefabID;
                _foodPlaced = foodPlaced;
                _random = new Random(foodPlaced + 1);
            }

            public void Execute(int index)
            {
                var randX = _position.x + _random.NextFloat(-50, 50);
                var randZ = _position.z + _random.NextFloat(-50, 50);
                var newposition = new float3(randX, _position.y, randZ);

                //BuildEntity returns an EntityInitialized that is used to set the default values of the
                //entity that will be built.
                var init = _entityFactory.BuildEntity(new EGID((uint)(_foodPlaced + index), _exclusiveBuildGroup), _threadIndex);

                init.Init(new DOTSEntityComponent(_prefabID));
                init.Init(
                    new PositionEntityComponent
                    {
                        position = newposition
                    });
            }
        }

        /// <summary>
        /// Set position of the DOTS entities after they have been created. This because food doesn't move and so a sync engine for food
        /// doesn't run. This is a very specific case, in general you should use a sync engine to set the position of the DOTS entities
        /// </summary>
        [BurstCompile]
        struct InitDOTSFoodPositionJob: IJobParallelFor
        {
            public NB<PositionEntityComponent> spawnPoints;
            public uint sveltoStartIndex;
            [ReadOnly] public NativeArray<Entity> createdEntities;
            [NativeDisableParallelForRestriction] public DOTSOperationsForSvelto entityManager;

            public void Execute(int currentIndex)
            {
                int index = (int)(sveltoStartIndex + currentIndex);
                var dotsEntity = createdEntities[currentIndex];

                ref PositionEntityComponent spawnComponent = ref spawnPoints[index];

                entityManager.SetComponent(
                    dotsEntity, new Translation()
                    {
                        Value = spawnComponent.position
                    });
            }
        }
    }
}