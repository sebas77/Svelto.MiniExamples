#if !UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
#error this demo takes completely over the DOTS initialization and ticking. UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP must be enabled
#endif
using Svelto.Context;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Entities;
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
        }

        public void OnContextInitialized<T>(T contextHolder)
        {
            ComposeEnginesRoot();

            _mainLoop = new MainLoop(_enginesToTick);
            _mainLoop.Run();
        }

        public void OnContextDestroyed(bool isInitialized)
        {
            _sveltoOverDotsEnginesGroupEnginesGroup.Dispose();
            _enginesRoot.Dispose();
            _mainLoop.Dispose();
            _simpleSubmitScheduler.Dispose();
        }

        void ComposeEnginesRoot()
        {
            var entityFactory   = _enginesRoot.GenerateEntityFactory();
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();

            _sveltoOverDotsEnginesGroupEnginesGroup = new SveltoOnDOTSEnginesGroup(_enginesRoot);
            _enginesToTick.Add(_sveltoOverDotsEnginesGroupEnginesGroup);

            LoadAssetAndCreatePrefabs(_sveltoOverDotsEnginesGroupEnginesGroup.world, out var redFoodPrefab
              , out var blueFootPrefab, out var redDoofusPrefab, out var blueDoofusPrefab);

            AddSveltoEngineToTick(new PlaceFoodOnClickEngine(redFoodPrefab, blueFootPrefab, entityFactory));
            AddSveltoEngineToTick(new SpawningDoofusEngine(redDoofusPrefab, blueDoofusPrefab, entityFactory));
            AddSveltoEngineToTick(new ConsumingFoodEngine(entityFunctions));
            AddSveltoEngineToTick(new LookingForFoodDoofusesEngine(entityFunctions));
            AddSveltoEngineToTick(new VelocityToPositionDoofusesEngine());

            _sveltoOverDotsEnginesGroupEnginesGroup.AddDOTSSubmissionEngine(new SpawnUnityEntityOnSveltoEntityEngine());
            _sveltoOverDotsEnginesGroupEnginesGroup.AddDOTSHandleLifetimeEngine(
                new HandleSpawnedEntityLifeTimeEngine());
            _sveltoOverDotsEnginesGroupEnginesGroup.AddSveltoToDOTSEngine(new RenderingDOTSDataSynchronizationEngine());
        }

        static void LoadAssetAndCreatePrefabs
        (World world, out Entity redFoodPrefab, out Entity blueFootPrefab, out Entity redDoofusPrefab
          , out Entity blueDoofusPrefab)
        {
            //I believe the proper way to do this now is to create a subscene, but I am not sure how it would
            //work with prefabs, so I am not testing it (yet)
            redFoodPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                Resources.Load("Sphere") as GameObject, new GameObjectConversionSettings
                {
                    DestinationWorld = world
                });
            blueFootPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                Resources.Load("Sphereblue") as GameObject, new GameObjectConversionSettings
                {
                    DestinationWorld = world
                });
            redDoofusPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                Resources.Load("RedCapsule") as GameObject, new GameObjectConversionSettings
                {
                    DestinationWorld = world
                });
            blueDoofusPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                Resources.Load("BlueCapsule") as GameObject, new GameObjectConversionSettings
                {
                    DestinationWorld = world
                });
        }

        void AddSveltoEngineToTick(IJobifiedEngine engine)
        {
            _enginesRoot.AddEngine(engine);
            _enginesToTick.Add(engine);
        }

        EnginesRoot                          _enginesRoot;
        readonly FasterList<IJobifiedEngine> _enginesToTick = new FasterList<IJobifiedEngine>();
        SimpleEntitiesSubmissionScheduler    _simpleSubmitScheduler;
        SveltoOnDOTSEnginesGroup             _sveltoOverDotsEnginesGroupEnginesGroup;
        MainLoop                             _mainLoop;
    }
}