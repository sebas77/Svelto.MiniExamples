using Svelto.Common;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class SyncCameraEntitiesToObjects: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncCameraEntitiesToObjects(GameObjectResourceManager manager)
        {
            _manager = manager;
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }

        public void Step()
        {
            var groups = entitiesDB.FindGroups<GameObjectEntityComponent, CameraOOPEntityComponent>();

            foreach (var ((entity, cameras, count), _) in entitiesDB
                        .QueryEntities<GameObjectEntityComponent, CameraOOPEntityComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    var go = _manager[entity[i].resourceIndex];
                    var camera = go.GetComponent<UnityEngine.Camera>();

                    cameras[i].camRay = camera.ScreenPointToRay(cameras[i].camRayInput);
                }
            }
        }

        public string name => nameof(SyncCameraEntitiesToObjects);
        readonly GameObjectResourceManager _manager;
    }
}