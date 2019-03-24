using Svelto.ECS.Example.Survive.Characters.Enemies;
using Svelto.ECS.Example.Survive.Characters.Player;
using Svelto.ECS.Example.Survive.Characters.Player.Gun;
using Svelto.ECS.Example.Survive.HUD;
using Svelto.Context;
using Svelto.ECS.Example.Survive.Camera;
using Svelto.ECS.Example.Survive.Characters;
using Svelto.ECS.Example.Survive.Characters.Sounds;
using UnityEngine;
using Svelto.ECS.Schedulers.Unity;
using Svelto.Tasks;

//Main is the Application Composition Root.
//A Composition Root is the where all the dependencies are 
//created and injected (I talk a lot about this in my articles)
//A composition root belongs to the Context, but
//a context can have more than a composition root.
//For example a factory is a composition root.
//Furthermore an application can have more than a context
//but this is more advanced and not part of this demo
namespace Svelto.ECS.Example.Survive
{
    /// <summary>
    ///IComposition root is part of Svelto.Context
    ///Svelto.Context is not formally part of Svelto.ECS, but
    ///it's helpful to use in an environment where a Context is
    ///not formally present, like in Unity. 
    /// </summary>
    public class Main : ICompositionRoot
    {
        public Main()
        {
            InitAssets();
            SetupEngines();
            SetupEntities();
        }

        void InitAssets()
        {
            //Do not copy this. initially I thought it was a good idea to use
            //Json serialization to replace resources, but it's less convenient
            //than I thought
            GameObject.Find("PrefabsSerializer").GetComponent<PrefabSerializer>().Init();
            GameObject.Find("EnemySpawnerData").GetComponent<SpawningData>().Init();
        }

