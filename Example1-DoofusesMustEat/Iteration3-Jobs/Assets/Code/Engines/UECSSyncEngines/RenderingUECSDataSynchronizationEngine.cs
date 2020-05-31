using Svelto.DataStructures;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Extensions.Unity;
using Unity.Entities;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [DisableAutoCreation]
    public class RenderingUECSDataSynchronizationEngine : SyncSveltoToUECSEngine, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        protected override void Execute() 
        {
            foreach (var group in GameGroups.DOOFUSES.Groups)
            {
                EntityCollection<PositionEntityComponent> collection =
                    entitiesDB.QueryEntities<PositionEntityComponent>(group);

                if (collection.count == 0)
                    continue;

                //there are usually two ways to sync Svelto entities with UECS entities
                //In some cases, like for the rendering, the 1:1 relationship is not necessary, hence UECS entities
                //just become a pool of entities to fetch and assign values to. Of course we need to be sure that the
                //entities are compatible, that's why we group the UECS entities like with do with the Svelto ones, using
                //the UECS shared component UECSSveltoGroupID.
                NB<PositionEntityComponent> entityCollection = collection.ToBuffer().buffer;

                //when it's time to sync, I have two options, iterate the svelto entities first or iterate the
                //UECS entities first. 
                Entities.ForEach((int entityInQueryIndex, ref Translation translation) =>
                   {
                       ref readonly var positionEntityComponent = ref entityCollection[entityInQueryIndex];

                       translation.Value = positionEntityComponent.position;
                   }).WithBurst()
                   //In order to fetch the unity entities from the same group of the svelto entities we will set 
                   //the group as a filter
                  .WithSharedComponentFilter(new UECSSveltoGroupID((uint) @group))
                  .ScheduleParallel();
            }
        }
    }
}