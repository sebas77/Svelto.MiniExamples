using Svelto.ECS;

namespace Logic.SveltoECS
{
    public class SwitchSirenLightOffSystem: IQueryingEntitiesEngine, IStepEngine<float>
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
            //switch On To Off
            foreach (var (indicies, group, onGroupFilter) in _entityFilterCollectionOn)
            {
                var (times, entityIDs, _) = entitiesDB.QueryEntities<TimeUntilSirenSwitch>(group);

                var offGroupfilter = _entityFilterCollectionOff.GetGroupFilter(group);

                for (int i = 0; i < indicies.count; i++)
                {
                    var index = indicies[i];
                    ref var timeUntilSirenSwitch = ref times[index];
                    if (timeUntilSirenSwitch.Value <= 0)
                    {
                        timeUntilSirenSwitch.Value = 0.1f;

                        var entityID = entityIDs[index];
                        onGroupFilter.Remove(entityID);
                        offGroupfilter.Add(entityID, index);
                        i--;
                    }
                }
            }
        }

        public string name => nameof(SwitchSirenLightOffSystem);

        EntityFilterCollection _entityFilterCollectionOff;
        EntityFilterCollection _entityFilterCollectionOn;
    }
}