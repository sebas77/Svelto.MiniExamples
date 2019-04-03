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
        
        protected override void Add(in UnityECSEntityStruct             entityView,
                                    ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        {
            if (previousGroup == null)
                SpawnUnityEntities(in entityView);
        }
        
        protected override void Remove(in UnityECSEntityStruct entityView, bool itsaSwap)
        {
            if (itsaSwap == false)
                DestroyEntity(in entityView);
        }

        void DestroyEntity(in UnityECSEntityStruct entityView)
        {
            if (_entityManager.IsCreated)
                _entityManager.DestroyEntity(entityView.uecsEntity);
        }

        public SpawnUnityEntityOnSveltoEntityEngine(World world)
        {
            _entityManager = world.EntityManager;
        }
        
        void SpawnUnityEntities(in UnityECSEntityStruct unityEcsEntityStruct)
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

        readonly EntityManager                _entityManager;
    }
}