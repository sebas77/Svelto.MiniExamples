#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using Svelto.Tasks.Unity.Internal;

namespace Svelto.Tasks
{
    namespace Lean.Unity
    {
        public class LateMonoRunner:LateMonoRunner<IEnumerator<TaskContract>>
        {
            public LateMonoRunner(string name) : base(name) { }
            public LateMonoRunner(string name, uint runningOrder) : base(name, runningOrder) { }
        }

        public class LateMonoRunner<T> : Svelto.Tasks.Unity.LateMonoRunner<LeanSveltoTask<T>> where T : IEnumerator<TaskContract>
        {
            public LateMonoRunner(string name) : base(name) { }
            public LateMonoRunner(string name, uint runningOrder) : base(name, runningOrder) { }
        }
    }

    namespace ExtraLean.Unity
    {
        public class LateMonoRunner: LateMonoRunner<IEnumerator>
        {
            public LateMonoRunner(string name) : base(name) { }
            public LateMonoRunner(string name, uint runningOrder) : base(name, runningOrder) { }
        }

        public class LateMonoRunner<T> : Svelto.Tasks.Unity.LateMonoRunner<ExtraLeanSveltoTask<T>> where T : IEnumerator
        {
            public LateMonoRunner(string name) : base(name) { }
            public LateMonoRunner(string name, uint runningOrder) : base(name, runningOrder) { }
        }
    }

    namespace Unity
    {
        public class LateMonoRunner<T> : SteppableRunner<T> where T : ISveltoTask
        {
            public LateMonoRunner(string name) : base(name)
            {
                UnityCoroutineRunner.StartLateCoroutine(_processEnumerator, 0);
            }
            
            public LateMonoRunner(string name, uint runningOrder) : base(name)
            {
                UnityCoroutineRunner.StartLateCoroutine(_processEnumerator, runningOrder);
            }
        }
    }
}
#endif
