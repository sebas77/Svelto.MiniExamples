#if UNITY_ECS
using Svelto.ECS.Native;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Allocator = Svelto.Common.Allocator;

namespace Svelto.ECS.SveltoOnDOTS
{
    public class SveltoOnDOTSHandleLifeTimeEngine<EntityComponent> : HandleLifeTimeEngine
                                                                   , IReactOnRemove<EntityComponent>
                                                                   , IReactOnSwap<EntityComponent>
        where EntityComponent : unmanaged, IEntityComponentForDOTS
    {
        public override void SetupQuery(EntityManager entityManager)
        {
            query = entityManager.CreateEntityQuery(typeof(DOTSEntityToSetup), typeof(DOTSSveltoEGID)
                                                  , typeof(DOTSSveltoGroupID));
        }

        public virtual void MovedTo(ref EntityComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            ECB.SetSharedComponent(entityComponent.dotsEntity, new DOTSSveltoGroupID(egid.groupID));

            ECB.SetComponent(entityComponent.dotsEntity, new DOTSSveltoEGID
            {
                egid = egid
            });
        }

        public virtual void Remove(ref EntityComponent entityComponent, EGID egid)
        {
            ECB.DestroyEntity(entityComponent.dotsEntity);
        }

        //Note: when this is called, the CommandBuffer is flushed so the not temporary DOTS entity ID will be used
        public override JobHandle ConvertPendingEntities(JobHandle jobHandle)
        {
            foreach (var group in entitiesDB.FindGroups<EntityComponent>())
            {
                var mapper = entitiesDB.QueryNativeMappedEntities<EntityComponent>(group);

                query.SetSharedComponentFilter(new DOTSSveltoGroupID(group));

                NativeArray<Entity> entityArray =
                    query.ToEntityArrayAsync((global::Unity.Collections.Allocator)Allocator.TempJob, out var handle1);
                NativeArray<DOTSSveltoEGID> componentArray =
                    query.ToComponentDataArrayAsync<DOTSSveltoEGID>(
                        (global::Unity.Collections.Allocator)Allocator.TempJob, out var handle2);

                jobHandle = JobHandle.CombineDependencies(jobHandle, handle1, handle2);

                var convertEntitiesJob = new Job()
                {
                    entities   = entityArray
                  , components = componentArray
                  , mapper     = mapper
                };

                jobHandle = convertEntitiesJob.ScheduleParallel(entityArray.Length, jobHandle);

                entityArray.Dispose(jobHandle);
                componentArray.Dispose(jobHandle);
            }

            return jobHandle;
        }

        struct Job : IJobParallelFor
        {
            public NativeArray<Entity>               entities;
            public NativeEGIDMapper<EntityComponent> mapper;
            public NativeArray<DOTSSveltoEGID>       components;

            public void Execute(int index)
            {
                mapper.Entity(components[index].egid.entityID).dotsEntity = entities[index];
            }
        }
    }
}
#endif