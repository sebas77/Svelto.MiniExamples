using System.Collections;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.OOPLayer;
using Svelto.ECS.Example.Survive.Player;
using UnityEngine.SceneManagement;

namespace Svelto.ECS.Example.Survive.HUD
{
    public class RestartGameOnPlayerDeathEngine: IQueryingEntitiesEngine, IReactOnSwapEx<GameObjectEntityComponent>,
            IStepEngine
    {
        public RestartGameOnPlayerDeathEngine()
        {
            _restartLevelAfterFewSeconds = RestartLevelAfterFewSeconds();
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<GameObjectEntityComponent> entities, ExclusiveGroupStruct fromGroup,
            ExclusiveGroupStruct toGroup)
        {
            if (PlayerAliveGroup.Includes(fromGroup) && Dead.Includes(toGroup)) //were the entities swapped EntityTargets and just died?
            {
                _execute = true;
            }
        }

        IEnumerator RestartLevelAfterFewSeconds()
        {
            WaitForSecondsEnumerator _waitForSeconds = new WaitForSecondsEnumerator(2);

            while (_waitForSeconds.MoveNext())
                yield return null;

            var guiEntityView = entitiesDB.QueryUniqueEntity<HUDEntityViewComponent>(ECSGroups.GUICanvas);
            guiEntityView.damageHUDComponent.animationState = new AnimationState(HUDAnimations.GameOver);

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
}