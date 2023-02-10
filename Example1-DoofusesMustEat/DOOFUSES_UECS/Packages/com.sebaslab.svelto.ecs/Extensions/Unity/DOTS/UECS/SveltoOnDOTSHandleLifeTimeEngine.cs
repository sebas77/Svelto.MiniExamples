#if UNITY_ECS

namespace Svelto.ECS.SveltoOnDOTS
{
    public interface ISveltoOnDOTSHandleLifeTimeEngine
    {
        EntityCommandBufferForSvelto entityCommandBuffer { set; }
    }

    /// <summary>
    /// Automatic Svelto Group -> DOTS archetype synchronization when necessary
    /// </summary>
    /// <typeparam name="DOTSEntityComponent"></typeparam>
    public class SveltoOnDOTSHandleLifeTimeEngine<DOTSEntityComponent>: ISveltoOnDOTSHandleLifeTimeEngine,
            IReactOnRemoveEx<DOTSEntityComponent>,
            IReactOnSwapEx<DOTSEntityComponent>
            where DOTSEntityComponent : unmanaged, IEntityComponentForDOTS
    {
        EntityCommandBufferForSvelto ECB { get; set; }

        public EntityCommandBufferForSvelto entityCommandBuffer
        {
            set => ECB = value;
        }

        public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<DOTSEntityComponent> collection, ExclusiveGroupStruct groupID)
        {
            var (entities, _) = collection;

            //todo this could be burstified
            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                ref var entityComponent = ref entities[i];

                ECB.DestroyEntity(entityComponent.dotsEntity);
            }
        }

        public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<DOTSEntityComponent> collection,
            ExclusiveGroupStruct _, ExclusiveGroupStruct toGroup)
        {
            var (entities, entityIDs, _) = collection;

            //todo this could be burstified
            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                ref var entityComponent = ref entities[i];
                ECB.SetSharedComponent(entityComponent.dotsEntity, new DOTSSveltoGroupID(toGroup));

                ECB.SetComponent(entityComponent.dotsEntity, new DOTSSveltoEGID
                {
                    egid = new EGID(entityIDs[i], toGroup)
                });
            }
        }
    }
}
#endif