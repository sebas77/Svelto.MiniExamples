using System.Collections;
using Svelto.ECS.Example.Survive.OOPLayer;

using UnityEngine;

namespace Svelto.ECS.Example.Survive.Camera
{
    //First step identify the entity type we want the engine to handle: CameraEntity
    //Second step name the engine according the behaviour and the entity: I.E.: CameraFollowTargetEngine
    //Third step start to write the code and create classes/fields as needed using refactoring tools 
    public class CameraFollowingTargetEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public CameraFollowingTargetEngine()
        {
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
            void TrackCameraTarget()
            {
                var (targets, cameras, cameraPositions, count) = entitiesDB
                   .QueryEntities<CameraTargetEntityReferenceComponent, CameraOOPEntityComponent,
                        PositionComponent>(Camera.Group);
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

                            cameraPosition.position = cameraTarget.position + camera.offset;
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

        readonly IEnumerator _update;
    }
}