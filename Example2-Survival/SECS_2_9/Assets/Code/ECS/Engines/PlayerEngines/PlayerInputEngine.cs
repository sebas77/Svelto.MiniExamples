using System.Collections;
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
        public IEntitiesDB entitiesDB { private get; set; }

        public void Ready() { ReadInput().Run(); }

        IEnumerator ReadInput()
        {
            while (true)
            {
                var playerEntityViews =
                    entitiesDB.QueryEntities<PlayerInputDataStruct>(ECSGroups.Player, out var targetsCount);

                for (int i = 0; i < targetsCount; i++)
                {
                    var h = CrossPlatformInputManager.GetAxisRaw("Horizontal");
                    var v = CrossPlatformInputManager.GetAxisRaw("Vertical");

                    playerEntityViews[i].input  = new Vector3(h, 0f, v);
                    playerEntityViews[i].camRay = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                    playerEntityViews[i].fire   = Input.GetButton("Fire1");
                }

                yield return null;
            }
        }
   }
}