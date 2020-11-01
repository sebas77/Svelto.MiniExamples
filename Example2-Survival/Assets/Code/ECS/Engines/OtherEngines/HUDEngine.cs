using System.Collections;
using Svelto.ECS.Example.Survive.Characters;
using Svelto.Tasks.Enumerators;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Svelto.ECS.Example.Survive.HUD
{
    /// <summary>
    /// Note I have been lazy with this class and not following the perfect abstraction rules. The project is too
    /// simple to abstract too much. However the mistakes are:
    /// - This engine shouldn't be aware of the concept of damage/health
    /// - this engine shouldn't know the players group
    /// - this engine shouldn't really have the responsibility to restart the level (ugh)
    /// </summary>
    public class HUDEngine : IQueryingEntitiesEngine
    {
        public HUDEngine(ITime time, IEntityStreamConsumerFactory consumerFactory)
        {
            _time = time;
            _consumerFactory = consumerFactory;
        }

        public EntitiesDB entitiesDB { set; private get; }

        public void Ready()
        {
            AnimateUI().Run();
            CheckForDamage().Run();
        }

        IEnumerator AnimateUI()
        {
            void AnimateUI()
            {
                var (buffer, count) = entitiesDB.QueryEntities<HUDEntityView>(ECSGroups.GUICanvas);
                for (int i = 0; i < count; ++i)
                {
                    var damageComponent = buffer[i].damageImageComponent;

                    damageComponent.imageColor = Color.Lerp(damageComponent.imageColor, Color.clear
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
            var _consumerHealth = _consumerFactory.GenerateConsumer<HealthComponent>(ECSGroups.PlayersGroup, "HUDEngine", 1);
            var _consumerDeath = _consumerFactory.GenerateConsumer<DeathComponent>(ECSGroups.PlayersGroup, "HUDEngine", 1);
            
            while (true)
            {
                while (_consumerHealth.TryDequeue(out var health))
                {
                    //An engine should never assume how many entities will be used, so we iterate over all the
                    //HUDEntityViews even if we know there is just one
                    var guiEntityView = entitiesDB.QueryUniqueEntity<HUDEntityView>(ECSGroups.GUICanvas);
        
                    var damageComponent = guiEntityView.damageImageComponent;
                    damageComponent.imageColor = damageComponent.flashColor;
        
                    guiEntityView.healthSliderComponent.value = health.currentHealth;
                }

                while (_consumerDeath.TryDequeue(out var death))
                {
                    var guiEntityView = entitiesDB.QueryUniqueEntity<HUDEntityView>(ECSGroups.GUICanvas);
                    guiEntityView.healthSliderComponent.value = 0;

                    RestartLevelAfterFewSeconds().Run();
                }

                yield return null;
            }
        }

        IEnumerator RestartLevelAfterFewSeconds()
        {
            _waitForSeconds.Reset(5);
            yield return _waitForSeconds;
        
            var guiEntityView = entitiesDB.QueryUniqueEntity<HUDEntityView>(ECSGroups.GUICanvas);
            guiEntityView.HUDAnimator.playAnimation = "GameOver";
        
            _waitForSeconds.Reset(2);
            yield return _waitForSeconds;
        
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        readonly WaitForSecondsEnumerator     _waitForSeconds = new WaitForSecondsEnumerator(5);
        ITime                                 _time;
        readonly IEntityStreamConsumerFactory _consumerFactory;
    }
}