#if PROFILE_SVELTO
#warning the global define PROFILE_SVELTO must be used only when it's necessary to start a profiling session to reduce the overhead of debugging code. Normally remove this define to get insights when errors happen
#endif
using Svelto.Context;
using Svelto.DataStructures;
using Svelto.ECS.MiniExamples.Doofuses.GameObjects.GameobjectLayer;
using Svelto.ECS.Miniexamples.Doofuses.GameObjectsLayer;
using Svelto.ECS.Schedulers;
using Svelto.ECS.SveltoOnDOTS;
using UnityEngine;

#if !PROFILE_SVELTO
#warning for maximum performance you need to enable PROFILE_SVELTO (this is not needed for a release client)
#endif
namespace Svelto.ECS.Miniexamples.Doofuses.Gameobjects
{
    public class SveltoCompositionRoot : ICompositionRoot
    {
        public void OnContextCreated<T>(T contextHolder)
        {
            QualitySettings.vSyncCount = -1;
            Cursor.lockState           = CursorLockMode.Locked;
            Cursor.visible             = false;

            _simpleSubmitScheduler = new EntitiesSubmissionScheduler();
            _enginesRoot           = new EnginesRoot(_simpleSubmitScheduler);
        }

        public void OnContextInitialized<T>(T contextHolder)
        {
             ComposeEnginesRoot();
             
             _mainLoop = new MainLoop(_enginesToTick, _simpleSubmitScheduler);

            _mainLoop.Run();
        }

        public void OnContextDestroyed(bool hasBeenInitialised) 
        {
            _ECSGameObjectsEntityManager.Dispose();
            _enginesRoot.Dispose();
            _mainLoop.Dispose();
            _simpleSubmitScheduler.Dispose();
        }

        void ComposeEnginesRoot()
        {
            var entityFactory   = _enginesRoot.GenerateEntityFactory();
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();
            
            _ECSGameObjectsEntityManager = new ECSGameObjectsEntityManager();
            
            GameObjectToSveltoCompositionRoot.Compose(_enginesRoot, _enginesToTick, _ECSGameObjectsEntityManager);
            
            LoadAssetAndCreatePrefabs(_ECSGameObjectsEntityManager, out var redFoodPrefab, out var blueFootPrefab
                                    , out var redDoofusPrefab, out var blueDoofusPrefab);

            //Compose the game level engines
            AddSveltoEngineToTick(new PlaceFoodOnClickEngine((int)redFoodPrefab, (int)blueFootPrefab, entityFactory));
            AddSveltoEngineToTick(new SpawningDoofusEngine((int)redDoofusPrefab, (int)blueDoofusPrefab, entityFactory));
            AddSveltoEngineToTick(new ConsumingFoodEngine(entityFunctions));
            AddSveltoEngineToTick(new LookingForFoodDoofusesEngine(entityFunctions));
            AddSveltoEngineToTick(new VelocityToPositionDoofusesEngine());
        }

        static void LoadAssetAndCreatePrefabs
        (ECSGameObjectsEntityManager gom, out uint redFoodPrefab, out uint blueFootPrefab, out uint redDoofusPrefab
       , out uint blueDoofusPrefab)
        {
            //Register the loaded prefabs in the GameObject layer. The returning id is the ECS id to the prefab
            redFoodPrefab    = gom.LoadAndRegisterPrefab("Sphere");
            blueFootPrefab   = gom.LoadAndRegisterPrefab("Sphereblue");
            redDoofusPrefab  = gom.LoadAndRegisterPrefab("RedCapsule");
            blueDoofusPrefab = gom.LoadAndRegisterPrefab("BlueCapsule");
        }

        void AddSveltoEngineToTick(IJobifiedEngine engine)
        {
            _enginesRoot.AddEngine(engine);
            _enginesToTick.Add(engine);
        }

        //the topmost composition root has the responsibility to hold all the main structures
        EnginesRoot                          _enginesRoot;
        EntitiesSubmissionScheduler    _simpleSubmitScheduler;
        MainLoop                             _mainLoop;
        ECSGameObjectsEntityManager                    _ECSGameObjectsEntityManager;
        
        readonly FasterList<IJobifiedEngine> _enginesToTick = new FasterList<IJobifiedEngine>();
    }
}