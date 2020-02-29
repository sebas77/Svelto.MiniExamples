using System;
using System.Collections;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using Svelto.Tasks.ExtraLean.Unity;
using Unity.Entities;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace Svelto.ECS.MiniExamples.Example1B
{
    public class SpawningDoofusEngine : IQueryingEntitiesEngine, IDisposable
    {
        public SpawningDoofusEngine(Entity redCapsule, Entity blueCapsule, IEntityFactory factory)
        {
            _redCapsule  = redCapsule;
            _blueCapsule = blueCapsule;
            _factory     = factory;
            _runner = new UpdateMonoRunner("SpawningDoofusEngine");
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { SpawningDoofuses().RunOn(_runner); }

        IEnumerator SpawningDoofuses()
        {
            //not really needed, can be useful to avoid run time allocations
            _factory
               .PreallocateEntitySpace<DoofusEntityDescriptor
                >(GroupCompound<GameGroups.DOOFUSES, GameGroups.RED>.BuildGroup, MaxNumberOfDoofuses);
            _factory
               .PreallocateEntitySpace<DoofusEntityDescriptor
                >(GroupCompound<GameGroups.DOOFUSES, GameGroups.BLUE>.BuildGroup, MaxNumberOfDoofuses);

            while (_numberOfDoofuses < MaxNumberOfDoofuses)
            {
                EntityStructInitializer init;
                PositionEntityStruct    positionEntityStruct;
                if (Random.value > 0.5)
                {
                    init = _factory.BuildEntity<DoofusEntityDescriptor>(_numberOfDoofuses,
                                                                        GroupCompound<GameGroups.DOOFUSES,
                                                                            GameGroups.RED, GameGroups.NOTEATING>.BuildGroup);
                    positionEntityStruct = new PositionEntityStruct
                    {
                        position = new float3(Random.value * 40, 0, Random.value * 40)
                    };
                    //these structs are used for ReactOnAdd callback to create unity Entities later
                    init.Init(new UnityEcsEntityStruct
                    {
                        uecsEntity    = _redCapsule,
                        spawnPosition = positionEntityStruct.position,
                    });
                }
                else
                {
                    positionEntityStruct = new PositionEntityStruct
                    {
                        position = new float3(Random.value * 40, 0, Random.value * 40)
                    };
                    init = _factory.BuildEntity<DoofusEntityDescriptor>(_numberOfDoofuses,
                                                                        GroupCompound<GameGroups.DOOFUSES,
                                                                            GameGroups.BLUE, GameGroups.NOTEATING>.BuildGroup);
                    //these structs are used for ReactOnAdd callback to create unity Entities later
                    init.Init(new UnityEcsEntityStruct
                    {
                        uecsEntity    = _blueCapsule,
                        spawnPosition = positionEntityStruct.position,
                    });
                }

                init.Init(positionEntityStruct);

                init.Init(new SpeedEntityStruct {speed = Random.Range(1, 10) / 10.0f});

                _numberOfDoofuses++;
            }

            yield break;
        }
        
        public void Dispose()
        {
            _runner?.Dispose();
        }

        readonly IEntityFactory _factory;
        readonly Entity         _redCapsule, _blueCapsule;
        uint                    _numberOfDoofuses;

        const int MaxNumberOfDoofuses = 10000;
        
        readonly UpdateMonoRunner _runner;
    }
}