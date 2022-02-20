#if UNITY_ECS
#define SLOW_ECB
using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    public readonly struct EntityCommandBufferForSvelto
    {
        internal EntityCommandBufferForSvelto(EntityCommandBuffer value, EntityManager manager)
        {
            _ECB      = value;
            _EManager = manager;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreatePureDOTSEntity(EntityArchetype jointArchetype)
        {
#if SLOW_ECB
            return _EManager.CreateEntity(jointArchetype);
#else
            return _ECB.CreateEntity(jointArchetype);
#endif            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(Entity e, in T component) where T : struct, IComponentData
        {
#if SLOW_ECB
            _EManager.SetComponentData<T>(e, component);
#else
            _ECB.SetComponent(e, component);
#endif    
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSharedComponent<T>(Entity e, in T component) where T : struct, ISharedComponentData
        {
#if SLOW_ECB
            _EManager.SetSharedComponentData<T>(e, component);
#else
            _ECB.SetSharedComponent(e, component);
#endif    
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Entity CreateDOTSEntityOnSvelto(Entity entityComponentPrefabEntity, EGID egid, bool mustHandleDOTSComponent)
        {
#if SLOW_ECB            
            Entity dotsEntity = _EManager.Instantiate(entityComponentPrefabEntity);
            
            //SharedComponentData can be used to group the DOTS ECS entities exactly like the Svelto ones
            _EManager.AddSharedComponentData(dotsEntity, new DOTSSveltoGroupID(egid.groupID));
            _EManager.AddComponentData(dotsEntity, new DOTSSveltoEGID(egid));
            if (mustHandleDOTSComponent)
                _EManager.AddSharedComponentData(dotsEntity, new DOTSEntityToSetup(egid.groupID));
#else       
            Entity dotsEntity = _ECB.Instantiate(entityComponentPrefabEntity);
            
            //SharedComponentData can be used to group the DOTS ECS entities exactly like the Svelto ones
            _ECB.AddSharedComponent(dotsEntity, new DOTSSveltoGroupID(egid.groupID));
            _ECB.AddComponent(dotsEntity, new DOTSSveltoEGID(egid));
            _ECB.AddSharedComponent<DOTSEntityToSetup>(dotsEntity, new DOTSEntityToSetup(egid.groupID));
#endif                

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
        internal Entity CreateDOTSEntityOnSvelto(EntityArchetype archetype, EGID egid, bool mustHandleDOTSComponent)
        {
#if SLOW_ECB            
            Entity dotsEntity = _EManager.CreateEntity(archetype);
            
            //SharedComponentData can be used to group the DOTS ECS entities exactly like the Svelto ones
            _EManager.AddSharedComponentData(dotsEntity, new DOTSSveltoGroupID(egid.groupID));
            _EManager.AddComponentData(dotsEntity, new DOTSSveltoEGID(egid));
            if (mustHandleDOTSComponent)
                _EManager.AddSharedComponentData(dotsEntity, new DOTSEntityToSetup(egid.groupID));
#else
            Entity dotsEntity = _ECB.CreateEntity(archetype);
            
            //SharedComponentData can be used to group the DOTS ECS entities exactly like the Svelto ones
            _ECB.AddSharedComponent(dotsEntity, new DOTSSveltoGroupID(egid.groupID));
            _ECB.AddComponent(dotsEntity, new DOTSSveltoEGID(egid));
            _ECB.AddSharedComponent<DOTSEntityToSetup>(dotsEntity, new DOTSEntityToSetup(egid.groupID));
#endif                

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
#if SLOW_ECB            
            return _EManager.CreateEntity(archetype);
#else      
            return _ECB.CreateEntity(archetype);
#endif    
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyEntity(Entity e)
        {
#if SLOW_ECB       
            _EManager.DestroyEntity(e);
#else    
            _ECB.DestroyEntity(e);
#endif    
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent<T>(Entity dotsEntity)
        {
#if SLOW_ECB            
            _EManager.RemoveComponent<T>(dotsEntity);
#else
            _ECB.RemoveComponent<T>(dotsEntity);
#endif    
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent<T>(Entity dotsEntity) where T : struct, IComponentData
        {
#if SLOW_ECB            
            _EManager.AddComponent<T>(dotsEntity);
#else
            _ECB.AddComponent<T>(dotsEntity);
#endif    
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent<T>(Entity dotsEntity, in T component) where T : struct, IComponentData
        {
#if SLOW_ECB            
            _EManager.AddComponentData(dotsEntity, component);
#else
            _ECB.AddComponent(dotsEntity, component);
#endif    
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSharedComponent<T>(Entity dotsEntity, in T component) where T : struct, ISharedComponentData
        {
#if SLOW_ECB            
            _EManager.AddSharedComponentData(dotsEntity, component);
#else
            _ECB.AddSharedComponent(dotsEntity, component);
#endif        
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBuffer<T>(Entity dotsEntity) where T : struct, IBufferElementData
        {
#if SLOW_ECB            
            _EManager.AddBuffer<T>(dotsEntity);
#else
            _ECB.AddBuffer<T>(dotsEntity);
#endif    
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityCommandBuffer.ParallelWriter AsParallelWriter()
        {
#if SLOW_ECB            
            throw new Exception();
#else            
            return _ECB.AsParallelWriter();
#endif            
        }
        
        readonly EntityCommandBuffer _ECB;
        readonly EntityManager       _EManager;
    }
}
#endif