using System;
using Svelto.Context;
using Svelto.ECS.Schedulers.Unity;
using Svelto.Tasks;
using Unity.Entities;    


namespace Svelto.ECS.MiniExamples.Example1
{
    public class SveltoCompositionRoot : ICompositionRoot
    {
        public void OnContextInitialized<T>(T contextHolder)
        {
            _enginesRoot = new EnginesRoot(new UnityEntitySubmissionScheduler());

            var context = contextHolder as SveltoContext;
            
            //add the engines we are going to use
            var renderingDataSyncronizationEngine = new RenderingDataSyncronizationEngine();
            
            _enginesRoot.AddEngine(new SpawningDoofusEngine
                                       (context.mesh, context.material, _enginesRoot.GenerateEntityFactory()));
            _enginesRoot.AddEngine(new MovingDoofusesEngine());
            _enginesRoot.AddEngine(renderingDataSyncronizationEngine);

            //one engine two ECS implementations :P
            World.Active.AddManager(renderingDataSyncronizationEngine);
        }

        public void OnContextDestroyed()
        {
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

