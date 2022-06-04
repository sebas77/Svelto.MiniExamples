#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using Svelto.Tasks.Unity.Internal;

namespace Svelto.Tasks
{
    namespace Lean.Unity
    {
        public class UpdateMonoRunner : UpdateMonoRunner<IEnumerator<TaskContract>>
        {
            public UpdateMonoRunner(string name) : base(name) {}
            public UpdateMonoRunner(string name, uint runningOrder) : base(name, runningOrder) {}
        }

        public class UpdateMonoRunner<T> : Svelto.Tasks.Lean.SteppableRunner
            where T : IEnumerator<TaskContract>
        {
            public UpdateMonoRunner(string name) : base(name)
            {
                UnityCoroutineRunner.StartLateCoroutine(_processEnumerator, 0);
            }
            
            public UpdateMonoRunner(string name, uint runningOrder) : base(name)
            {
                UnityCoroutineRunner.StartLateCoroutine(_processEnumerator, runningOrder);
            }
        }
    }

    namespace ExtraLean.Unity
    {
        public class UpdateMonoRunner : UpdateMonoRunner<IEnumerator>
        {
            public UpdateMonoRunner(string name) : base(name) {}
            public UpdateMonoRunner(string name, uint runningOrder) : base(name, runningOrder) {}
        }

        public class UpdateMonoRunner<T> : Svelto.Tasks.ExtraLean.SteppableRunner
        {
            public UpdateMonoRunner(string name) : base(name)
            {
                UnityCoroutineRunner.StartLateCoroutine(_processEnumerator, 0);
            }
            
            public UpdateMonoRunner(string name, uint runningOrder) : base(name)
            {
                UnityCoroutineRunner.StartLateCoroutine(_processEnumerator, runningOrder);
            }
        }
    }
}
#endif