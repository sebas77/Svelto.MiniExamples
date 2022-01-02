using Svelto.ECS.SveltoOnDOTS;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1C
{
    /// <summary>
    /// In a Svelto<->UECS scenario, is common to have UECS entity created on creation of Svelto ones. Same for
    /// destruction.
    /// Note this can be easily moved to using Entity Command Buffer and I should do it at a given point
    /// </summary>
    public class SpawnUnityEntityOnSveltoEntityEngine : SveltoOnDOTSHandleCreationEngine, IQueryingEntitiesEngine
                                                      , IReactOnAdd<SpawnPointEntityComponent>
                                                     
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void Add(ref SpawnPointEntityComponent entityComponent, EGID egid)
        {
            Entity dotsEntity = CreateDOTSEntityOnSvelto(entityComponent.prefabEntity, egid);

            ECB.SetComponent(dotsEntity, new Translation
            {
                Value = new float3(entityComponent.spawnPosition.x, entityComponent.spawnPosition.y
                                 , entityComponent.spawnPosition.z)
            });
        }
    }
}