using System;
using Svelto.DataStructures;
using UnityEngine;

namespace Svelto.ECS.Example.Survive
{
    /// <summary>
    /// If you are wondering why bothering with a decoupled way to sort the engines in the list, instead than
    /// just to use the engines as they are found in the list, you are correct to wonder.
    /// In this example it doesn't make much sense, but when you start to use encapsulated composition roots
    /// in packaged assemblies, this method allows to avoid circular dependencies. It's here just for
    /// demonstration purposes
    /// </summary>
    public class TickEnginesGroup : SortedEnginesGroup<IStepEngine, SortedTickedEnginesOrder>
    {
        public TickEnginesGroup(FasterList<IStepEngine> engines) : base(engines)
        {
            var tickingSystem = new GameObject("Ticking System");

            _tickingSystem        = tickingSystem.AddComponent<UpdateMe>();
            _tickingSystem.update = Step;
        }

        readonly UpdateMe _tickingSystem;

        class UpdateMe : MonoBehaviour
        {
            internal Action update;

            public UpdateMe(Action update) { this.update = update; }

            void Update() { update(); }
        }
    }
}