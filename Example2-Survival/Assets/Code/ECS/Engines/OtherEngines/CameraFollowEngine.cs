using System.Collections;
using Svelto.Tasks;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Camera
{
    //First step identify the entity type we want the engine to handle: CameraEntity
    //Second step name the engine according the behaviour and the entity: I.E.: CameraFollowTargetEngine
    //Third step start to write the code and create classes/fields as needed using refactoring tools 
    public class CameraFollowTargetEngine : IQueryingEntitiesEngine
    {
        readonly ITime _time;

        public CameraFollowTargetEngine(ITime time) { _time = time; }

        public void Ready()
        {
            //Note standard schedulers are deprecated in Svelto.Tasks 2.0 and indeed they are not a good idea
            //they are ok for simple projects like this
            PhysicUpdate().RunOnScheduler(StandardSchedulers.physicScheduler);
        }

        public EntitiesDB entitiesDB { get; set; }

        IEnumerator PhysicUpdate()
        {
            //wait for the camera to be created (the task just spins)
            while (entitiesDB.HasAny<CameraEntityView>(ECSGroups.Camera) == false)
                yield return null;
            //wait for the camera target to be created (the task just spins)
            while (entitiesDB.HasAny<CameraTargetEntityView>(ECSGroups.CameraTargetGroup) == false)
                yield return null;
            
            var cameraTargetEntityView     = entitiesDB.QueryUniqueEntity<CameraTargetEntityView>(ECSGroups.PlayersGroup);
            var cameraEntityView = entitiesDB.QueryUniqueEntity<CameraEntityView>(ECSGroups.Camera);
            var offset           = cameraEntityView.positionComponent.position - cameraTargetEntityView.targetComponent.position;
            var smoothing = 5.0f;
            
            void TrackCameraTarget()
            {
                ref var camera       = ref entitiesDB.QueryUniqueEntity<CameraEntityView>(ECSGroups.Camera);
                var     cameraTarget = entitiesDB.QueryUniqueEntity<CameraTargetEntityView>(ECSGroups.PlayersGroup);

                var targetCameraPos = cameraTarget.targetComponent.position + offset;

                camera.transformComponent.position = Vector3.Lerp(camera.positionComponent.position, targetCameraPos
                                                                , smoothing * _time.deltaTime);
            }

            while (true)
            {
                //A QueryUniqueEntity is an exception more than anything else and using it instead of the
                //standard pattern, leads to some awkward checks like these
                while (entitiesDB.HasAny<CameraEntityView>(ECSGroups.Camera) == false)
                    yield return null;
                while (entitiesDB.HasAny<CameraTargetEntityView>(ECSGroups.CameraTargetGroup) == false)
                    yield return null;
                
                TrackCameraTarget();

                yield return null;
            }
        }
    }
}