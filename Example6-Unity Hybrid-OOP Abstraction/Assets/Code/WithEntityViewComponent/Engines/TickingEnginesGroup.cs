using System;
using Svelto.DataStructures;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction
{
    class TickingEnginesGroup : UnsortedEnginesGroup<IStepEngine>
    {
        public TickingEnginesGroup(FasterList<IStepEngine> engines) : base(engines)
        {
            var tickingSystem = new GameObject("Ticking System");

            tickingSystem.AddComponent<UpdateMe>().update = Step;
        }

        class UpdateMe : MonoBehaviour
        {
            internal Action update;

            public UpdateMe(Action update) { this.update = update; }

            void Update() { update(); }
        }
    }
}