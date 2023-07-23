using Svelto.ECS;
using Unity.Mathematics;

namespace Logic.SveltoECS
{
    public class SwitchSirenLightOnSystem: IQueryingEntitiesEngine, IStepEngine<float>
    {
        public void Ready()
        {
            var filters = entitiesDB.GetFilters();
            
            _entityFilterCollectionOff = filters.GetPersistentFilter<PositionDC>(VechilesFilterIds.VehiclesWithSirenOff);
            _entityFilterCollectionOn = filters.GetPersistentFilter<PositionDC>(VechilesFilterIds.VehiclesWithSirenOn);
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Step(in float time)
        {
            //switch Off To On
            foreach ((EntityFilterIndices indicies, ExclusiveGroupStruct group, var offGroupfilter) in _entityFilterCollectionOff)
            {
                var (healths, times, sirens, entityIDs, _) = entitiesDB.QueryEntities<HealthDC, TimeUntilSirenSwitch, SirenLight>(group);

                var onGroupFilter = _entityFilterCollectionOn.GetOrCreateGroupFilter(group);
                
                for (int i = 0; i < indicies.count; i++)
                {
                    var index = indicies[i];
                    ref var timeUntilSirenSwitch = ref times[index];
                    if (timeUntilSirenSwitch.Value <= 0)
                    {
                        var health = healths[index].Value;
                        sirens[index].LightIntensity = (int)math.min(150 - health, 100);
                        timeUntilSirenSwitch.Value = health / 100;

                        var entityID = entityIDs[index];
                        offGroupfilter.Remove(entityID);
                        onGroupFilter.Add(entityID, index);
                        i--;
                    }
                }
            }
        }

        public string name => nameof(SwitchSirenLightOnSystem);
        
        EntityFilterCollection _entityFilterCollectionOff;
        EntityFilterCollection _entityFilterCollectionOn;
    }
}