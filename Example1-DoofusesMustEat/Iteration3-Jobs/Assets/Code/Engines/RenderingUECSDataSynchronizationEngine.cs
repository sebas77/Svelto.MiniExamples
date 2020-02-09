using Svelto.ECS.EntityStructs;
using Svelto.ECS.Extensions.Unity;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1B
{
    [DisableAutoCreation]
    public class RenderingUECSDataSynchronizationEngine : JobComponentSystem, IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle combinedDependencies = default;
            foreach (var group in GameGroups.DOOFUSES.Groups)
            {
                var collection = entitiesDB.QueryEntities<PositionEntityStruct>(group);

                if (collection.count == 0) continue;

                var entityCollection = collection.GetNativeEnumerator<PositionEntityStruct>();

                inputDeps = Entities.ForEach((ref Translation translation) =>
                                             {
                                                 ref readonly var positionEntityStruct =
                                                     ref entityCollection.threadSafeNext.position;

                                                 translation.Value =
                                                     new float3(positionEntityStruct.x, positionEntityStruct.y,
                                                                positionEntityStruct.z);
                                             }).WithBurst()
                                     //In order to fetch the unity entities from the same group of the svelto entities we will set 
                                     //the group as a filter
                                    .WithSharedComponentFilter(new UECSSveltoGroupID((uint) @group)).Schedule(inputDeps);

                combinedDependencies = JobHandle.CombineDependencies(combinedDependencies,
                                                                     new DisposeJob<EntityCollection<
                                                                             PositionEntityStruct>.EntityNativeIterator<
                                                                             PositionEntityStruct>>(entityCollection)
                                                                        .Schedule(inputDeps));
            }

            return combinedDependencies;
        }
    }
}