#if UNITY_2017_3_OR_NEWER && ENABLE_PLATFORM_PROFILER && !UNITY_2018_3_OR_NEWER
using System;
using Svelto.Common.Internal;

namespace Svelto.Common
{
    public struct DisposableSampler : IDisposable
    {
        Action         _endAction;
        Action<object> _beginAction;
        object         _beginInfo;

        public DisposableSampler(Action<object> beginAction, object beginInfo, Action endEndAction):this()
        {
            Start(beginAction, beginInfo, endEndAction);
        }

#if DISABLE_CHECKS
		[Conditional("__NEVER_DEFINED__")]
#endif
        void Start(Action<object> beginAction, object beginInfo, Action endEndAction)
        {
            _endAction = endEndAction;
            _beginAction = beginAction;
            _beginInfo = beginInfo;

            _beginAction?.Invoke(_beginInfo);
        }

        public void Dispose()
        {
            End();
        }

#if DISABLE_CHECKS
		[Conditional("__NEVER_DEFINED__")]
#endif
        void End()
        {
            _endAction?.Invoke();
        }

        public InverseDisposableStruct Yield()
        {
            _endAction?.Invoke();

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

    public struct PlatformProfilerMT : IPlatformProfiler<DisposableSampler>
    {
        static readonly Action<object> BEGIN_SAMPLE_ACTION =
            info => UnityEngine.Profiling.Profiler.BeginSample(info as string);
        static readonly Action END_SAMPLE_ACTION =() => UnityEngine.Profiling.Profiler.EndSample(); 
         
        public PlatformProfilerMT(string info)
        {
            UnityEngine.Profiling.Profiler.BeginThreadProfiling("Svelto.Tasks", info);

            BEGIN_SAMPLE_ACTION(info);
        }
        
        public DisposableSampler Sample(string samplerName, string samplerInfo = null)
        {
#if !PROFILER        
            var name = samplerName.FastConcat("-", samplerInfo);
#else
            var name = samplerName;
#endif            
            return new DisposableSampler(BEGIN_SAMPLE_ACTION, name, END_SAMPLE_ACTION);
        }

        public DisposableSampler Sample<T>(T samplerName, string samplerInfo = null)
        {
            return Sample(samplerName.TypeName(), samplerInfo);
        }

        public void Dispose()
        {
            END_SAMPLE_ACTION();
            
            UnityEngine.Profiling.Profiler.EndThreadProfiling();
        }
    }

    public struct PlatformProfiler: IPlatformProfiler<DisposableSampler>
    {
        static readonly Action END_SAMPLE_ACTION  = () => UnityEngine.Profiling.Profiler.EndSample(); 
        static readonly Action<object> BEGIN_SAMPLE_ACTION =
            (info) => UnityEngine.Profiling.Profiler.BeginSample(info as string);

        public PlatformProfiler(string info)
        {
            BEGIN_SAMPLE_ACTION(info);
        }
        
        public DisposableSampler Sample(string samplerName, string samplerInfo = null)
        {
#if !PROFILER                    
            var name = samplerName.FastConcat("-", samplerInfo);
#else
            var name = samplerName;
#endif            
            
            return new DisposableSampler(BEGIN_SAMPLE_ACTION, name, END_SAMPLE_ACTION);
        }

        public DisposableSampler Sample<T>(T sampled, string samplerInfo = null)
        {
            return Sample(sampled.TypeName(), samplerInfo);
        }

        public void Dispose()
        {
            END_SAMPLE_ACTION();
        }
    }
}
#endif