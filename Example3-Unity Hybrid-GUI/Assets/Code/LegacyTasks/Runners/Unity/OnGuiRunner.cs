#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using Svelto.Tasks.Unity.Internal;

namespace Svelto.Tasks
{
    namespace Lean.Unity
    {
        public class OnGuiRunner : OnGuiRunner<IEnumerator<TaskContract>>
        {
            public OnGuiRunner(string name) : base(name) {}
            public OnGuiRunner(string name, uint runningOrder) : base(name, runningOrder) {}
        }

        public class OnGuiRunner<T> : Svelto.Tasks.Unity.OnGuiRunner<LeanSveltoTask<T>>
            where T : IEnumerator<TaskContract>
        {
            public OnGuiRunner(string name) : base(name) {}
            public OnGuiRunner(string name, uint runningOrder) : base(name, runningOrder) {}
        }
    }

    namespace ExtraLean.Unity
    {
        public class OnGuiRunner : OnGuiRunner<IEnumerator>
        {
            public OnGuiRunner(string name) : base(name) {}
            public OnGuiRunner(string name, uint runningOrder) : base(name, runningOrder) {}
        }

        public class OnGuiRunner<T> : Svelto.Tasks.Unity.OnGuiRunner<ExtraLeanSveltoTask<T>> where T : IEnumerator
        {
            public OnGuiRunner(string name) : base(name) {}
            public OnGuiRunner(string name, uint runningOrder) : base(name, runningOrder) {}
        }
    }

    namespace Unity
    {
        public class OnGuiRunner<T> : SteppableRunner<T> where T : ISveltoTask
        {
            public OnGuiRunner(string name) : base(name)
            {
                UnityCoroutineRunner.StartLateCoroutine(_processEnumerator, 0);
            }
            
            public OnGuiRunner(string name, uint runningOrder) : base(name)
            {
                UnityCoroutineRunner.StartLateCoroutine(_processEnumerator, runningOrder);
            }
        }
    }
}
#endif