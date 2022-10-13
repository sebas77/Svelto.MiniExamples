using System.Collections;
using Svelto.ECS.Example.Survive.Camera;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Player
{
    /// <summary>
    ///     if you need to test input, you can mock this class alternatively you can mock the implementor.
    /// </summary>
    public class PlayerInputEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public EntitiesDB entitiesDB { private get; set; }
        public string     name       => nameof(PlayerInputEngine);
        
        public void       Ready()    { _readInput = ReadInput(); }
        public void       Step()     { _readInput.MoveNext(); }

        IEnumerator ReadInput()
        {
            void IteratePlayersInput()
            {
                foreach (var ((playerComponents, playersCount), _) in entitiesDB.QueryEntities<PlayerInputDataComponent>(Player.Groups))
                {
                    for (int i = 0; i < playersCount; i++)
                    {
                        var h = Input.GetAxisRaw("Horizontal");
                        var v = Input.GetAxisRaw("Vertical");

                        playerComponents[i].input = new Vector3(h, 0f, v);
                        playerComponents[i].fire  = Input.GetButton("Fire1");
                    }
                }

                foreach (var ((cameraComponents, camerasCount), _) in entitiesDB
                   .QueryEntities<CameraEntityComponent>(Camera.Camera.Groups))
                {
                    for (int i = 0; i < camerasCount; i++)
                        cameraComponents[i].camRayInput = Input.mousePosition;
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