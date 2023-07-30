using System;
using Svelto.Common;
using Svelto.ECS.Miniexamples.Doofuses.GameObjectsLayer;
using Svelto.ECS.Native;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Svelto.ECS.Miniexamples.Doofuses.Gameobjects
{
    [Sequenced(nameof(DoofusesEngineNames.SpawningDoofusEngine))]
    public class SpawningDoofusEngine : IQueryingEntitiesEngine, IJobifiedEngine, IDisposable
    {
        public SpawningDoofusEngine(int redCapsule, int blueCapsule, IEntityFactory factory)
        {
            _redCapsule        = redCapsule;
            _blueCapsule       = blueCapsule;
            _factory           = factory.ToNative<DoofusEntityDescriptor>(nameof(SpawningDoofusEngine));
            _threadLocalRandomA = new ThreadLocalRandom(256);
            _threadLocalRandomB = new ThreadLocalRandom(256);
        }

        public EntitiesDB entitiesDB { get; set; }
        public string     name       => nameof(SpawningDoofusEngine);

        public void Ready() { }

        public JobHandle Execute(JobHandle inputDeps)
        {
            if (_done == true)
                return inputDeps;

            var spawnRed = new SpawningJob()
            {
                _group             = GameGroups.RED_DOOFUSES_NOT_EATING.BuildGroup
              , _factory           = _factory
              , _entity            = _redCapsule
              , _threadLocalRandom = _threadLocalRandomA
            }.ScheduleParallel(MaxNumberOfDoofuses, inputDeps);

            var spawnBlue = new SpawningJob()
            {
                _group             = GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup
              , _factory           = _factory
              , _entity            = _blueCapsule
              , _threadLocalRandom = _threadLocalRandomB
            }.ScheduleParallel(MaxNumberOfDoofuses, inputDeps);

            //Yeah this shouldn't be solved like this, but I keep it in this way for simplicity sake 
            _done = true;

            return JobHandle.CombineDependencies(spawnBlue, spawnRed);
        }

        readonly NativeEntityFactory _factory;
        readonly int                 _redCapsule, _blueCapsule;

        const int MaxNumberOfDoofuses = 5000;

        bool                       _done;
        readonly ThreadLocalRandom _threadLocalRandomA;
        readonly ThreadLocalRandom _threadLocalRandomB;

        [BurstCompile]
        struct SpawningJob : IJobParallelFor
        {
            internal NativeEntityFactory _factory;
            internal int                 _entity;
            internal ThreadLocalRandom   _threadLocalRandom;
            internal ExclusiveBuildGroup _group;

#pragma warning disable 649
            //thread index is necessary to build entity in parallel in Svelto ECS
            [NativeSetThreadIndex] int _threadIndex;
#pragma warning restore 649

            public void Execute(int index)
            {
                var x = _threadLocalRandom.Next(0, 40, _threadIndex);
                var z = _threadLocalRandom.Next(0, 40, _threadIndex);
                var positionEntityComponent = new PositionEntityComponent
                {
                    position = new float3(x, 0, z)
                };
                //these structs are used for ReactOnAdd callback to create unity Entities later
                var uecsComponent = new GameObjectEntityComponent
                {
                    prefabID      = _entity
                  , spawnPosition = positionEntityComponent.position
                };

                var init = _factory.BuildEntity((uint)index, _group, _threadIndex);

                init.Init(uecsComponent);
                init.Init(positionEntityComponent);
                init.Init(new SpeedEntityComponent
                {
                    speed = (float)_threadLocalRandom.NextDouble(0.1f, 1.0f, _threadIndex)
                });
            }
        }

        public void Dispose()
        {
            _threadLocalRandomA.Dispose();
            _threadLocalRandomB.Dispose();
        }
    }
}