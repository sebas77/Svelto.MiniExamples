using System.Collections;
using System.Collections.Generic;
using Code.ECS.Shared;
using Svelto.ECS.Example.Survive.Camera;
using Svelto.ECS.Example.Survive.Player.Gun;
using Svelto.ECS.Example.Survive.ResourceManager;
// using Svelto.ECS.Example.Survive.Player.Gun;
// using Svelto.ECS.Example.Survive.ResourceManager;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Player
{
    public class PlayerSpawner
    {
        public PlayerSpawner(GameObjectResourceManager gameObjectResourceManager, IEntityFactory entityFactory)
        {
            _gameObjectResourceManager = gameObjectResourceManager;
            _entityFactory             = entityFactory;
        }

        public IEnumerator SpawnPlayer()
        {
            var playerLoading = _gameObjectResourceManager.Build("Player");

            while (playerLoading.MoveNext()) yield return null;

            var cameraLoading = _gameObjectResourceManager.Build("CameraPrefab");

            while (cameraLoading.MoveNext()) yield return null;

            uint playerID = playerLoading.Current.Value;
            uint cameraID = cameraLoading.Current.Value;

            BuildPlayerEntity(playerID, out var playerInitializer);
            BuildGunEntity(playerInitializer);
            BuildCameraEntity(ref playerInitializer, cameraID);
        }

        void BuildPlayerEntity(uint playerID, out EntityInitializer playerInitializer)
        {
            //Build the Svelto ECS entity for the player. Svelto.ECS has the unique feature to let the user decide
            //the ID of the entity (which must be anyway unique). The user may think that using, for example, 
            //the GameObject.GetInstanceID() value as entity ID is a good idea, as it would be simple to fetch the
            //entity from the outcome of unity callbacks (like collisions). While you can do so, you must not think
            //it's the only way to go. For this reason I decide instead to use 0 for this example and show how
            //to handle the situation.
            //ECSGroups.Player is the group where the entity player will be built. I usually expect a 
            //group for entity descriptor. It is the safest way to go, but advanced users may decide to use different
            //groups layout if needed.
            //if the Svelto entity is linked to an external OOP resource, like the GameObject in this case, the
            //relative implementor must be passed to the BuildEntity method.
            //Pure ECS (no OOP) entities do not need implementors passed.
            playerInitializer = _entityFactory.BuildEntity<PlayerEntityDescriptor>(0, Player.BuildGroup);

            //BuildEntity returns an initializer that can be used to initialise all the entity components generated
            //by the entity descriptor. In this case I am initializing just the Health.
            //being lazy here, it should be read from json file
            playerInitializer.Init(new HealthComponent
            {
                currentHealth = 100
            });
            playerInitializer.Init(new SpeedComponent(6));
            playerInitializer.Init(new PlayerEntityComponent()
            {
                resourceIndex = playerID
            });

            var playerResource = _gameObjectResourceManager[playerID];
            playerInitializer.Init(new PositionComponent()
            {
                position = playerResource.transform.position
            });
        }

        void BuildGunEntity(EntityInitializer playerInitializer)
        {
            //Gun and player are two different entities, but they are linked by the EGID
            //in this case we assume that we know at all the time the ID of the gun and the group where the gun is
            //but this is not often the case when groups must be swapped.

            //Note: in the last refactoring I changed the code to not rely on the knowledge that the ID is known
            //as this trick cannot be used to determine an EGID anymore once group compounds are used. 
            //therefore I switched to the use of EntityReferences.
            var init = _entityFactory.BuildEntity<PlayerGunEntityDescriptor>(playerInitializer.EGID.entityID,
                Survive.Weapons.Gun.BuildGroup);

            //being lazy here, it should be read from json file
            init.Init(new GunAttributesComponent()
            {
                timeBetweenBullets = 0.15f, range = 100f, damagePerShot = 20,
            });

            playerInitializer.Init(new PlayerWeaponComponent()
            {
                weapon = init.reference
            });
        }

        /// <summary>
        /// This demo has just one camera, but it would be simple to create a camera for each player for a split
        /// screen scenario. 
        /// <param name="playerID"></param>
        /// <param name="gameObject"></param>
        void BuildCameraEntity(ref EntityInitializer playerID, uint cameraID)
        {
            var cameraResource = _gameObjectResourceManager[cameraID];
            // //implementors can be attatched at run time, while not? Check the player spawner engine to 
            // //read more about implementors
            var playerPosition = playerID.Get<PositionComponent>();

            var cameraInit =
                _entityFactory.BuildEntity<CameraEntityDescriptor>(playerID.EGID.entityID, Camera.Camera.BuildGroup);

            cameraInit.Init(new CameraTargetEntityReferenceComponent() { targetEntity = playerID.reference });
            cameraInit.Init(new CameraEntityComponent()
            {
                offset = cameraResource.transform.position - playerPosition.position
            });
            playerID.Init(new CameraReferenceComponent()
            {
                cameraReference = cameraInit.reference
            });
        }

        readonly IEntityFactory            _entityFactory;
        readonly GameObjectResourceManager _gameObjectResourceManager;
        IEnumerator                        _spawnPlayer;
    }
}