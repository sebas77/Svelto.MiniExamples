using System.Collections;
using Svelto.ECS.Example.Survive.OOPLayer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Svelto.ECS.Example.Survive
{
    public class RestartGameOnPlayerDeathEngine: IQueryingEntitiesEngine,
        IReactOnAddAndRemove<GameObjectEntityComponent>, IStepEngine
    {
        public RestartGameOnPlayerDeathEngine()
        {
            _restartLevelAfterFewSeconds = RestartLevelAfterFewSeconds();
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Add(ref GameObjectEntityComponent entityComponent, EGID egid) { }

        public void Remove(ref GameObjectEntityComponent entityComponent, EGID egid)
        {
            _execute = true;
        }

        public void Ready() { }

        IEnumerator RestartLevelAfterFewSeconds()
        {
            WaitForSecondsEnumerator _waitForSeconds = new WaitForSecondsEnumerator(5);

            while (_waitForSeconds.MoveNext())
                yield return null;

            var guiEntityView = entitiesDB.QueryUniqueEntity<AnimationComponent>(ECSGroups.GUICanvas);
            guiEntityView.animationState = new AnimationState(HUDAnimations.Die);

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

        public string name => nameof(RestartGameOnPlayerDeathEngine);

        readonly IEntityFunctions _DBFunctions;
        readonly IEnumerator _restartLevelAfterFewSeconds;
        bool _execute;
    }
    
    public static class HUDAnimations
    {
        public static int Die =  Animator.StringToHash("GameOver");
    }
}