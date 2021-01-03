using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;
using Svelto.Tasks;
using Svelto.Tasks.ExtraLean;
using Svelto.Tasks.ExtraLean.Unity;
using Unity.Jobs;

namespace Svelto.ECS.MiniExamples.Example1C
{
    class MainLoop
    {
        public MainLoop(FasterList<IJobifiedEngine> enginesToTick)
        {
            _enginesToTick  = enginesToTick;
        }
        
        IEnumerator Loop()
        {
            var sveltoEngines = new SortedDoofusesEnginesExecutionGroup(_enginesToTick);

            JobHandle jobs = default;

            while (true)
            {
                //Engines are executed in ordered fashion exploiting the Svelto ISequenceOrder pattern
                jobs = sveltoEngines.Execute(jobs);

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
        readonly FasterList<IJobifiedEngine> _enginesToTick;
    }
}