using System;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Entities;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [DisableAutoCreation]
    public class RenderingDOTSDataSynchronizationEngine : SyncSveltoToDOTSEngine, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
        }

        protected override void OnUpdate()
        {
            EntityFilterCollection blueFilters = entitiesDB.GetFilters()
               .GetOrCreatePersistentFilter<PositionEntityComponent>(GameFilters.BLUE_DOOFUSES_MESHES);
            
            foreach (var (indices, group) in blueFilters)
            {
                var (positions, _) = entitiesDB.QueryEntities<PositionEntityComponent>(@group);
            
                Entities.WithNone<SpecialBlue>().ForEach((int entityInQueryIndex, ref Translation translation) =>
                    {
                        ref readonly var positionEntityComponent = ref positions[indices[entityInQueryIndex]];
            
                        translation.Value = positionEntityComponent.position;
                    })
                    //In order to fetch the unity entities from the same group of the svelto entities we will set 
                    //the group as a filter. The data is set in such a way each group handles a different prefab
                    //but what if I want one group to handle multiple prefabs?
                   .WithSharedComponentFilter(new DOTSSveltoGroupID(@group)).ScheduleParallel();
            }
            
            EntityFilterCollection specialBlueFilters = entitiesDB.GetFilters()
               .GetOrCreatePersistentFilter<PositionEntityComponent>(GameFilters.SPECIAL_BLUE_DOOFUSES_MESHES);
            
            foreach (var (indices, group) in specialBlueFilters)
            {
                var (positions, _) = entitiesDB.QueryEntities<PositionEntityComponent>(@group);

                Entities.WithAny<SpecialBlue>().ForEach((int entityInQueryIndex, ref Translation translation) =>
                    {
                        ref readonly var positionEntityComponent = ref positions[indices[entityInQueryIndex]];
            
                        translation.Value = positionEntityComponent.position;
                    })
                    //In order to fetch the unity entities from the same group of the svelto entities we will set 
                    //the group as a filter. The data is set in such a way each group handles a different prefab
                    //but what if I want one group to handle multiple prefabs?
                   .WithSharedComponentFilter(new DOTSSveltoGroupID(@group)).ScheduleParallel();
            }
            
            foreach (var ((positions, _), group) in entitiesDB.QueryEntities<PositionEntityComponent>(GameGroups
                        .RED.Groups))
            {
                //there are usually two ways to sync Svelto entities with UECS entities
                //In some cases, like for the rendering, the 1:1 relationship is not necessary, hence UECS entities
                //just become a pool of entities to fetch and assign values to. Of course we need to be sure that the
                //entities are compatible, that's why we group the UECS entities like with do with the Svelto ones, using
                //the UECS shared component UECSSveltoGroupID.
            
                //when it's time to sync, I have two options, iterate the svelto entities first or iterate the
                //UECS entities first. 
                Entities.ForEach((int entityInQueryIndex, ref Translation translation) =>
                    {
                        ref readonly var positionEntityComponent = ref positions[entityInQueryIndex];
            
                        translation.Value = positionEntityComponent.position;
                    })
                    //In order to fetch the unity entities from the same group of the svelto entities we will set 
                    //the group as a filter. The data is set in such a way each group handles a different prefab
                    //but what if I want one group to handle multiple prefabs?
                   .WithSharedComponentFilter(new DOTSSveltoGroupID(@group)).ScheduleParallel();
            }
        }

        public override string name => nameof(RenderingDOTSDataSynchronizationEngine);
    }
}