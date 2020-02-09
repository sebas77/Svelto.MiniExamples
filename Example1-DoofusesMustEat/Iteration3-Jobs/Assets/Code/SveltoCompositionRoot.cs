using System;
using Svelto.Context;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Schedulers.Unity;
using Svelto.Tasks;
using Unity.Entities;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1B
{
    public class SveltoCompositionRoot : ICompositionRoot, ICustomBootstrap
    {
        static World _world;

        EnginesRoot _enginesRoot;

        public void OnContextInitialized<T>(T contextHolder)
        {
            QualitySettings.vSyncCount = -1;

            _enginesRoot = new EnginesRoot(new UnityEntitySubmissionScheduler());

            //add the engines we are going to use
            var generateEntityFactory = _enginesRoot.GenerateEntityFactory();

            var redfoodEntity =
                GameObjectConversionUtility.ConvertGameObjectHierarchy(Resources.Load("Sphere") as GameObject,
                                                                       new GameObjectConversionSettings()
                                                                           {DestinationWorld = _world});
            var bluefoodEntity =
                GameObjectConversionUtility.ConvertGameObjectHierarchy(Resources.Load("Sphereblue") as GameObject,
                                                                       new GameObjectConversionSettings()
                                                                           {DestinationWorld = _world});
            var redDoofusEntity =
                GameObjectConversionUtility.ConvertGameObjectHierarchy(Resources.Load("RedCapsule") as GameObject,
                                                                       new GameObjectConversionSettings()
                                                                           {DestinationWorld = _world});
            var blueDoofusEntity =
                GameObjectConversionUtility.ConvertGameObjectHierarchy(Resources.Load("BlueCapsule") as GameObject,
                                                                       new GameObjectConversionSettings()
                                                                           {DestinationWorld = _world});

            _enginesRoot.AddEngine(new PlaceFoodOnClickEngine(redfoodEntity, bluefoodEntity, generateEntityFactory));
            _enginesRoot.AddEngine(new SpawningDoofusEngine(redDoofusEntity, blueDoofusEntity, generateEntityFactory));
            _enginesRoot.AddEngine(new SpawnUnityEntityOnSveltoEntityEngine(_world));
            _enginesRoot.AddEngine(new ConsumingFoodEngine(_enginesRoot.GenerateEntityFunctions()));
            
            AddSveltoUECSEngine(new LookingForFoodDoofusesEngine());
            AddSveltoUECSEngine(new VelocityToPositionDoofusesEngine());
            AddSveltoUECSEngine(new RenderingUECSDataSynchronizationEngine());
        }

        void AddSveltoUECSEngine<T>(T engine) where T : ComponentSystemBase, IEngine
        {
            _world.AddSystem(engine);
            var simulationSystemGroup = _world.GetExistingSystem<SimulationSystemGroup>();
            simulationSystemGroup.AddSystemToUpdateList(engine);
            _enginesRoot.AddEngine(engine);
        }

        public void OnContextDestroyed()
        {
            DoofusesStandardSchedulers.StopAndCleanupAllDefaultSchedulers();
            TaskRunner.Stop();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            _enginesRoot?.Dispose();
        }

        public void OnContextCreated<T>(T contextHolder) { }

        /// <summary>
        /// A bit messy, it's not from UnityECS 0.2.0 and I will need to study it better
        /// </summary>
        /// <param name="defaultWorldName"></param>
        /// <returns></returns>
        public bool Initialize(string defaultWorldName)
        {
            //            Physics.autoSimulation = false;
            _world = new World("Custom world");

            World.DefaultGameObjectInjectionWorld = _world;
            var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(_world, systems);
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(_world);

            return true;
        }
    }
}