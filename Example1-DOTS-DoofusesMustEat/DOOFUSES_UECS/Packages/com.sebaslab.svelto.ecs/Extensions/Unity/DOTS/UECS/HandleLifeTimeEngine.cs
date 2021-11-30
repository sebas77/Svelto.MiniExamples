using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    public abstract class HandleLifeTimeEngine
    {
        protected          EntityCommandBufferForSvelto ECB        { get; private set; }
        protected          EntityQuery                  query      { get; set; }
        protected internal EntitiesDB                   entitiesDB { get; internal set; }

        internal EntityCommandBuffer entityCommandBuffer
        {
            set => ECB = new EntityCommandBufferForSvelto(value);
        }

        public abstract JobHandle ConvertPendingEntities(JobHandle jobHandle);

        public abstract void SetupQuery(EntityManager entityManager);

        protected internal void OnCreate()
        {
        }
    }
}