using System.Collections;
using Svelto.ECS.Example.Survive.Camera;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public class PlayerMovementEngine : IQueryingEntitiesEngine, IStepEngine
    {
        const float camRayLength = 100f; // The length of the ray from the camera into the scene.

        public PlayerMovementEngine(IRayCaster raycaster)
        {
            _rayCaster = raycaster;
            _tick      = Tick();
        }

        public EntitiesDB entitiesDB { private get; set; }

        public void   Ready() { }
        public void   Step()  { _tick.MoveNext(); }
        public string name    => nameof(PlayerMovementEngine);

        IEnumerator Tick()
        {
            while (true)
            {
                //Exploit everywhere the power of deconstruct to tuples. Every query entities can be deconstruct
                //to what you are going to use directly
                var (playersInput, speedInfos, playerViews, count) =
                    entitiesDB.QueryEntities<PlayerInputDataComponent, SpeedComponent, PlayerEntityViewComponent>(
                        ECSGroups.PlayersGroup);

                //this demo has just one player, but I tried to abstract the assumption.
                for (int i = 0; i < count; i++)
                {
                    Movement(playersInput[i], ref playerViews[i], speedInfos[i]);
                    Turning(
                        ref entitiesDB.QueryEntity<CameraEntityViewComponent>(
                            playerViews[i].ID.entityID, ECSGroups.Camera), ref playerViews[i]);
                }

                yield return null; //don't forget to yield or you will enter in an infinite loop!
            }
        }

        /// <summary>
        ///     In order to keep the class testable, we need to reduce the number of
        ///     dependencies injected through static classes at its minimum.
        ///     Implementors are the place where platform dependencies can be transformed into
        ///     entity components, so that here we can use inputComponent instead of
        ///     the class Input.
        /// </summary>
        /// <param name="playerInput"></param>
        /// <param name="playerComponent"></param>
        void Movement
        (in PlayerInputDataComponent playerInput, ref PlayerEntityViewComponent playerComponent
       , in SpeedComponent speedComponent)
        {
            // Normalise the movement vector and make it proportional to the speed per second.
            var movement = playerInput.input.normalized * speedComponent.movementSpeed;

            // Move the player to it's current position plus the movement.
            playerComponent.rigidBodyComponent.velocity = movement;
        }

        void Turning(ref CameraEntityViewComponent cameraInfo, ref PlayerEntityViewComponent playerComponent)
        {
            // Create a ray from the mouse cursor on screen in the direction of the camera.
            var camRay = cameraInfo.cameraComponent.camRay;

            // Perform the raycast and if it hits something on the floor layer...
            if (_rayCaster.CheckHit(camRay, camRayLength, floorMask, out var point))
            {
                // Create a vector from the player to the point on the floor the raycast from the mouse hit.
                var playerToMouse = point - playerComponent.positionComponent.position;

                // Ensure the vector is entirely along the floor plane.
                playerToMouse.y = 0f;

                // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
                var newRotation = Quaternion.LookRotation(playerToMouse);

                // Set the player's rotation to this new rotation.
                playerComponent.transformComponent.rotation = newRotation;
            }
        }

        readonly IRayCaster _rayCaster;

        readonly int floorMask = LayerMask.GetMask("Floor"); // A layer mask so that a ray can be cast just at gameobjects on the floor layer.

        readonly IEnumerator _tick;
    }
}