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
                         entitiesDB.QueryEntities<PlayerInputDataComponent>(Player.Groups))
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
                    gunComponents[0].fired = Input.GetButton("Fire1");

                //is it correct that a player engine iterates cameras? The answer is no: in a pure sense this
                //engine has too many responsibilities. A camera engine should do this.
                var (cameraComponents, camerasCount) = entitiesDB
                   .QueryEntities<CameraOOPEntityComponent>(Camera.Camera.Group);
                for (int i = 0; i < camerasCount; i++)
                    cameraComponents[i].camRayInput = Input.mousePosition;
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