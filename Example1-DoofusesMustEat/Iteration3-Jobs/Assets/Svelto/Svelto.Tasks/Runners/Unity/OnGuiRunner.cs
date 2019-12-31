
using Svelto.Tasks.ExtraLean;
using Svelto.Tasks.Lean;
using Svelto.Tasks.Unity.Internal;
#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using Svelto.Tasks.Internal;

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
        public abstract class OnGuiRunner<T> : OnGuiRunner<T, StandardRunningTasksInfo> where T : ISveltoTask
        {
            protected OnGuiRunner(string name) : base(name, 0, new StandardRunningTasksInfo()) {}
            protected OnGuiRunner(string name, uint runningOrder) : base(name, runningOrder,
                new StandardRunningTasksInfo()) {}
        }

        public abstract class OnGuiRunner<T, TFlowModifier> : BaseRunner<T> where T : ISveltoTask
            where TFlowModifier : IRunningTasksInfo
        {
            protected OnGuiRunner(string name, uint runningOrder, TFlowModifier modifier) : base(name)
            {
                modifier.runnerName = name;

                _processEnumerator =
                    new CoroutineRunner<T>.Process<TFlowModifier>
                        (_newTaskRoutines, _coroutines, _flushingOperation, modifier);

                UnityCoroutineRunner.StartOnGuiCoroutine(_processEnumerator, runningOrder);
            }
        }
    }
}
#endif