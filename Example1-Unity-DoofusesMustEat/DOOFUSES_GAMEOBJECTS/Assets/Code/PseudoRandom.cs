using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public struct ThreadLocalRandom: IDisposable
{
    [NativeDisableParallelForRestriction]
    NativeArray<Random> random;
    
    public ThreadLocalRandom(int size)
    {
        Random _random = new Random((uint)DateTime.Now.Ticks);
        
        random = new NativeArray<Random>(size, Allocator.Persistent);
        for (var i = 0; i < 256; i++)
            random[i] = new Random(_random.NextUInt());

    }

    /// <summary>See <see cref="Unity.Mathematics.Random.Next(int, int)" /></summary>
    [BurstCompile]
    public int Next(int minValue, int maxValue, int threadIndex)
    {
        unsafe
        {
            return UnsafeUtility.ArrayElementAsRef<Random>(random.GetUnsafePtr(), threadIndex).NextInt(minValue, maxValue);
        }
    }

    /// <summary>
    ///     REturns a random double within a specified range.
    /// </summary>
    [BurstCompile]
    public double NextDouble(float minValue, float maxValue, int threadIndex)
    {
        unsafe
        {
            return UnsafeUtility.ArrayElementAsRef<Random>(random.GetUnsafePtr(), threadIndex).NextFloat(minValue, maxValue);
        }
    }

    public void Dispose()
    {
        random.Dispose();
    }
}