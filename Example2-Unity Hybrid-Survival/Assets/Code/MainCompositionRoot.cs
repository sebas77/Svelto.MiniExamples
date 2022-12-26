using System;
using Svelto.Context;
using Svelto.DataStructures;
using Svelto.ECS.Example.Survive.Camera;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.Enemies;
using Svelto.ECS.Example.Survive.Player;
using Svelto.ECS.Example.Survive.HUD;
using Svelto.ECS.Example.Survive.OOPLayer;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Schedulers;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

//Note on this example:
//When developing with svelto the user would need to plan a bit what entities need to be used and
//what behaviours these entities will have. Engines can be added over the time, so the key is to have
//a good idea of the entities to work on. All the reasoning should always start from the EntityDescriptors
//Entity Components can be added over the time of course, but it's important to establish what entities
//will come into play.
//The key entities (and relative entity descriptors) for this demo are:
//Player
//PlayerGun
//Enemy
//Camera
//HUD
//Once the first entity descriptors are designed, it becomes simpler to start build these entities and
//write the engines for their behaviours.

//Note: this demo relies heavily on the publisher/consumer pattern. Engines can iterate quickly over entities
//but when they react on entity changes, iterating all the entities can be a waste. Relying too much on the
//publisher/consumer can have some drawbacks, so with Svelto.ECS 3.0 filters can be used instead. This doesn't
//mean that filters are better than the publisher/consumer model. Each tool must be used wisely.

//Main is the Application Composition Root. A Composition Root is the where all the dependencies are 
//created and injected (I talk a lot about this in my articles) A composition root belongs to the Context, but
//a context can have more than a composition root. For example a factory is a composition root.
//Furthermore an application can have more than a context/composition root but this is more advanced and not part of this demo
namespace Svelto.ECS.Example.Survive
{
    /// <summary>
    ///     IComposition root is part of Svelto.Context. Svelto.Context is not formally part of Svelto.ECS, but
    ///     it's helpful to use in an environment where a Context is not present, like in Unity.
    ///     It's a bootstrap! (check MainContext.cs)
    /// </summary>
    public class MainCompositionRoot: ICompositionRoot
    {
        public MainCompositionRoot()
        {
            QualitySettings.vSyncCount = 1;
        }

        public void OnContextCreated<T>(T contextHolder) { }

        public void OnContextInitialized<T>(T contextHolder)
        {
            CompositionRoot(contextHolder as UnityContext);
        }

        public void OnContextDestroyed(bool hasBeenActivated)
        {
            //final clean up
            _enginesRoot?.Dispose();
        }

        /// <summary>
        ///     Before to start, let's review some of the Svelto.ECS terms:
        ///     - Entity:
        ///     it must be a real and concrete entity that you can explain in terms of game design. The name of each
        ///     entity should reflect a specific concept from the game design domain
        ///     - Engines (Systems):
        ///     Where all the logic lies. Engines operates on Entity Components
        ///     - IEntityComponent:
        ///     It's an Entity Component which can be used with Pure ECS
        ///     - IEntityViewComponent:
        ///     structs implementing this are used to wrap Objects that come from OOP libraries. You will never use it unless
        ///     you are forced to mix your ECS code with OOP code because of external libraries or platforms. These special
        ///     "Hybrid" component can hold only interfaces
        ///     - Implementors:
        ///     The EntityComponent exposed interfaces must be implemented by Implementors that are actually the
        ///     Objects you need to wrap.
        ///     - EntityDescriptors:
        ///     Gives a way to formalise your Entity, it also defines the components that must
        ///     be generated once the Entity is built
        /// </summary>
        void CompositionRoot(UnityContext contextHolder)
        {
//the SimpleEntitiesSubmissionScheduler is the scheduler that is used by the EnginesRoot to know
//when to submit the entities. Custom ones can be created for special cases. This is the simplest default and it must
//be ticked explicitly.
            var entitySubmissionScheduler = new SimpleEntitiesSubmissionScheduler();
//The Engines Root is the core of Svelto.ECS. You shouldn't inject the EngineRoot,
//therefore the composition root class must hold a reference or it will be garbage collected.
            _enginesRoot = new EnginesRoot(entitySubmissionScheduler);
//The EntityFactory can be injected inside factories (or engine acting as factories) to build new entities
//dynamically
            var entityFactory = _enginesRoot.GenerateEntityFactory();
//The entity functions is a set of utility operations on Entities, including removing an entity. I couldn't
//find a better name so far.
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();
            var entityStreamConsumerFactory = _enginesRoot.GenerateConsumerFactory();

//wrap non testable unity static classes, so that can be mocked if needed (or implementation can change in general, without changing the interface).
            IRayCaster rayCaster = new RayCaster();
            ITime time = new Time();
//GameObjectFactory allows to create GameObjects without using the Static method GameObject.Instantiate.
//While it seems a complication it's important to keep the engines testable and not coupled with hard
//dependencies
            var gameObjectResourceManager = new GameObjectResourceManager();
//IStepEngines are engine that can be stepped (ticked) manually and explicitly with a Step() method
            var orderedEngines = new FasterList<IStepEngine>();
            var unorderedEngines = new FasterList<IStepEngine>();

//This example has been refactored to show some advanced users of Svelto.ECS in a simple scenario
//to know more about ECS abstraction layers read: https://www.sebaslab.com/ecs-abstraction-layers-and-modules-encapsulation/

//Setup all the layers engines
            OOPLayerContext.Setup(orderedEngines, _enginesRoot, gameObjectResourceManager);
            DamageContextLayer.DamageLayerSetup(entityStreamConsumerFactory, _enginesRoot, orderedEngines);
            CameraLayerContext.Setup(unorderedEngines, _enginesRoot);
            PlayerLayerContext.Setup(
                rayCaster, time, entityFunctions, entityStreamConsumerFactory, unorderedEngines, orderedEngines,
                _enginesRoot);
            EnemyLayerContext.EnemyLayerSetup(
                entityFactory, entityStreamConsumerFactory, time, entityFunctions,
                unorderedEngines, orderedEngines, new WaitForSubmissionEnumerator(entitySubmissionScheduler),
                _enginesRoot, gameObjectResourceManager);
            HudLayerContext.Setup(entityStreamConsumerFactory, unorderedEngines, orderedEngines, _enginesRoot);

//group engines for order of execution. Ordering and Ticking is 100% user responsibility. This is just one of the possible way to achieve the result desired
            orderedEngines.Add(new SurvivalUnsortedEnginesGroup(unorderedEngines));
            orderedEngines.Add(new TickEngine(entitySubmissionScheduler));
            var sortedEnginesGroup = new SortedEnginesGroup(orderedEngines);

//PlayerSpawner is not an engine, it could have been, but since it doesn't have an update, it's better to be a factory
            var playerSpanwer = new PlayerFactory(gameObjectResourceManager, entityFactory);

            BuildGUIEntitiesFromScene(contextHolder, entityFactory);

            StartMainLoop(sortedEnginesGroup, entitySubmissionScheduler, playerSpanwer);

//Attach Svelto Inspector: for more info https://github.com/sebas77/svelto-ecs-inspector-unity
#if DEBUG
            SveltoInspector.Attach(_enginesRoot);
#endif
        }

