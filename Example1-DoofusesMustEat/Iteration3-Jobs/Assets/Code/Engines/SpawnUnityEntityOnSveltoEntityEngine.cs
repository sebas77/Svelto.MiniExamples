using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1B
{
    public class SpawnUnityEntityOnSveltoEntityEngine
        : IReactOnAddAndRemove<UnityEcsEntityStructStruct>, IQueryingEntitiesEngine
    {
        readonly EntityManager _entityManager;

        public SpawnUnityEntityOnSveltoEntityEngine(World world) { _entityManager = world.EntityManager; }

        public IEntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void Add(ref UnityEcsEntityStructStruct entityView, EGID egid) { SpawnUnityEntities(in entityView); }
        public void Remove(ref UnityEcsEntityStructStruct entityView, EGID egid) { DestroyEntity(in entityView); }

        void DestroyEntity(in UnityEcsEntityStructStruct entityView)
        {
            if (_entityManager.IsCreated)
                _entityManager.DestroyEntity(entityView.uecsEntity);
        }

        void SpawnUnityEntities(in UnityEcsEntityStructStruct unityEcsEntityStructStruct)
        {
            var uecsEntity = _entityManager.Instantiate(unityEcsEntityStructStruct.uecsEntity);

            entitiesDB.QueryEntity<UnityEcsEntityStructStruct>(unityEcsEntityStructStruct.ID).uecsEntity = uecsEntity;

            _entityManager.AddComponentData(uecsEntity, new Translation
            {
                Value = new float3(unityEcsEntityStructStruct.spawnPosition.x,
                                   unityEcsEntityStructStruct.spawnPosition.y,
                                   unityEcsEntityStructStruct.spawnPosition.z)
            });
        }
    }
}