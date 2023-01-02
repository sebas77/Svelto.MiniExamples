using System.Threading.Tasks;
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
        }

        public void OnContextDestroyed(bool isInitialized)
        {
            try
            {
                _sveltoOverDotsEnginesGroupEnginesGroup.Dispose();
                _enginesRoot.Dispose();
                _mainLoop.Dispose();
                _simpleSubmitScheduler.Dispose();
            }
            catch
            {
                
            }
        }

        async Task ComposeEnginesRoot()
        {
            while (PrefabsHolders.Ready == false)
                await Task.Yield();
            
            var entityFactory   = _enginesRoot.GenerateEntityFactory();
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();

            var defaultWorld = World.DefaultGameObjectInjectionWorld;

            _sveltoOverDotsEnginesGroupEnginesGroup = new SveltoOnDOTSEnginesGroup(_enginesRoot);
            _enginesToTick.Add(_sveltoOverDotsEnginesGroupEnginesGroup);
            
            _sveltoOverDotsEnginesGroupEnginesGroup.world.EntityManager.CopyAndReplaceEntitiesFrom(defaultWorld.EntityManager);
            
            defaultWorld.Dispose();
//
//            LoadAssetAndCreatePrefabs(
//                out var redFoodPrefab
//              , out var blueFootPrefab, out var redDoofusPrefab, out var blueDoofusPrefab, out var specialBlueDoofusPrefab);
//
//            AddSveltoEngineToTick(new SpawnFoodOnClickEngine(redFoodPrefab, blueFootPrefab, entityFactory));
//            AddSveltoEngineToTick(new SpawningDoofusEngine(redDoofusPrefab, blueDoofusPrefab, specialBlueDoofusPrefab, entityFactory));
//            AddSveltoEngineToTick(new ConsumingFoodEngine(entityFunctions));
//            AddSveltoEngineToTick(new LookingForFoodDoofusesEngine(entityFunctions));
//            AddSveltoEngineToTick(new VelocityToPositionDoofusesEngine());
//
//            _sveltoOverDotsEnginesGroupEnginesGroup.AddDOTSSubmissionEngine(new SpawnUnityEntityOnSveltoEntityEngine());
//            _sveltoOverDotsEnginesGroupEnginesGroup.AddDOTSSubmissionEngine(new SetFiltersOnBlueDoofusesSpawnedEngine());
//            _sveltoOverDotsEnginesGroupEnginesGroup.AddSveltoToDOTSEngine(new RenderingDOTSDataSynchronizationEngine());
//            
//            _mainLoop = new MainLoop(_enginesToTick);
//            _mainLoop.Run();
        }
        
//        static void LoadAssetAndCreatePrefabs(out Entity redFoodPrefab, out Entity blueFootPrefab,
//            out Entity redDoofusPrefab, out Entity blueDoofusPrefab, out Entity specialBlueDoofusPrefab)
//        {
//            redFoodPrefab = PrefabsHolders.GetEntityS(4);
//            blueFootPrefab = PrefabsHolders.GetEntityS(3);
//            redDoofusPrefab = PrefabsHolders.GetEntityS(1);
//            blueDoofusPrefab = PrefabsHolders.GetEntityS(0);
//            specialBlueDoofusPrefab = PrefabsHolders.GetEntityS(2);
//        }

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