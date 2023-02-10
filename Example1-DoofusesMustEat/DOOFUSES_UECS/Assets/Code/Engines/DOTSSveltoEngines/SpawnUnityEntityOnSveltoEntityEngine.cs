using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.DoofusesDOTS
{
    /// <summary>
    /// In a Svelto<->UECS scenario, is common to have UECS entity created on creation of Svelto ones. Same for
    /// destruction.
    /// Note this can be easily moved to using Entity Command Buffer and I should do it at a given point
    /// but at the moment (Entities 0.17) ECB performance is really bad when ShareComponents are used, which
    /// I need to rely on
    /// </summary>
    public class SpawnUnityEntityOnSveltoEntityEngine: SveltoOnDOTSHandleCreationEngine,
            IReactOnAddEx<SpawnPointEntityComponent>, IQueryingEntitiesEngine
    {
        public void Add((uint start, uint end) rangeOfEntities,
            in EntityCollection<SpawnPointEntityComponent> collection, ExclusiveGroupStruct groupID)
        {
            var (buffer, entityIDs, _) = collection;

            new SpawnJob(entityIDs, buffer, groupID, ECB.AsParallelWriter(), rangeOfEntities.start)
                   .ScheduleParallel(rangeOfEntities.end - rangeOfEntities.start, default).Complete();
        }

        public override string name => nameof(SpawnUnityEntityOnSveltoEntityEngine);

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }

        [BurstCompile]
        readonly struct SpawnJob: IJobParallelFor
        {
            readonly NativeEntityIDs _entityIDs;
            readonly NB<SpawnPointEntityComponent> _spawnPoints;
            readonly ExclusiveGroupStruct _groupID;
            readonly EntityCommandBuffer.ParallelWriter _ECB;
            readonly uint _start;

            public SpawnJob(NativeEntityIDs entityIDs, NB<SpawnPointEntityComponent> spawnPoints,
                ExclusiveGroupStruct groupID, EntityCommandBuffer.ParallelWriter ecb, uint start)
            {
                _entityIDs = entityIDs;
                _spawnPoints = spawnPoints;
                _groupID = groupID;
                _ECB = ecb;
                _start = start;
            }

            public void Execute(int currentIndex)
            {
                int index = (int)(_start + currentIndex);

                var entity = _entityIDs[index];
                ref var spawnComponent = ref _spawnPoints[index];

                var dotsEntity = EntityCommandBufferForSvelto.CreateDOTSEntityOnSvelto(
                    index, _ECB, spawnComponent.prefabEntity, new EGID(entity, _groupID), true);

                //setting the initial DOTS translation, necessary for the food since the food entities do not sync translation
                _ECB.SetComponent(
                    index, dotsEntity, new Translation
                    {
                            Value = spawnComponent.spawnPosition
                    });
            }
        }
    }
}