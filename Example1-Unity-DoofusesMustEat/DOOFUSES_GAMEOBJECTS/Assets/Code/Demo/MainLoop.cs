#if !PROFILE_SVELTO
#warning for maximum performance, please define PROFILE_SVELTO
#endif

using System;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Jobs;
using UnityEngine;

//////////////////////////////////////////
/// This demo can still be optimised. For example is useless to transform the meal entities since they don't move.
/// This demo is also not using filters like the other variances, this because DOTS TransformArray is very limiting
/// to use and wouldn't work well with filters.
/// //////////////////////////////////////////
namespace Svelto.ECS.Miniexamples.Doofuses.Gameobjects
{
    class MainLoop
    {
        public MainLoop(FasterList<IJobifiedEngine> enginesToTick,
            EntitiesSubmissionScheduler simpleEntitiesSubmissionScheduler)
        {
            _enginesToTick                     = enginesToTick;
            _simpleEntitiesSubmissionScheduler = simpleEntitiesSubmissionScheduler;
            _sveltoEngines                     = new SortedDoofusesEnginesExecutionGroup(_enginesToTick);
            _job                               = default;
        }
        
        void Loop()
        {
            _job = _sveltoEngines.Execute(_job);
            
            _job.Complete();  //Job Sync Point
            
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
        readonly EntitiesSubmissionScheduler   _simpleEntitiesSubmissionScheduler;
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