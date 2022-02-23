using System;
using Svelto.Common;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Native;
using Svelto.ECS.SveltoOnDOTS;
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
        public SpawningDoofusEngine(Entity redCapsule, Entity blueCapsule, Entity specialBlueCapsule,
            IEntityFactory factory)
        {
            _redCapsule         = redCapsule;
            _blueCapsule        = blueCapsule;
            _specialBlueCapsule = specialBlueCapsule;
            _factory            = factory.ToNative<DoofusEntityDescriptor>(nameof(SpawningDoofusEngine));
        }

        public EntitiesDB entitiesDB { get; set; }
        public string     name       => nameof(SpawningDoofusEngine);

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

            var spawnRed = new SpawningJob()
            {
                _group   = GameGroups.RED_DOOFUSES_NOT_EATING.BuildGroup,
                _factory = _factory,
                _entity  = _redCapsule,
                _random  = new Random(1234567)
            }.Schedule(MaxNumberOfDoofuses, _jobHandle);

            //Adding the complexity of the special blue capsule to show how to use filters to handle different
            //entity states in the same group.
            ExclusiveBuildGroup exclusiveBuildGroup = GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup;

            if (exclusiveBuildGroup.isInvalid)
                throw new Exception();

            var spawnBlue = new SpawningJob()
            {
                _group          = exclusiveBuildGroup,
                _factory        = _factory,
                _entity         = _blueCapsule,
                _specialEntity  = _specialBlueCapsule,
                _random         = new Random(7654321),
                _useFilters     = true
            }.Schedule(MaxNumberOfDoofuses, _jobHandle);

            //Yeah this shouldn't be solved like this, but I keep it in this way for simplicity sake 
            _done = true;

            return JobHandle.CombineDependencies(spawnBlue, spawnRed);
        }

        readonly NativeEntityFactory _factory;
        readonly Entity              _redCapsule, _blueCapsule, _specialBlueCapsule;

        const int MaxNumberOfDoofuses = 10000;

        bool _done;

        [BurstCompile]
        struct SpawningJob : IJobFor
        {
            internal NativeEntityFactory _factory;
            internal Entity              _entity;
            internal ExclusiveBuildGroup _group;
            internal Random              _random;

            internal Entity                              _specialEntity;
            internal bool                                _useFilters;

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

                //this special component is used to ReactOnAdd and create DOTS entities on Svelto entities
                var isSpecial = (index & 3) == 0;

                var useFilters = isSpecial && _useFilters;
                var createDOTSEntityOnSveltoComponent = new SpawnPointEntityComponent(useFilters,
                    useFilters ? _specialEntity : _entity, positionEntityComponent.position);

                var egid = new EGID((uint)index, _group);
                var init = _factory.BuildEntity(egid, _threadIndex);

                init.Init(createDOTSEntityOnSveltoComponent);
                init.Init(positionEntityComponent);
                init.Init(new SpeedEntityComponent
                {
                    speed = _random.NextFloat(0.1f, 1.0f)
                });
            }
        }
    }
}