using System;
using System.Runtime.CompilerServices;

namespace FixedMaths
{
    /// <summary>
    ///     Implements a fixed point number, based on https://en.wikipedia.org/wiki/Q_%28number_format%29
    /// </summary>
    public readonly struct FixedPoint:IEquatable<FixedPoint>
    {
        public const short Q = 12;
        const        int   K = 1 << (Q - 1);

        static readonly int
            IntegerMaxValue = (int) Math.Pow(2, sizeof(int) * 8 - 1 - Q) - 1; // -1 is because int is singed

        static readonly int
            IntegerMinValue = -(int) Math.Pow(2, sizeof(int) * 8 - 1 - Q); // -1 is because int is singed

        static readonly int FractionalMaxValue = (int) Math.Pow(2, Q) - 1;

        public static FixedPoint MaxValue    = new FixedPoint(IntegerMaxValue << Q);
        public static FixedPoint MinValue    = new FixedPoint(IntegerMinValue << Q);
        public static FixedPoint NaN         = new FixedPoint(0, true);
        public static FixedPoint Pi          = new FixedPoint(12868);
        public static FixedPoint NegativeOne = new FixedPoint(-1 << Q);
        public static FixedPoint Zero        = new FixedPoint(0);
        public static FixedPoint Half        = new FixedPoint((1 << Q) / 2);
        public static FixedPoint One         = new FixedPoint(1 << Q);
        public static FixedPoint Two         = new FixedPoint(2 << Q);
        public static FixedPoint Three       = new FixedPoint(3 << Q);
        public static FixedPoint Four        = new FixedPoint(4 << Q);

        public static FixedPoint
            Kludge = new FixedPoint(1); // Used to avoid division by zero errors, by adding a tiny tiny bit of a number

        public static int ConvertToInteger(FixedPoint fp) { return fp.Value >> Q; }

        public static float ConvertToFloat(FixedPoint fp)
        {
            if (fp._isNaN)
                return float.NaN;

            var integer   = fp.Value >> Q;
            var remainder = fp.Value - (integer << Q);

            var fractional = (float) remainder / FractionalMaxValue;

            return integer + fractional;
        }

        public static double ConvertToDouble(FixedPoint fp)
        {
            if (fp._isNaN)
                return double.NaN;

            var integer   = fp.Value >> Q;
            var remainder = fp.Value - (integer << Q);

            var fractional = (float) remainder / FractionalMaxValue;

            return integer + fractional;
        }

        public static FixedPoint From(int value)
        {
            value = Math.Clamp(value, IntegerMinValue, IntegerMaxValue);

            return new FixedPoint(value << Q);
        }

        /// <summary>
        ///     Only call this if you want to avoid any automatic conversion.
        /// </summary>
        /// <param name="value">Contains the exact internal value</param>
        /// <returns>FixedPoint based on the passed value</returns>
        public static FixedPoint FromExplicit(int value) { return new FixedPoint(value); }

        public static FixedPoint From(float value)
        {
            if (float.IsNaN(value))
                return NaN;

            value = Math.Clamp(value, IntegerMinValue, IntegerMaxValue);

            var integer    = (int) value;
            var fractional = (int) Math.Round(FractionalMaxValue * (value - integer));

            var result = integer << Q;
            result += Math.Min(fractional, FractionalMaxValue);

            return new FixedPoint(result);
        }

        public static FixedPoint From(double value)
        {
            if (double.IsNaN(value))
                return NaN;

            value = Math.Clamp(value, IntegerMinValue, IntegerMaxValue);

            var integer    = (int) value;
            var fractional = (int) Math.Round(FractionalMaxValue * (value - integer));

            var result = integer << Q;
            result += Math.Min(fractional, FractionalMaxValue);

            return new FixedPoint(result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint operator +(FixedPoint fp1, FixedPoint fp2)
        {
            if (fp1._isNaN || fp2._isNaN)
                return NaN;

            var value = (fp1.Value >> Q) + (fp2.Value >> Q);

            if (value >= IntegerMaxValue)
                return MaxValue;

            if (value <= IntegerMinValue)
                return MinValue;

            return new FixedPoint(fp1.Value + fp2.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint operator -(FixedPoint fp1, FixedPoint fp2)
        {
            if (fp1._isNaN || fp2._isNaN)
                return NaN;

            var value = (fp1.Value >> Q) - (fp2.Value >> Q);

            if (value >= IntegerMaxValue)
                return MaxValue;

            if (value <= IntegerMinValue)
                return MinValue;

            return new FixedPoint(fp1.Value - fp2.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint operator -(FixedPoint fp) { return fp * NegativeOne; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint operator *(FixedPoint fp1, FixedPoint fp2)
        {
            if (fp1._isNaN || fp2._isNaN)
                return NaN;

            var temp = (long) fp1.Value * fp2.Value;

            var result = temp + ((temp & K) << 1);
            result >>= Q;

            if (result > IntegerMaxValue << Q)
                return MaxValue;

            if (result < IntegerMinValue << Q)
                return MinValue;

            return new FixedPoint((int) result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint operator /(FixedPoint fp1, FixedPoint fp2)
        {
            if (fp1._isNaN || fp2._isNaN)
                return NaN;

            var temp = (long) fp1.Value << Q;

            if (temp >= 0 && fp2.Value >= 0 || temp < 0 && fp2.Value < 0)
                temp += fp2.Value / 2;
            else
                temp -= fp2.Value / 2;

            return new FixedPoint((int) (temp / fp2.Value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(FixedPoint fp1, FixedPoint fp2) { return fp1.Value > fp2.Value; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(FixedPoint fp1, FixedPoint fp2) { return fp1.Value < fp2.Value; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(FixedPoint fp1, FixedPoint fp2) { return fp1.Value >= fp2.Value; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(FixedPoint fp1, FixedPoint fp2) { return fp1.Value <= fp2.Value; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint operator %(FixedPoint fp1, FixedPoint fp2)
        {
            return new FixedPoint(fp1.Value % fp2.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint operator ^(FixedPoint fp1, FixedPoint fp2)
        {
            return From(ConvertToInteger(fp1) ^ ConvertToInteger(fp2));
        }

        /// <summary>
        ///     Contains a signed 20.12 formatted number, allows +/- 524288.999755859375
        ///     http://netwinder.osuosl.org/pub/netwinder/docs/nw/fix1FAQ.html Contains details on how this number was determined
        /// </summary>
        public int Value { get; }

        readonly bool _isNaN;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        FixedPoint(int value, bool isNaN = false)
        {
            Value  = value;
            _isNaN = isNaN;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPoint Squared() { return this * this; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(FixedPoint other) { return Value == other.Value && _isNaN == other._isNaN; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() { return HashCode.Combine(Value, _isNaN); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() { return _isNaN ? "NaN" : $"{ConvertToFloat(this):0.0000}"; }
    }
}