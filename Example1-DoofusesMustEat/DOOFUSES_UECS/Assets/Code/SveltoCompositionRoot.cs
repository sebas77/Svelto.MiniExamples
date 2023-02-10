//#error I am in the process to update this demo to DOTS 1.0, It will take some time. If you are interested in checking an older version, please check the GIT history anything from a0495c7af07ef7248c06d6f326e4f75d2578df06 and before will work with DOTS 0.51. This demo won't work as expected at the moment.

#if !UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP_RUNTIME_WORLD
#error this demo takes completely over the DOTS initialization and ticking. UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP must be enabled
#endif

using System.Threading.Tasks;
using Svelto.Context;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Rendering;
using Unity.Scenes;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace Svelto.ECS.MiniExamples.DoofusesDOTS
{
    public class SveltoCompositionRoot: ICompositionRoot
    {
        public void OnContextCreated<T>(T contextHolder)
        {
            QualitySettings.vSyncCount = -1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _simpleSubmitScheduler = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(_simpleSubmitScheduler);
        }

        public void OnContextInitialized<T>(T contextHolder)
        {
#pragma warning disable CS4014
            ComposeEnginesRoot();
#pragma warning restore CS4014
        }

        public void OnContextDestroyed(bool isInitialized)
        {
            _sveltoOverDotsEnginesGroupEnginesGroup.Dispose();
            _enginesRoot.Dispose();
            _mainLoop?.Dispose();
            _simpleSubmitScheduler.Dispose();
        }

        async Task ComposeEnginesRoot()
        {
            var entityFactory = _enginesRoot.GenerateEntityFactory();
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();

            //SveltoOnDOTSEnginesGroup is the DOTS Wrapper to facilitate Svelto <-> DOTS integration
            _sveltoOverDotsEnginesGroupEnginesGroup = new SveltoOnDOTSEnginesGroup(_enginesRoot);
            _enginesToTick.Add(_sveltoOverDotsEnginesGroupEnginesGroup);

            var (redFoodPrefab, blueFoodPrefab, redDoofusPrefab, blueDoofusPrefab, specialBlueDoofusPrefab) =
                    await LoadAssetAndCreatePrefabs(_sveltoOverDotsEnginesGroupEnginesGroup.world);

            EntityManager worldEntityManager = _sveltoOverDotsEnginesGroupEnginesGroup.world.EntityManager;
            worldEntityManager.AddComponent<SpecialBluePrefab>(specialBlueDoofusPrefab);

            //Standard Svelto Engines
            AddSveltoEngineToTick(new SpawningDoofusEngine(redDoofusPrefab, blueDoofusPrefab, specialBlueDoofusPrefab, entityFactory));
            AddSveltoEngineToTick(new SpawnFoodOnClickEngine(redFoodPrefab, blueFoodPrefab, entityFactory));
            AddSveltoEngineToTick(new ConsumingFoodEngine(entityFunctions));
            AddSveltoEngineToTick(new LookingForFoodDoofusesEngine(entityFunctions));
            AddSveltoEngineToTick(new VelocityToPositionDoofusesEngine());

            //Svelto engines that runs 
            _sveltoOverDotsEnginesGroupEnginesGroup.AddDOTSSubmissionEngine(new SpawnUnityEntityOnSveltoEntityEngine());
            _sveltoOverDotsEnginesGroupEnginesGroup.AddDOTSSubmissionEngine(new SetFiltersOnBlueDoofusesSpawnedEngine());
            _sveltoOverDotsEnginesGroupEnginesGroup.AddSveltoToDOTSEngine(new RenderingDOTSDataSynchronizationEngine());

            _mainLoop = new MainLoop(_enginesToTick);
            _mainLoop.Run();
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

            return (component.RedFood, component.BlueFood, component.RedDoofus, component.BlueDoofus, component.SpecialDoofus);
        }

        void AddSveltoEngineToTick(IJobifiedEngine engine)
        {
            _enginesRoot.AddEngine(engine);
            _enginesToTick.Add(engine);
        }

        EnginesRoot _enginesRoot;
        readonly FasterList<IJobifiedEngine> _enginesToTick = new FasterList<IJobifiedEngine>();
        SimpleEntitiesSubmissionScheduler _simpleSubmitScheduler;
        SveltoOnDOTSEnginesGroup _sveltoOverDotsEnginesGroupEnginesGroup;
        MainLoop _mainLoop;
    }
}