        //Svelto ECS doesn't provide a ticking system, the user is responsible for it
        async void StartMainLoop(SortedEnginesGroup enginesToTick,
            SimpleEntitiesSubmissionScheduler unityEntitySubmissionScheduler, PlayerFactory playerSpanwer)
        {
            await playerSpanwer.StartSpawningPlayerTask();

            RunSveltoUpdateInTheEarlyUpdate(enginesToTick, unityEntitySubmissionScheduler);
        }

        void BuildGUIEntitiesFromScene(UnityContext contextHolder, IEntityFactory entityFactory)
        {
            /// An EntityDescriptorHolder is a special Svelto.ECS hybrid class dedicated to the unity platform.
            /// Once attached to a gameobject it automatically retrieves implementors from the hierarchy.
            /// This pattern is usually useful for guis where complex hierarchy of gameobjects are necessary, but
            /// otherwise you should always create entities in factories. 
            /// The gui of this project is ultra simple and is all managed by one entity only. This way won't do
            /// for a complex GUI.
            /// Note that creating an entity to manage a complex gui like this, is OK only for such a simple scenario
            /// otherwise a widget-like design should be adopted.
            ///
            /// UPDATE: NOTE -> SveltoGUIHelper is now deprecated. Managing GUIs with Entities is not recommended
            /// it's best to use a proper GUI framework and sync models with entity components in sync engines
            /// Building from EntityDescriptorHolders is also sort of unnecessary too (as in there could be better
            /// ways to achieve the same result)
            SveltoGUIHelper.Create<HUDEntityDescriptorHolder>(
                ECSGroups.HUD, contextHolder.transform, entityFactory, true);
        }

        void RunSveltoUpdateInTheEarlyUpdate(SortedEnginesGroup enginesToTick,
            SimpleEntitiesSubmissionScheduler unityEntitySubmissionScheduler)
        {
            PlayerLoopSystem defaultLoop = PlayerLoop.GetDefaultPlayerLoop();

            // Find the position of the early update in the default loop
            int earlyUpdateIndex = -1;
            for (int i = 0; i < defaultLoop.subSystemList.Length; i++)
            {
                if (defaultLoop.subSystemList[i].type == typeof(EarlyUpdate))
                {
                    earlyUpdateIndex = i + 3;
                    break;
                }
            }

            // Insert a custom update before the early update
            if (earlyUpdateIndex >= 0)
            {
                PlayerLoopSystem[] newSubSystemList = new PlayerLoopSystem[defaultLoop.subSystemList.Length + 1];
                Array.Copy(defaultLoop.subSystemList, newSubSystemList, earlyUpdateIndex);
                newSubSystemList[earlyUpdateIndex] = new PlayerLoopSystem
                {
                        type = typeof(MainCompositionRoot),
                        updateDelegate = Update
                };
                Array.Copy(
                    defaultLoop.subSystemList, earlyUpdateIndex, newSubSystemList, earlyUpdateIndex + 1,
                    defaultLoop.subSystemList.Length - earlyUpdateIndex);
                defaultLoop.subSystemList = newSubSystemList;
            }

            // Set the modified player loop
            PlayerLoop.SetPlayerLoop(defaultLoop);

            void Update()
            {
                if (_enginesRoot.IsValid())
                {
                    enginesToTick.Step();
                }
            }
        }

        EnginesRoot _enginesRoot;
    }
}