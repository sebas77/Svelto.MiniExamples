namespace Svelto.ECS.Example.Survive.OOPLayer
{
    /// <summary>
    /// Cameras set position directly, it should be considered an exception, as objects should be physic or path driven
    /// </summary>
    public class SyncNavToObjects: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncNavToObjects(GameObjectResourceManager manager)
        {
            _manager = manager;
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }
        public void Step()
        {
            //only cameras
            var groups = entitiesDB.FindGroups<GameObjectEntityComponent, NavMeshComponent>();
            //position only sync
            foreach (var ((entity, navs, count), _) in entitiesDB
                            .QueryEntities<GameObjectEntityComponent, NavMeshComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    var go = _manager[entity[i].resourceIndex];

                    var navMesh = go.GetComponent<NavMeshBehaviour>();

                    navMesh.setCapsuleAsTrigger = navs[i].setCapsuleAsTrigger;
                    navMesh.navMeshEnabled = navs[i].navMeshEnabled;
                    if (navs[i].navMeshEnabled)
                        navMesh.navMeshDestination = navs[i].navMeshDestination;
                }
            }
        }

        public string name => nameof(SyncNavToObjects);

        readonly GameObjectResourceManager _manager;
    }
}