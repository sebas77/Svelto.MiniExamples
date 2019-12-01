#if UNITY_2018_3_OR_NEWER && ENABLE_PLATFORM_PROFILER
using System;
using Svelto.Common.Internal;
using Unity.Profiling;

namespace Svelto.Common
{
     public struct DisposableSampler : IDisposable
    {
        ProfilerMarker _auto;

        public DisposableSampler(ProfilerMarker auto)
        {
            _auto = auto;
            _auto.Begin();
        }

#if DISABLE_CHECKS
		[Conditional("__NEVER_DEFINED__")]
#endif
        public void Dispose()
        {
            _auto.End();
        }
    }
    
    public struct PlatformProfilerMT : IPlatformProfiler<DisposableSampler>
    {
        public PlatformProfilerMT(string info)
        {
            _platformProfilerImplementation = new PlatformProfiler(info);
        }
        
        public void Dispose()
        {
            _platformProfilerImplementation.Dispose();
        }

        public DisposableSampler Sample(string samplerName, string samplerInfo = null)
        {
            return _platformProfilerImplementation.Sample(samplerName, samplerInfo);
        }

        public DisposableSampler Sample<W>(W sampled, string samplerInfo = null)
        {
            return _platformProfilerImplementation.Sample(sampled, samplerInfo);
        }
        
        PlatformProfiler _platformProfilerImplementation;
    }

    public struct PlatformProfiler: IPlatformProfiler<DisposableSampler>
    {
        public PlatformProfiler(string info)
        {}
        
        public DisposableSampler Sample(string samplerName, string samplerInfo = null)
        {
#if !PROFILER
            var name = samplerInfo != null ? samplerName.FastConcat("-", samplerInfo) : samplerName;
#else
            var name = samplerName;
#endif            
            return new DisposableSampler(new ProfilerMarker(name));
        }

        public DisposableSampler Sample<T>(T sampled, string samplerInfo = null)
        {
            return Sample(sampled.TypeName(), samplerInfo);
        }

        public void Dispose()
        {}
    }
}
#endif