using Svelto.ECS;
using Svelto.ECS.Experimental;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace Logic.SveltoECS
{
//    public static class VechilesFilterIds
//    {
//        internal static readonly FilterContextID VehicleFilterContext = FilterContextID.GetNewContextID();
//    }
//
    public class SpawnVehiclesSystem: IQueryingEntitiesEngine, IStepEngine<float>
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
                if (entitiesDB.Count<TeamDC>(VehicleSirenOff.BuildGroup + i) +  entitiesDB.Count<TeamDC>(VehicleSirenOn.BuildGroup + i)>= maxVehicleCount)
                    continue;
                
                var egid = new EGID((uint)Count++, VehicleSirenOff.BuildGroup + i);
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
        { }

        public EntitiesDB entitiesDB { get; set; }

        public string name => nameof(SpawnVehiclesSystem);

        readonly IEntityFactory _entityFactory;
        
        static int Count = 0;
    }
};