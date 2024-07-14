using System;
using System.Diagnostics;
using System.Threading;

namespace Svelto.Common
{
    public readonly struct StandardProfiler : IDisposable
    {
        static readonly ThreadLocal<Stopwatch> _stopwatch = new ThreadLocal<Stopwatch>(() => new Stopwatch());

        readonly string _info;

        static StandardProfiler()
        {
            _stopwatch.Value.Start();
        }

        //It doesn't make any sense to profile with two different patterns, either it's trough the main struct
        //or through the Sample method. If both are provided, Sample is basically never used.
        public StandardProfiler(string info)
        {
            _info = info;
        }

        public double elapsed => _stopwatch.Value.ElapsedTicks / Stopwatch.Frequency;

        public StandardDisposableSampler Sample()
        {
            return new StandardDisposableSampler(_info, _stopwatch.Value);
        }

        public StandardDisposableSampler Sample(string samplerName)
        {
            return new StandardDisposableSampler(_info.FastConcat(samplerName), _stopwatch.Value);
        }

        public StandardDisposableSampler Sample<T>(T samplerName)
        {
            return new StandardDisposableSampler(_info.FastConcat(samplerName.ToString()), _stopwatch.Value);
        }

        public void Dispose()
        { }
    }

    public readonly struct StandardDisposableSampler : IDisposable
    {
        readonly Stopwatch _watch;
        readonly long      _startTime;
        readonly string    _samplerName;

        public StandardDisposableSampler(string samplerName, Stopwatch stopwatch)
        {
            _watch       = stopwatch;
            _startTime   = stopwatch.ElapsedTicks;
            _samplerName = samplerName;
        }

        public void Dispose()
        {
            var stopwatchElapsedTicks = (_watch.ElapsedTicks - _startTime);

            Svelto.Console.Log(_samplerName.FastConcat(" -> ").FastConcat(stopwatchElapsedTicks / 10000.0).FastConcat(" ms"));
        }
    }
}