#if UNITY_ECS
using System.Runtime.CompilerServices;
using Unity.Entities;

namespace Svelto.ECS.SveltoOnDOTS
{
    public readonly struct EntityCommandBufferForSvelto
    {
        internal EntityCommandBufferForSvelto(EntityCommandBuffer value)
        {
            ECB = value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreatePureDOTSEntity(EntityArchetype jointArchetype)
        {
            return ECB.CreateEntity(jointArchetype);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(Entity e, in T component) where T : struct, IComponentData
        {
            ECB.SetComponent(e, component);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSharedComponent<T>(Entity e, in T component) where T : struct, ISharedComponentData
        {
            ECB.SetSharedComponent(e, component);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Entity CreateDOTSEntityOnSvelto(Entity entityComponentPrefabEntity, EGID egid)
        {
            Entity dotsEntity = ECB.Instantiate(entityComponentPrefabEntity);
            
            //SharedComponentData can be used to group the DOTS ECS entities exactly like the Svelto ones
            ECB.AddSharedComponent(dotsEntity, new DOTSSveltoGroupID(egid.groupID));
            ECB.AddComponent(dotsEntity, new DOTSSveltoEGID(egid));
            ECB.AddComponent<DOTSEntityToSetup>(dotsEntity);

            return dotsEntity;
        }
        
        /// <summary>
        /// This method assumes that the Svelto entity with EGID egid has also dotsEntityComponent
        /// among the descriptors
        /// </summary>
        /// <param name="archetype"></param>
        /// <param name="egid"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Entity CreateDOTSEntityOnSvelto(EntityArchetype archetype, EGID egid)
        {
            Entity dotsEntity = ECB.CreateEntity(archetype);
            
            //SharedComponentData can be used to group the DOTS ECS entities exactly like the Svelto ones
            ECB.AddSharedComponent(dotsEntity, new DOTSSveltoGroupID(egid.groupID));
            ECB.AddComponent(dotsEntity, new DOTSSveltoEGID(egid));
            ECB.AddComponent<DOTSEntityToSetup>(dotsEntity);

            return dotsEntity;
        }
        
        /// <summary>
        /// in this case the user decided to create a DOTS entity that is self managed and not managed
        /// by the framework
        /// </summary>
        /// <param name="archetype"></param>
        /// <param name="wireEgid"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Entity CreateDOTSEntityUnmanaged(EntityArchetype archetype)
        {
            return ECB.CreateEntity(archetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyEntity(Entity e)
        {
            ECB.DestroyEntity(e);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent<T>(Entity dotsEntity)
        {
            ECB.RemoveComponent<T>(dotsEntity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent<T>(Entity dotsEntity) where T : struct, IComponentData
        {
            ECB.AddComponent<T>(dotsEntity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent<T>(Entity dotsEntity, in T component) where T : struct, IComponentData
        {
            ECB.AddComponent(dotsEntity, component);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBuffer<T>(Entity dotsEntity) where T : struct, IBufferElementData
        {
            ECB.AddBuffer<T>(dotsEntity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityCommandBuffer.ParallelWriter AsParallelWriter()
        {
            return ECB.AsParallelWriter();
        }
        
        readonly EntityCommandBuffer                ECB;
    }
}
#endif