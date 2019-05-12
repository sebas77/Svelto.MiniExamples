using System;
using Svelto.Common.Internal;

namespace Svelto.Common
{
    public interface IPlatformProfiler
    {
        DisposableStruct Sample(string samplerName, string samplerInfo = null);
        DisposableStruct Sample<T>(T sampled, string samplerInfo = null);
        DisposableStruct StartNewSession(string name);
    }
    
    public struct DisposableStruct : IDisposable
    {
        readonly Action<object> _endAction;
        readonly object _endInfo;
        readonly Action<object> _beginAction;
        readonly object         _beginInfo;

        public DisposableStruct(Action<object> beginAction, object beginInfo, Action<object> endEndAction, object endInfo)
        {
            _endAction = endEndAction;
            _endInfo = endInfo;
            _beginAction = beginAction;
            _beginInfo = beginInfo;

            _beginAction?.Invoke(_beginInfo);
        }

        public void Dispose()
        {
            _endAction?.Invoke(_endInfo);
        }

        public InverseDisposableStruct Yield()
        {
            _endAction?.Invoke(_endInfo);

            return new InverseDisposableStruct(_beginAction, _beginInfo);
        }
    }
    
    public struct InverseDisposableStruct : IDisposable
    {
        readonly object _beginInfo;
        readonly Action<object> _beginAction;

        public InverseDisposableStruct(Action<object> beginAction, object beginInfo)
        {
            _beginInfo = beginInfo;
            _beginAction = beginAction;
        }

        public void Dispose()
        {
            _beginAction?.Invoke(_beginInfo);
        }
    }
#if UNITY_2017_3_OR_NEWER && ENABLE_PLATFORM_PROFILER    
    public struct PlatformProfilerMT : IPlatformProfiler
    {
        static readonly Action<object> END_SAMPLE_ACTION = (info) => UnityEngine.Profiling.Profiler.EndSample(); 
        static readonly Action<object> END_SESSION_ACTION = (info) => UnityEngine.Profiling.Profiler.EndThreadProfiling();
        
        static readonly Action<object> BEGIN_SAMPLE_ACTION  = (info) => UnityEngine.Profiling.Profiler.BeginSample(info as string); 
        static readonly Action<object> BEGIN_SESSION_ACTION = (info) => UnityEngine.Profiling.Profiler.BeginThreadProfiling("Svelto.Tasks", info  as string);
        
        public DisposableStruct Sample(string samplerName, string samplerInfo = null)
        {
            var name = samplerName.FastConcat("-", samplerInfo);
            
            return new DisposableStruct(BEGIN_SAMPLE_ACTION, name, END_SAMPLE_ACTION, null);
        }

        public DisposableStruct Sample<T>(T samplerName, string samplerInfo = null)
        {
            return Sample(samplerName.TypeName(), samplerInfo);
        }

        public DisposableStruct StartNewSession(string name)
        {
            return new DisposableStruct(BEGIN_SESSION_ACTION, name, END_SESSION_ACTION, null);
        }
    }

    public struct PlatformProfiler: IPlatformProfiler
    {
        static readonly Action<object> END_SAMPLE_ACTION  = (info) => UnityEngine.Profiling.Profiler.EndSample(); 
        static readonly Action<object> BEGIN_SAMPLE_ACTION  = (info) => UnityEngine.Profiling.Profiler.BeginSample(info as string); 
        
        public DisposableStruct Sample(string samplerName, string samplerInfo = null)
        {
            var name = samplerName.FastConcat("-", samplerInfo);
            
            return new DisposableStruct(BEGIN_SAMPLE_ACTION, name, END_SAMPLE_ACTION, null);
        }

        public DisposableStruct Sample<T>(T sampled, string samplerInfo = null)
        {
            return Sample(sampled.TypeName(), samplerInfo);
        }

        public DisposableStruct StartNewSession(string name)
        {
            return new DisposableStruct();
        }
    }
#else    
    public struct PlatformProfilerMT : IPlatformProfiler
    {
        public DisposableStruct Sample(string samplerName, string samplerInfo = null)
        {
            return new DisposableStruct();
        }

        public DisposableStruct Sample<T>(T sampled, string samplerInfo = null)
        {
            return new DisposableStruct();
        }

        public DisposableStruct StartNewSession(string name)
        {
            return new DisposableStruct();
        }
    }

    public struct PlatformProfiler: IPlatformProfiler
    {
        public DisposableStruct Sample(string samplerName, string samplerInfo = null)
        {
            return new DisposableStruct();
        }
        
        public DisposableStruct Sample<T>(T samplerName, string samplerInfo = null)
        {
            return new DisposableStruct();
        }

        public DisposableStruct StartNewSession(string name)
        {
            return new DisposableStruct();
        }
    }
#endif
}