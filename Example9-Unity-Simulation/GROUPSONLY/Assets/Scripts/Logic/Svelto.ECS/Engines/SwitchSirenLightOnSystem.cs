using Svelto.ECS;
using Unity.Mathematics;

namespace Logic.SveltoECS
{
    public class SwitchSirenLightOnSystem: IQueryingEntitiesEngine, IStepEngine<float>
    {
        public SwitchSirenLightOnSystem(IEntityFunctions entityFunctions)
        {
            _entityFunctions = entityFunctions;
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }

        public void Step(in float time)
        {
            foreach (var ((healths, times, sirens, entityIDs, entitiesWithSirenOff), group) in
                     entitiesDB.QueryEntities<HealthDC, TimeUntilSirenSwitch, SirenLight>(VehicleSirenOff.Groups))
            {
                for (int i = 0; i < entitiesWithSirenOff; i++)
                {
                    ref var timeUntilSirenSwitch = ref times[i];
                    if (timeUntilSirenSwitch.Value <= 0)
                    {
                        var health = healths[i].Value;
                        ref var siren = ref sirens[i];
                        siren.LightIntensity = (int)math.min(150 - health, 100);
                        timeUntilSirenSwitch.Value = health / 100;
                        uint team = VehicleSirenOff.Offset(group);
                        _entityFunctions.SwapEntityGroup<VehicleDescriptor>(new EGID(entityIDs[i], group), VehicleSirenOn.BuildGroup + (uint)team);
                    }
                }
            }
        }

        public string name => nameof(SwitchSirenLightOnSystem);
        
        readonly IEntityFunctions _entityFunctions;
    }
}