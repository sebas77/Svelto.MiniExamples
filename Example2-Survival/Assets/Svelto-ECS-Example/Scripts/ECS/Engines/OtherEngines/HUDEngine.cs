using Svelto.Tasks.Enumerators;
using System.Collections;
using Svelto.ECS.Example.Survive.Characters;
using Svelto.ECS.Example.Survive.Characters.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Svelto.ECS.Example.Survive.HUD
{
    public class HUDEngine : IQueryingEntitiesEngine, IStep<PlayerDeathCondition>
    {
        public IEntitiesDB entitiesDB { set; private get; }

        public HUDEngine(ITime time)
        {
            _time = time;
        }

        public void Ready()
        {
            AnimateUI().Run();
            CheckForDamage().Run();
        }

        IEnumerator AnimateUI()
        {
            while (true)
            {
                var entities = entitiesDB.QueryEntities<HUDEntityView>(ECSGroups.ExtraStuff);
                      foreach (var guiEntityView in entities)
                      {
                          var damageComponent = guiEntityView.damageImageComponent;

                          damageComponent.imageColor =
                              Color.Lerp(damageComponent.imageColor, Color.clear,
                                         damageComponent.speed * UnityEngine.Time.deltaTime);
                      };
                
                
                yield return null;
            }
        }

        /// <summary>
        /// the damaged flag is polled. I am still torn about the poll vs push problem, so more investigation is needed
        /// Maybe solved in future with the refactored version of DispatchOnSet/Change 
        /// </summary>
        /// <param name="entitiesDb"></param>
        
        IEnumerator CheckForDamage()
        {
            while (true)
            {
                var entities =
                    entitiesDB.QueryEntities<DamageableEntityStruct, HealthEntityStruct>(ECSGroups.Player, out var numberOfPlayers);
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    if (entities.Item1[i].damaged == false) continue;

                    //An engine should never assume how many entities will be used, so we iterate over all the
                    //HUDEntityViews even if we know there is just one
                    
                    var guiEntityView = entitiesDB.QueryUniqueEntity<HUDEntityView>(ECSGroups.ExtraStuff);

                    
                        var damageComponent = guiEntityView.damageImageComponent;
                        damageComponent.imageColor = damageComponent.flashColor;

                        guiEntityView.healthSliderComponent.value = entities.Item2[i].currentHealth;
                }

                yield return null;
            }
        }
        
        IEnumerator RestartLevelAfterFewSeconds()
        {
            _waitForSeconds.Reset(5);
            yield return _waitForSeconds;

            var guiEntityView = entitiesDB.QueryUniqueEntity<HUDEntityView>(ECSGroups.ExtraStuff);
            guiEntityView.HUDAnimator.playAnimation = "GameOver";

            _waitForSeconds.Reset(2);
            yield return _waitForSeconds;

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void Step(PlayerDeathCondition condition, EGID id)
        {
            var guiEntityView = entitiesDB.QueryUniqueEntity<HUDEntityView>(ECSGroups.ExtraStuff);
            guiEntityView.healthSliderComponent.value = 0;

            RestartLevelAfterFewSeconds().Run();
        }

        readonly WaitForSecondsEnumerator  _waitForSeconds = new WaitForSecondsEnumerator(5);
        ITime                     _time;
    }
}

