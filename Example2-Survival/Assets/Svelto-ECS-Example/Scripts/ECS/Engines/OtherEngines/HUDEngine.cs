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
                entitiesDB.ExecuteOnEntities(ECSGroups.ExtraStuff, ref _time,
                                             (ref HUDEntityView guiEntityView, ref ITime time, IEntitiesDB entitiesDb,
                                              int               index) =>
                      {
                          var damageComponent = guiEntityView.damageImageComponent;

                          damageComponent.imageColor =
                              Color.Lerp(damageComponent.imageColor, Color.clear,
                                         damageComponent.speed * time.deltaTime);
                      });
                
                
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
                int numberOfPlayers;
                var damageablePlayerEntities =
                    entitiesDB.QueryEntities<DamageableEntityStruct>(ECSGroups.Player, out numberOfPlayers);
                var playerHealthEntities =
                    entitiesDB.QueryEntities<HealthEntityStruct>(ECSGroups.Player, out numberOfPlayers);
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    if (damageablePlayerEntities[i].damaged == false) continue;

                    //An engine should never assume how many entities will be used, so we iterate over all the
                    //HUDEntityViews even if we know there is just one
                    entitiesDB.ExecuteOnEntities(ECSGroups.ExtraStuff, ref playerHealthEntities[i].currentHealth,
                                                 (ref HUDEntityView guiEntityView, ref int refhealth,
                                                  IEntitiesDB       entitiesdb,    int     innerIndex) =>
                                                 {
                                                     var damageComponent = guiEntityView.damageImageComponent;
                                                     damageComponent.imageColor = damageComponent.flashColor;

                                                     guiEntityView.healthSliderComponent.value = refhealth;
                                                 });
                }

                yield return null;
            }
        }
        
        IEnumerator RestartLevelAfterFewSeconds()
        {
            _waitForSeconds.Reset(5);
            yield return _waitForSeconds;

            int hudEntityViewsCount;
            var hudEntityViews =
                entitiesDB.QueryEntities<HUDEntityView>(ECSGroups.ExtraStuff, out hudEntityViewsCount);
            for (int i = 0; i < hudEntityViewsCount; i++)
                hudEntityViews[i].HUDAnimator.playAnimation = "GameOver";

            _waitForSeconds.Reset(2);
            yield return _waitForSeconds;

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void Step(PlayerDeathCondition condition, EGID id)
        {
            int hudEntityViewsCount;
            var hudEntityViews =
                entitiesDB.QueryEntities<HUDEntityView>(ECSGroups.ExtraStuff, out hudEntityViewsCount);
            for (int i = 0; i < hudEntityViewsCount; i++)
                hudEntityViews[i].healthSliderComponent.value = 0;

            RestartLevelAfterFewSeconds().Run();
        }

        readonly WaitForSecondsEnumerator  _waitForSeconds = new WaitForSecondsEnumerator(5);
        ITime                     _time;
    }
}

