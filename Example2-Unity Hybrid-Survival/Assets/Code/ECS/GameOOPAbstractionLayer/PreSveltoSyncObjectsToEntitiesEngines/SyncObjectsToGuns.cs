namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class SyncObjectsToGuns: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncObjectsToGuns(GameObjectResourceManager manager)
        {
            _manager = manager;
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }

        public void Step()
        {
            var groups = entitiesDB.FindGroups<GameObjectEntityComponent, GunOOPEntityComponent>();

            foreach (var ((entity, guns, count), _) in entitiesDB
                            .QueryEntities<GameObjectEntityComponent, GunOOPEntityComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    ref var gunOopEntityComponent = ref guns[i];

                    var go = _manager[entity[i].resourceIndex];
                    var psfx = go.GetComponent<PlayerShootingFX>();
                    gunOopEntityComponent.shootRay = psfx.shootCastRay;
                }
            }
        }

        public string name => nameof(SyncObjectsToGuns);
        readonly GameObjectResourceManager _manager;
    }
}