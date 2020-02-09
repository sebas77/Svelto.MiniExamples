using System;
using System.Diagnostics;
using System.Threading;

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
    public struct StandardProfiler: IPlatformProfiler
    {
        static readonly ThreadLocal<Stopwatch> _stopwatch = new ThreadLocal<Stopwatch>(() => new Stopwatch());

        readonly long _startTime;
        readonly string _info;

        public StandardProfiler(string info)
        {
            _stopwatch.Value.Start();
            _startTime = _stopwatch.Value.ElapsedTicks;
            _info = info;
        }

        public DisposableSampler Sample(string samplerName, string samplerInfo = null)
        {
            return new DisposableSampler();
        }
        
        public DisposableSampler Sample<T>(T samplerName, string samplerInfo = null)
        {
            return new DisposableSampler();
        }

        public void Dispose()
        {
            _stopwatch.Value.Stop();
            var stopwatchElapsedTicks = (_stopwatch.Value.ElapsedTicks - _startTime);
            Svelto.Console.LogDebug(_info,  stopwatchElapsedTicks / 10000);
        }
    }
}