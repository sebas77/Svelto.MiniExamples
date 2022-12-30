using System.Collections;
using Svelto.ECS.Example.Survive.Camera;
using Svelto.ECS.Example.Survive.OOPLayer;

using UnityEngine;

namespace Svelto.ECS.Example.Survive.Player
{
    public class PlayerMovementEngine : IQueryingEntitiesEngine, IStepEngine, IReactOnSwapEx<RigidBodyComponent>
    {
        const float camRayLength = 100f; // The length of the ray from the camera into the scene.

        public PlayerMovementEngine(IRayCaster raycaster)
        {
            _rayCaster = raycaster;
            _tick      = Tick();
        }

        public EntitiesDB entitiesDB { private get; set; }

        public void Ready() { }

        public void Step() => _tick.MoveNext();

        public string name => nameof(PlayerMovementEngine);
        
        public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<RigidBodyComponent> entities, ExclusiveGroupStruct fromGroup,
            ExclusiveGroupStruct toGroup)
        {
            if (toGroup == PlayerDeadGroup.BuildGroup)
            {
                var (buffer, _) = entities;

                for (int i = (int)rangeOfEntities.start; i < rangeOfEntities.end; i++)
                {
                    buffer[i].velocity = default;
                }
            }
        }

        IEnumerator Tick()
        {
            void ProcessPlayerInput()
            {
                //Exploit everywhere the power of deconstruct to tuples. Every query entities can be deconstruct
                //to what you are going to use directly
                foreach (var ((playersInput, rbs, count), _) in entitiesDB
                            .QueryEntities<PlayerInputDataComponent, RigidBodyComponent>(PlayerAliveGroup.Groups))
                {
                    for (int i = 0; i < count; i++)
                    {
                        Movement(playersInput[i], ref rbs[i]);
                    }
                }
                
                foreach (var ((rotations, cameraReference, pos, count), _) in entitiesDB
                            .QueryEntities<RotationComponent, CameraReferenceComponent,
                                 PositionComponent>(PlayerAliveGroup.Groups))
                {
                    for (int i = 0; i < count; i++)
                    {
                        Turning(
                            entitiesDB.QueryEntity<CameraOOPEntityComponent>(cameraReference[i].cameraReference
                               .ToEGID(entitiesDB)), ref pos[i], ref rotations[i]);
                    }
                }
            }

            while (true)
            {
                ProcessPlayerInput();

                yield return null; //don't forget to yield or you will enter in an infinite loop!
            }
        }

        void Movement(in PlayerInputDataComponent playerInput, ref RigidBodyComponent playerComponent)
        {
            // Normalise the movement vector and make it proportional to the speed per second.
            var movement = playerInput.input.normalized * PlayerSpeed;

            // Move the player to it's current position plus the movement.
            playerComponent.velocity = movement;
        }

        void Turning(in CameraOOPEntityComponent cameraInfo, ref PositionComponent pos, ref RotationComponent rotation)
        {
            // Create a ray from the mouse cursor on screen in the direction of the camera.
            if (_rayCaster.CheckHit(cameraInfo.camRay, camRayLength, floorMask, out var point))
            {
                // Create a vector from the player to the point on the floor the raycast from the mouse hit.
                var playerToMouse = point - pos.position;

                // Ensure the vector is entirely along the floor plane.
                playerToMouse.y = 0f;

                // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
                var newRotation = Quaternion.LookRotation(playerToMouse);

                // Set the player's rotation to this new rotation.
                rotation.rotation = newRotation;
            }
        }

        readonly IRayCaster _rayCaster;
        readonly IEnumerator _tick;

        readonly int floorMask = LayerMask.GetMask("Floor"); // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
        const float PlayerSpeed = 6.0f;
    }
}