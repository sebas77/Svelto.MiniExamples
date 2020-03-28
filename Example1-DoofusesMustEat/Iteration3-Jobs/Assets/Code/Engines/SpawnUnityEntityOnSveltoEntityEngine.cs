using Svelto.Common;
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
        : IReactOnAddAndRemove<UnityEcsEntityStruct>, IQueryingEntitiesEngine
    {
        readonly EntityManager _entityManager;

        public SpawnUnityEntityOnSveltoEntityEngine(World world) { _entityManager = world.EntityManager; }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void Add(ref UnityEcsEntityStruct entityView, EGID egid) { SpawnUnityEntities(entityView, egid); }
        public void Remove(ref UnityEcsEntityStruct entityView, EGID egid) { DestroyEntity(entityView); }

        void DestroyEntity(in UnityEcsEntityStruct entityView)
        {
            if (_entityManager.IsCreated)
                _entityManager.DestroyEntity(entityView.uecsEntity);
        }

        void SpawnUnityEntities(in UnityEcsEntityStruct unityEcsEntityStruct, in EGID egid)
        {
            var uecsEntity = _entityManager.Instantiate(unityEcsEntityStruct.uecsEntity);

            entitiesDB.QueryEntity<UnityEcsEntityStruct>(unityEcsEntityStruct.ID).uecsEntity = uecsEntity;

            //SharedComponentData can be used to group the UECS entities exactly like the Svelto ones
            _entityManager.AddSharedComponentData(uecsEntity, new UECSSveltoGroupID(egid.groupID));
            _entityManager.AddComponentData(uecsEntity, new Translation
            {
                Value = new float3(unityEcsEntityStruct.spawnPosition.x,
                                   unityEcsEntityStruct.spawnPosition.y,
                                   unityEcsEntityStruct.spawnPosition.z)
            });
        }
    }
}