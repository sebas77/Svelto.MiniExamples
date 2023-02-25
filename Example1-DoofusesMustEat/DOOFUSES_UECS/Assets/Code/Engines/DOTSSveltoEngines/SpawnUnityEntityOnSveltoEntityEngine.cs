using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.EntityComponents;
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
    /// In a Svelto<->DOTS ECS scenario, is common to have DOTS ECS entities built on creation of Svelto ones.
    /// These engines are usually specialised and must implement ISveltoOnDOTSStructuralEngine
    /// to give to the user the tools necessary to spawn DOTS entities.
    /// todo note on destruction and moveto
    /// todo need to simplify this
    /// </summary>
    public class SpawnUnityEntityOnSveltoEntityEngine: ISveltoOnDOTSStructuralEngine,
            IReactOnAddEx<DOTSEntityComponent>, IQueryingEntitiesEngine
    {
        public DOTSOperationsForSvelto DOTSOperations { get; set; }

        public string name => nameof(SpawnUnityEntityOnSveltoEntityEngine);

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }

        //Collect all the DOTSEntityComponent entities submitted this frame and create a DOTS entity for each of them
        public void Add((uint start, uint end) rangeOfEntities,
            in EntityCollection<DOTSEntityComponent> entities, ExclusiveGroupStruct groupID)
        {
            var (sveltoOnDOTSEntities, _) = entities;
            var (positions, _) = entitiesDB.QueryEntities<PositionEntityComponent>(groupID);

            SyncDOTSPosition job = default;
            job.spawnPoints = positions;
            job.sveltoStartIndex = rangeOfEntities.start;
            job.entityManager = DOTSOperations;

            using (new PlatformProfiler("CreateDOTSEntityOnSveltoBatched"))
            {
                //Standard way to create DOTS entities from a Svelto ones. The returning job must be completed by the end of the frame
                var DOTSEntities = DOTSOperations.CreateDOTSEntityOnSveltoBatched(
                    sveltoOnDOTSEntities[0].dotsEntity, rangeOfEntities, groupID, sveltoOnDOTSEntities);

                job.createdEntities = DOTSEntities;
            }
            
            var jobHandle = job.ScheduleParallel(job.createdEntities.Length, default);
            DOTSOperations.AddJobToComplete(jobHandle);
        }

        [BurstCompile]
        struct SyncDOTSPosition: IJobParallelFor
        {
            public NB<PositionEntityComponent> spawnPoints;
            public uint sveltoStartIndex;
            [ReadOnly] public NativeArray<Entity> createdEntities;
            [NativeDisableParallelForRestriction] public DOTSOperationsForSvelto entityManager;

            public void Execute(int currentIndex)
            {
                int index = (int)(sveltoStartIndex + currentIndex);
                var dotsEntity = createdEntities[currentIndex];

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