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
    public class PlayerInputEngine : IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { private get; set; }

        public void Ready() { ReadInput().RunOnScheduler(StandardSchedulers.earlyScheduler); }

        IEnumerator ReadInput()
        {
            void IteratePlayersInput(EntityCollection<PlayerInputDataComponent> groups)
            {
                var (playerEntityViews, playersCount) = groups;
                
                for (int i = 0; i < playersCount; i++)
                {
                    var h = CrossPlatformInputManager.GetAxisRaw("Horizontal");
                    var v = CrossPlatformInputManager.GetAxisRaw("Vertical");

                    playerEntityViews[i].input  = new Vector3(h, 0f, v);
                    playerEntityViews[i].camRay = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                    playerEntityViews[i].fire   = Input.GetButton("Fire1");
                }
            }

            while (true)
            {
                var groups =
                    entitiesDB.QueryEntities<PlayerInputDataComponent>(ECSGroups.PlayersGroup);

                IteratePlayersInput(groups);

                yield return null;
            }
        }
   }
}