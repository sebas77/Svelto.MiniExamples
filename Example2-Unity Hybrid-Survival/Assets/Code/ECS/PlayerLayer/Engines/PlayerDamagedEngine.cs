using Svelto.Common;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.OOPLayer;

namespace Svelto.ECS.Example.Survive.Player
{
    [Sequenced(nameof(PlayerEnginesNames.PlayerDamagedEngine))]
    public class PlayerDamagedEngine : IStepEngine, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            _sveltoFilters = entitiesDB.GetFilters();
        }

        public void Step()
        {
            var damagedEntitiesFilter =
                    _sveltoFilters.GetTransientFilter<HealthComponent>(FilterIDs.DamagedEntitiesFilter);

            foreach (var (filteredIndices, group) in damagedEntitiesFilter)
            {
                if (PlayerAliveGroup.Includes(group)) //is it a player to be damaged? 
                {
                    var (sounds, _) = entitiesDB.QueryEntities<SoundComponent>(group);

                    for (int i = 0; i < filteredIndices.count; i++)
                    {
                        sounds[filteredIndices[i]].playOneShot = (int)AudioType.damage;
                    }
                }
            }
        }

        public string name => nameof(PlayerDamagedEngine);
        
        EntitiesDB.SveltoFilters _sveltoFilters;
    }
}