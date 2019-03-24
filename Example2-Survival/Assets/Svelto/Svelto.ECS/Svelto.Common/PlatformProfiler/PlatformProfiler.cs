using System;

namespace Svelto.Common
{
    public interface IPlatformProfiler
    {
        DisposableStruct BeginSample(string samplerName, string samplerInfo = null);
        DisposableStruct StartNewSession(string name);
    }
    
    public struct DisposableStruct : IDisposable
    {
        readonly Action<object> _action;
        readonly object _sampler;

        public DisposableStruct(Action<object> action, object sampler)
        {
            _action = action;
            _sampler = sampler;
        }

        public void Dispose()
        {
            if (_action != null)
                _action(_sampler);
        }
    }
#if UNITY_5 || UNITY_5_3_OR_NEWER    
    public struct PlatformProfilerMT : IPlatformProfiler
    {
        static readonly Action<object> _action = (sampler) => UnityEngine.Profiling.Profiler.EndSample(); 
        static readonly Action<object> _action2 = (sampler) => UnityEngine.Profiling.Profiler.EndThreadProfiling();
        
        public DisposableStruct BeginSample(string samplerName, string samplerInfo = null)
        {
            UnityEngine.Profiling.Profiler.BeginSample(samplerName);
            
            return new DisposableStruct(_action, null);
        }

        public DisposableStruct StartNewSession(string name)
        {
            UnityEngine.Profiling.Profiler.BeginThreadProfiling("Svelto.Tasks", name);
            
            return new DisposableStruct(_action2, null);
        }
    }

    public struct PlatformProfiler: IPlatformProfiler
    {
        static readonly Action<object> _action = (sampler) => UnityEngine.Profiling.Profiler.EndSample();
        
        public DisposableStruct BeginSample(string samplerName, string samplerInfo = null)
        {
            UnityEngine.Profiling.Profiler.BeginSample(samplerName);
            
            return new DisposableStruct(_action, null);
        }

        public DisposableStruct StartNewSession(string name)
        {
            return new DisposableStruct();
        }
    }
#endif
}