using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Schedulers;
using Svelto.Tasks;
using Svelto.Tasks.ExtraLean;
using Svelto.Tasks.ExtraLean.Unity;

using Unity.Jobs;

namespace Svelto.ECS.MiniExamples.Example1C
{
    /// <summary>
    /// As usual we create our own loop. These demo loops are simple but show the foundation how what it can be done
    /// Loop usually tick engines and submit entities.
    /// </summary>
    class MainLoop
    {
        public MainLoop(FasterList<IJobifiedEngine> enginesToTick, SimpleEntitiesSubmissionScheduler scheduler)
        {
            _enginesToTick  = enginesToTick;
            _scheduler = scheduler;
        }
        
        IEnumerator Loop()
        {
            var sveltoEngines = new SortedDoofusesEnginesExecutionGroup(_enginesToTick);

            JobHandle jobs = default;

            while (true)
            {
                //Engines are executed in ordered fashion exploiting the Svelto ISequenceOrder pattern
                jobs = sveltoEngines.Execute(jobs);
                
                jobs.Complete(); //Job Sync Point
                
                _scheduler.SubmitEntities();

                yield return Yield.It;
            }
        }

        public void Dispose() 
        {
            _mainThreadScheduler.Dispose();
        }

        public void Run()
        {
            Loop().RunOn(_mainThreadScheduler);
        }

        readonly EarlyUpdateMonoRunner _mainThreadScheduler = new EarlyUpdateMonoRunner("MainThreadScheduler");
        
        readonly FasterList<IJobifiedEngine>       _enginesToTick;
        readonly SimpleEntitiesSubmissionScheduler _scheduler;
    }
}