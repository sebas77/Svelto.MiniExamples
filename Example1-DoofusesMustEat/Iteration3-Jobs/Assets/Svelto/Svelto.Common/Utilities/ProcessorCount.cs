using System;

namespace Svelto.Common
{
    public static class ProcessorCount
    {
        static readonly int processorCount = Environment.ProcessorCount;

        public static int Batch(uint length)
        {
            return (int) Math.Max((int) (length / (processorCount)), 1);
        }
    }
}