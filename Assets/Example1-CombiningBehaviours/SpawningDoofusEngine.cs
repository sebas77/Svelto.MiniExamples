using Svelto.ECS.Components;
using Svelto.ECS.EntityStructs;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class SpawningDoofusEngine : IQueryingEntitiesEngine
    {
        public SpawningDoofusEngine(GameObject capsule, IEntityFactory factory, World world)
        {
            _factory = factory;
            _prefab        = GameObjectConversionUtility.ConvertGameObjectHierarchy(capsule, world);
            _entityManager = world.EntityManager;
        }
        
        public IEntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            SpawningDoofuses();
        }
        
        void SpawningDoofuses()
        {
            _factory.PreallocateEntitySpace<DoofusEntityDescriptor>(GameGroups.DOOFUSES, 10000);
            
            while (_numberOfDoofuses < 10000)
            {
                var init = _factory.BuildEntity<DoofusEntityDescriptor>(_numberOfDoofuses, GameGroups.DOOFUSES);

                var positionEntityStruct = new PositionEntityStruct
                {
                    position = new ECSVector3(Random.value * 40, 0, Random.value * 40)
                };
                init.Init(positionEntityStruct);
                
                SpawnUnityECSEntity(ref positionEntityStruct);
                
                _numberOfDoofuses++;
            }
        }

        void SpawnUnityECSEntity(ref PositionEntityStruct positionEntityStruct)
        {
            var instance = _entityManager.Instantiate(_prefab);
                
            _entityManager.SetComponentData(instance, new Translation { Value = new float3(positionEntityStruct.position.x,
                                                   positionEntityStruct.position.y, positionEntityStruct.position.z)});
        }

        readonly IEntityFactory _factory;
        int                     _numberOfDoofuses;
        Entity _prefab;
        EntityManager _entityManager;
    }
}