        /// <summary>
        /// Before to start a review of Svelto.ECS terminologies:
        /// - Entity:
        ///     it must be a real and concrete entity that you can explain
        ///     in terms of game design. The name of each entity should reflect
        ///     a specific concept from the game design domain
        /// - Engines (Systems):
        ///     Where all the logic lies. Engines operates on EntityViews or EntityStructs
        /// - EntityViews and EntitiyViewStructs:
        ///     EntityViews maps Entity Components. The Engines can't
        ///     access directly to each entity (as a single set of components), but
        ///     through component sets defined by the EntityView.
        ///     They act as component filters and expose only the entity components
        ///     that the Engine is interested in.
        ///     EntityViews are actually defined with the Engine so they
        ///     come together with the engine and in the same namespace of the engine.
        ///     EntityViewStructs should always be used, while EntityViews as
        ///     class use should be considered an exception. 
        /// - Component Interfaces:
        ///     Components must be seen as data holders. There may be implementation
        ///     exceptions, but the interface must declare a group
        ///     of readable and/or writeable data.
        ///     In Svelto.ECS components are always interfaces declaring
        ///     Setters and Getters of Value Types. DispatchOnSet
        ///     and DispatchOnChange must not be seen as events, but
        ///     as pushing of data instead of data polling, similar
        ///     to the concept of DataBinding.
        /// - Implementors:
        ///     Being components interfaces, they must be implemented through
        ///     Implementors. The relation Implementors to Components
        ///     is not 1:1 so that you can group several
        ///     components into fewer implementors. This allows to easily
        ///     share data between components. Implementors also act
        ///     as bridge between the platform and Svelto.ECS.
        ///     Since Components can hold only value types, Implementors
        ///     are the objects that can interact directly with the platform
        ///     objects, I.E.: RigidBody, Transform and so on.
        ///     Note: IComponents must hold only valuetypes for
        ///     code design purposes and not optmization purposes.
        ///     The reason is that all the logic must lie in the engines
        ///     so Components cannot hold references to instances that can
        ///     expose functions with logic.
        /// - EntityStructs:
        ///     In order to write Data Oriented Cache Friendly and allocation 0 code, Svelto.ECS
        ///     also supports EntityStructs. 
        /// - EntityDescriptors:
        ///     Gives a way to formalize your Entity in svelto.ECS, it also
        ///     defoines the EntityViews, EntityStructs and EntityViewStructs that must be generated once the
        ///     Entity is built
        /// </summary>
        void SetupEngines()
        {
            //The Engines Root is the core of Svelto.ECS. You must NEVER inject the EngineRoot
            //as it is, therefore the composition root must hold a reference or it will be 
            //GCed.
            //the UnitySumbmissionEntityViewScheduler is the scheduler that is used by the EnginesRoot to know
            //when to inject the EntityViews. You shouldn't use a custom one unless you know what you 
            //are doing or you are not working with Unity.
            _unityEntitySubmissionScheduler = new UnityEntitySubmissionScheduler();
            _enginesRoot                    = new EnginesRoot(_unityEntitySubmissionScheduler);
            //Engines root can never be held by anything else than the context itself to avoid leaks
            //That's why the EntityFactory and EntityFunctions are generated.
            //The EntityFactory can be injected inside factories (or engine acting as factories)
            //to build new entities dynamically
            _entityFactory = _enginesRoot.GenerateEntityFactory();
            //The entity functions is a set of utility operations on Entities, including
            //removing an entity. I couldn't find a better name so far.
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();

            //the ISequencer is one of the 2 official ways available in Svelto.ECS 
            //to communicate. They are mainly used for two specific cases:
            //1) specify a strict execution order between engines (engine logic
            //is executed horizontally instead than vertically, I will talk about this
            //in my articles). 2) filter a data token passed as parameter through
            //engines. The ISequencer is also not the common way to communicate
            //between engines
            PlayerDeathSequencer playerDeathSequence = new PlayerDeathSequencer();
            EnemyDeathSequencer  enemyDeathSequence  = new EnemyDeathSequencer();

            //wrap non testable unity static classes, so that 
            //can be mocked if needed.
            IRayCaster rayCaster = new RayCaster();
            ITime      time      = new Time();

            //Player related engines. ALL the dependencies must be solved at this point
            //through constructor injection.
            var playerShootingEngine  = new PlayerGunShootingEngine(rayCaster, time);
            var playerMovementEngine  = new PlayerMovementEngine(rayCaster, time);
            var playerAnimationEngine = new PlayerAnimationEngine();
            var playerDeathEngine     = new PlayerDeathEngine(playerDeathSequence, entityFunctions);

            //Enemy related engines
            var enemyAnimationEngine = new EnemyAnimationEngine(time, enemyDeathSequence, entityFunctions);
            var enemyAttackEngine    = new EnemyAttackEngine(time);
            var enemyMovementEngine  = new EnemyMovementEngine();

            //GameObjectFactory allows to create GameObjects without using the Static
            //method GameObject.Instantiate. While it seems a complication
            //it's important to keep the engines testable and not
            //coupled with hard dependencies references (read my articles to understand
            //how dependency injection works and why solving dependencies
            //with static classes and singletons is a terrible mistake)
            GameObjectFactory gameObjectFactory = new GameObjectFactory();
            //Factory is one of the few patterns that work very well with ECS. Its use is highly encouraged
            IEnemyFactory enemyFactory       = new EnemyFactory(gameObjectFactory, _entityFactory);
            var           enemySpawnerEngine = new EnemySpawnerEngine(enemyFactory, entityFunctions);
            var           enemyDeathEngine   = new EnemyDeathEngine(entityFunctions, enemyDeathSequence);

            //hud and sound engines
            var hudEngine         = new HUDEngine(time);
            var damageSoundEngine = new DamageSoundEngine();
            var scoreEngine       = new ScoreEngine();

            //The ISequencer implementation is very simple, but allows to perform
            //complex concatenation including loops and conditional branching.
            //These two sequencers are a real stretch and are shown only for explanatory purposes. 
            //Please do not see sequencers as a way to dispatch or broadcast events, they are meant only and exclusively
            //to guarantee the order of execution of the involved engines.
            //For this reason the use of sequencers is and must be actually rare, as perfectly encapsulated engines
            //do not need to be executed in specific order.
            //a Sequencer can: 
            //- ensure the order of execution through one step only (one step executes in order several engines)
            //- ensure the order of execution through several steps. Each engine inside each step has the responsibility
            //to trigger the next step through the use of the Next() function
            //- create paths with branches and loop using the Condition parameter.
            playerDeathSequence.SetSequence(playerDeathEngine,
                                            playerMovementEngine,
                                            playerAnimationEngine,
                                            enemyAnimationEngine,
                                            damageSoundEngine,
                                            hudEngine);

            enemyDeathSequence.SetSequence(enemyDeathEngine, scoreEngine, damageSoundEngine, enemyAnimationEngine,
                                           enemySpawnerEngine);


            //All the logic of the game must lie inside engines
            //Player engines
            _enginesRoot.AddEngine(playerMovementEngine);
            _enginesRoot.AddEngine(playerAnimationEngine);
            _enginesRoot.AddEngine(playerShootingEngine);
            _enginesRoot.AddEngine(new PlayerInputEngine());
            _enginesRoot.AddEngine(new PlayerGunShootingFXsEngine());
            _enginesRoot.AddEngine(playerDeathEngine);

            //enemy engines
            _enginesRoot.AddEngine(enemySpawnerEngine);
            _enginesRoot.AddEngine(enemyAttackEngine);
            _enginesRoot.AddEngine(enemyMovementEngine);
            _enginesRoot.AddEngine(enemyAnimationEngine);
            _enginesRoot.AddEngine(enemyDeathEngine);
            //other engines
            _enginesRoot.AddEngine(new ApplyingDamageToTargetsEngine());
            _enginesRoot.AddEngine(new CameraFollowTargetEngine(time));
            _enginesRoot.AddEngine(new CharactersDeathEngine());
            _enginesRoot.AddEngine(damageSoundEngine);
            _enginesRoot.AddEngine(hudEngine);
            _enginesRoot.AddEngine(scoreEngine);
        }


