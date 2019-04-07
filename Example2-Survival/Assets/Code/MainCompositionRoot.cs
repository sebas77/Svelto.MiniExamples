using Svelto.Context;
using Svelto.ECS.Example.Player;
using Svelto.ECS.Example.Survive.Camera;
using Svelto.ECS.Example.Survive.Characters;
using Svelto.ECS.Example.Survive.Characters.Enemies;
using Svelto.ECS.Example.Survive.Characters.Player;
using Svelto.ECS.Example.Survive.Characters.Player.Gun;
using Svelto.ECS.Example.Survive.Characters.Sounds;
using Svelto.ECS.Example.Survive.HUD;
using Svelto.ECS.Example.Survive.ResourceManager;
using Svelto.ECS.Schedulers.Unity;
using Svelto.ECS.Unity;
using Svelto.Tasks;
using UnityEngine;

//Main is the Application Composition Root. A Composition Root is the where all the dependencies are 
//created and injected (I talk a lot about this in my articles) A composition root belongs to the Context, but
//a context can have more than a composition root. For example a factory is a composition root.
//Furthermore an application can have more than a context but this is more advanced and not part of this demo
namespace Svelto.ECS.Example.Survive
{
    /// <summary>
    ///     IComposition root is part of Svelto.Context. Svelto.Context is not formally part of Svelto.ECS, but
    ///     it's helpful to use in an environment where a Context is not present, like in Unity.
    ///     It's a bootstrap! (check MainContext.cs)
    /// </summary>
    public class MainCompositionRoot : ICompositionRoot
    {
        EnginesRoot                    _enginesRoot;
        IEntityFactory                 _entityFactory;
        UnityEntitySubmissionScheduler _unityEntitySubmissionScheduler;

        public MainCompositionRoot()
        {
            QualitySettings.vSyncCount = 1;
            
            SetupEngines();
            SetupEntities();
        }

        public void OnContextCreated<T>(T contextHolder) { BuildEntitiesFromScene(contextHolder as UnityContext); }
        public void OnContextInitialized<T>(T contextHolder) { }
        public void OnContextDestroyed()
        {
            //final clean up
            _enginesRoot.Dispose();

            //Tasks can run across level loading, so if you don't want that, the runners must be stopped explicitly.
            //careful because if you don't do it and unintentionally leave tasks running, you will cause leaks
            TaskRunner.StopAndCleanupAllDefaultSchedulers();
        }

