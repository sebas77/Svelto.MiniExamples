#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using Svelto.Tasks.Unity.Internal;

namespace Svelto.Tasks
{
    namespace Lean.Unity
    {
        public class EndOfFrameMonoRunner:EndOfFrameMonoRunner<IEnumerator<TaskContract>>
        {
            public EndOfFrameMonoRunner(string name) : base(name)
            {
            }
            public EndOfFrameMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }

        public class EndOfFrameMonoRunner<T> : Svelto.Tasks.Unity.EndOfFrameMonoRunner<LeanSveltoTask<T>> where T : IEnumerator<TaskContract>
        {
            public EndOfFrameMonoRunner(string name) : base(name)
            {
            }
            public EndOfFrameMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }
    }

    namespace ExtraLean.Unity
    {
        public class EndOfFrameMonoRunner:EndOfFrameMonoRunner<IEnumerator>
        {
            public EndOfFrameMonoRunner(string name) : base(name)
            {
            }
            public EndOfFrameMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }

        public class EndOfFrameMonoRunner<T> : Svelto.Tasks.Unity.EndOfFrameMonoRunner<ExtraLeanSveltoTask<T>> where T : IEnumerator
        {
            public EndOfFrameMonoRunner(string name) : base(name)
            {
            }
            public EndOfFrameMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }
    }

    namespace Unity
    {
        public class EndOfFrameMonoRunner<T> : SteppableRunner<T> where T : ISveltoTask
        {
            public EndOfFrameMonoRunner(string name) : base(name)
            {
                UnityCoroutineRunner.StartLateCoroutine(_processEnumerator, 0);
            }
            
            public EndOfFrameMonoRunner(string name, uint runningOrder) : base(name)
            {
                UnityCoroutineRunner.StartLateCoroutine(_processEnumerator, runningOrder);
            }
        }
    }
}
#endif
