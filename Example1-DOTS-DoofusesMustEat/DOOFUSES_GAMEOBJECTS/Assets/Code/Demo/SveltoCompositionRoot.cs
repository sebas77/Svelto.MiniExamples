#if PROFILE_SVELTO
#warning the global define PROFILE_SVELTO must be used only when it's necessary to start a profiling session to reduce the overhead of debugging code. Normally remove this define to get insights when errors happen
#endif
using Svelto.Context;
using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.MiniExamples.GameObjectsLayer;
using Svelto.ECS.Schedulers;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public class SveltoCompositionRoot : ICompositionRoot
    {
        public void OnContextCreated<T>(T contextHolder)
        {
            QualitySettings.vSyncCount = -1;
            Cursor.lockState           = CursorLockMode.Locked;
            Cursor.visible             = false;

            _simpleSubmitScheduler = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot           = new EnginesRoot(_simpleSubmitScheduler);

            _mainLoop = new MainLoop(_enginesToTick, _simpleSubmitScheduler);
        }

        public void OnContextInitialized<T>(T contextHolder)
        {
             ComposeEnginesRoot();

            _mainLoop.Run();
        }

        public void OnContextDestroyed()
        {
            _enginesRoot.Dispose();
            _mainLoop.Dispose();
            _simpleSubmitScheduler.Dispose();
        }

        void ComposeEnginesRoot()
        {
            var entityFactory   = _enginesRoot.GenerateEntityFactory();
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();
            
            GameObjectToSveltoCompositionRoot.Compose(_enginesRoot, _enginesToTick, out _gameObjectManager);
            
            LoadAssetAndCreatePrefabs(_gameObjectManager, out var redFoodPrefab, out var blueFootPrefab
                                    , out var redDoofusPrefab, out var blueDoofusPrefab);

            //Compose the game level engines
            AddSveltoEngineToTick(new PlaceFoodOnClickEngine(redFoodPrefab, blueFootPrefab, entityFactory));
            AddSveltoEngineToTick(new SpawningDoofusEngine(redDoofusPrefab, blueDoofusPrefab, entityFactory));
            AddSveltoEngineToTick(new ConsumingFoodEngine(entityFunctions));
            AddSveltoEngineToTick(new LookingForFoodDoofusesEngine(entityFunctions));
            AddSveltoEngineToTick(new VelocityToPositionDoofusesEngine());
        }

        static void LoadAssetAndCreatePrefabs
        (GameObjectManager gom, out int redFoodPrefab, out int blueFootPrefab, out int redDoofusPrefab
       , out int blueDoofusPrefab)
        {
            //Register the loaded prefabs in the GameObject layer. The returning id is the ECS id to the prefab
            redFoodPrefab    = gom.RegisterPrefab(Resources.Load("Sphere") as GameObject);
            blueFootPrefab   = gom.RegisterPrefab(Resources.Load("Sphereblue") as GameObject);
            redDoofusPrefab  = gom.RegisterPrefab(Resources.Load("RedCapsule") as GameObject);
            blueDoofusPrefab = gom.RegisterPrefab(Resources.Load("BlueCapsule") as GameObject);
        }

        void AddSveltoEngineToTick(IJobifiedEngine engine)
        {
            _enginesRoot.AddEngine(engine);
            _enginesToTick.Add(engine);
        }

        //the topmost composition root has the responsibility to hold all the main structures
        EnginesRoot                          _enginesRoot;
        SimpleEntitiesSubmissionScheduler    _simpleSubmitScheduler;
        MainLoop                             _mainLoop;
        GameObjectManager                    _gameObjectManager;
        
        readonly FasterList<IJobifiedEngine> _enginesToTick = new FasterList<IJobifiedEngine>();
    }
}