        /// <summary>
        ///     Before to start, let's review some of the Svelto.ECS terms:
        ///     - Entity:
        ///     it must be a real and concrete entity that you can explain in terms of game design. The name of each
        ///     entity should reflect a specific concept from the game design domain
        ///     - Engines (Systems):
        ///     Where all the logic lies. Engines operates on EntityViewStructs and EntityStructs
        ///     - EntityStructs:
        ///     EntityStructs is the preferred way to store entity data. They are just plain structs of pure data (no
        ///     objects)
        ///     - EntityViewStructs:
        ///     EntityViewStructs are used to wrap Objects that come from OOP libraries. You will never use it unless
        ///     you are forced to mix your ECS code with OOP code because of external libraries or platforms.
        ///     The Objects are known to svelto through Component Interfaces. 
        ///     - Component Interfaces:
        ///     Components must be seen as data holders. In Svelto.ECS components are always interfaces declaring
        ///     Setters and Getters of Value Types coming from the Objects they wrap 
        ///     - Implementors:
        ///     The components interfaces must be implemented through Implementors and the implementors are the
        ///     Objects you need to wrap.
        ///     - EntityDescriptors:
        ///     Gives a way to formalise your Entity, it also defines the EntityStructs and EntityViewStructs that must
        ///     be generated once the Entity is built
        /// </summary>
        void SetupEngines()
        {
            //The Engines Root is the core of Svelto.ECS. You shouldn't inject the EngineRoot,
            //therefore the composition root must hold a reference or it will be GCed.
            //the UnitySumbmissionEntityViewScheduler is the scheduler that is used by the EnginesRoot to know
            //when to submit the entities. Custom ones can be created for special cases.
            _unityEntitySubmissionScheduler = new UnityEntitySubmissionScheduler();
            _enginesRoot                    = new EnginesRoot(_unityEntitySubmissionScheduler);
            //The EntityFactory can be injected inside factories (or engine acting as factories) to build new entities
            //dynamically
            _entityFactory = _enginesRoot.GenerateEntityFactory();
            //The entity functions is a set of utility operations on Entities, including removing an entity. I couldn't
            //find a better name so far.
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();

            //Sequencers are the official way to guarantee order between engines, but may not be the best way for
            //your product.
            var playerDeathSequence = new PlayerDeathSequencer();
            var enemyDeathSequence  = new EnemyDeathSequencer();

            //wrap non testable unity static classes, so that can be mocked if needed.
            IRayCaster rayCaster = new RayCaster();
            ITime      time      = new Time();

            //Player related engines. ALL the dependencies must be solved at this point through constructor injection.
            var playerShootingEngine  = new PlayerGunShootingEngine(rayCaster, time);
            var playerMovementEngine  = new PlayerMovementEngine(rayCaster, time);
            var playerAnimationEngine = new PlayerAnimationEngine();
            var playerDeathEngine     = new PlayerDeathEngine(playerDeathSequence, entityFunctions);

            //Enemy related engines
            var enemyAnimationEngine = new EnemyAnimationEngine(time, enemyDeathSequence, entityFunctions);
            var enemyAttackEngine    = new EnemyAttackEngine(time);
            var enemyMovementEngine  = new EnemyMovementEngine();

            //GameObjectFactory allows to create GameObjects without using the Static method GameObject.Instantiate.
            //While it seems a complication it's important to keep the engines testable and not coupled with hard
            //dependencies
            var gameObjectFactory = new GameObjectFactory();
            //Factory is one of the few patterns that work very well with ECS. Its use is highly encouraged
            var enemyFactory       = new EnemyFactory(gameObjectFactory, _entityFactory);
            var enemySpawnerEngine = new EnemySpawnerEngine(enemyFactory, entityFunctions);
            var enemyDeathEngine   = new EnemyDeathEngine(entityFunctions, enemyDeathSequence);

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
            playerDeathSequence.SetSequence(playerDeathEngine, playerMovementEngine, playerAnimationEngine,
                                            enemyAnimationEngine, damageSoundEngine, hudEngine);

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
            _enginesRoot.AddEngine(new PlayerSpawnerEngine(gameObjectFactory, _entityFactory));
            
            
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

        void SetupEntities()
        {
            /// <summary>
            ///     While until recently I thought that creating entities in the context would be all right, I am coming
            ///     to realise that engines should always handle the creation of entities. 
            /// </summary>
            BuildCameraEntity();
        }

        void BuildCameraEntity()
        {
            var implementor = UnityEngine.Camera.main.gameObject.AddComponent<CameraImplementor>();

            _entityFactory.BuildEntity<CameraEntityDescriptor>((uint) UnityEngine.Camera.main.GetInstanceID(),
                                                               ECSGroups.ExtraStuff, new[] {implementor});
        }

        /// <summary>
        ///     This is a possible approach to create Entities from already existing GameObject in the scene
        ///     It is absolutely not necessary and I wouldn't rely on this in production
        /// </summary>
        /// <param name="contextHolder"></param>
        void BuildEntitiesFromScene(UnityContext contextHolder)
        {
            //An EntityDescriptorHolder is a special Svelto.ECS class created to exploit GameObjects to dynamically
            //retrieve implementors attached to it and then build a Svelto Entity.
            var entities = contextHolder.GetComponentsInChildren<IEntityDescriptorHolder>();

            //This pattern is usually useful for guis where complex hierarchy of gameobjects are necessary, but
            //otherwise you should always create entities in factories. A Better GUI only example is necessary
            for (var i = 0; i < entities.Length; i++)
            {
                var entityDescriptorHolder = entities[i];
                var entityViewsToBuild     = entityDescriptorHolder.GetDescriptor();
                _entityFactory
                   .BuildEntity(
                        new EGID((uint) ((MonoBehaviour) entityDescriptorHolder).gameObject.GetInstanceID(),
                                 ECSGroups.ExtraStuff), entityViewsToBuild,
                                (entityDescriptorHolder as MonoBehaviour).GetComponentsInChildren<IImplementor>());
            }
        }
    }
}