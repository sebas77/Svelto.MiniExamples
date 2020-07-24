using System;
using System.Diagnostics;
using System.Threading;

namespace Svelto.Common
{
    public readonly struct StandardProfiler : IDisposable
    {
        static readonly ThreadLocal<Stopwatch> _stopwatch = new ThreadLocal<Stopwatch>(() => new Stopwatch());

        readonly long   _startTime;
        readonly string _info;

        public StandardProfiler(string info)
        {
            _info = info;
            _stopwatch.Value.Start();
            _startTime = _stopwatch.Value.ElapsedTicks;
        }

        public StandardDisposableSampler Sample(string samplerName, string samplerInfo = null)
        {
            return new StandardDisposableSampler(samplerName, _stopwatch.Value);
        }

        public StandardDisposableSampler Sample<T>(T samplerName, string samplerInfo = null)
        {
            return new StandardDisposableSampler(samplerName.ToString(), _stopwatch.Value);
        }

        public void Dispose()
        {
            _stopwatch.Value.Stop();
            var stopwatchElapsedTicks = (_stopwatch.Value.ElapsedTicks - _startTime);
            Svelto.Console.Log(_info.FastConcat(" -> ").FastConcat(stopwatchElapsedTicks / 10000.0));
        }
    }

    public class StandardDisposableSampler : IDisposable
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
            Svelto.Console.Log(_samplerName.FastConcat(" -> ").FastConcat(stopwatchElapsedTicks / 10000.0));
        }
    }
}