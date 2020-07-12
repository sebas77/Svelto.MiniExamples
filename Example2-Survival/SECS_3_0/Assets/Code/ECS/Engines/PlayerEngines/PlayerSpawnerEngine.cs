using System.Collections;
using System.Collections.Generic;
using Svelto.ECS.Example.Survive;
using Svelto.ECS.Example.Survive.Characters;
using Svelto.ECS.Example.Survive.Characters.Player;
using Svelto.ECS.Example.Survive.ResourceManager;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Hybrid;
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
        
        public EntitiesDB entitiesDB { private get; set; }

        public void Ready() { SpawnPlayer().Run(); }

        IEnumerator SpawnPlayer()
        {
            IEnumerator<GameObject> loadingAsync = _gameobjectFactory.Build("Player");
            
            yield return loadingAsync; //wait until the asset is loaded and the gameobject built
            
            GameObject player = loadingAsync.Current;
            
            //Get the gameobject "implementors". Implementors can be monobehaviours that can be used with Svelto.ECS
            //unluckily the gun is found in the same prefab of the character, so the best thing to do is to search
            //for all the implementors in the hiearchy and pass them to the BuildEntity. The BuildEntity will then
            //search for the implementors the need (player and guns separately)
            
            List<IImplementor> implementors = new List<IImplementor>();
            player.GetComponentsInChildren(true, implementors);
            
            //EghidHolderImplementor is a special framework provided implementor that tracks the EGID 
            //of the entity linked to the implementor
            IImplementor egidHoldImplementor = player.AddComponent<EGIDHolderImplementor>();
            implementors.Add(egidHoldImplementor);
            
            //Build the Svelto ECS entity for the player. Svelto.ECS has the unique feature to let the user decide
            //the ID of the entity (which must be anyway unique). The user may think that using, for example, 
            //the GameObject.GetInstanceID() value as entity ID is a good idea, as it would be simple to fetch the
            //entity from the outcome of unity callbacks (like collisions). While you can do so, you must not think
            //it's the only way to go. For this reason I decide instead to use 0 for this example and show how
            //to handle the situation.
            //ECSGroups.PlayersGroup is the group where the entity player will be built. I usually expect a 
            //group for entity descriptor. It is the safest way to go, but advanced users may decide to use different
            //groups layout if needed.
            //if the Svelto entity is linked to an external OOP resource, like the GameObject in this case, the
            //relative implementor must be passed to the BuildEntity method.
            //Pure ECS (no OOP) entities do not need implementors passed.
            var playerInitializer =
                _entityFactory.BuildEntity<PlayerEntityDescriptor>(0, ECSGroups.PlayersGroup, implementors);
            
            //BuildEntity returns an initializer that can be used to initialise all the entity components generated
            //by the entity descriptor. In this case I am initializing just the Health.
            playerInitializer.Init(new HealthComponent {currentHealth = 100});
            
            //Gun and player are two different entities, but they are linked by the EGID
            //in this case we assume that we know at all the time the ID of the gun and the group where the gun is
            //but this is not often the case when groups must be swapped.
            _entityFactory.BuildEntity<PlayerGunEntityDescriptor>((uint) playerInitializer.EGID.entityID,
                                                                  ECSGroups.PlayersGunsGroup, implementors);

        }
        
        readonly IEntityFactory    _entityFactory;
        readonly GameObjectFactory _gameobjectFactory;
    }
}