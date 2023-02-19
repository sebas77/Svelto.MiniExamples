#if UNITY_ECS
using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Svelto.ECS.SveltoOnDOTS
{
    public readonly struct DOTSBatchedOperationsForSvelto
    {
        internal DOTSBatchedOperationsForSvelto(EntityManager manager)
        {
            _EManager = manager;
        }
        
        public EntityArchetype CreateArchetype(params ComponentType[] types)
        {
            return _EManager.CreateArchetype(types);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(Entity e, in T component) where T : unmanaged, IComponentData
        {
            _EManager.SetComponentData(e, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSharedComponent<T>(Entity e, in T component) where T : unmanaged, ISharedComponentData
        {
            _EManager.SetSharedComponent(e, component);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Entity CreateDOTSEntityOnSvelto(Entity prefabEntity, ExclusiveGroupStruct groupID, EntityReference reference)
        {
            Entity dotsEntity = _EManager.Instantiate(prefabEntity);

            //SharedComponentData can be used to group the DOTS ECS entities exactly like the Svelto ones
            _EManager.AddSharedComponent(dotsEntity, new DOTSSveltoGroupID(groupID));
            _EManager.AddComponent<DOTSSveltoReference>(dotsEntity);
            _EManager.SetComponentData(dotsEntity, new DOTSSveltoReference(reference));

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
        internal Entity CreateDOTSEntityOnSvelto(EntityArchetype archetype, ExclusiveGroupStruct groupID, EntityReference reference)
        {
            Entity dotsEntity = _EManager.CreateEntity(archetype);

            //SharedComponentData can be used to group the DOTS ECS entities exactly like the Svelto ones
            _EManager.AddSharedComponent(dotsEntity, new DOTSSveltoGroupID(groupID));
            _EManager.AddComponent<DOTSSveltoReference>(dotsEntity);
            _EManager.SetComponentData(dotsEntity, new DOTSSveltoReference(reference));
            
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
        internal Entity CreateDOTSEntity(EntityArchetype archetype)
        {
            return _EManager.CreateEntity(archetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyEntity(Entity e)
        {
            _EManager.DestroyEntity(e);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent<T>(Entity dotsEntity)
        {
            _EManager.RemoveComponent<T>(dotsEntity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent<T>(Entity dotsEntity) where T : unmanaged, IComponentData
        {
            _EManager.AddComponent<T>(dotsEntity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent<T>(Entity dotsEntity, in T component) where T : unmanaged, IComponentData
        {
            _EManager.AddComponent<T>(dotsEntity);
            _EManager.SetComponentData(dotsEntity, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSharedComponent<T>(Entity dotsEntity, in T component) where T : unmanaged, ISharedComponentData
        {
            _EManager.AddSharedComponent(dotsEntity, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBuffer<T>(Entity dotsEntity) where T : unmanaged, IBufferElementData
        {
            _EManager.AddBuffer<T>(dotsEntity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSharedComponentBatched<SharedComponentData>(NativeArray<Entity> nativeArray, SharedComponentData SCD)
                where SharedComponentData : unmanaged, ISharedComponentData
        {
            //does this need to be wrapped in a burst pointer func?
            _EManager.SetSharedComponent(nativeArray, SCD);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<Entity> CreateDOTSEntityOnSveltoBatched(Entity dotsEntity, int range, ExclusiveGroupStruct groupID, Allocator allocator)
        {
            var nativeArray = _EManager.Instantiate(dotsEntity, range, allocator);
            _EManager.AddSharedComponent(nativeArray, new DOTSSveltoGroupID(groupID));
            _EManager.AddComponent<DOTSSveltoReference>(nativeArray);
                return nativeArray;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyEntitiesBatched(NativeArray<Entity> nativeArray)
        {
            _EManager.DestroyEntity(nativeArray);
        }
        
        readonly EntityManager       _EManager;
    }
}
#endif