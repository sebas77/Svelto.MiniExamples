using System;
using Svelto.Context;
using Svelto.ECS.Schedulers.Unity;
using Svelto.Tasks;


namespace Svelto.ECS.MiniExamples.Example1
{
    public class SveltoCompositionRoot : ICompositionRoot
    {
        public void OnContextInitialized<T>(T contextHolder)
        {
            _enginesRoot = new EnginesRoot(new UnityEntitySubmissionScheduler());

            var context = contextHolder as SveltoContext;
            
            ThreadSynchronizationSignal _signal = new ThreadSynchronizationSignal("name");
            
            //add the engines we are going to use
            _enginesRoot.AddEngine(new RenderingDataSyncronizationEngine(_signal));
            
            _enginesRoot.AddEngine(new SpawningDoofusEngine
                                       (context.mesh, context.material, _enginesRoot.GenerateEntityFactory())); 
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

