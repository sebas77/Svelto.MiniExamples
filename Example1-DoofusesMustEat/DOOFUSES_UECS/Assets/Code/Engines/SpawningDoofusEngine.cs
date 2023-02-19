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
    public class SpawningDoofusEngine: IQueryingEntitiesEngine, IJobifiedEngine
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
        public string name => nameof(SpawningDoofusEngine);

        public void Ready() { }

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

            EntitiesDB.SveltoFilters sveltoNativeFilters = entitiesDB.GetFilters();

            //We know that if the entity is created in the BLUE_DOOFUSES_NOT_EATING group, 
            //SpawnPointEntityComponent and PositionEntityComponent elements will be aligned
            //and indexable with the same index.
            var specialBlueFilters = sveltoNativeFilters.GetOrCreatePersistentFilter<PositionEntityComponent>(
                GameFilters.SPECIAL_BLUE_DOOFUSES_MESHES);
            var blueFilters = sveltoNativeFilters.GetOrCreatePersistentFilter<PositionEntityComponent>(GameFilters.BLUE_DOOFUSES_MESHES);

            var specialBlueFilter = specialBlueFilters.GetOrCreateGroupFilter(blueDoofusesNotEating);
            var blueFilter = blueFilters.GetOrCreateGroupFilter(blueDoofusesNotEating);

            var spawnRed = new SpawningJob()
            {
                _group = GameGroups.RED_DOOFUSES_NOT_EATING.BuildGroup,
                _factory = _factory,
                _prefabID = _redCapsule,
                _random = new Random(1234567)
            }.ScheduleParallel(MaxNumberOfDoofuses, _jobHandle);

            //Adding the complexity of the special blue capsule to show how to use filters to handle different
            //entity states in the same group.

//            var spawnBlue = new SpawningJob()
//            {
//                _group = blueDoofusesNotEating,
//                _factory = _factory,
//                _prefabID = _blueCapsule,
//                _random = new Random(7654321),
//            }.ScheduleParallel(MaxNumberOfDoofuses / 2, _jobHandle);

//            spawnBlue = new SetFiltersJob()
//            {
//                filter = blueFilter,
//                entityIDs = _factory,
//                start = _blueCapsule,
//            }.Schedule(MaxNumberOfDoofuses / 2, spawnBlue);

//            var spawnSpecialBlue = new SpawningJob()
//            {
//                _group = blueDoofusesNotEating,
//                _factory = _factory,
//                _prefabID = _specialBlueCapsule,
//                _random = new Random(7651),
//            }.ScheduleParallel(MaxNumberOfDoofuses / 2, _jobHandle);

//            spawnSpecialBlue = new SetFiltersJob()
//            {
//                filter = specialBlueFilter,
//                entityIDs = _factory,
//                start = _blueCapsule,
//            }.Schedule(MaxNumberOfDoofuses / 2, spawnBlue);

            //Yeah this shouldn't be solved like this, but I keep it in this way for simplicity sake 
            _done = true;

            return JobHandle.CombineDependencies(default, spawnRed, default);
        }

        readonly NativeEntityFactory _factory;
        readonly Entity _redCapsule, _blueCapsule, _specialBlueCapsule;

        const int MaxNumberOfDoofuses = 10000;

        bool _done;

//        [BurstCompile]
//        struct SetFiltersJob: IJob
//        {
//            public EntityFilterCollection.GroupFilters filter;
//            public NativeEntityIDs entityIDs;
//            public uint start;
//
//            public void Execute()
//            {
//                index = (int)(index + start);
//
//                //This filter already know the group, so it needs only the entityID, plus the position
//                //of the entity in the array.
//                filter.Add(entityIDs[index], (uint)index);
//            }
//        }

        [BurstCompile]
        struct SpawningJob: IJobParallelFor
        {
            internal NativeEntityFactory _factory;
            internal Entity _prefabID;
            internal ExclusiveBuildGroup _group;
            internal Random _random;

#pragma warning disable 649
            //thread index is necessary to build entity in parallel in Svelto ECS
            [NativeSetThreadIndex] int _threadIndex;
#pragma warning restore 649

            public void Execute(int index)
            {
                var egid = new EGID((uint)index, _group);
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