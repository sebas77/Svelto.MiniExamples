using System;
using System.Collections;
using Svelto.ECS.Example.Survive.Characters.Player;
using Svelto.ECS.Example.Survive.HUD;
using UnityEngine.SceneManagement;

namespace Svelto.ECS.Example.Survive
{
    public class RestartGameOnPlayerDeathEngine : IQueryingEntitiesEngine, IReactOnAddAndRemove<PlayerEntityViewComponent>, IStepEngine
    {
        public RestartGameOnPlayerDeathEngine()
        {
            _restartLevelAfterFewSeconds = RestartLevelAfterFewSeconds();
        }
        
        public EntitiesDB entitiesDB { get; set; }
        
        public void                           Add(ref PlayerEntityViewComponent entityComponent, EGID egid)    
        {}

        public void Remove(ref PlayerEntityViewComponent entityComponent, EGID egid)
        {
            _execute = true;
        }

        public void Ready() { }

        IEnumerator RestartLevelAfterFewSeconds()
        {
            WaitForSecondsEnumerator _waitForSeconds = new WaitForSecondsEnumerator(5);

            while (_waitForSeconds.MoveNext())
                yield return null;
        
            var guiEntityView = entitiesDB.QueryUniqueEntity<HUDEntityViewComponent>(ECSGroups.GUICanvas);
            guiEntityView.HUDAnimator.playAnimation = "GameOver";
        
            _waitForSeconds.Reset(2);
            while (_waitForSeconds.MoveNext())
                yield return null;
        
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void Step()
        {
            if (_execute)
                _restartLevelAfterFewSeconds.MoveNext();
        }
        public   string                       name   => nameof(RestartGameOnPlayerDeathEngine);
        
        readonly IEntityFunctions             _DBFunctions;
        readonly IEntityStreamConsumerFactory _consumerFactory;
        readonly IEnumerator                  _restartLevelAfterFewSeconds;
        bool                                  _execute;
    }
}