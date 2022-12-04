using Svelto.Common;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class SyncGunToObjectsEngine: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncGunToObjectsEngine(GameObjectResourceManager manager)
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
                    gunOopEntityComponent.shootRay = psfx.shootRay;

                    var effectState = gunOopEntityComponent.GetStateAndReset();
                    if (effectState == PlayState.start)
                    {
                        psfx.PlayEffects(gunOopEntityComponent.lineEndPosition);
                        gunOopEntityComponent.effectsDisplayTime = psfx.effectsDisplayTime;
                    }
                    else if (effectState == PlayState.stop)
                    {
                        psfx.StopEffects();
                    }
                }
            }
        }

        public string name => nameof(SyncGameObjectsEngine);
        readonly GameObjectResourceManager _manager;
    }
}