        /// <summary>
        /// While until recently I thought that creating entities in the context would be all right, I am coming to
        /// realise that engines should always handle the creation of entities. I will refactor this when the right
        /// time comes.
        /// </summary>
        void SetupEntities()
        {
            BuildPlayerEntities();
            BuildCameraEntity();
        }

        public void OnContextCreated<T>(T contextHolder)
        {
            BuildEntitiesFromScene(contextHolder as UnityContext);
        }

        void BuildPlayerEntities()
        {
            var prefabsDictionary = new PrefabsDictionary();

            var player = prefabsDictionary.Istantiate("Player");

            //Building entities explicitly should be always preferred and MUST be used if an implementor doesn't need to
            //be a Monobehaviour. You should strive to create implementors not as monobehaviours. Implementors as
            //monobehaviours are meant only to function as bridge between Svelto.ECS and Unity3D. Using implementor as
            //monobehaviour just to read serialized data from the editor, is also a bad practice, use a Json file
            //instead. The Player Entity is made of EntityViewStruct+Implementors as monobehaviours and EntityStructs.
            //The PlayerInputDataStruct doesn't need to be initialized (yay!!) but the HealthEntityStruct does.
            //Here I show the official method to do it
            var initializer =
                _entityFactory.BuildEntity<PlayerEntityDescriptor>(player.GetInstanceID(), ECSGroups.Player,
                                                                   player.GetComponents<IImplementor>());
            initializer.Init(new HealthEntityStruct {currentHealth = 100});

            //unluckily the gun is parented in the original prefab, so there is no easy way to create it explicitly, I
            //have to create if from the existing gameobject.
            var gun = player.GetComponentInChildren<PlayerShootingImplementor>();

            _entityFactory.BuildEntity<PlayerGunEntityDescriptor>(gun.gameObject.GetInstanceID(), ECSGroups.Player,
                                                                  new[] {gun});
        }

        void BuildCameraEntity()
        {
            var implementor = UnityEngine.Camera.main.gameObject.AddComponent<CameraImplementor>();

            _entityFactory.BuildEntity<CameraEntityDescriptor>(UnityEngine.Camera.main.GetInstanceID(),
                                                               ECSGroups.ExtraStuff, new[] {implementor});
        }

        /// <summary>
        /// This is a possible approach to create Entities from already existing GameObject in the scene
        /// It is absolutely not necessary and I wouldn't rely on this in production
        /// </summary>
        /// <param name="contextHolder"></param>
        void BuildEntitiesFromScene(UnityContext contextHolder)
        {
            //An EntityDescriptorHolder is a special Svelto.ECS class created to exploit
            //GameObjects to dynamically retrieve the Entity information attached to it.
            //Basically a GameObject can be used to hold all the information needed to create
            //an Entity and later queries to build the entitity itself.
            //This allows to trigger a sort of polymorphic code that can be re-used to 
            //create several type of entities.

            IEntityDescriptorHolder[] entities = contextHolder.GetComponentsInChildren<IEntityDescriptorHolder>();

            //However this common pattern in Svelto.ECS application exists to automatically
            //create entities from gameobjects already presented in the scene.
            //I still suggest to avoid this method though and create entities always
            //manually and explicitly. Basically EntityDescriptorHolder should be avoided
            //whenever not strictly necessary.

            for (int i = 0; i < entities.Length; i++)
            {
                var entityDescriptorHolder = entities[i];
                var entityViewsToBuild     = entityDescriptorHolder.GetDescriptor();
                _entityFactory.BuildEntity
                    (new EGID(((MonoBehaviour) entityDescriptorHolder).gameObject.GetInstanceID(), ECSGroups.ExtraStuff),
                     entityViewsToBuild,
                     (entityDescriptorHolder as MonoBehaviour).GetComponentsInChildren<IImplementor>());
            }
        }

        public void OnContextInitialized()
        {
        }

        public void OnContextDestroyed()
        {
            //final clean up
            _enginesRoot.Dispose();

            //Tasks can run across level loading, so if you don't want
            //that, the runners must be stopped explicitly.
            //careful because if you don't do it and 
            //unintentionally leave tasks running, you will cause leaks
            TaskRunner.StopAndCleanupAllDefaultSchedulers();
        }

        EnginesRoot                    _enginesRoot;
        IEntityFactory                 _entityFactory;
        UnityEntitySubmissionScheduler _unityEntitySubmissionScheduler;
    }

    /// <summary>
    ///At least One GameObject containing a UnityContext must be present in the scene.
    ///All the monobehaviours existing in gameobjects child of the UnityContext one, 
    ///can be later queried, usually to create entities from statically created
    ///gameobjects. 
    /// </summary>
    public class MainContext : UnityContext<Main>
    {
    }
}