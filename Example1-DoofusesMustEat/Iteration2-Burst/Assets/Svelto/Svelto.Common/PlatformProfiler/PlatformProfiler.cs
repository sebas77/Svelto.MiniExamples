using System;

namespace Svelto.Common
{
    public interface IPlatformProfiler<out T>: IDisposable where T:IDisposable
    {
        T Sample(string samplerName, string samplerInfo = null);
        T Sample<W>(W sampled, string samplerInfo = null);
    }
    
#if !ENABLE_PLATFORM_PROFILER
    public struct DisposableSampler : IDisposable
    {
        public void Dispose()
        {}
    }
    
    public struct PlatformProfilerMT : IPlatformProfiler<DisposableSampler>
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

    public struct PlatformProfiler: IPlatformProfiler<DisposableSampler>
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