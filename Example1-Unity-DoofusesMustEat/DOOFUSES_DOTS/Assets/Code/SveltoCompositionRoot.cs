using System.Threading.Tasks;
using Svelto.Context;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Scenes;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace Svelto.ECS.MiniExamples.DoofusesDOTS
{
    /// <summary>
    /// This is a standard Svelto Composition Root, the DOTS bit will come later
    /// </summary>
    public class SveltoCompositionRoot: ICompositionRoot
    {
        public void OnContextCreated<T>(T contextHolder)
        {
            QualitySettings.vSyncCount = -1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            //create the standard sheduler
            _simpleSubmitScheduler = new SimpleEntitiesSubmissionScheduler();
            //create the standard engines root
            _enginesRoot = new EnginesRoot(_simpleSubmitScheduler);
        }

        public void OnContextInitialized<T>(T contextHolder)
        {
            JobsUtility.JobDebuggerEnabled = true;
            
#pragma warning disable CS4014
            ComposeEnginesRoot();
#pragma warning restore CS4014
        }

        public void OnContextDestroyed(bool isInitialized)
        {
            _sveltoOnDotsEnginesGroup.Dispose();
            _enginesRoot.Dispose();
            _mainLoop?.Dispose();
            _simpleSubmitScheduler.Dispose();
        }

        async Task ComposeEnginesRoot()
        {
            var entityFactory = _enginesRoot.GenerateEntityFactory();
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();

            //The core of SveltoOnDOTS. SveltoOnDOTSEnginesGroup is the DOTS Wrapper to facilitate Svelto <-> DOTS integration
            //fundamentally is a Svelto engine container about all the SveltoOnDOTS engines. these engines are:
            //1) ISveltoOnDOTSStructuralEngine: these engines are used to create DOTS entities over Svelto entities
            //2) SyncSveltoToDOTSEngine: these engines DOTS SystemBase engines and are used to sync Svelto data to DOTS data
            //3) SyncDOTSToSveltoEngine: these engines DOTS SystemBase engines and are used to sync DOTS data to Svelto data
            //4) pure DOTS SystemBase/ISystem engines: these engines are used to perform DOTS only logic
            
            _sveltoOnDotsEnginesGroup = new SveltoOnDOTSEnginesGroup(_enginesRoot);
            _enginesToTick.Add(_sveltoOnDotsEnginesGroup);

            var (redFoodPrefab, blueFoodPrefab, redDoofusPrefab, blueDoofusPrefab, specialBlueDoofusPrefab) =
                    await LoadAssetAndCreatePrefabs(_sveltoOnDotsEnginesGroup.world);

            //Standard Svelto Engines
            AddSveltoEngineToTick(new ConsumingFoodEngine(entityFunctions));
            AddSveltoEngineToTick(new LookingForFoodDoofusesEngine(entityFunctions));
            AddSveltoEngineToTick(new VelocityToPositionDoofusesEngine());
            
            //SveltoOnDots structural engines these are added through
            //sveltoOnDotsEnginesGroupEnginesGroup.AddSveltoOnDOTSSubmissionEngine(structuralEngine);
            AddSveltoEngineToTick(new SpawningDoofusEngine(redDoofusPrefab, blueDoofusPrefab, specialBlueDoofusPrefab, entityFactory));
            AddSveltoEngineToTick(new SpawnFoodOnClickEngine(redFoodPrefab, blueFoodPrefab, entityFactory));

            //SveltoOnDOTS sync engines these are added through
            //sveltoOnDotsEnginesGroupEnginesGroup.AddSveltoToDOTSSyncEngine(syncEngine); or
            //sveltoOnDotsEnginesGroupEnginesGroup.AddDOTSToSveltoSyncEngine(syncEngine);
            //depending on the direction of the sync
            _sveltoOnDotsEnginesGroup.AddSveltoToDOTSSyncEngine(new RenderingDOTSPositionSyncEngine());
            
            //being _sveltoOnDotsEnginesGroup an engine group, it will tick all the engines added to it (structural and sync)
            //it will also run the DOTS ECS systems linked to the DOTS World created by the SveltoOnDOTSEnginesGroup
            //it is expected to be ticked too, so normally it's found inside a SortedEnginesGroup
            //A standard SveltoOOnDOTS frame runs like
            // Svelto (GameLogic) Engines Run first (thanks to the SortedEnginesGroup, it's basically user choice)
            // Then this Engine is ticked, causing:
            // All the Jobs to be completed (it's a sync point)
            // Synchronizations engines to be executed (Svelto to DOTS ECS)
            // Submission of Entities to be executed
            // Svelto Add/Remove callbacks to be called
            // ISveltoOnDOTSStructuralEngine to be executed
            /// DOTS ECS engines to executed
            /// Synchronizations engines to be executed (DOTS ECS To Svelto)
            _mainLoop = new MainLoop(_enginesToTick);
            _mainLoop.Run();
        }
        
        struct Component<T>
        {
            public int Id;
        }

        static async Task<(Entity, Entity, Entity, Entity, Entity)> LoadAssetAndCreatePrefabs(World customWorld)
        {
            var customWorldUnmanaged = customWorld.Unmanaged;
            var sceneGuid = new Hash128("52b77c03e0aae864286b8f57b3216c18");
            var sceneEntity = SceneSystem.LoadSceneAsync(customWorldUnmanaged, new EntitySceneReference(sceneGuid, 0));

            while (SceneSystem.IsSceneLoaded(customWorldUnmanaged, sceneEntity) == false)
            {
                customWorld.Update();
                await Task.Yield();
            }

            EntityManager worldEntityManager = customWorld.EntityManager;
            var query = worldEntityManager.CreateEntityQuery(typeof(PrefabsComponents));
            var prefabHolder = query.ToComponentDataArray<PrefabsComponents>(Allocator.Temp);

            var component = prefabHolder[0];
            //add the DOTS special blue IEnableable component so that we can filter them later for the sync of the positions
            worldEntityManager.AddComponent<SpecialBluePrefab>(component.SpecialDoofus);

            return (component.RedFood, component.BlueFood, component.RedDoofus, component.BlueDoofus, component.SpecialDoofus);
        }

        void AddSveltoEngineToTick(IJobifiedEngine engine)
        {
            _enginesToTick.Add(engine);
            
            if (engine is ISveltoOnDOTSStructuralEngine structuralEngine)
                _sveltoOnDotsEnginesGroup.AddSveltoOnDOTSSubmissionEngine(structuralEngine);
            else
                _enginesRoot.AddEngine(engine);
        }
        
        EnginesRoot _enginesRoot;
        readonly FasterList<IJobifiedEngine> _enginesToTick = new FasterList<IJobifiedEngine>();
        SimpleEntitiesSubmissionScheduler _simpleSubmitScheduler;
        SveltoOnDOTSEnginesGroup _sveltoOnDotsEnginesGroup;
        MainLoop _mainLoop;
    }
}
