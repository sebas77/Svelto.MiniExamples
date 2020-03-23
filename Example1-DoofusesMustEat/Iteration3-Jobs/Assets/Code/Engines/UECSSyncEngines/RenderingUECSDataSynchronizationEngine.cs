using Svelto.ECS.EntityStructs;
using Svelto.ECS.Extensions.Unity;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [DisableAutoCreation]
    public class RenderingUECSDataSynchronizationEngine : SystemBase, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        protected override void OnUpdate()
        {
            foreach (var group in GameGroups.DOOFUSES.Groups)
            {
                var collection = entitiesDB.QueryEntities<PositionEntityStruct>(group);

                if (collection.count == 0) continue;

                //there are usually two ways to sync Svelto entities with UECS entities
                //In some cases, like for the rendering, the 1:1 relationship is not necessary, hence UECS entities
                //just become a pool of entities to fetch and assign values to. Of course we need to be sure that the
                //entities are compatible, that's why we group the UECS entities like with do with the Svelto ones, using
                //the UECS shared component UECSSveltoGroupID.
                var entityCollection = collection.GetNativeEnumerator<PositionEntityStruct>();

                //when it's time to sync, I have two options, iterate the svelto entities first or iterate the
                //UECS entities first. 
                var deps = Entities.ForEach((ref Translation translation) =>
                    {
                        ref readonly var positionEntityStruct = ref entityCollection.threadSafeNext.position;

                        translation.Value =
                            new float3(positionEntityStruct.x, positionEntityStruct.y, positionEntityStruct.z);
                    }).WithBurst()
                    //In order to fetch the unity entities from the same group of the svelto entities we will set 
                    //the group as a filter
                    .WithSharedComponentFilter(new UECSSveltoGroupID((uint) @group)).Schedule(Dependency);

                Dependency = new DisposeJob<EntityCollection<PositionEntityStruct>.EntityNativeIterator<
                        PositionEntityStruct>>(entityCollection).Schedule(deps);
            }
        }
    }
}