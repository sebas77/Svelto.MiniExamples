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
                                                      , IReactOnAddAndRemove<SpawnPointEntityComponent>
                                                     
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void Add(ref SpawnPointEntityComponent entityComponent, EGID egid)
        {
            Entity uecsEntity = ECB.Instantiate(entityComponent.prefabEntity);

            //SharedComponentData can be used to group the UECS entities exactly like the Svelto ones
            ECB.AddSharedComponent(uecsEntity, new UECSSveltoGroupID(egid.groupID));
            ECB.AddComponent(uecsEntity, new UpdateUECSEntityAfterSubmission(egid));
            ECB.SetComponent(uecsEntity, new Translation
            {
                Value = new float3(entityComponent.spawnPosition.x, entityComponent.spawnPosition.y
                                 , entityComponent.spawnPosition.z)
            });
        }

        public void Remove(ref SpawnPointEntityComponent entityComponent, EGID egid) {  }
    }
}