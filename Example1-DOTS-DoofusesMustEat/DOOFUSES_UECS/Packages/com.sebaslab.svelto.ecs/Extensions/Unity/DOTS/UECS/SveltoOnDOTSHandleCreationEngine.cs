#if UNITY_ECS
using System;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    /// SubmissionEngine is a dedicated DOTS ECS Svelto.ECS engine that allows using the DOTS ECS
    /// EntityCommandBuffer for fast creation of DOTS entities
    /// </summary>
    public abstract class SveltoOnDOTSHandleCreationEngine
    {
        protected          EntityCommandBufferForSvelto ECB           { get; private set; }
        [Obsolete("<color=orange>Attention: the use of EntityManager directly is deprecated. ECB MUST BE USED INSTEAD</color>")]
        protected internal EntityManager                entityManager { get; internal set; }

        internal EntityCommandBuffer entityCommandBuffer
        {
            set => ECB = new EntityCommandBufferForSvelto(value);
        }
        
        protected EntityArchetype CreateArchetype(params ComponentType[] types)
        {
            return entityManager.CreateArchetype(types);
        }

        protected Entity CreateDOTSEntityOnSvelto(Entity entityComponentPrefabEntity, EGID egid)
        {
            return ECB.CreateDOTSEntityOnSvelto(entityComponentPrefabEntity, egid);
        }
        
        protected Entity CreateDOTSEntityOnSvelto(EntityArchetype archetype, EGID egid)
        {
            return ECB.CreateDOTSEntityOnSvelto(archetype, egid);
        }
        
        protected internal virtual void OnCreate()
        {
        }

        protected internal virtual JobHandle OnUpdate()
        {
            return default;
        }
    }
}
#endif