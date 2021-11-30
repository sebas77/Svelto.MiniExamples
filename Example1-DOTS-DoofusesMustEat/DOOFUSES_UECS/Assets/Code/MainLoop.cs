using System;
using Svelto.DataStructures;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Jobs;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1C
{
    class MainLoop
    {
        public MainLoop(FasterList<IJobifiedEngine> enginesToTick)
        {
            _enginesToTick = enginesToTick;
            _sveltoEngines = new SortedDoofusesEnginesExecutionGroup(_enginesToTick);
            _job           = default;
        }
        
        void Loop()
        {
            //pure DOTS, no need to complete any job, just be sure that the previous lot is an input dependency                
            _job = _sveltoEngines.Execute(_job);
        }

        public void Dispose() 
        { }

        public void Run()
        {
            GameObject ticker = new GameObject("Ticker");
            ticker.AddComponent<TickerComponent>().callBack = Loop;
        }

        readonly FasterList<IJobifiedEngine>         _enginesToTick;
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