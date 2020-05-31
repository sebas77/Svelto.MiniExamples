using System;
using System.Diagnostics;

namespace Svelto.Common
{
    public interface IPlatformProfiler: IDisposable
    {
        DisposableSampler Sample(string samplerName, string samplerInfo = null);
        DisposableSampler Sample<W>(W sampled, string samplerInfo = null);
    }
    
#if !ENABLE_PLATFORM_PROFILER
    public struct DisposableSampler : IDisposable
    {
        public void Dispose()
        {}
    }
    
    public struct PlatformProfilerMT : IPlatformProfiler
    {
        public PlatformProfilerMT(string info)
        {}
        
        public DisposableSampler Sample(string samplerName, string samplerInfo = null)
        {
            return new DisposableSampler();
        }

        public DisposableSampler Sample<T>(T sampled, string samplerInfo = null)
        {
            return new DisposableSampler();
        }

        public void Dispose()
        {}
    }

    public struct PlatformProfiler: IPlatformProfiler
    {
        public PlatformProfiler(string info)
        {}

        public DisposableSampler Sample(string samplerName, string samplerInfo = null)
        {
            return new DisposableSampler();
        }
        
        public DisposableSampler Sample<T>(T samplerName, string samplerInfo = null)
        {
            return new DisposableSampler();
        }

        public void Dispose()
        {}
    }
#endif
}