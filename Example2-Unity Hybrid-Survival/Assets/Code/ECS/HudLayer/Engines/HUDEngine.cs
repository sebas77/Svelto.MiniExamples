using System.Collections;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.Player;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.HUD
{
    /// <summary>
    /// Note I have been lazy with this class and not following the perfect abstraction rules. The project is too
    /// simple to abstract too much. However the mistakes are:
    /// - This engine shouldn't be aware of the concept of damage/health
    /// - this engine shouldn't know the players group
    /// - this engine shouldn't really have the responsibility to restart the level (ugh)
    /// </summary>
    public class HUDEngine: IQueryingEntitiesEngine, IStepEngine
    {
        public HUDEngine()
        {
            _animateUI = AnimateUI();
            _checkForDamage = CheckForDamage();
        }

        public EntitiesDB entitiesDB { set; private get; }

        public void Ready() { }

        public void Step()
        {
            _animateUI.MoveNext();
            _checkForDamage.MoveNext();
        }

        public string name => nameof(HUDEngine);

        IEnumerator AnimateUI()
        {
            void AnimateUI()
            {
                var (buffer, count) = entitiesDB.QueryEntities<HUDEntityViewComponent>(ECSGroups.GUICanvas);
                for (int i = 0; i < count; ++i)
                {
                    var damageComponent = buffer[i].damageHUDComponent;

                    damageComponent.imageColor = Color.Lerp(
                        damageComponent.imageColor, Color.clear
                      , damageComponent.speed * UnityEngine.Time.deltaTime);
                }
            }

            while (true)
            {
                AnimateUI();

                yield return null;
            }
        }

        IEnumerator CheckForDamage()
        {
            while (entitiesDB.HasAny<HUDEntityViewComponent>(ECSGroups.GUICanvas) == false)
                yield return null;

            var _sveltoFilters = entitiesDB.GetFilters();

            while (true)
            {
                var damagedEntitiesFilter =
                        _sveltoFilters.GetTransientFilter<HealthComponent>(FilterIDs.DamagedEntitiesFilter);
                var guiEntityView = entitiesDB.QueryUniqueEntity<HUDEntityViewComponent>(ECSGroups.GUICanvas);

                RefHelper(damagedEntitiesFilter, guiEntityView);

                yield return null;
            }

            void RefHelper(in EntityFilterCollection damagedEntitiesFilter, HUDEntityViewComponent guiEntityView)
            {
                foreach (var (filteredIndices, group) in damagedEntitiesFilter)
                {
                    if (PlayerAliveGroup.Includes(group)) //is it a player to be damaged? 
                    {
                        var (healths, _) = entitiesDB.QueryEntities<HealthComponent>(group);

                        if (filteredIndices.count > 0)
                        {
                            var damageComponent = guiEntityView.damageHUDComponent;
                            damageComponent.imageColor = damageComponent.flashColor;
                            guiEntityView.healthSliderComponent.value = healths[filteredIndices[0]].currentHealth;
                        }
                    }
                }
            }
        }

        readonly IEnumerator _animateUI;
        readonly IEnumerator _checkForDamage;
    }
}