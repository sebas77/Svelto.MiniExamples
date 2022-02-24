using System;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.SveltoOnDOTS;

namespace Svelto.ECS.MiniExamples.Example1C
{
    /// <summary>
    /// Now that the blue doofuses entities have been submitted, we fetch them to add them to filters according
    /// the mesh uses (blue or specialblue)
    /// </summary>
    public class SetFiltersOnBlueDoofusesSpawned : SveltoOnDOTSHandleCreationEngine,
        IReactOnAddEx<SpawnPointEntityComponent>, IQueryingEntitiesEngine
    {
        public void Add((uint start, uint end) rangeOfEntities,
            in EntityCollection<SpawnPointEntityComponent> collection, ExclusiveGroupStruct groupID)
        {
            if (groupID == GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup)
            {
                var sveltoNativeFilters = entitiesDB.GetFilters();

                //We know that if the entity is created in the BLUE_DOOFUSES_NOT_EATING group, 
                //SpawnPointEntityComponent and PositionEntityComponent elements will be aligned
                //and indexable with the same index.
                var specialBlueFilters =
                    sveltoNativeFilters.GetOrCreatePersistentFilter<PositionEntityComponent>(GameFilters
                       .SPECIAL_BLUE_DOOFUSES_MESHES);
                EntityFilterCollection blueFilters =
                    sveltoNativeFilters.GetOrCreatePersistentFilter<PositionEntityComponent>(GameFilters
                       .BLUE_DOOFUSES_MESHES);

                var specialBlueFilter = specialBlueFilters.GetGroupFilter(groupID);
                var blueFilter        = blueFilters.GetGroupFilter(groupID);

                var (buffer, entityIDs, _) = collection;
                
                int countSpecial = 0, count = 0;

                for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
                {
                    ref var entityComponent = ref buffer[i];

                    if (entityComponent.isSpecial)
                    {
                        //This filter already know the group, so it needs only the entityID, plus the position
                        //of the entity in the array.
                        specialBlueFilter.Add(entityIDs[i], i);
                        countSpecial++;
                    }
                    else
                    {
                        blueFilter.Add(entityIDs[i], i);
                        count++;
                    }
                }
            }
        }
        
        public override string name => nameof(SpawnUnityEntityOnSveltoEntityEngine);
        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }
    }
}