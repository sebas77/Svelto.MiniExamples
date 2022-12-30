using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class SyncGunEntitiesToObjects: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncGunEntitiesToObjects(GameObjectResourceManager manager)
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

                    var effectState = gunOopEntityComponent.GetStateAndReset();
                    switch (effectState)
                    {
                        case PlayState.start:
                            psfx.PlayEffects(gunOopEntityComponent.lineEndPosition);
                            break;
                        case PlayState.stop:
                            psfx.StopEffects();
                            break;
                        case PlayState.play:
                            gunOopEntityComponent.effectsEnabledForTime -= Time.deltaTime;
                            if (gunOopEntityComponent.effectsEnabledForTime <= 0)
                                gunOopEntityComponent.effectsEnabled = false;
                            break;
                    }
                }
            }
        }

        public string name => nameof(SyncGunEntitiesToObjects);
        readonly GameObjectResourceManager _manager;
    }
}