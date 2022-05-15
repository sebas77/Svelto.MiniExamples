#if !PROFILE_SVELTO
#warning for maximum performance, please define PROFILE_SVELTO
#endif

using System;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Jobs;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1C
{
    class MainLoop
    {
        public MainLoop(FasterList<IJobifiedEngine> enginesToTick,
            SimpleEntitiesSubmissionScheduler simpleEntitiesSubmissionScheduler)
        {
            _enginesToTick                     = enginesToTick;
            _simpleEntitiesSubmissionScheduler = simpleEntitiesSubmissionScheduler;
            _sveltoEngines                     = new SortedDoofusesEnginesExecutionGroup(_enginesToTick);
            _job                               = default;
        }
        
        void Loop()
        {
            //pure DOTS, no need to complete any job, just be sure that the previous lot is an input dependency                
            _job = _sveltoEngines.Execute(_job);
            
            _simpleEntitiesSubmissionScheduler.SubmitEntities();
        }

        public void Dispose() 
        { }

        public void Run()
        {
            GameObject ticker = new GameObject("Ticker");
            ticker.AddComponent<TickerComponent>().callBack = Loop;
        }

        readonly FasterList<IJobifiedEngine>         _enginesToTick;
        readonly SimpleEntitiesSubmissionScheduler   _simpleEntitiesSubmissionScheduler;
        readonly SortedDoofusesEnginesExecutionGroup _sveltoEngines;
        JobHandle                                    _job;
    }

    internal class TickerComponent:MonoBehaviour
    {
        public Action callBack;

        void Update()
        {
            callBack();
        }
    }
}