using Svelto.Common;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Internal;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Allocator = Unity.Collections.Allocator;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [Sequenced(nameof(DoofusesEngineNames.SpawningDoofusEngine))]
    public class SpawningDoofusEngine : IQueryingEntitiesEngine, IJobifiableEngine
    {
        public SpawningDoofusEngine(Entity redCapsule, Entity blueCapsule, IEntityFactory factory)
        {
            _redCapsule  = redCapsule;
            _blueCapsule = blueCapsule;
            _factory     = factory.ToNative<DoofusEntityDescriptor>(Allocator.Persistent);
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        readonly NativeEntityFactory _factory;
        readonly Entity         _redCapsule, _blueCapsule;

        const int MaxNumberOfDoofuses = 10000;

        readonly InternalGroup blueDoofusesNotEating =
            GroupCompound<GameGroups.DOOFUSES, GameGroups.BLUE, GameGroups.NOTEATING>.BuildGroup;

        readonly InternalGroup redDoofusesNotEating =
            GroupCompound<GameGroups.DOOFUSES, GameGroups.RED, GameGroups.NOTEATING>.BuildGroup;

        bool _done;

        public JobHandle Execute(JobHandle _jobHandle)
        {
            if (_done == true)
                return _jobHandle;
            
            new SpawningJob(blueDoofusesNotEating, redDoofusesNotEating
                          , _factory, _redCapsule, _blueCapsule)
               .Schedule(MaxNumberOfDoofuses, ProcessorCount.Batch(MaxNumberOfDoofuses), _jobHandle);

            _done = true;

            return _jobHandle;
        }

        [BurstCompile]
        struct SpawningJob: IJobParallelFor
        {
            readonly NativeEntityFactory _factory;
            readonly Entity              _redCapsule;
            readonly Entity              _blueCapsule;
            readonly InternalGroup       _blueDoofusesNotEating;
            readonly InternalGroup       _redDoofusesNotEating;
            
            Random     _random;

#pragma warning disable 649
            [NativeSetThreadIndex] int _threadIndex;
#pragma warning restore 649

            public SpawningJob
            (InternalGroup blueDoofusesGroup, InternalGroup redDoofusesGroup, NativeEntityFactory nativeFactor
           , Entity red, Entity blue):this()
            {
                _random = new Random(1234567);
                _blueDoofusesNotEating = blueDoofusesGroup;
                _redDoofusesNotEating  = redDoofusesGroup;
                _factory               = nativeFactor;
                _redCapsule            = red;
                _blueCapsule           = blue;
            }

            public void Execute(int index)
            {
                NativeEntityComponentInitializer init;
                PositionEntityComponent          positionEntityComponent;
                UnityEcsEntityComponent          uecsComponent;

                if (_random.NextFloat(0.0f, 1.0f) > 0.5)
                {
                    positionEntityComponent = new PositionEntityComponent
                    {
                        position = new float3(_random.NextFloat(0.0f, 40.0f), 0, _random.NextFloat(0.0f, 40.0f))
                    };
                    //these structs are used for ReactOnAdd callback to create unity Entities later
                    uecsComponent = new UnityEcsEntityComponent
                    {
                        uecsEntity = _redCapsule, spawnPosition = positionEntityComponent.position
                    };

                    init = _factory.BuildEntity((uint) index, _redDoofusesNotEating, _threadIndex);

                }
                else
                {
                    positionEntityComponent = new PositionEntityComponent
                    {
                        position = new float3(_random.NextFloat(0.0f, 40.0f), 0, _random.NextFloat(0.0f, 40.0f))
                    };
                    //these structs are used for ReactOnAdd callback to create unity Entities later
                    uecsComponent = new UnityEcsEntityComponent
                    {
                        uecsEntity = _blueCapsule, spawnPosition = positionEntityComponent.position
                    };

                    init = _factory.BuildEntity((uint) index, _blueDoofusesNotEating, _threadIndex);
                }

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