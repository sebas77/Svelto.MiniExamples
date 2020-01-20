using Svelto.ECS.EntityStructs;
using Svelto.ECS.Extensions.Unity;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1B
{
    [DisableAutoCreation]
    public class RenderingDataSynchronizationEngine : JobComponentSystem, IQueryingEntitiesEngine
    {
        EntityQuery        m_Group;
        public IEntitiesDB entitiesDB { get; set; }

        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<UnityECSDoofusesGroup>());
        }

        public void Ready() { }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle combinedDependencies = default;
            foreach (var group in GameGroups.DOOFUSES.Groups)
            {
                var collection       = entitiesDB.QueryEntities<PositionEntityStruct>(group);
                
                if (collection.length == 0) continue;
                
                var entityCollection = collection.GetNativeEnumerator<PositionEntityStruct>();

                var deps = new RenderingSyncJob(entityCollection).Schedule(m_Group, inputDeps);
                combinedDependencies = JobHandle.CombineDependencies(combinedDependencies,
                    new DisposeJob<EntityCollection<PositionEntityStruct>.EntityNativeIterator<PositionEntityStruct>>
                            (entityCollection).Schedule(deps));
            }

            return combinedDependencies;
        }

        [BurstCompile]
        struct RenderingSyncJob : IJobForEach<Translation, UnityECSDoofusesGroup>
        {
            EntityCollection<PositionEntityStruct>.EntityNativeIterator<PositionEntityStruct> entityCollection;

            public RenderingSyncJob(
                EntityCollection<PositionEntityStruct>.EntityNativeIterator<PositionEntityStruct> entityCollection)
            {
                this.entityCollection = entityCollection;
            }

            public void Execute(ref Translation translation, [ReadOnly] ref UnityECSDoofusesGroup c1)
            {
                ref readonly var positionEntityStruct = ref entityCollection.threadSafeNext.position;

                translation.Value = new float3(positionEntityStruct.x, positionEntityStruct.y, positionEntityStruct.z);
            }
        }
    }
}