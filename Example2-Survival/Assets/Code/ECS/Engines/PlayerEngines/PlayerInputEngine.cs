using System.Collections;
using Svelto.Tasks;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace Svelto.ECS.Example.Survive.Characters.Player
{
    /// <summary>
    ///     if you need to test input, you can mock this class
    ///     alternatively you can mock the implementor.
    /// </summary>
    public class PlayerInputEngine : SingleEntityReactiveEngine<PlayerEntityViewStruct>, IQueryingEntitiesEngine
    {
        readonly ITaskRoutine<IEnumerator> _taskRoutine;

        public PlayerInputEngine()
        {
            _taskRoutine = TaskRunner.Instance.AllocateNewTaskRoutine();
            _taskRoutine.SetEnumerator(ReadInput());
        }

        public IEntitiesDB entitiesDB { private get; set; }

        public void Ready() { }

        IEnumerator ReadInput()
        {
            //wait for the player to spawn
            while (entitiesDB.HasAny<PlayerEntityViewStruct>(ECSGroups.Player) == false)
                yield return null; //skip a frame

            var playerEntityViews =
                entitiesDB.QueryEntities<PlayerInputDataStruct>(ECSGroups.Player, out var targetsCount);

            while (true)
            {
                var h = CrossPlatformInputManager.GetAxisRaw("Horizontal");
                var v = CrossPlatformInputManager.GetAxisRaw("Vertical");

                playerEntityViews[0].input  = new Vector3(h, 0f, v);
                playerEntityViews[0].camRay = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                playerEntityViews[0].fire   = Input.GetButton("Fire1");

                yield return null;
            }
        }

        protected override void Add(ref PlayerEntityViewStruct           entityView,
                                    ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        {
            _taskRoutine.Start();
        }

        protected override void Remove(ref PlayerEntityViewStruct entityView, bool itsaSwap) { _taskRoutine.Stop(); }
    }
}