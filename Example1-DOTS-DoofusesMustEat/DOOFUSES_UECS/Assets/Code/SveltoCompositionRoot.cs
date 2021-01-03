#if PROFILE_SVELTO
#warning the global define PROFILE_SVELTO must be used only when it's necessary to start a profiling session to reduce the overhead of debugging code. Normally remove this define to get insights when errors happen
#endif
#if !UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
#error this demo takes completely over the UECS initialization and ticking. UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP must be enabled
#endif
using Svelto.Context;
using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Schedulers;
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

            _simpleSubmitScheduler                  = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot                            = new EnginesRoot(_simpleSubmitScheduler);
            _mainLoop = new MainLoop(_enginesToTick);
        }

        public void OnContextInitialized<T>(T contextHolder)
        {
            ComposeEnginesRoot();

            _mainLoop.Run();
        }

        public void OnContextDestroyed()
        {
            _sveltoOverUecsEnginesGroupEnginesGroup.Dispose();
            _enginesRoot.Dispose();
            _mainLoop.Dispose();
            _simpleSubmitScheduler.Dispose();
        }

        void ComposeEnginesRoot()
        {
            var entityFactory   = _enginesRoot.GenerateEntityFactory();
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();
            
            _sveltoOverUecsEnginesGroupEnginesGroup = new SveltoOverUECSEnginesGroup(_enginesRoot);
            _enginesToTick.Add(_sveltoOverUecsEnginesGroupEnginesGroup);
            
            LoadAssetAndCreatePrefabs(_sveltoOverUecsEnginesGroupEnginesGroup.world, out var redFoodPrefab
                                    , out var blueFootPrefab, out var redDoofusPrefab, out var blueDoofusPrefab);

            AddSveltoEngineToTick(new PlaceFoodOnClickEngine(redFoodPrefab, blueFootPrefab, entityFactory));
            AddSveltoEngineToTick(new SpawningDoofusEngine(redDoofusPrefab, blueDoofusPrefab, entityFactory));
            AddSveltoEngineToTick(new ConsumingFoodEngine(entityFunctions));
            AddSveltoEngineToTick(new LookingForFoodDoofusesEngine(entityFunctions));
            AddSveltoEngineToTick(new VelocityToPositionDoofusesEngine());

            _sveltoOverUecsEnginesGroupEnginesGroup.AddUECSSubmissionEngine(new SpawnUnityEntityOnSveltoEntityEngine());
            _sveltoOverUecsEnginesGroupEnginesGroup.AddSveltoToUECSEngine(new RenderingUECSDataSynchronizationEngine());
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
        SveltoOverUECSEnginesGroup           _sveltoOverUecsEnginesGroupEnginesGroup;
        MainLoop                             _mainLoop;
    }
}