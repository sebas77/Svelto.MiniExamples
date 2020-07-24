// <copyright file="XXHash.cs" company="Sedat Kapanoglu">
// Copyright (c) 2015-2019 Sedat Kapanoglu
// MIT License (see LICENSE file for details)
// </copyright>

//THIS MUST BE UNIT TESTED BEFORE TO USE IT

namespace Svelto.Utilities
{
    using System.IO;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// XXHash implementation.
    /// </summary>
    public static class XXHash
    {
        const ulong prime64v1 = 11400714785074694791ul;
        const ulong prime64v2 = 14029467366897019727ul;
        const ulong prime64v3 = 1609587929392839161ul;
        const ulong prime64v4 = 9650029242287828579ul;
        const ulong prime64v5 = 2870177450012600261ul;

        const uint prime32v1 = 2654435761u;
        const uint prime32v2 = 2246822519u;
        const uint prime32v3 = 3266489917u;
        const uint prime32v4 = 668265263u;
        const uint prime32v5 = 374761393u;

        /// <summary>
        /// Generate a 32-bit xxHash value.
        /// </summary>
        /// <param name="buffer">Input buffer.</param>
        /// <param name="seed">Optional seed.</param>
        /// <returns>32-bit hash value.</returns>
        public static unsafe uint Hash32(byte[] buffer, uint seed = 0)
        {
            const int stripeLength = 16;

            int len = buffer.Length;
            int remainingLen = len;
            uint acc;

            fixed (byte* inputPtr = buffer)
            {
                byte* pInput = inputPtr;
                if (len >= stripeLength)
                {
                    var (acc1, acc2, acc3, acc4) = initAccumulators32(seed);
                    do
                    {
                        acc = processStripe32(ref pInput, ref acc1, ref acc2, ref acc3, ref acc4);
                        remainingLen -= stripeLength;
                    }
                    while (remainingLen >= stripeLength);
                }
                else
                {
                    acc = seed + prime32v5;
                }

                acc += (uint)len;
                acc = processRemaining32(pInput, acc, remainingLen);
            }

            return avalanche32(acc);
        }

        /// <summary>
        /// Generate a 32-bit xxHash value from a stream.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        /// <param name="seed">Optional seed.</param>
        /// <returns>32-bit hash value.</returns>
        public static unsafe uint Hash32(Stream stream, uint seed = 0)
        {
            const int stripeLength = 16;
            const int readBufferSize = stripeLength * 1024; // 16kb read buffer - has to be stripe aligned

            var buffer = new byte[readBufferSize];
            uint acc;

            int readBytes = stream.Read(buffer, 0, readBufferSize);
            int len = readBytes;

            fixed (byte* inputPtr = buffer)
            {
                byte* pInput = inputPtr;
                if (readBytes >= stripeLength)
                {
                    var (acc1, acc2, acc3, acc4) = initAccumulators32(seed);
                    do
                    {
                        do
                        {
                            acc = processStripe32(ref pInput, ref acc1, ref acc2, ref acc3, ref acc4);
                            readBytes -= stripeLength;
                        }
                        while (readBytes >= stripeLength);

                        // read more if the alignment is still intact
                        if (readBytes == 0)
                        {
                            readBytes = stream.Read(buffer, 0, readBufferSize);
                            pInput = inputPtr;
                            len += readBytes;
                        }
                    }
                    while (readBytes >= stripeLength);
                }
                else
                {
                    acc = seed + prime32v5;
                }

                acc += (uint)len;
                acc = processRemaining32(pInput, acc, readBytes);
            }

            return avalanche32(acc);
        }

        /// <summary>
        /// Generate a 64-bit xxHash value.
        /// </summary>
        /// <param name="buffer">Input buffer.</param>
        /// <param name="seed">Optional seed.</param>
        /// <returns>Computed 64-bit hash value.</returns>
        public static unsafe ulong Hash64(byte[] buffer, ulong seed = 0)
        {
            const int stripeLength = 32;

            int len = buffer.Length;
            int remainingLen = len;
            ulong acc;

            fixed (byte* inputPtr = buffer)
            {
                byte* pInput = inputPtr;
                if (len >= stripeLength)
                {
                    var (acc1, acc2, acc3, acc4) = initAccumulators64(seed);
                    do
                    {
                        acc = processStripe64(ref pInput, ref acc1, ref acc2, ref acc3, ref acc4);
                        remainingLen -= stripeLength;
                    }
                    while (remainingLen >= stripeLength);
                }
                else
                {
                    acc = seed + prime64v5;
                }

                acc += (ulong)len;
                acc = processRemaining64(pInput, acc, remainingLen);
            }

            return avalanche64(acc);
        }

