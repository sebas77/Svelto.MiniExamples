using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Internal;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Allocator = Unity.Collections.Allocator;

namespace Svelto.ECS.MiniExamples.DoofusesDOTS
{
    /// <summary>
    /// In a Svelto<->UECS scenario, is common to have DOTS ECS entity built on creation of Svelto ones.
    /// todo note on destruction and moveto
    /// </summary>
    public class SpawnUnityEntityOnSveltoEntityEngine: SveltoOnDOTSHandleStructuralChangesEngine,
            IReactOnAddEx<DOTSEntityComponent>, IQueryingEntitiesEngine
    {
        public override string name => nameof(SpawnUnityEntityOnSveltoEntityEngine);

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }
        
        public void Add((uint start, uint end) rangeOfEntities,
            in EntityCollection<DOTSEntityComponent> entities, ExclusiveGroupStruct groupID)
        {
            var (buffer, ids, _) = entities;
            var (positions, _) = entitiesDB.QueryEntities<PositionEntityComponent>(groupID);

            job.spawnPoints = positions;
            job.DOSTEntityComponents = buffer;
            job.ids = ids;
            job.sveltoStartIndex = rangeOfEntities.start;
            job.entityReferenceMap = entitiesDB.GetEntityReferenceMap(groupID);

            using (new PlatformProfiler("CreateDOTSEntityOnSveltoBatched"))
            {
                var sveltoOnDOTSEntities = DOTSOperations.CreateDOTSEntityOnSveltoBatched(
                    buffer[0].dotsEntity, (int)(rangeOfEntities.end - rangeOfEntities.start), groupID, Allocator.TempJob);

                job.createdEntities = sveltoOnDOTSEntities;
            }
        }

        protected override JobHandle OnPostSubmission()
        {
            if (job.createdEntities.IsCreated)
            {
                job.entityManager = DOTSOperations;

                return job.ScheduleParallel(job.createdEntities.Length, default);
            }

            return default;
        }

        protected override void CleanUp()
        {
            if (job.createdEntities.IsCreated)
                job.createdEntities.Dispose();
        }

        SpawnJob job;

        [BurstCompile]
        struct SpawnJob: IJobParallelFor
        {
            public NB<PositionEntityComponent> spawnPoints;
            public uint sveltoStartIndex;
            [ReadOnly] public NativeArray<Entity> createdEntities;
            [NativeDisableParallelForRestriction] public DOTSBatchedOperationsForSvelto entityManager;
            public NativeEntityIDs ids;
            public SharedSveltoDictionaryNative<uint, EntityReference> entityReferenceMap;
            public NB<DOTSEntityComponent> DOSTEntityComponents;

            public void Execute(int currentIndex)
            {
                int index = (int)(sveltoStartIndex + currentIndex);
                var dotsEntity = createdEntities[currentIndex];
                
                DOSTEntityComponents[index].dotsEntity = dotsEntity;

                entityManager.SetComponent(
                    dotsEntity, new DOTSSveltoReference
                    {
                        entityReference = entityReferenceMap[ids[index]]
                    });
                
                ref PositionEntityComponent spawnComponent = ref spawnPoints[index];

                entityManager.SetComponent(
                    dotsEntity, new Translation()
                    {
                        Value = spawnComponent.position
                    });
            }
        }
    }
}