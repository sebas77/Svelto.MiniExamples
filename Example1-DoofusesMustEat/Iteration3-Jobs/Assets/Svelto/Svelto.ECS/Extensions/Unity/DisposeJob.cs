#if UNITY_2019_2_OR_NEWER
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

        public void Execute()
        {
            try
            {
                _entityCollection.Dispose();
            }
            catch (Exception e)
            {
                Svelto.Console.LogException(typeof(T).ToString().FastConcat(" "), e);
            }
        }
        
        readonly T _entityCollection;
    }
}
#endif