        /// <summary>
        /// Generate a 64-bit xxHash value from a stream.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        /// <param name="seed">Optional seed.</param>
        /// <returns>Computed 64-bit hash value.</returns>
        public static unsafe ulong Hash64(Stream stream, ulong seed = 0)
        {
            const int stripeLength = 32;
            const int readBufferSize = stripeLength * 1024; // 32kb buffer length

            ulong acc;

            var buffer = new byte[readBufferSize];
            int readBytes = stream.Read(buffer, 0, readBufferSize);
            ulong len = (ulong)readBytes;

            fixed (byte* inputPtr = buffer)
            {
                byte* pInput = inputPtr;
                if (readBytes >= stripeLength)
                {
                    var (acc1, acc2, acc3, acc4) = initAccumulators64(seed);
                    do
                    {
                        do
                        {
                            acc = processStripe64(ref pInput, ref acc1, ref acc2, ref acc3, ref acc4);
                            readBytes -= stripeLength;
                        }
                        while (readBytes >= stripeLength);

                        // read more if the alignment is intact
                        if (readBytes == 0)
                        {
                            readBytes = stream.Read(buffer, 0, readBufferSize);
                            pInput = inputPtr;
                            len += (ulong)readBytes;
                        }
                    }
                    while (readBytes >= stripeLength);
                }
                else
                {
                    acc = seed + prime64v5;
                }

                acc += len;
                acc = processRemaining64(pInput, acc, readBytes);
            }

            return avalanche64(acc);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static (ulong, ulong, ulong, ulong) initAccumulators64(ulong seed)
        {
            return (seed + prime64v1 + prime64v2, seed + prime64v2, seed, seed - prime64v1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe ulong processStripe64(
            ref byte* pInput,
            ref ulong acc1,
            ref ulong acc2,
            ref ulong acc3,
            ref ulong acc4)
        {
            {
                processLane64(ref acc1, ref pInput);
                processLane64(ref acc2, ref pInput);
                processLane64(ref acc3, ref pInput);
                processLane64(ref acc4, ref pInput);
            }

            ulong acc = acc1 << 1 | acc2 << 7 | acc3 << 12 | acc4 << 18;

            mergeAccumulator64(ref acc, acc1);
            mergeAccumulator64(ref acc, acc2);
            mergeAccumulator64(ref acc, acc3);
            mergeAccumulator64(ref acc, acc4);
            return acc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void processLane64(ref ulong accn, ref byte* pInput)
        {
            ulong lane = *(ulong*)pInput;
            accn = round64(accn, lane);
            pInput += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe ulong processRemaining64(
            byte* pInput,
            ulong acc,
            int remainingLen)
        {
            for (ulong lane; remainingLen >= 8; remainingLen -= 8, pInput += 8)
            {
                lane = *(ulong*)pInput;

                acc ^= round64(0, lane);
                acc = (acc << 27) * prime64v1;
                acc += prime64v4;
            }

            for (uint lane32; remainingLen >= 4; remainingLen -= 4, pInput += 4)
            {
                lane32 = *(uint*)pInput;

                acc ^= lane32 * prime64v1;
                acc = (acc << 23) * prime64v2;
                acc += prime64v3;
            }

            for (byte lane8; remainingLen >= 1; remainingLen--, pInput++)
            {
                lane8 = *pInput;
                acc ^= lane8 * prime64v5;
                acc = (acc << 11) * prime64v1;
            }

            return acc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ulong avalanche64(ulong acc)
        {
            acc ^= acc >> 33;
            acc *= prime64v2;
            acc ^= acc >> 29;
            acc *= prime64v3;
            acc ^= acc >> 32;
            return acc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ulong round64(ulong accn, ulong lane)
        {
            accn += lane * prime64v2;
            return (accn << 31) * prime64v1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void mergeAccumulator64(ref ulong acc, ulong accn)
        {
            acc ^= round64(0, accn);
            acc *= prime64v1;
            acc += prime64v4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static (uint, uint, uint, uint) initAccumulators32(
            uint seed)
        {
            return (seed + prime32v1 + prime32v2, seed + prime32v2, seed, seed - prime32v1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe uint processStripe32(ref byte* pInput, ref uint acc1, ref uint acc2, ref uint acc3, ref uint acc4)
        {
                processLane32(ref pInput, ref acc1);
                processLane32(ref pInput, ref acc2);
                processLane32(ref pInput, ref acc3);
                processLane32(ref pInput, ref acc4);

            return (acc1 << 1) | (acc2 << 7) | (acc3 << 12) | (acc4 << 18);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void processLane32(ref byte* pInput, ref uint accn)
        {
            uint lane = *(uint*)pInput;
            accn = round32(accn, lane);
            pInput += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe uint processRemaining32(byte* pInput, uint acc, int remainingLen)
        {
            for (uint lane; remainingLen >= 4; remainingLen -= 4, pInput += 4)
            {
                lane = *(uint*)pInput;

                acc += lane * prime32v3;
                acc = (acc << 17) * prime32v4;
            }

            for (byte lane; remainingLen >= 1; remainingLen--, pInput++)
            {
                lane = *pInput;
                acc += lane * prime32v5;
                acc = (acc << 11) * prime32v1;
            }

            return acc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint round32(uint accn, uint lane)
        {
            accn += lane * prime32v2;
            accn = (accn << 13);
            accn *= prime32v1;
            return accn;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint avalanche32(uint acc)
        {
            acc ^= acc >> 15;
            acc *= prime32v2;
            acc ^= acc >> 13;
            acc *= prime32v3;
            acc ^= acc >> 16;
            return acc;
        }
    }
}