using System;
using Svelto.Common;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Internal;
using Svelto.ECS.Native;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
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
    [Sequenced(nameof(DoofusesEngineNames.SpawningDoofusEngine))]
    public class SpawningDoofusEngine: ISveltoOnDOTSStructuralEngine, IQueryingEntitiesEngine, IJobifiedEngine, IReactOnAddEx<DOTSEntityComponent>
    {
        public SpawningDoofusEngine(Entity redCapsule, Entity blueCapsule, Entity specialBlueCapsule,
            IEntityFactory factory)
        {
            _redCapsule = redCapsule;
            _blueCapsule = blueCapsule;
            _specialBlueCapsule = specialBlueCapsule;
            _factory = factory.ToNative<DoofusEntityDescriptor>(nameof(SpawningDoofusEngine));
        }

        public EntitiesDB entitiesDB { get; set; }

        public DOTSOperationsForSvelto DOTSOperations { get; set; }

        public string name => nameof(SpawningDoofusEngine);

        public void Ready()
        {
            EntitiesDB.SveltoFilters sveltoNativeFilters = entitiesDB.GetFilters();

            //Create filter with ID SPECIAL_BLUE_DOOFUSES_MESHES linked to the component PositionEntityComponent
            var specialBlueFilters = sveltoNativeFilters.CreatePersistentFilter<PositionEntityComponent>(GameFilters.SPECIAL_BLUE_DOOFUSES_MESHES);
            var blueFilters = sveltoNativeFilters.CreatePersistentFilter<PositionEntityComponent>(GameFilters.BLUE_DOOFUSES_MESHES);

            //Create a group filter with ID BLUE_DOOFUSES_NOT_EATING linked to the group GameGroups.BLUE_DOOFUSES_NOT_EATING
            //now it is possible to iterate only on the filtered entities that are part of the group
            _specialBlueFilter = specialBlueFilters.CreateGroupFilter(GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup);
            _blueFilter = blueFilters.CreateGroupFilter(GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup);
        }

        /// <summary>
        /// This job will create the Svelto Entities, the DOTS entites are created on reaction inside
        /// SpawnUnityEntityOnSveltoEntityEngine
        /// </summary>
        /// <param name="_jobHandle"></param>
        /// <returns></returns>
        public JobHandle Execute(JobHandle _jobHandle)
        {
            if (_done == true)
                return _jobHandle;

            ExclusiveBuildGroup blueDoofusesNotEating = GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup;

            var spawnRed = new SpawningJob()
            {
                _group = GameGroups.RED_DOOFUSES_NOT_EATING.BuildGroup,
                _factory = _factory,
                _random = new Random(1234567)
            }.Schedule(MaxNumberOfDoofuses, _jobHandle);

            var spawnBlue = new SpawningJob()
            {
                _group = blueDoofusesNotEating,
                _factory = _factory,
                _random = new Random(7654321),
            }.Schedule(MaxNumberOfDoofuses, _jobHandle);

            //Yeah this shouldn't be solved like this, but I keep it in this way for simplicity sake 
            _done = true;

            return JobHandle.CombineDependencies(spawnBlue, spawnRed);
        }

        /// <summary>
        /// Now that entities have been submitted to the Svelto DB, we can set the filters if required. We want to subgroup the
        /// BLUE_DOOFUSES_NOT_EATING group in two subgroups, one for the blue doofuses and one for the special blue doofuses.
        /// Cool thing about persistent filter is that they are updated when entities swap groups. Special blue doofuses will follow the same
        /// entities regardless in which groups they are found at any point (same for blue).
        /// </summary>
        public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<DOTSEntityComponent> entities, ExclusiveGroupStruct groupID)
        {
            if (GameGroups.DOOFUSES.Includes(groupID) == true)
            {
                var (sveltoOnDOTSEntities, entityIDs, _) = entities;

                //if it's a blue doofus belonging to a blue group (whatever the adjectives/states are)
                if (GameGroups.BLUE_DOOFUSES.Includes(groupID))
                {
                    uint blueDoofusesCount = (uint)Mathf.CeilToInt(((rangeOfEntities.end - rangeOfEntities.start) * 3.0f) / 4.0f);
                    uint specialBlueDoofusesCount = (uint)Mathf.FloorToInt((rangeOfEntities.end - rangeOfEntities.start) / 4.0f);

                    //part of the blue stays normal blue
                    DOTSOperations.CreateDOTSEntityFromSveltoBatched(
                        _blueCapsule, (0, blueDoofusesCount), groupID, sveltoOnDOTSEntities, entityIDs,
                        out var creationJobHandleBlue);

                    JobHandle creationJobHandleSpecialBlue = default;
                    if (specialBlueDoofusesCount > 0)
                    {
                        //the other part of the blue becomes special blue
                        DOTSOperations.CreateDOTSEntityFromSveltoBatched(
                            _specialBlueCapsule, (blueDoofusesCount, blueDoofusesCount + specialBlueDoofusesCount), groupID, sveltoOnDOTSEntities, entityIDs, out creationJobHandleSpecialBlue);
                    }
                    
                    var combined = JobHandle.CombineDependencies(creationJobHandleBlue, creationJobHandleSpecialBlue);

                    //Put the svelto blue doofuses in the blue svelto filter
                    DOTSOperations.AddJobToComplete(
                        new SetFiltersJob()
                        {
                            filter = _blueFilter,
                            length = blueDoofusesCount,
                            entityIDs = entityIDs
                        }.Schedule(combined));

                    //Put the svelto blue doofuses in the special blue svelto filter
                    DOTSOperations.AddJobToComplete(
                        new SetFiltersJob()
                        {
                            filter = _specialBlueFilter,
                            start = blueDoofusesCount,
                            length = specialBlueDoofusesCount,
                            entityIDs = entityIDs
                        }.Schedule(combined));
                }
                else
                {
                    //Standard way to create DOTS entities from a Svelto ones. The returning job must be completed by the end of the frame
                    DOTSOperations.CreateDOTSEntityFromSveltoBatched(
                        _redCapsule, rangeOfEntities, groupID, sveltoOnDOTSEntities, entityIDs,
                        out _);
                }
            }
        }

        public void OnOperationsReady()
        {
        }

        public void OnPostSubmission() { }

        readonly NativeEntityFactory _factory;
        readonly Entity _redCapsule, _blueCapsule, _specialBlueCapsule;

        const int MaxNumberOfDoofuses = 10000;

        bool _done;
        EntityFilterCollection.GroupFilters _specialBlueFilter;
        EntityFilterCollection.GroupFilters _blueFilter;
        ISveltoOnDOTSStructuralEngine _sveltoOnDotsStructuralEngineImplementation;
        DOTSOperationsForSvelto _dotsOperations;

        [BurstCompile]
        struct SetFiltersJob: IJob
        {
            public EntityFilterCollection.GroupFilters filter;
            public uint length;
            public NativeEntityIDs entityIDs;
            public uint start;

            public void Execute()
            {
                var end = (length - 1) + start;
                for (int index = (int)end; index >= start; index--)
                        //This filter already know the group, so it needs only the entityID, plus the position
                        //of the entity in the array.
                    filter.Add(entityIDs[index], (uint)index);
            }
        }

        [BurstCompile]
        struct SpawningJob: IJobFor
        {
            internal NativeEntityFactory _factory;
            internal ExclusiveBuildGroup _group;

            internal Random _random;

#pragma warning disable 649
            //thread index is necessary to build entity in parallel in Svelto ECS
            [NativeSetThreadIndex] int _threadIndex;
#pragma warning restore 649

            public void Execute(int index)
            {
                var egid = new EGID((uint)(index), _group);
                var init = _factory.BuildEntity(egid, _threadIndex);

                //Just created svelto entity components can be set before submissions
                init.Init(
                    new PositionEntityComponent
                    {
                        position = new float3(_random.NextFloat(0.0f, 80.0f), 0, _random.NextFloat(0.0f, 80.0f))
                    });
                init.Init(
                    new SpeedEntityComponent
                    {
                        speed = _random.NextFloat(0.1f, 1.0f)
                    });
            }
        }
    }
}