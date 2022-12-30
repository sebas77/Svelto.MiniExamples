using System.Collections;
using System.Threading.Tasks;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Example.Survive.Camera;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.OOPLayer;
using Svelto.ECS.Example.Survive.Player.Gun;


namespace Svelto.ECS.Example.Survive.Player
{
    public class PlayerFactory
    {
        public PlayerFactory(GameObjectResourceManager gameObjectResourceManager, IEntityFactory entityFactory)
        {
            _gameObjectResourceManager = gameObjectResourceManager;
            _entityFactory = entityFactory;
        }

        public async Task StartSpawningPlayerTask()
        {
            var playerLoading = await _gameObjectResourceManager.Build("Player");
            var cameraLoading = await _gameObjectResourceManager.Build("CameraPrefab");

            ValueIndex playerID = playerLoading;
            ValueIndex cameraID = cameraLoading;

            Init();

            void Init()
            {
                BuildPlayerEntity(playerID, out var playerInitializer);
                BuildGunEntity(playerInitializer, playerID);
                BuildCameraEntity(ref playerInitializer, cameraID);
            }
        }

        void BuildPlayerEntity(ValueIndex playerID, out EntityInitializer playerInitializer)
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
            playerInitializer = _entityFactory.BuildEntity<PlayerEntityDescriptor>(0, PlayerAliveGroup.BuildGroup);

            //BuildEntity returns an initializer that can be used to initialise all the entity components generated
            //by the entity descriptor. In this case I am initializing just the Health.
            //being lazy here, it should be read from json file
            playerInitializer.Init(
                new HealthComponent
                {
                        currentHealth = 100
                });
            playerInitializer.Init(
                new GameObjectEntityComponent()
                {
                        resourceIndex = playerID,
                        layer = GAME_LAYERS.PLAYER_LAYER
                });
            playerInitializer.Init(
                new AnimationComponent()
                {
                        animationState = new AnimationState(PlayerAnimations.IsWalking, false)
                });

            var playerResource = _gameObjectResourceManager[playerID];
            playerResource.GetComponent<EntityReferenceHolder>().reference = playerInitializer.reference.ToULong();
            playerInitializer.Init(
                new PositionComponent()
                {
                        position = playerResource.transform.position
                });
        }

        void BuildGunEntity(EntityInitializer playerInitializer, ValueIndex valueIndex)
        {
            //Gun and player are two different entities, but they are linked by the EGID
            //in this case we assume that we know at all the time the ID of the gun and the group where the gun is
            //but this is not often the case when groups must be swapped.

            //Note: in the last refactoring I changed the code to not rely on the knowledge that the ID is known
            //as this trick cannot be used to determine an EGID anymore once group compounds are used. 
            //therefore I switched to the use of EntityReferences.
            var init = _entityFactory.BuildEntity<PlayerGunEntityDescriptor>(
                playerInitializer.EGID.entityID,
                PlayerGun.Gun.Group);

            //being lazy here, it should be read from json file
            init.Init(
                new GunComponent()
                {
                        timeBetweenBullets = 0.3f,
                        range = 100f,
                        damagePerShot = 20,
                        timer = 0.3f
                });

            var gunObject = _gameObjectResourceManager[valueIndex].GetComponentInChildren<PlayerShootingFX>()
                   .gameObject;
            var gunIndex = _gameObjectResourceManager.Add(gunObject);

            init.Init(
                new GameObjectEntityComponent
                {
                        resourceIndex = gunIndex
                });

            //set the gun to the player component
            playerInitializer.Init(
                new WeaponComponent()
                {
                        weapon = init.reference
                });
        }

        /// <summary>
        /// This demo has just one camera, but it would be simple to create a camera for each player for a split
        /// screen scenario. 
        /// <param name="playerID"></param>
        /// <param name="gameObject"></param>
        void BuildCameraEntity(ref EntityInitializer playerID, ValueIndex cameraID)
        {
            var cameraResource = _gameObjectResourceManager[cameraID];
            var playerPosition = playerID.Get<PositionComponent>();

            var cameraInit =
                    _entityFactory.BuildEntity<CameraEntityDescriptor>(playerID.EGID.entityID, Camera.Camera.Group);

            cameraInit.Init(
                new CameraTargetEntityReferenceComponent()
                {
                        targetEntity = playerID.reference
                });
            cameraInit.Init(
                new CameraOOPEntityComponent()
                {
                        offset = cameraResource.transform.position - playerPosition.position
                });
            cameraInit.Init(
                new GameObjectEntityComponent
                {
                        resourceIndex = cameraID
                });

            playerID.Init(
                new CameraReferenceComponent()
                {
                        cameraReference = cameraInit.reference
                });
        }

        readonly IEntityFactory _entityFactory;
        readonly GameObjectResourceManager _gameObjectResourceManager;
        IEnumerator _spawnPlayer;
    }
}