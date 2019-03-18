using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class SpawnUnityEntityOnSveltoEntityEngine : SingleEntityEngine<UnityECSEntityStruct>, IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }

        public void Ready()
        {}
        
        protected override void Add(ref UnityECSEntityStruct             entityView,
                                    ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        {
            //this pattern must be improved, it's in the to do list
            //move to its engine, it's spawning all the unity entities
            if (previousGroup == null)
                SpawnUnityEntities(ref entityView);
        }

        protected override void Remove(ref UnityECSEntityStruct entityView) {  }

        public SpawnUnityEntityOnSveltoEntityEngine(World world)
        {
            _entityManager = world.EntityManager;
        }
        
        //it seems that the add callback was enough
        void SpawnUnityEntities(ref UnityECSEntityStruct unityEcsEntityStruct)
        {
            var uecsEntity = _entityManager.Instantiate(unityEcsEntityStruct.uecsEntity);

            unityEcsEntityStruct.uecsEntity = uecsEntity;

            _entityManager.AddComponent(uecsEntity, unityEcsEntityStruct.unityComponent);

            _entityManager.SetComponentData(uecsEntity, new Translation
            {
                Value = new float3(unityEcsEntityStruct.spawnPosition.x, unityEcsEntityStruct.spawnPosition.y,
                                   unityEcsEntityStruct.spawnPosition.z)

            });
        }

        readonly EntityManager                _entityManager;
    }
}