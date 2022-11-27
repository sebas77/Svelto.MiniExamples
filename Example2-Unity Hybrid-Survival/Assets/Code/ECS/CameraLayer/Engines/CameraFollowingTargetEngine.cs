using System.Collections;
using Svelto.ECS.Example.Survive.Transformable;
using UnityEngine;
using UnityEngine.UIElements;

namespace Svelto.ECS.Example.Survive.Camera
{
    //First step identify the entity type we want the engine to handle: CameraEntity
    //Second step name the engine according the behaviour and the entity: I.E.: CameraFollowTargetEngine
    //Third step start to write the code and create classes/fields as needed using refactoring tools 
    public class CameraFollowingTargetEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public CameraFollowingTargetEngine(ITime time)
        {
            _time   = time;
            _update = Update();
        }

        public void Ready()
        {
        }

        public EntitiesDB entitiesDB { get; set; }
        public string     name       => nameof(CameraFollowingTargetEngine);

        public void Step()
        {
            _update.MoveNext();
        }

        IEnumerator Update()
        {
            var smoothing = 5.0f;

            void TrackCameraTarget()
            {
                foreach (var ((targets, cameras, cameraPositions, count), _) in entitiesDB
                            .QueryEntities<CameraTargetEntityReferenceComponent, CameraEntityComponent,
                                 PositionComponent>(Camera.Groups))
                {
                    for (uint i = 0; i < count; i++)
                    {
                        ref var camera         = ref cameras[i];
                        ref var cameraPosition = ref cameraPositions[i];

                        EntityReference entityReference = targets[i].targetEntity;
                        //The camera target can be destroyed while the camera is still active
                        if (entityReference.ToEGID(entitiesDB, out var targetEGID))
                        {
                            ref var cameraTarget = ref entitiesDB.QueryEntity<PositionComponent>(targetEGID);

                            var targetCameraPos = cameraTarget.position + camera.offset;

                            cameraPosition.position = Vector3.Lerp(cameraPosition.position, targetCameraPos,
                                smoothing * _time.deltaTime);
                        }
                    }
                }
            }

            while (true)
            {
                TrackCameraTarget();

                yield return null;
            }
        }

        readonly ITime       _time;
        readonly IEnumerator _update;
    }
}