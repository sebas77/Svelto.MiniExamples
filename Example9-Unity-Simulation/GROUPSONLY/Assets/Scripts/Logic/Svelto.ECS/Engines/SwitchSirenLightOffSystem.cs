// Copyright (c) Sean Nowotny

using Svelto.ECS;

namespace Logic.SveltoECS
{
    public class SwitchSirenLightOffSystem: IQueryingEntitiesEngine, IStepEngine<float>
    {
        public SwitchSirenLightOffSystem(IEntityFunctions entityFunctions)
        {
            _entityFunctions = entityFunctions;
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }

        public void Step(in float time)
        {
            foreach (var ((times, entityIDs, entitiesWithSirenOn), group) in
                     entitiesDB.QueryEntities<TimeUntilSirenSwitch>(VehicleSirenOn.Groups))
            {
                for (int i = 0; i < entitiesWithSirenOn; i++)
                {
                    ref var timeUntilSirenSwitch = ref times[i];
                    if (timeUntilSirenSwitch.Value <= 0)
                    {
                        uint team = VehicleSirenOn.Offset(group);
                        _entityFunctions.SwapEntityGroup<VehicleDescriptor>(new EGID(entityIDs[i], group), VehicleSirenOff.BuildGroup + (uint)team);
                    }
                }
                
                for (int i = 0; i < entitiesWithSirenOn; i++)
                {
                    ref var timeUntilSirenSwitch = ref times[i];
                    if (timeUntilSirenSwitch.Value <= 0)
                    {
                        timeUntilSirenSwitch.Value = 1.0f;
                    }
                }
            }
        }

        public string name => nameof(SwitchSirenLightOffSystem);
        
        readonly IEntityFunctions _entityFunctions;
    }
}