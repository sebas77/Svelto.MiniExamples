using System;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Entities;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.DoofusesDOTS
{
    [DisableAutoCreation]
    public partial class RenderingDOTSDataSynchronizationEngine : SyncSveltoToDOTSEngine, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        //add a not about the fact it's not synchronising food
        protected override void OnSveltoUpdate()
        {
            var sveltoFilters = entitiesDB.GetFilters();
            
            //find all the filters where BLUE_DOFFUSES are found. Blue DOOFUSES are found in a set with a material being blue
            EntityFilterCollection blueFilters = sveltoFilters
               .GetOrCreatePersistentFilter<PositionEntityComponent>(GameFilters.BLUE_DOOFUSES_MESHES);
            
            //TODO rewrite this comment, explain that SpecialBlue can be used here since this is a specialised engine
            //there are usually two ways to sync Svelto entities with UECS entities
            //In some cases, like for the rendering, the 1:1 relationship is not necessary, hence UECS entities
            //just become a pool of entities to fetch and assign values to. Of course we need to be sure that the
            //entities are compatible, that's why we group the UECS entities like with do with the Svelto ones, using
            //the UECS shared component UECSSveltoGroupID.
            
            //when it's time to sync, I have two options, iterate the svelto entities first or iterate the
            //UECS entities first. 
//            foreach (var (filterIndices, group) in blueFilters)
//            {
//                var (positions, _) = entitiesDB.QueryEntities<PositionEntityComponent>(@group);
//            
//                //All the blue doofuses are the same under the Svelto point of view, so they can be considered a pool and the order
//                //or 1:1 relations ship doesn't count
//                Entities.WithNone<SpecialBluePrefab>().ForEach((int entityInQueryIndex, ref Translation translation) =>
//                    {
//                        ref readonly var positionEntityComponent = ref positions[filterIndices[entityInQueryIndex]];
//            
//                        translation.Value = positionEntityComponent.position;
//                    })
//                    //In order to fetch the unity entities from the same group of the svelto entities we will set 
//                    //the group as a filter. The data is set in such a way each group handles a different prefab
//                    //but what if I want one group to handle multiple prefabs? Filters allow solving the issue as I can
//                    //sub group Svelto groups through them.
//                   .WithSharedComponentFilter(new DOTSSveltoGroupID(@group)).ScheduleParallel();
//            }
//            
//            EntityFilterCollection specialBlueFilters = sveltoFilters
//               .GetOrCreatePersistentFilter<PositionEntityComponent>(GameFilters.SPECIAL_BLUE_DOOFUSES_MESHES);
//            
//            foreach (var (filterIndices, group) in specialBlueFilters)
//            {
//                var (positions, _) = entitiesDB.QueryEntities<PositionEntityComponent>(@group);
//
//                Entities.WithAny<SpecialBluePrefab>().ForEach((int entityInQueryIndex, ref Translation translation) =>
//                    {
//                        ref readonly var positionEntityComponent = ref positions[filterIndices[entityInQueryIndex]];
//            
//                        translation.Value = positionEntityComponent.position;
//                    }).WithSharedComponentFilter(new DOTSSveltoGroupID(@group)).ScheduleParallel();
//            }
            
            foreach (var ((positions, _), group) in entitiesDB.QueryEntities<PositionEntityComponent>(GameGroups
                        .RED.Groups))
            {
                Entities.ForEach((int entityInQueryIndex, ref Translation translation) =>
                    {
                        ref readonly var positionEntityComponent = ref positions[entityInQueryIndex];
            
                        translation.Value = positionEntityComponent.position;
                    })
                       .WithSharedComponentFilter(new DOTSSveltoGroupID(@group)).ScheduleParallel();
            }
        }

        public override string name => nameof(RenderingDOTSDataSynchronizationEngine);
    }
}
