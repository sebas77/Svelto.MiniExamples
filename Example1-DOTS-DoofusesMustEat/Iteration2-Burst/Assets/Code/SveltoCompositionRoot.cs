using System;
using Svelto.Context;
using Svelto.ECS.Schedulers.Unity;
using Svelto.Tasks;
using Unity.Entities;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1B
{
    public class SveltoCompositionRoot : ICompositionRoot, ICustomBootstrap
    {
        static World _world;
        static RenderingDataSynchronizationEngine _renderingDataSynchronizationEngine;

        EnginesRoot _enginesRoot;

        public void OnContextInitialized<T>(T contextHolder)
        {
            QualitySettings.vSyncCount = -1;
            
            _enginesRoot = new EnginesRoot(new UnityEntitySubmissionScheduler());

            //add the engines we are going to use
            var generateEntityFactory = _enginesRoot.GenerateEntityFactory();

            var foodEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(Resources.Load("Sphere") as GameObject, _world);
            _world.EntityManager.AddComponent<UnityECSFoodGroup>(foodEntity);
            _enginesRoot.AddEngine(new PlaceFoodOnClickEngine(foodEntity, generateEntityFactory));
            var doofusEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(Resources.Load("Capsule") as GameObject, _world);
            _world.EntityManager.AddComponent<UnityECSDoofusesGroup>(doofusEntity);
            _enginesRoot.AddEngine(new SpawningDoofusEngine(doofusEntity, generateEntityFactory));
            
            _enginesRoot.AddEngine(new LookingForFoodDoofusesEngine());
            _enginesRoot.AddEngine(new SpawnUnityEntityOnSveltoEntityEngine(_world));
            _enginesRoot.AddEngine(new VelocityToPositionDoofusesEngine());
            _enginesRoot.AddEngine(new ConsumingFoodEngine(_enginesRoot.GenerateEntityFunctions()));            
      //      _enginesRoot.AddEngine(new DieOfHungerDoofusesEngine(_enginesRoot.GenerateEntityFunctions()));
            
            //one engine two ECS implementations :P
            _enginesRoot.AddEngine(_renderingDataSynchronizationEngine);
        }

        public void OnContextDestroyed()
        {
            DoofusesStandardSchedulers.StopAndCleanupAllDefaultSchedulers();
            TaskRunner.Stop();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            _enginesRoot.Dispose();
        }

        public void OnContextCreated<T>(T contextHolder)
        {}

        /// <summary>
        /// A bit messy, it's not from UnityECS 0.2.0 and I will need to study it better
        /// </summary>
        /// <param name="defaultWorldName"></param>
        /// <returns></returns>
        public bool Initialize(string defaultWorldName)
        {
//            Physics.autoSimulation = false;
            _world = new World("Custom world");
            
            _renderingDataSynchronizationEngine = new RenderingDataSynchronizationEngine();
            World.DefaultGameObjectInjectionWorld = _world;
            var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(_world, systems);
            _world.AddSystem(_renderingDataSynchronizationEngine);
            var simulationSystemGroup = _world.GetExistingSystem<SimulationSystemGroup>();
            simulationSystemGroup.AddSystemToUpdateList(_renderingDataSynchronizationEngine);
            simulationSystemGroup.SortSystemUpdateList();
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(_world);
            
            return true;
        }
    }
}

