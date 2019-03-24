using System;
using Svelto.Context;
using Svelto.ECS.Schedulers.Unity;
using Svelto.Tasks;
using Unity.Entities;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class SveltoCompositionRoot : ICompositionRoot
    {
        public void OnContextInitialized<T>(T contextHolder)
        {
            QualitySettings.vSyncCount = -1;
            
            _enginesRoot = new EnginesRoot(new UnityEntitySubmissionScheduler());

            var context = contextHolder as SveltoContext;
            //add the engines we are going to use
            World world = World.Active;
            var generateEntityFactory = _enginesRoot.GenerateEntityFactory();
            _enginesRoot.AddEngine(new PlaceFoodOnClickEngine(GameObjectConversionUtility.ConvertGameObjectHierarchy(
                            context.food, world), generateEntityFactory));
            _enginesRoot.AddEngine(new SpawningDoofusEngine
                                       (GameObjectConversionUtility.ConvertGameObjectHierarchy(context.capsule,
                                                                              world), generateEntityFactory));
            
            _enginesRoot.AddEngine(new LookingForFoodDoofusesEngine(_enginesRoot.GenerateEntityFunctions()));
            _enginesRoot.AddEngine(new ConsumingFoodEngine(_enginesRoot.GenerateEntityFunctions()));
            _enginesRoot.AddEngine(new SpawnUnityEntityOnSveltoEntityEngine(world));
            _enginesRoot.AddEngine(new VelocityToPositionDoofusesEngine());
            _enginesRoot.AddEngine(new DieOfHungerDoofusesEngine(_enginesRoot.GenerateEntityFunctions()));
            
            //one engine two ECS implementations :P
            var renderingDataSynchronizationEngine = new RenderingDataSynchronizationEngine(world);
            _enginesRoot.AddEngine(renderingDataSynchronizationEngine);
        }

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

