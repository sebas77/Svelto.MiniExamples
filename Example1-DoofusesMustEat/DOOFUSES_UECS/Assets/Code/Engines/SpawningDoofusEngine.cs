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
using Random = Unity.Mathematics.Random;

namespace Svelto.ECS.MiniExamples.DoofusesDOTS
{
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

        DOTSOperationsForSvelto ISveltoOnDOTSStructuralEngine.DOTSOperations { get; set; }

        public string name => nameof(SpawningDoofusEngine);

        public void Ready()
        {
            EntitiesDB.SveltoFilters sveltoNativeFilters = entitiesDB.GetFilters();

            var specialBlueFilters = sveltoNativeFilters.GetOrCreatePersistentFilter<PositionEntityComponent>(GameFilters.SPECIAL_BLUE_DOOFUSES_MESHES);
            var blueFilters = sveltoNativeFilters.GetOrCreatePersistentFilter<PositionEntityComponent>(GameFilters.BLUE_DOOFUSES_MESHES);

            _specialBlueFilter = specialBlueFilters.GetOrCreateGroupFilter(GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup);
            _blueFilter = blueFilters.GetOrCreateGroupFilter(GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup);
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
                _prefabID = _redCapsule,
                _startID = 0,
                _random = new Random(1234567)
            }.ScheduleParallel(MaxNumberOfDoofuses, _jobHandle);

            uint halfNumberOfDoofuses = MaxNumberOfDoofuses / 2;
            var spawnBlue = new SpawningJob()
            {
                _group = blueDoofusesNotEating,
                _factory = _factory,
                _prefabID = _blueCapsule,
                _startID = 0,
                _random = new Random(7654321),
            }.ScheduleParallel(halfNumberOfDoofuses, _jobHandle);

            var spawnSpecialBlue = new SpawningJob()
            {
                _group = blueDoofusesNotEating,
                _factory = _factory,
                _prefabID = _specialBlueCapsule,
                _startID = halfNumberOfDoofuses,
                _random = new Random((uint)DateTime.Now.Ticks),
            }.ScheduleParallel(halfNumberOfDoofuses, _jobHandle);

            //Yeah this shouldn't be solved like this, but I keep it in this way for simplicity sake 
            _done = true;

            return JobHandle.CombineDependencies(spawnBlue, spawnRed, spawnSpecialBlue);
        }
        
        /// <summary>
        /// Now that entities have been submitted to the Svelto DB, we can set the filters if required. We want to subgroup the
        /// BLUE_DOOFUSES_NOT_EATING group in two subgroups, one for the blue doofuses and one for the special blue doofuses.
        /// Cool thing about persistent filter is that they are updated when entities swap groups. Special blue doofuses will follow the same
        /// entities regardless in which groups they are found at any point (same for blue).
        /// </summary>
        public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<DOTSEntityComponent> entities, ExclusiveGroupStruct groupID)
        {
            if (groupID == GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup)
            {
                var (buffer, entityIDs, _) = entities;

                new SetFiltersJob()
                {
                    filter = buffer[0].dotsEntity == _blueCapsule ? _blueFilter : _specialBlueFilter,
                    length = (int)(rangeOfEntities.end - rangeOfEntities.start),
                    entityIDs = entityIDs
                }.Run();
            }
        }

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
            public int length;
            public NativeEntityIDs entityIDs;

            public void Execute()
            {
                for (int index = length - 1; index >= 0; index--)
                        //This filter already know the group, so it needs only the entityID, plus the position
                        //of the entity in the array.
                    filter.Add(entityIDs[index], (uint)index);
            }
        }

        [BurstCompile]
        struct SpawningJob: IJobParallelFor
        {
            internal NativeEntityFactory _factory;
            internal Entity _prefabID;
            internal ExclusiveBuildGroup _group;
            internal Random _random;
            //The blue and special blue doofuses are built in the same group. Entities cannot have the same ID in the same group. so
            //we need to offset the ID of the second group
            internal uint _startID;

#pragma warning disable 649
            //thread index is necessary to build entity in parallel in Svelto ECS
            [NativeSetThreadIndex] int _threadIndex;
#pragma warning restore 649

            public void Execute(int index)
            {
                var egid = new EGID((uint)(_startID + index), _group);
                var init = _factory.BuildEntity(egid, _threadIndex);

                init.Init(new DOTSEntityComponent(_prefabID));
                init.Init(
                    new PositionEntityComponent
                    {
                        position = new float3(_random.NextFloat(0.0f, 40.0f), 0, _random.NextFloat(0.0f, 40.0f))
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