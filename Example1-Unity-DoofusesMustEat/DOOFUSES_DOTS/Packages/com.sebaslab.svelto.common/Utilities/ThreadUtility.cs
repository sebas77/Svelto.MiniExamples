#if NETFX_CORE
using System.Threading.Tasks;
#endif
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Svelto.Utilities
{
    public static class ThreadUtility
    {
        public static uint processorNumber => (uint)Environment.ProcessorCount;
        public static string currentThreadName => Thread.CurrentThread.Name;
        public static int currentThreadId => Thread.CurrentThread.ManagedThreadId;
        /// <summary>
        /// The main difference between Yield and Sleep(0) is that Yield doesn't allow a switch of context
        /// that is the core is given to a thread that is already running on that core. Sleep(0) may cause
        /// a context switch, yielding the processor to a thread from ANY process. Thread.Yield yields
        /// the processor to any thread associated with the current core.
        /// Remember that sleep(1) does FORCE a context switch instead, while with sleep(0) is only if required.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Yield()
        {
#if NETFX_CORE && !NET_STANDARD_2_0 && !NETSTANDARD2_0
            #error Svelto doesn't support UWP without NET_STANDARD_2_0 support
#endif
            Thread.Yield();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TakeItEasy()
        {
#if NETFX_CORE && !NET_STANDARD_2_0 && !NETSTANDARD2_0
            #error Svelto doesn't support UWP without NET_STANDARD_2_0 support
#endif
            Thread.Sleep(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Relax()
        {
#if NETFX_CORE && !NET_STANDARD_2_0 && !NETSTANDARD2_0
            #error Svelto doesn't support UWP without NET_STANDARD_2_0 support
#endif
            Thread.Sleep(0);
        }

        /// <summary>
        /// Yield the thread every so often. Remember I don't do Spin because
        /// Spin is equivalent of while(); and I don't see the point of it in most
        /// of the expected scenarios. I don't do Sleep(0) because it can still
        /// cause a context switch;
        /// </summary>
        /// <param name="quickIterations">will be increment by 1</param>
        /// <param name="frequency">must be power of 2</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Wait(ref int quickIterations, int frequency = 256)
        {
            if ((quickIterations++ & (frequency - 1)) == 0)
                Yield();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LongestWaitLeft(float timeInTicks, ref int quickIterations, in Stopwatch watch, int frequency = 256)
        {
            if (timeInTicks - watch.ElapsedTicks <= 16_000)
            {
                if ((quickIterations++ & (frequency - 1)) == 0)
                    Relax();
                else
                    Yield();
            }
            else
                TakeItEasy();
        }

        /// DO NOT TOUCH THE NUMBERS, THEY ARE THE BEST BALANCE BETWEEN CPU OCCUPATION AND RESUME SPEED
        /// DO NOT ADD THREAD.SLEEP(1) it KILLS THE RESUME
       [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LongWaitLeft(float timeInTicks, ref int quickIterations, in Stopwatch watch, int frequency = 256)
        {
            if (timeInTicks - watch.ElapsedTicks <= 16_000)
            {
                if ((quickIterations++ & (frequency - 1)) == 0)
                    Yield();
            }
            else
            {
                if ((quickIterations++ & ((frequency << 3) - 1)) == 0)
                    Relax();
                else
                    Yield();
            }
        }
        
        /// DO NOT TOUCH THE NUMBERS, THEY ARE THE BEST BALANCE BETWEEN CPU OCCUPATION AND RESUME SPEED
        /// DO NOT ADD THREAD.SLEEP(1) it KILLS THE RESUME
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LongWait(ref int quickIterations, in Stopwatch watch, int frequency = 256)
        {
            if (watch.ElapsedTicks < 16_000)
            {
                if ((quickIterations++ & (frequency - 1)) == 0)
                    Yield();
            }
            else
            {
                if ((quickIterations++ & ((frequency << 3) - 1)) == 0)
                    Relax();
                else
                    Yield();
            }
        }
        
        public static void SleepWithOneEyeOpen(float waitTimeMs, Stopwatch stopwatch, int frequency = 64)
        {
            int quickIterations = 0;
            var timeInTicks = waitTimeMs * (Stopwatch.Frequency / 1000);
            
            stopwatch.Restart();
            while (stopwatch.ElapsedTicks < timeInTicks)
                LongestWaitLeft(timeInTicks, ref quickIterations, stopwatch, frequency);
        }
        
        public static void MemoryBarrier() => Interlocked.MemoryBarrier();
    }
}