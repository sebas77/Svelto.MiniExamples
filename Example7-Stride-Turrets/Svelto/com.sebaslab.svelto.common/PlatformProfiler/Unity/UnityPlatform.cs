#if UNITY_2018_3_OR_NEWER && ENABLE_PLATFORM_PROFILER
using System;
using Svelto.Common.Internal;
using Unity.Profiling;

namespace Svelto.Common
{
    public struct DisposableSampler : IDisposable
    {
        ProfilerMarker _marker;

        public DisposableSampler(ProfilerMarker marker)
        {
            _marker = marker;
            _marker.Begin();
        }

#if DISABLE_CHECKS
		[Conditional("__NEVER_DEFINED__")]
#endif
        public void Dispose()
        {
            _marker.End();
        }
    }

    public struct PlatformProfilerMT : IPlatformProfiler
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

    public struct PlatformProfiler : IPlatformProfiler
    {
        readonly ProfilerMarker? maker;

        public PlatformProfiler(string info)
        {
            maker = new ProfilerMarker(info);
            maker.Value.Begin();
        }

        public DisposableSampler Sample(string samplerName, string samplerInfo = null)
        {
#if !PROFILE_SVELTO
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
        {
            maker?.End();
        }

        public void Pause()
        {
            maker?.End();
        }

        public void Resume()
        {
            maker.Value.Begin();
        }

        public PauseProfiler Yield() { return new PauseProfiler(maker.Value); }
    }

    public readonly struct PauseProfiler : IDisposable
    {
        public PauseProfiler(ProfilerMarker maker)
        {
            _maker = maker;
            _maker.End();
        }

        public void Dispose()
        {
            _maker.Begin();
        }
        
        readonly ProfilerMarker _maker;

    }
}
#endif