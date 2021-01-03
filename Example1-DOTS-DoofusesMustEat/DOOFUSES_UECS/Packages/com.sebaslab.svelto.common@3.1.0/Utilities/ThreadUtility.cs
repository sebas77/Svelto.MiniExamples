#if NETFX_CORE
using System.Threading.Tasks;
#endif
using System.Threading;

namespace Svelto.Utilities
{
    public static class ThreadUtility
    {
        /// <summary>
        /// The main difference between Yield and Sleep(0) is that Yield doesn't allow a switch of context
        /// that is the core is given to a thread that is already running on that core. Sleep(0) may cause
        /// a context switch
        /// to another thread.  
        /// </summary>
        public static void Yield()
        {
#if NETFX_CORE && !NET_STANDARD_2_0 && !NETSTANDARD2_0
            #error Svelto doesn't support UWP without NET_STANDARD_2_0 support
#endif
            Thread.Yield(); 
        }

        public static void TakeItEasy()
        {
#if NETFX_CORE && !NET_STANDARD_2_0 && !NETSTANDARD2_0
            #error Svelto doesn't support UWP without NET_STANDARD_2_0 support
#endif
            Thread.Sleep(1); 
        }

        /// <summary>
        /// Yield the thread every so often
        /// </summary>
        /// <param name="quickIterations">will be increment by 1</param>
        /// <param name="frequency">must be power of 2</param>
        public static void Wait(ref int quickIterations, int frequency = 256)
        {
            if ((quickIterations++ & (frequency - 1)) == 0)
                Yield();
        }
    }
}