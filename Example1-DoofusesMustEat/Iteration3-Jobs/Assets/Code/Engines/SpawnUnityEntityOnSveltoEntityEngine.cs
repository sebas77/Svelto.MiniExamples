using Svelto.ECS.Extensions.Unity;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1C
{
    /// <summary>
    /// In a Svelto<->UECS scenario, is common to have UECS entity created on creation of Svelto ones. Same for
    /// destruction.
    /// </summary>
    public class SpawnUnityEntityOnSveltoEntityEngine
        : IReactOnAddAndRemove<UnityEcsEntityComponent>, IQueryingEntitiesEngine
    {
        readonly EntityManager _entityManager;

        public SpawnUnityEntityOnSveltoEntityEngine(World world) { _entityManager = world.EntityManager; }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void Add(ref UnityEcsEntityComponent entityComponent, EGID egid) { SpawnUnityEntities(entityComponent, egid); }
        public void Remove(ref UnityEcsEntityComponent entityComponent, EGID egid) { DestroyEntity(entityComponent); }

        void DestroyEntity(in UnityEcsEntityComponent entityComponent)
        {
            if (_entityManager.IsCreated)
                _entityManager.DestroyEntity(entityComponent.uecsEntity);
        }

        void SpawnUnityEntities(in UnityEcsEntityComponent unityEcsEntityComponent, in EGID egid)
        {
            var uecsEntity = _entityManager.Instantiate(unityEcsEntityComponent.uecsEntity);

            entitiesDB.QueryEntity<UnityEcsEntityComponent>(egid).uecsEntity = uecsEntity;

            //SharedComponentData can be used to group the UECS entities exactly like the Svelto ones
            _entityManager.AddSharedComponentData(uecsEntity, new UECSSveltoGroupID(egid.groupID));
            _entityManager.AddComponentData(uecsEntity, new Translation
            {
                Value = new float3(unityEcsEntityComponent.spawnPosition.x,
                                   unityEcsEntityComponent.spawnPosition.y,
                                   unityEcsEntityComponent.spawnPosition.z)
            });
        }
    }
}