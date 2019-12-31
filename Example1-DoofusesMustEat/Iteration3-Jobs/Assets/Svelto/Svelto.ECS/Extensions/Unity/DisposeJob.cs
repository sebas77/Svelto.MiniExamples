#if UNITY_2019_3_OR_NEWER
using System;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public struct DisposeJob<T>:IJob where T:struct,IDisposable
    {
        public DisposeJob(T disposable)
        {
            _entityCollection = disposable;
        }

        public void Execute() { _entityCollection.Dispose(); }
        
        readonly T _entityCollection;
    }
}
#endif