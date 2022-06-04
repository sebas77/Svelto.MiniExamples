#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using Svelto.Tasks.Unity.Internal;

namespace Svelto.Tasks
{
    namespace Lean.Unity
    {
        public class PhysicMonoRunner:PhysicMonoRunner<IEnumerator<TaskContract>>
        {
            public PhysicMonoRunner(string name) : base(name)
            {
            }
            public PhysicMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }

        public class PhysicMonoRunner<T> : Svelto.Tasks.Unity.PhysicMonoRunner<LeanSveltoTask<T>> where T : IEnumerator<TaskContract>
        {
            public PhysicMonoRunner(string name) : base(name)
            {
            }
            public PhysicMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }
    }

    namespace ExtraLean.Unity
    {
        public class PhysicMonoRunner:PhysicMonoRunner<IEnumerator>
        {
            public PhysicMonoRunner(string name) : base(name)
            {
            }
            public PhysicMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }

        public class PhysicMonoRunner<T> : Svelto.Tasks.Unity.PhysicMonoRunner<ExtraLeanSveltoTask<T>> where T : IEnumerator
        {
            public PhysicMonoRunner(string name) : base(name)
            {
            }
            public PhysicMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }
    }

    namespace Unity
    {
        public class PhysicMonoRunner<T> : SteppableRunner<T> where T : ISveltoTask
        {
            public PhysicMonoRunner(string name) : base(name)
            {
                UnityCoroutineRunner.StartLateCoroutine(_processEnumerator, 0);
            }
            
            public PhysicMonoRunner(string name, uint runningOrder) : base(name)
            {
                UnityCoroutineRunner.StartLateCoroutine(_processEnumerator, runningOrder);
            }
        }
    }
}
#endif
