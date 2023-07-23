using Svelto.ECS;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace Logic.SveltoECS
{
    public class SpawnVehiclesSystem: IQueryingEntitiesEngine, IStepEngine<float>, IReactOnAddEx<PositionDC>
    {
        public SpawnVehiclesSystem(IEntityFactory entityFactory)
        {
            _entityFactory = entityFactory;
        }

        public void Step(in float time)
        {
            var maxTeamCount = Data.MaxTeamCount;
            var maxVehicleCount = math.floor((float)Data.MaxVehicleCount / (float)Data.MaxTeamCount);

            for (uint i = 0; i < maxTeamCount; i++)
            {
                if (entitiesDB.Count<TeamDC>(VehicleTag.BuildGroup + i) >= maxVehicleCount)
                    continue;

                var egid = new EGID((uint)Count++, VehicleTag.BuildGroup + i);
                var init = _entityFactory.BuildEntity<VehicleDescriptor>(egid);
                
                init.Init(
                    new TeamDC()
                    {
                        Value = i
                    });
                init.Init(
                    new PositionDC
                    {
                        Value = new float2(Random.Range(0.0f, 100.0f), Random.Range(0.0f, 100.0f))
                    });
                init.Init(
                    new HealthDC
                    {
                        Value = 100
                    });
            }
        }

        public void Ready()
        {
            var filter = entitiesDB.GetFilters();

            _filterOff = filter.CreatePersistentFilter<PositionDC>(VechilesFilterIds.VehiclesWithSirenOff);
            filter.CreatePersistentFilter<PositionDC>(VechilesFilterIds.VehiclesWithSirenOn);
        }
        
        //Entities can be added in to filters only after they are inserted in the database. Exploiting the add callback is a common pattern to 
        //solve the issue
        public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<PositionDC> entities, ExclusiveGroupStruct groupID)
        {
            var filter = _filterOff.GetOrCreateGroupFilter(groupID);
            var (_, entityIDs, _) = entities;
            for (var index = rangeOfEntities.start; index < rangeOfEntities.end; index++)
            {
                filter.Add(entityIDs[index], index);
            }
        }

        public EntitiesDB entitiesDB { get; set; }

        public string name => nameof(SpawnVehiclesSystem);

        readonly IEntityFactory _entityFactory;

        static int Count = 0;
        EntityFilterCollection _filterOff;
    }
};