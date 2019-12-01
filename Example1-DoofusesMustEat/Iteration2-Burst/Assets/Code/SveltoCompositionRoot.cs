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
        public bool Initialize(string defaultWorldName)
        {
            QualitySettings.vSyncCount = -1;
//            Physics.autoSimulation = false;
            
            _enginesRoot = new EnginesRoot(new UnityEntitySubmissionScheduler());

           // var context = contextHolder as SveltoContext;
            //add the engines we are going to use
            var generateEntityFactory = _enginesRoot.GenerateEntityFactory();
            var world = new World("Custom world");

            
         //   _enginesRoot.AddEngine(new PlaceFoodOnClickEngine(GameObjectConversionUtility.ConvertGameObjectHierarchy(
           //                 context.food, world), generateEntityFactory));
            //_enginesRoot.AddEngine(new SpawningDoofusEngine
              //                         (GameObjectConversionUtility.ConvertGameObjectHierarchy(context.capsule,
                //                                                              world), generateEntityFactory));
            
            _enginesRoot.AddEngine(new LookingForFoodDoofusesEngine());
         //   _enginesRoot.AddEngine(new ConsumingFoodEngine(_enginesRoot.GenerateEntityFunctions()));
            _enginesRoot.AddEngine(new SpawnUnityEntityOnSveltoEntityEngine(world));
            _enginesRoot.AddEngine(new VelocityToPositionDoofusesEngine());
//            _enginesRoot.AddEngine(new DieOfHungerDoofusesEngine(_enginesRoot.GenerateEntityFunctions()));
            
            //one engine two ECS implementations :P
            var renderingDataSynchronizationEngine = new RenderingDataSynchronizationEngine(world);
            _enginesRoot.AddEngine(renderingDataSynchronizationEngine);
            Debug.Log("Executing bootstrap");
            
            world.AddSystem(renderingDataSynchronizationEngine);
            World.DefaultGameObjectInjectionWorld = world;
            var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);

            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);
            return true;
            
        }

        public void OnContextInitialized<T>(T contextHolder) {  }

        public void OnContextDestroyed()
        {
            DoofusesStandardSchedulers.StopAndCleanupAllDefaultSchedulers();
            TaskRunner.StopAndCleanupAllDefaultSchedulers();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            _enginesRoot.Dispose();
        }

        public void OnContextCreated<T>(T contextHolder)
        {}

        EnginesRoot _enginesRoot;
        
        
    }
}

