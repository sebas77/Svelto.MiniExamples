using Unity.Entities;
 using Unity.Mathematics;
 using Unity.Transforms;
 
 namespace Svelto.ECS.MiniExamples.Example1B
 {
     public class SpawnUnityEntityOnSveltoEntityEngine : SingleEntityEngine<UnityEcsEntityStructStruct>, IQueryingEntitiesEngine
     {
         public IEntitiesDB entitiesDB { get; set; }
 
         public void Ready()
         {}
         
         protected override void Add(in UnityEcsEntityStructStruct             entityView,
                                     ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
         {
             if (previousGroup == null)
                 SpawnUnityEntities(in entityView);
         }
         
         protected override void Remove(in UnityEcsEntityStructStruct entityView, bool itsaSwap)
         {
             if (itsaSwap == false)
                 DestroyEntity(in entityView);
         }
 
         void DestroyEntity(in UnityEcsEntityStructStruct entityView)
         {
             if (_entityManager.IsCreated)
                 _entityManager.DestroyEntity(entityView.uecsEntity);
         }
 
         public SpawnUnityEntityOnSveltoEntityEngine(World world)
         {
             _entityManager = world.EntityManager;
         }
         
         void SpawnUnityEntities(in UnityEcsEntityStructStruct unityEcsEntityStructStruct)
         {
             var uecsEntity = _entityManager.Instantiate(unityEcsEntityStructStruct.uecsEntity);
 
             entitiesDB.QueryEntity<UnityEcsEntityStructStruct>(unityEcsEntityStructStruct.ID).uecsEntity = uecsEntity;
             
             _entityManager.AddComponent(uecsEntity, unityEcsEntityStructStruct.unityComponent);
 
             _entityManager.SetComponentData(uecsEntity, new Translation
             {
                 Value = new float3(unityEcsEntityStructStruct.spawnPosition.x, unityEcsEntityStructStruct.spawnPosition.y,
                                    unityEcsEntityStructStruct.spawnPosition.z)
 
             });
         }
 
         readonly EntityManager                _entityManager;
     }
 }