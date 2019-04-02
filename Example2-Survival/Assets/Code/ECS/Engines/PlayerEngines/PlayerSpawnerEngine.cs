using System.Collections;
using Svelto.ECS.Example.Survive;
using Svelto.ECS.Example.Survive.Characters;
using Svelto.ECS.Example.Survive.Characters.Player;
using Svelto.ECS.Example.Survive.ResourceManager;
using UnityEngine;

namespace Svelto.ECS.Example.Player
{
    public class PlayerSpawnerEngine : IQueryingEntitiesEngine
    {
        public PlayerSpawnerEngine(GameObjectFactory gameobjectFactory, IEntityFactory entityFactory)
        {
            _gameobjectFactory = gameobjectFactory;
            _entityFactory = entityFactory;
        }
        
        public IEntitiesDB entitiesDB { private get; set; }

        public void Ready() { SpawnPlayer().Run(); }

        IEnumerator SpawnPlayer()
        {
            var enumerator = _gameobjectFactory.Build("Player");
            
            yield return enumerator;
            
            GameObject player = enumerator.Current;
            
            //implementors are ONLY necessary if you need to wrap objects around entity view structs. In the case
            //of Unity, they are needed to wrap Monobehaviours and not used in any other case
            
            var implementors = player.GetComponentsInChildren<IImplementor>();

            //Initialize an entity inside a composition root is a so-so practice, better to have an engineSpawner.
            var initializer =
                _entityFactory.BuildEntity<PlayerEntityDescriptor>((uint) player.GetInstanceID(), ECSGroups.Player,
                                                                   player.GetComponents<IImplementor>());
            initializer.Init(new HealthEntityStruct {currentHealth = 100});

            //unluckily the gun is parented in the original prefab, so there is no easy way to create it explicitly, I
            //have to create if from the existing gameobject.
            var gunImplementor = player.GetComponentInChildren<PlayerShootingImplementor>();

            _entityFactory.BuildEntity<PlayerGunEntityDescriptor>((uint) gunImplementor.gameObject.GetInstanceID(),
                                                                  ECSGroups.Player, new[] {gunImplementor});

        }
        
        readonly IEntityFactory    _entityFactory;
        readonly GameObjectFactory _gameobjectFactory;
    }
}