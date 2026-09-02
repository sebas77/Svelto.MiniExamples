using System;
using Svelto.DataStructures;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.MiniExamples.DoofusesDOTS;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.DoofusesDOTS
{
    /// <summary>
    /// Sync SveltoTODOTS engines are also DOTS ECS systems and MUST BE added explicitly using SveltoOnDOTS methods 
    /// </summary>
    [DisableAutoCreation]
    public partial class RenderingDOTSPositionSyncEngine: SyncSveltoToDOTSEngine, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        protected override void OnCreate()
        {
            _blueDoofusesQuery = new EntityQueryBuilder(Allocator.Temp)
               .WithAllRW<LocalTransform>()
               .WithAll<DOTSSveltoGroupID>()
               .WithNone<SpecialBluePrefab>()
               .Build(this);

            _specialBlueDoofusesQuery = new EntityQueryBuilder(Allocator.Temp)
               .WithAllRW<LocalTransform>()
               .WithAll<DOTSSveltoGroupID>()
               .WithAny<SpecialBluePrefab>()
               .Build(this);

            _redDoofusesQuery = new EntityQueryBuilder(Allocator.Temp)
               .WithAllRW<LocalTransform>()
               .WithAll<DOTSSveltoGroupID>()
               .Build(this);
        }

        //add a not about the fact it's not synchronising food
        protected override void OnSveltoUpdate()
        {
            var sveltoFilters = entitiesDB.GetFilters();

            //find all the filters where BLUE_DOFFUSES are found. Blue DOOFUSES are found in a set with a material being blue
            EntityFilterCollection blueFilters = sveltoFilters.GetPersistentFilter<PositionEntityComponent>(GameFilters.BLUE_DOOFUSES_MESHES);

            //sync engines are usually semi specialised engines. They can get quite abstract using FindGroup, or they can be semi-abstract
            //using GroupCompounds like in this example
            //Being this engine semi-abstract, it knows about SpecialBluePrefab and can use the tag to filter DOTS ECS entities

            //In some cases, like for the rendering, the 1:1 relationship is not necessary, hence DOTS ECS entities
            //just become a pool of entities to fetch and assign values to. Of course we need to be sure that the
            //entities are compatible, that's why we group the DOTS ECS entities like with do with the Svelto ones, using
            //the DOTS ECS shared component DOTS ECSSveltoGroupID.

            //when it's time to sync, I have two options, iterate the svelto entities first or iterate the
            //DOTS ECS entities first. 
            foreach ((EntityFilterIndices filterIndices, ExclusiveGroupStruct group) in blueFilters)
            {
                var (positions, _) = entitiesDB.QueryEntities<PositionEntityComponent>(@group);

                //All the blue doofuses are the same under the Svelto point of view, so they can be considered a pool and the order
                //or 1:1 relations ship doesn't count
                //In order to fetch the unity entities from the same group of the svelto entities we will set
                //the group as a filter. The data is set in such a way each group handles a different prefab
                //but what if I want one group to handle multiple prefabs? Filters allow solving the issue as I can
                //sub group Svelto groups through them.
                _blueDoofusesQuery.SetSharedComponentFilter(new DOTSSveltoGroupID(@group));

                Dependency = new SyncFilteredPositionsJob
                {
                    positions = positions, filterIndices = filterIndices
                }.ScheduleParallel(_blueDoofusesQuery, Dependency);
            }

            EntityFilterCollection specialBlueFilters = sveltoFilters
                   .GetPersistentFilter<PositionEntityComponent>(GameFilters.SPECIAL_BLUE_DOOFUSES_MESHES);

            foreach (var (filterIndices, group) in specialBlueFilters)
            {
                var (positions, _) = entitiesDB.QueryEntities<PositionEntityComponent>(@group);

                _specialBlueDoofusesQuery.SetSharedComponentFilter(new DOTSSveltoGroupID(@group));

                Dependency = new SyncFilteredPositionsJob
                {
                    positions = positions, filterIndices = filterIndices
                }.ScheduleParallel(_specialBlueDoofusesQuery, Dependency);
            }

            foreach (var ((positions, _), group) in entitiesDB.QueryEntities<PositionEntityComponent>(GameGroups.RED.Groups))
            {
                _redDoofusesQuery.SetSharedComponentFilter(new DOTSSveltoGroupID(@group));

                Dependency = new SyncPositionsJob
                {
                    positions = positions
                }.ScheduleParallel(_redDoofusesQuery, Dependency);
            }
        }

        public override string name => nameof(RenderingDOTSPositionSyncEngine);

        [BurstCompile]
        partial struct SyncFilteredPositionsJob : IJobEntity
        {
            public NB<PositionEntityComponent> positions;
            public EntityFilterIndices filterIndices;

            void Execute([EntityIndexInQuery] int entityInQueryIndex, ref LocalTransform translation)
            {
                translation.Position = positions[filterIndices[entityInQueryIndex]].position;
            }
        }

        [BurstCompile]
        partial struct SyncPositionsJob : IJobEntity
        {
            public NB<PositionEntityComponent> positions;

            void Execute([EntityIndexInQuery] int entityInQueryIndex, ref LocalTransform translation)
            {
                translation.Position = positions[entityInQueryIndex].position;
            }
        }

        EntityQuery _blueDoofusesQuery;
        EntityQuery _specialBlueDoofusesQuery;
        EntityQuery _redDoofusesQuery;
    }
}
