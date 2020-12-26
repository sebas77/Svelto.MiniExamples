using System;
using Svelto.DataStructures;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction
{
    class TickingEnginesGroup:UnsortedEnginesGroup<ITickingEngine>
    {
        public TickingEnginesGroup(FasterList<ITickingEngine> engines) : base(engines)
        {
            GameObject tickingSystem = new GameObject();
            tickingSystem.AddComponent<UpdateMe>().update = updateEngines;
        }

        void updateEngines()
        {
            base.StepAll();
        }

        class UpdateMe:MonoBehaviour
        {
            void Update()
            {
                update();
            }

            public UpdateMe(Action update) { this.update = update; }

            internal System.Action update;
        }
    }
}