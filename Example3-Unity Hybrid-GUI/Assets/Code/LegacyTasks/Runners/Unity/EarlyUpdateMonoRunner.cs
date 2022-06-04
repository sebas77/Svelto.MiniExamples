#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using Svelto.Tasks.Unity.Internal;

namespace Svelto.Tasks
{
    namespace Lean.Unity
    {
        public class EarlyUpdateMonoRunner : EarlyUpdateMonoRunner<IEnumerator<TaskContract>>
        {
            public EarlyUpdateMonoRunner(string name) : base(name)
            {
            }

            public EarlyUpdateMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }

        public class EarlyUpdateMonoRunner<T> : Svelto.Tasks.Unity.EarlyUpdateMonoRunner<LeanSveltoTask<T>>
            where T : IEnumerator<TaskContract>
        {
            public EarlyUpdateMonoRunner(string name) : base(name)
            {
            }

            public EarlyUpdateMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }
    }

    namespace ExtraLean.Unity
    {
        public class EarlyUpdateMonoRunner : EarlyUpdateMonoRunner<IEnumerator>
        {
            public EarlyUpdateMonoRunner(string name) : base(name)
            {
            }

            public EarlyUpdateMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }

        public class EarlyUpdateMonoRunner<T> : Svelto.Tasks.Unity.EarlyUpdateMonoRunner<ExtraLeanSveltoTask<T>>
            where T : IEnumerator
        {
            public EarlyUpdateMonoRunner(string name) : base(name)
            {
            }

            public EarlyUpdateMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }
    }

    namespace Unity
    {
        public class EarlyUpdateMonoRunner<T> : SteppableRunner<T> where T : ISveltoTask
        {
            public EarlyUpdateMonoRunner(string name) : base(name)
            {
                UnityCoroutineRunner.StartEarlyUpdateCoroutine(_processEnumerator, 0);
            }
            
            public EarlyUpdateMonoRunner(string name, uint runningOrder) : base(name)
            {
                UnityCoroutineRunner.StartEarlyUpdateCoroutine(_processEnumerator, runningOrder);
            }
        }
    }
}
#endif