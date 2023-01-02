//#error I am in the process to update this demo to DOTS 1.0, It will take some time. If you are interested in checking an older version, please check the GIT history anything from a0495c7af07ef7248c06d6f326e4f75d2578df06 and before will work with DOTS 0.51. This demo won't work as expected at the moment.

#if !UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
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
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hash128 = Unity.Entities.Hash128;

namespace Svelto.ECS.MiniExamples.Example1C
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
            ComposeEnginesRoot();
        }

        public void OnContextDestroyed(bool isInitialized)
        {
            _sveltoOverDotsEnginesGroupEnginesGroup.Dispose();
            _enginesRoot.Dispose();
            _mainLoop.Dispose();
            _simpleSubmitScheduler.Dispose();
        }

        async Task ComposeEnginesRoot()
        {
            var entityFactory = _enginesRoot.GenerateEntityFactory();
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();

            _sveltoOverDotsEnginesGroupEnginesGroup = new SveltoOnDOTSEnginesGroup(_enginesRoot);
            _enginesToTick.Add(_sveltoOverDotsEnginesGroupEnginesGroup);

            var customWorld = _sveltoOverDotsEnginesGroupEnginesGroup.world;
            customWorld.Update();
            var sceneGuid = SceneSystem.GetSceneGUID(
                ref customWorld.Unmanaged.GetExistingSystemState<SceneSystem>(),
                "Assets/Scene/CombiningBehaviours/Prefabs.unity");
            var sceneGuid2 = new Hash128("52b77c03e0aae864286b8f57b3216c18");
            var sceneEntity = SceneSystem.LoadSceneAsync(
                customWorld.Unmanaged,
                new EntitySceneReference(
                    sceneGuid2, 0));

            while (SceneSystem.IsSceneLoaded(customWorld.Unmanaged, sceneEntity)
                == false)
            {
                customWorld.Update();
                await Task.Yield();
            }

            LoadAssetAndCreatePrefabs(
                out var redFoodPrefab
              , out var blueFootPrefab, out var redDoofusPrefab, out var blueDoofusPrefab,
                out var specialBlueDoofusPrefab, customWorld.EntityManager);

            AddSveltoEngineToTick(
                new SpawningDoofusEngine(redDoofusPrefab, blueDoofusPrefab, specialBlueDoofusPrefab, entityFactory));
            AddSveltoEngineToTick(new SpawnFoodOnClickEngine(redFoodPrefab, blueFootPrefab, entityFactory));
            AddSveltoEngineToTick(new ConsumingFoodEngine(entityFunctions));
            AddSveltoEngineToTick(new LookingForFoodDoofusesEngine(entityFunctions));
            AddSveltoEngineToTick(new VelocityToPositionDoofusesEngine());

            _sveltoOverDotsEnginesGroupEnginesGroup.AddDOTSSubmissionEngine(new SpawnUnityEntityOnSveltoEntityEngine());
            _sveltoOverDotsEnginesGroupEnginesGroup.AddDOTSSubmissionEngine(
                new SetFiltersOnBlueDoofusesSpawnedEngine());
            _sveltoOverDotsEnginesGroupEnginesGroup.AddSveltoToDOTSEngine(new RenderingDOTSDataSynchronizationEngine());

            _mainLoop = new MainLoop(_enginesToTick);
            _mainLoop.Run();
        }

        static void LoadAssetAndCreatePrefabs(out Entity redFoodPrefab, out Entity blueFoodPrefab,
            out Entity redDoofusPrefab, out Entity blueDoofusPrefab, out Entity specialBlueDoofusPrefab,
            EntityManager worldEntityManager)
        {
            var query = worldEntityManager.CreateEntityQuery(typeof(PrefabsHolder.PrefabsComponents));

            var prefabHolder = query.ToComponentDataArray<PrefabsHolder.PrefabsComponents>(Allocator.Temp);

            var component = prefabHolder[0];

            redFoodPrefab = component.RedFood;
            blueFoodPrefab = component.BlueFood;
            redDoofusPrefab = component.RedDoofus;
            blueDoofusPrefab = component.BlueDoofus;
            specialBlueDoofusPrefab = component.SpecialDoofus;
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