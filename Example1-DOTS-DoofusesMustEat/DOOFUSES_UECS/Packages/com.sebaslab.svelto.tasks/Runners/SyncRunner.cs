using System;
using System.Collections.Generic;
using System.Threading;

namespace Svelto.Tasks
{
    public class SyncRunner : BaseSyncRunner<IEnumerator<TaskContract>>
    {
        public SyncRunner(int timeout = 1000) : base(timeout) {}
    }

    public class SyncRunner<T> : BaseSyncRunner<T> where T : IEnumerator<TaskContract>
    {
        public SyncRunner(int timeout = 1000) : base(timeout) {}
    }

    public static class LocalSyncRunners<T> where T : IEnumerator<TaskContract>
    {
        public static readonly ThreadLocal<SyncRunner<T>> syncRunner =
            new ThreadLocal<SyncRunner<T>>(() => new SyncRunner<T>());
    }

    public class BaseSyncRunner<T> where T : IEnumerator<TaskContract>
    {
        public bool isStopping { private set; get; }
        public bool isKilled   => false;

        protected BaseSyncRunner(int timeout = 1000)
        {
            _timeout        = timeout;
            _taskCollection = new SerialTaskCollection<T>();
        }

        public void ExecuteCoroutine(in T leanSveltoTask)
        {
            _taskCollection.Clear();
            _taskCollection.Add(leanSveltoTask);
            _taskCollection.Complete(_timeout);
        }

        /// <summary>
        /// todo, this could make sense in a multi-threaded scenario
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Pause() { throw new NotImplementedException(); }

        /// <summary>
        /// todo, this could make sense in a multi-threaded scenario
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Resume() { throw new NotImplementedException(); }

        /// <summary>
        /// todo, this could make sense in a multi-threaded scenario
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Stop() { throw new NotImplementedException(); }

        public void Flush() { }

        public void Dispose() { }

        public uint numberOfRunningTasks    => 0;
        public uint numberOfQueuedTasks     => 0;
        public uint numberOfProcessingTasks => 0;

        readonly int                     _timeout;
        readonly SerialTaskCollection<T> _taskCollection;
    }
}