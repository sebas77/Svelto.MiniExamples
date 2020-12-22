using System.Collections;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using Unity.Entities;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace Svelto.ECS.MiniExamples.Example1B
{
    public class SpawningDoofusEngine : IQueryingEntitiesEngine
    {
        public SpawningDoofusEngine(Entity capsule, IEntityFactory factory)
        {
            _capsule = capsule;
            _factory = factory;
        }

        public IEntitiesDB entitiesDB { get; set; }

        public void Ready() { SpawningDoofuses().RunOn(StandardSchedulers.updateScheduler); }

        IEnumerator SpawningDoofuses()
        {
            //not really needed, can be useful to avoid run time allocations
            _factory.PreallocateEntitySpace<DoofusEntityDescriptor>(GameGroups.DOOFUSES, MaxNumberOfDoofuses);

            while (_numberOfDoofuses < MaxNumberOfDoofuses)
            {
                var init = _factory.BuildEntity<DoofusEntityDescriptor>(_numberOfDoofuses, GameGroups.DOOFUSES);

                var positionEntityStruct = new PositionEntityStruct
                {
                    position = new float3(Random.value * 40, 0, Random.value * 40)
                };

                init.Init(positionEntityStruct);
                init.Init(new UnityEcsEntityStructStruct
                {
                    uecsEntity = _capsule,
                    spawnPosition = positionEntityStruct.position,
                });
                init.Init(new SpeedEntityStruct {speed = Random.Range(1, 10) / 10.0f});

                _numberOfDoofuses++;

//                yield return Yield.It;
            }
            
            yield break;
        }

        readonly IEntityFactory _factory;
        readonly Entity         _capsule;
        uint                    _numberOfDoofuses;

        public const int MaxNumberOfDoofuses = 10000;
    }
}