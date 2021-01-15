#define USE_ENTITY_MANAGER

using System;
using Svelto.ECS.Extensions.Unity;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1C
{
    /// <summary>
    /// In a Svelto<->UECS scenario, is common to have UECS entity created on creation of Svelto ones. Same for
    /// destruction.
    /// Note this can be easily moved to using Entity Command Buffer and I should do it at a given point
    /// </summary>
    [DisableAutoCreation]
    public class SpawnUnityEntityOnSveltoEntityEngine : SubmissionEngine, IQueryingEntitiesEngine
                                                      , IReactOnAddAndRemove<UnityEcsEntityComponent>
                                                      , IReactOnSwap<UnityEcsEntityComponent>
    {
        protected override void OnUpdate() 
        { }

        public EntitiesDB          entitiesDB                                    { get; set; }
        public void                Ready()                                       {  }
        
        public void Add(ref UnityEcsEntityComponent entityComponent, EGID egid)
        {
            SpawnUnityEntities(entityComponent, egid);
        }

        public void Remove(ref UnityEcsEntityComponent entityComponent, EGID egid)
        {
            DestroyEntity(entityComponent);
        }

        public void MovedTo(ref UnityEcsEntityComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
#if USE_ENTITY_MANAGER            
            EntityManager.SetSharedComponentData(entityComponent.uecsEntity, new UECSSveltoGroupID(egid.groupID));
#else
            ECB.SetSharedComponent(entityComponent.uecsEntity, new UECSSveltoGroupID(egid.groupID));
#endif            
        }

        void DestroyEntity(in UnityEcsEntityComponent entityComponent)
        {
#if USE_ENTITY_MANAGER                    
            EntityManager.DestroyEntity(entityComponent.uecsEntity);
#else
                ECB.DestroyEntity(entityComponent.uecsEntity);
#endif            
        }

        void SpawnUnityEntities(in UnityEcsEntityComponent unityEcsEntityComponent, in EGID egid)
        {
#if USE_ENTITY_MANAGER            
            Entity uecsEntity = EntityManager.Instantiate(unityEcsEntityComponent.uecsEntity);

            //SharedComponentData can be used to group the UECS entities exactly like the Svelto ones
            EntityManager.AddSharedComponentData(uecsEntity, new UECSSveltoGroupID(egid.groupID));
            EntityManager.SetComponentData(uecsEntity, new Translation
            {
                Value = new float3(unityEcsEntityComponent.spawnPosition.x, unityEcsEntityComponent.spawnPosition.y
                                 , unityEcsEntityComponent.spawnPosition.z)
            });
#else
            Entity uecsEntity = ECB.Instantiate(unityEcsEntityComponent.uecsEntity);

            //SharedComponentData can be used to group the UECS entities exactly like the Svelto ones
            ECB.AddSharedComponent(uecsEntity, new UECSSveltoGroupID(egid.groupID));
            ECB.SetComponent(uecsEntity, new Translation
            {
                Value = new float3(unityEcsEntityComponent.spawnPosition.x, unityEcsEntityComponent.spawnPosition.y
                                 , unityEcsEntityComponent.spawnPosition.z)
            });
#endif            
            
            entitiesDB.QueryEntity<UnityEcsEntityComponent>(egid).uecsEntity = uecsEntity;
        }
    }
}