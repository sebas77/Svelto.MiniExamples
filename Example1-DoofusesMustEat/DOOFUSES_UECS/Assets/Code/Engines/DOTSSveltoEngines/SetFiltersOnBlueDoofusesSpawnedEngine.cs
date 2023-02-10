using Svelto.DataStructures;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Internal;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Burst;
using Unity.Jobs;

namespace Svelto.ECS.MiniExamples.DoofusesDOTS
{
    /// <summary>
    /// Now that the blue doofuses entities have been submitted, we fetch them to add them to filters according
    /// the mesh uses (blue or specialblue)
    /// </summary>
    public class SetFiltersOnBlueDoofusesSpawnedEngine : SveltoOnDOTSHandleCreationEngine,
        IReactOnAddEx<SpawnPointEntityComponent>, IQueryingEntitiesEngine
    {
        public void Add((uint start, uint end) rangeOfEntities,
            in EntityCollection<SpawnPointEntityComponent> collection, ExclusiveGroupStruct groupID)
        {
            if (groupID == GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup)
            {
                EntitiesDB.SveltoFilters sveltoNativeFilters = entitiesDB.GetFilters();

                //We know that if the entity is created in the BLUE_DOOFUSES_NOT_EATING group, 
                //SpawnPointEntityComponent and PositionEntityComponent elements will be aligned
                //and indexable with the same index.
                var specialBlueFilters =
                    sveltoNativeFilters.GetOrCreatePersistentFilter<PositionEntityComponent>(GameFilters
                       .SPECIAL_BLUE_DOOFUSES_MESHES);
                EntityFilterCollection blueFilters =
                    sveltoNativeFilters.GetOrCreatePersistentFilter<PositionEntityComponent>(GameFilters
                       .BLUE_DOOFUSES_MESHES);

                var specialBlueFilter = specialBlueFilters.GetOrCreateGroupFilter(groupID);
                var blueFilter        = blueFilters.GetOrCreateGroupFilter(groupID);

                var (buffer, entityIDs, _) = collection;

                new SpawningJob()
                {
                    blueFilter        = blueFilter,
                    specialBlueFilter = specialBlueFilter,
                    entityIDs         = entityIDs,
                    collection        = buffer,
                    start             = rangeOfEntities.start
                }.Run((int)(rangeOfEntities.end - rangeOfEntities.start));
            }
        }

        public override string name => nameof(SpawnUnityEntityOnSveltoEntityEngine);

        public void Ready()
        {
        }

        public EntitiesDB entitiesDB { get; set; }

        [BurstCompile]
        struct SpawningJob : IJobFor
        {
            public NB<SpawnPointEntityComponent>       collection;
            public EntityFilterCollection.GroupFilters blueFilter;
            public EntityFilterCollection.GroupFilters specialBlueFilter;
            public NativeEntityIDs                     entityIDs;
            public uint                                start;

            public void Execute(int index)
            {
                index = (int)(index + start);

                ref var entityComponent = ref collection[index];

                if (entityComponent.isSpecial)
                {
                    //This filter already know the group, so it needs only the entityID, plus the position
                    //of the entity in the array.
                    specialBlueFilter.Add(entityIDs[index], (uint)index);
                }
                else
                {
                    blueFilter.Add(entityIDs[index], (uint)index);
                }
            }
        }
    }
}