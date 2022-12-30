using System.Collections;
using Svelto.ECS.Example.Survive.OOPLayer;
using Svelto.ECS.Example.Survive.Player.Gun;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Player
{
    /// <summary>
    ///     if you need to test input, you can mock this class 
    /// </summary>
    public class PlayerInputEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public EntitiesDB entitiesDB { private get; set; }
        public string     name       => nameof(PlayerInputEngine);
        
        public void       Ready()    { _readInput = ReadInput(); }
        public void       Step()     { _readInput.MoveNext(); } //tick method, steps in the ReadInput() state machine

        IEnumerator ReadInput()
        {
            void IteratePlayersInput()
            {
                //although this game has just one player, iterating like this is a svelto pattern that works on
                //0, 1 or N entities
                foreach (var ((playerComponents, _), _) in
                         entitiesDB.QueryEntities<PlayerInputDataComponent>(PlayerAliveGroup.Groups))
                {
                    //of course in this example we can assume we have just one player
                    {
                        var h = Input.GetAxisRaw("Horizontal");
                        var v = Input.GetAxisRaw("Vertical");

                        playerComponents[0].input = new Vector3(h, 0f, v);
                    }
                }

                var (gunComponents, count) = entitiesDB.QueryEntities<GunComponent>(PlayerGun.Gun.Group);
                for (int i = count - 1; i >= 0; i--)
                    gunComponents[i].fired = Input.GetButton("Fire1");

                //is it correct that a player engine iterates cameras? The answer is no: in a pure sense this
                //engine has too many responsibilities. A camera engine should do this.
                var (cameraComponents, camerasCount) = entitiesDB
                   .QueryEntities<CameraOOPEntityComponent>(Camera.Camera.Group);
                for (int i = camerasCount - 1; i >= 0; i--)
                {
                    float mouseX = Input.mousePosition.x;
                    float mouseY = Input.mousePosition.y;
                    float screenX = Screen.width;
                    float screenY = Screen.height;

                    if (!(mouseX < 0) && !(mouseX > screenX) && !(mouseY < 0) && !(mouseY > screenY))
                    {
                        cameraComponents[i].camRayInput = Input.mousePosition;
                        cameraComponents[i].inputRead = true;
                    }
                    else
                    {
                        cameraComponents[i].inputRead = false;
                    }
                }
            }

            while (true)
            {
                IteratePlayersInput();

                yield return null;
            }
        }

        IEnumerator _readInput;
    }
}