using System.Collections;
using Svelto.Tasks.ExtraLean;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class SpawnUnityEntityOnSveltoEntityEngine : IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            SpawnUnityEntities().RunOn(StandardSchedulers.updateScheduler);
        }
        
        public SpawnUnityEntityOnSveltoEntityEngine(World world, IEntityStreamConsumerFactory consumerFactory)
        {
            _consumerFactory = consumerFactory;
            _entityManager = world.EntityManager;
        }
        
        IEnumerator SpawnUnityEntities()
        {
            var consumer = _consumerFactory.GenerateConsumer<UnityECSEntityStruct>("UnityECSSpawner", 2);
            
            while (true)
            {
                while (consumer.TryDequeue(out var unityEcsEntityStruct))
                {
                    var uecsEntity = _entityManager.Instantiate(unityEcsEntityStruct.uecsEntity);

                    entitiesDB.QueryEntity<UnityECSEntityStruct>(unityEcsEntityStruct.ID).uecsEntity = uecsEntity;

                    _entityManager.AddComponent(uecsEntity, unityEcsEntityStruct.unityComponent);

                    _entityManager.SetComponentData(uecsEntity, new Translation
                    {
                        Value = new float3(unityEcsEntityStruct.spawnPosition.x, unityEcsEntityStruct.spawnPosition.y,
                                           unityEcsEntityStruct.spawnPosition.z)

                    });
                }

                yield return null;
            }
        }

        readonly EntityManager                _entityManager;
        readonly IEntityStreamConsumerFactory _consumerFactory;
    }
}