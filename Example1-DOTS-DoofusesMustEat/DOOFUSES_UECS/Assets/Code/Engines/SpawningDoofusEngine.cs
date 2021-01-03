using Svelto.Common;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Extensions.Unity;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [Sequenced(nameof(DoofusesEngineNames.SpawningDoofusEngine))]
    public class SpawningDoofusEngine : IQueryingEntitiesEngine, IJobifiedEngine
    {
        public SpawningDoofusEngine(Entity redCapsule, Entity blueCapsule, IEntityFactory factory)
        {
            _redCapsule  = redCapsule;
            _blueCapsule = blueCapsule;
            _factory     = factory.ToNative<DoofusEntityDescriptor>(nameof(SpawningDoofusEngine));
        }

        public EntitiesDB entitiesDB { get; set; }
        public string     name       => nameof(SpawningDoofusEngine);

        public void Ready() { }

        public JobHandle Execute(JobHandle _jobHandle)
        {
            if (_done == true)
                return _jobHandle;

            var spawnRed = new SpawningJob()
            {
                _group   = GameGroups.RED_DOOFUSES_NOT_EATING.BuildGroup
              , _factory = _factory
              , _entity  = _redCapsule
              , _random  = new Random(1234567)
            }.ScheduleParallel(MaxNumberOfDoofuses, _jobHandle);

            var spawnBlue = new SpawningJob()
            {
                _group   = GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup
              , _factory = _factory
              , _entity  = _blueCapsule
              , _random  = new Random(7654321)
            }.ScheduleParallel(MaxNumberOfDoofuses, _jobHandle);

            //Yeah this shouldn't be solved like this, but I keep it in this way for simplicity sake 
            _done = true;

            return JobHandle.CombineDependencies(spawnBlue, spawnRed);
        }
        
        readonly NativeEntityFactory _factory;
        readonly Entity              _redCapsule, _blueCapsule;

        const int MaxNumberOfDoofuses = 10000;

        bool _done;

        [BurstCompile]
        struct SpawningJob : IJobParallelFor
        {
            internal NativeEntityFactory  _factory;
            internal Entity               _entity;
            internal ExclusiveBuildGroup _group;
            internal Random               _random;

#pragma warning disable 649
            //thread index is necessary to build entity in parallel in Svelto ECS
            [NativeSetThreadIndex] int _threadIndex;
#pragma warning restore 649

            public void Execute(int index)
            {
                var positionEntityComponent = new PositionEntityComponent
                {
                    position = new float3(_random.NextFloat(0.0f, 40.0f), 0, _random.NextFloat(0.0f, 40.0f))
                };
                //these structs are used for ReactOnAdd callback to create unity Entities later
                var uecsComponent = new UnityEcsEntityComponent
                {
                    uecsEntity    = _entity
                  , spawnPosition = positionEntityComponent.position
                };

                var init = _factory.BuildEntity((uint) index, _group, _threadIndex);

                init.Init(uecsComponent);
                init.Init(positionEntityComponent);
                init.Init(new SpeedEntityComponent
                {
                    speed = _random.NextFloat(0.1f, 1.0f)
                });
            }
        }
    }
}