namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class SyncCameraObjectsToEntities: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncCameraObjectsToEntities(GameObjectResourceManager manager)
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

                    if (cameras[i].inputRead == true)
                        cameras[i].camRay = camera.ScreenPointToRay(cameras[i].camRayInput);
                }
            }
        }

        public string name => nameof(SyncCameraObjectsToEntities);
        readonly GameObjectResourceManager _manager;
    }
}