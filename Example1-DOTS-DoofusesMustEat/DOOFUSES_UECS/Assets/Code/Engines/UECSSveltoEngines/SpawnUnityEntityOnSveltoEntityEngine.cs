using Svelto.ECS.SveltoOnDOTS;
using Unity.Entities;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1C
{
    /// <summary>
    /// In a Svelto<->UECS scenario, is common to have UECS entity created on creation of Svelto ones. Same for
    /// destruction.
    /// Note this can be easily moved to using Entity Command Buffer and I should do it at a given point
    /// but at the moment (Entities 0.17) ECB performance is really bad when ShareComponents are used, which
    /// I need to rely on
    /// </summary>
    public class SpawnUnityEntityOnSveltoEntityEngine : SveltoOnDOTSHandleCreationEngine,
        IReactOnAddEx<SpawnPointEntityComponent>, IQueryingEntitiesEngine
    {
        public void Add((uint start, uint end) rangeOfEntities,
            in EntityCollection<SpawnPointEntityComponent> collection, ExclusiveGroupStruct groupID)
        {
            var (buffer, entityIDs, _) = collection;

            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                ref var entityComponent = ref buffer[i];
                Entity dotsEntity =
                    CreateDOTSEntityOnSvelto(entityComponent.prefabEntity, new EGID(entityIDs[i], groupID), true);
                
                if (entityComponent.isSpecial)
                    ECB.AddComponent<SpecialBlue>(dotsEntity);
                
                ECB.SetComponent(dotsEntity, new Translation
                {
                    Value = entityComponent.spawnPosition
                });
            }
        }

        public override string     name => nameof(SpawnUnityEntityOnSveltoEntityEngine);
        public void Ready() {
        }

        public EntitiesDB entitiesDB { get; set; }
    }

    public struct SpecialBlue : IComponentData
    {
    }
}