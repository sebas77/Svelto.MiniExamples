using System;
using System.Collections;
using Svelto.Common;
using Svelto.Context;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.Tasks;
using Svelto.Tasks.ExtraLean;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public class EnginesExecutionOrder : JobifiableEnginesGroup<IJobifiableEngine, DoofusesEnginesOrder>
    {
        public EnginesExecutionOrder(FasterReadOnlyList<IJobifiableEngine> engines) : base(engines)
        {
        }
    }

    public struct DoofusesEnginesOrder : ISequenceOrder
    {
        public string[] enginesOrder => new string[] { };
    }

    public class SveltoCompositionRoot : ICompositionRoot, ICustomBootstrap
    {
        static World _world;

        EnginesRoot _enginesRoot;
        FasterList<IJobifiableEngine> _enginesToTick;
        SimpleSubmissioncheduler _simpleSubmitScheduler;

        void StartTicking(FasterList<IJobifiableEngine> engines)
        {
            MainThreadTick(engines).RunOn(DoofusesStandardSchedulers.mainThreadScheduler);
        }

        IEnumerator MainThreadTick(FasterList<IJobifiableEngine> engines)
        {
            EnginesExecutionOrder order = new EnginesExecutionOrder(new FasterReadOnlyList<IJobifiableEngine>(engines));

            JobHandle jobs = default;
            
            while (true)
            {
                jobs.Complete();
                
                //Sync point on the mainthread:
                //Svelto entities are added/removed/swapped
                //callback functions are called (which may create UECS entities)
                _simpleSubmitScheduler.SubmitEntities();

                //schedule all jobs and let them run until next frame;
                order.Execute(jobs);
                
                yield return Yield.It;
            }
        }

        public void OnContextInitialized<T>(T contextHolder)
        {
            QualitySettings.vSyncCount = -1;

            _simpleSubmitScheduler = new SimpleSubmissioncheduler();
            _enginesRoot = new EnginesRoot(_simpleSubmitScheduler);
            _enginesToTick = new FasterList<IJobifiableEngine>();

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

            AddSveltoEngineToTick(new PlaceFoodOnClickEngine(redfoodEntity, bluefoodEntity, generateEntityFactory));
            AddSveltoEngineToTick(new SpawningDoofusEngine(redDoofusEntity, blueDoofusEntity, generateEntityFactory));
            AddSveltoEngineToTick(new ConsumingFoodEngine(_enginesRoot.GenerateEntityFunctions()));
            AddSveltoEngineToTick(new LookingForFoodDoofusesEngine());
            AddSveltoEngineToTick(new VelocityToPositionDoofusesEngine());
            
            AddSveltoCallbackEngine(new SpawnUnityEntityOnSveltoEntityEngine(_world));
            
            AddSveltoUECSEngine(new RenderingUECSDataSynchronizationEngine());

            StartTicking(_enginesToTick);
        }

        void AddSveltoCallbackEngine(IReactEngine engine)
        {
            _enginesRoot.AddEngine(engine);
        }

        void AddSveltoEngineToTick(IJobifiableEngine engine)
        {
            _enginesRoot.AddEngine(engine);
            _enginesToTick.Add(engine);
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
            
            GC.Collect();
            GC.WaitForPendingFinalizers();

            _enginesRoot?.Dispose();
        }

        public void OnContextCreated<T>(T contextHolder) { }

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