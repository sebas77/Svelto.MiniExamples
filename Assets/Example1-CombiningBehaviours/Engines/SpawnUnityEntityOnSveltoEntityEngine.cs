using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class SpawnUnityEntityOnSveltoEntityEngine : SingleEntityEngine<UnityECSEntityStruct>
    {
        public SpawnUnityEntityOnSveltoEntityEngine(World world)
        {
            _entityManager = world.EntityManager;
        }
        
        protected override void Add(ref UnityECSEntityStruct unityEcsEntityView)
        {
            SpawnUnityECSEntity(ref unityEcsEntityView);
        }

        protected override void Remove(ref UnityECSEntityStruct unityEcsEntityView)
        {
            //remove from unity ECS?
        }
        
        void SpawnUnityECSEntity(ref UnityECSEntityStruct unityEcsEntityStruct)
        {
            var instance = _entityManager.Instantiate(unityEcsEntityStruct.prefab);
            
            _entityManager.AddComponent(instance, unityEcsEntityStruct.unityComponent);
                
            _entityManager.SetComponentData(instance, new Translation
            {
                Value = new float3(unityEcsEntityStruct.spawnPosition.x,
                unityEcsEntityStruct.spawnPosition.y, unityEcsEntityStruct.spawnPosition.z)
                
            });
        }

        int                     _numberOfDoofuses;
        readonly EntityManager  _entityManager;
    }
}