#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using Svelto.Tasks.Unity.Internal;

namespace Svelto.Tasks
{
    /// <summary>
    /// while you can instantiate a BaseRunner, you should use the standard one whenever possible. Instantiating
    /// multiple runners will defeat the initial purpose to get away from the Unity monobehaviours internal updates.
    /// MonoRunners are disposable though, so at least be sure to dispose the ones that are unused
    /// CoroutineMonoRunner is the only Unity based Svelto.Tasks runner that can handle Unity YieldInstructions
    /// You should use YieldInstructions only when extremely necessary as often an Svelto.Tasks IEnumerator
    /// replacement is available.
    /// </summary>
    namespace Lean.Unity
    {
        public class CoroutineMonoRunner : Svelto.Tasks.Unity.CoroutineMonoRunner<LeanSveltoTask<IEnumerator<TaskContract>>>
        {
            public CoroutineMonoRunner(string name) : base(name)
            {
            }
            public CoroutineMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }

        public class CoroutineMonoRunner<T> : Svelto.Tasks.Unity.CoroutineMonoRunner<LeanSveltoTask<T>> where T : IEnumerator<TaskContract>
        {
            public CoroutineMonoRunner(string name) : base(name)
            {
            }
            public CoroutineMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }
    }

    namespace ExtraLean.Unity
    {
        public class CoroutineMonoRunner : Svelto.Tasks.Unity.CoroutineMonoRunner<ExtraLeanSveltoTask<IEnumerator>>
        {
            public CoroutineMonoRunner(string name) : base(name)
            {
            }
            public CoroutineMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }

        public class CoroutineMonoRunner<T> : Svelto.Tasks.Unity.CoroutineMonoRunner<ExtraLeanSveltoTask<T>> where T : IEnumerator
        {
            public CoroutineMonoRunner(string name) : base(name)
            {
            }
            public CoroutineMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }
    }

    namespace Unity
    {
        public class CoroutineMonoRunner<T> : SteppableRunner<T> where T : ISveltoTask
        {
            public CoroutineMonoRunner(string name) : base(name)
            {
                UnityCoroutineRunner.StartLateCoroutine(_processEnumerator, 0);
            }
            
            public CoroutineMonoRunner(string name, uint runningOrder) : base(name)
            {
                UnityCoroutineRunner.StartLateCoroutine(_processEnumerator, runningOrder);
            }
        }
    }
}
#endif
