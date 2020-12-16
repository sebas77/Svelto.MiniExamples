using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FixedMaths.Data;

namespace FixedMaths
{
    public static class MathFixedPoint
    {
        static readonly FixedPoint TrigonometricSteps = FixedPoint.From(MathFixedPointConstants.TrigonometricSteps);

        static readonly FixedPoint HyperbolicMediumResolution =
            FixedPoint.FromExplicit(MathFixedPointConstants.HyperbolicMediumResolution);

        static readonly FixedPoint HyperbolicLowResolution =
            FixedPoint.FromExplicit(MathFixedPointConstants.HyperbolicLowResolution);

        static readonly FixedPoint HyperbolicHighResolutionStep =
            FixedPoint.FromExplicit(MathFixedPointConstants.HyperbolicHighResolutionStep);

        static readonly FixedPoint HyperbolicMediumResolutionStep =
            FixedPoint.FromExplicit(MathFixedPointConstants.HyperbolicMediumResolutionStep);

        static readonly FixedPoint HyperbolicLowResolutionStep =
            FixedPoint.FromExplicit(MathFixedPointConstants.HyperbolicLowResolutionStep);

        static readonly FixedPoint SqrtMediumResolution =
            FixedPoint.FromExplicit(MathFixedPointConstants.SqrtMediumResolution);

        static readonly FixedPoint SqrtLowResolution =
            FixedPoint.FromExplicit(MathFixedPointConstants.SqrtLowResolution);

        static readonly FixedPoint SqrtHighResolutionStep =
            FixedPoint.FromExplicit(MathFixedPointConstants.SqrtHighResolutionStep);

        static readonly FixedPoint SqrtMediumResolutionStep =
            FixedPoint.FromExplicit(MathFixedPointConstants.SqrtMediumResolutionStep);

        static readonly FixedPoint SqrtLowResolutionStep =
            FixedPoint.FromExplicit(MathFixedPointConstants.SqrtLowResolutionStep);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Abs(FixedPoint fp)
        {
            if (fp >= FixedPoint.Zero)
                return fp;

            return fp * FixedPoint.NegativeOne;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Acos(FixedPoint fp)
        {
            var data = ProcessedTableService.Instance.AcosData;

            if (!data.ContainsKey(fp.Value))
                return FixedPoint.NaN;

            return data[fp.Value];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Acosh(FixedPoint fp)
        {
            return HyperbolicLookup(fp, ProcessedTableService.Instance.AcoshData, FixedPoint.One);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Asin(FixedPoint fp)
        {
            if (!ProcessedTableService.Instance.AsinData.ContainsKey(fp.Value))
                return FixedPoint.NaN;

            return ProcessedTableService.Instance.AsinData[fp.Value];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Asinh(FixedPoint fp)
        {
            return HyperbolicLookup(fp, ProcessedTableService.Instance.AsinhData, FixedPoint.One);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Atan(FixedPoint fp)
        {
            if (!ProcessedTableService.Instance.AtanData.ContainsKey(fp.Value))
                return FixedPoint.NaN;

            return ProcessedTableService.Instance.AtanData[fp.Value];
        }

        // Copied from https://dspguru.com/dsp/tricks/fixed-point-atan2-with-self-normalization/
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Atan2(FixedPoint y, FixedPoint x)
        {
            if (y.Equals(FixedPoint.Zero) && x.Equals(FixedPoint.Zero))
                return FixedPoint.Zero;

            var coefficient1 = FixedPoint.Pi / FixedPoint.Four;
            var coefficient2 = FixedPoint.Three * coefficient1;
            var absY         = Abs(y) + FixedPoint.Kludge; // + 1e-10      // kludge to prevent 0/0 condition

            FixedPoint angle;
            if (x >= FixedPoint.Zero)
            {
                var r = (x - absY) / (x + absY);
                angle = coefficient1 - coefficient1 * r;
            }
            else
            {
                var r = (x + absY) / (absY - x);
                angle = coefficient2 - coefficient1 * r;
            }

            if (y < FixedPoint.Zero)
                return angle * FixedPoint.NegativeOne; // negate if in quad III or IV

            return angle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Atanh(FixedPoint fp)
        {
            if (!ProcessedTableService.Instance.AtanhData.ContainsKey(fp.Value))
                return FixedPoint.NaN;

            return ProcessedTableService.Instance.AtanhData[fp.Value];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Ceiling(FixedPoint fp)
        {
            var integer   = fp.Value >> FixedPoint.Q;
            var remainder = fp.Value - (integer << FixedPoint.Q);

            return remainder > 0
                ? FixedPoint.FromExplicit((integer + 1) << FixedPoint.Q)
                : FixedPoint.FromExplicit(integer << FixedPoint.Q);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Clamp(FixedPoint value, FixedPoint min, FixedPoint max)
        {
            return FixedPoint.FromExplicit(Math.Clamp(value.Value, min.Value, max.Value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint ClampMagnitude(FixedPointVector2 v, FixedPoint clamp)
        {
            return Sqrt(Clamp(FixedPointVector2.Dot(v, v), -clamp, clamp));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Cos(FixedPoint fp)
        {
            return TrigonometricLookup(fp, ProcessedTableService.Instance.CosData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Cosh(FixedPoint fp)
        {
            return HyperbolicLookup(Abs(fp), ProcessedTableService.Instance.CoshData, FixedPoint.Zero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Floor(FixedPoint fp)
        {
            return FixedPoint.FromExplicit((fp.Value >> FixedPoint.Q) << FixedPoint.Q);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Magnitude(FixedPointVector2 v) { return Sqrt(FixedPointVector2.Dot(v, v)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Max(FixedPoint a, FixedPoint b) { return a >= b ? a : b; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Min(FixedPoint a, FixedPoint b) { return a < b ? a : b; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Round(FixedPoint fp)
        {
            var integer   = fp.Value >> FixedPoint.Q;
            var remainder = fp.Value - (integer << FixedPoint.Q);

            return remainder >= FixedPoint.Half.Value
                ? FixedPoint.FromExplicit((integer + 1) << FixedPoint.Q)
                : FixedPoint.FromExplicit(integer << FixedPoint.Q);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint RoundToNearestStep(FixedPoint fp, FixedPoint step)
        {
            var floored = Floor(fp / step) * step;

            return fp - floored > step / FixedPoint.Half ? floored + step : floored;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Sign(FixedPoint fp)
        {
            if (fp.Equals(FixedPoint.Zero))
                return FixedPoint.Zero;

            return fp > FixedPoint.Zero ? FixedPoint.One : FixedPoint.NegativeOne;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Sin(FixedPoint fp)
        {
            return TrigonometricLookup(fp, ProcessedTableService.Instance.SinData) * Sign(fp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Sinh(FixedPoint fp)
        {
            return HyperbolicLookup(Abs(fp), ProcessedTableService.Instance.SinhData, FixedPoint.Zero) * Sign(fp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Sqrt(FixedPoint fp)
        {
            var data = ProcessedTableService.Instance.SqrtData;

            if (fp < FixedPoint.Zero)
                return FixedPoint.NaN;

            if (fp > FixedPoint.Zero && fp < SqrtMediumResolution)
                return data[RoundToNearestStep(fp, SqrtHighResolutionStep)];

            if (fp >= SqrtMediumResolution && fp < SqrtLowResolution)
                return data[RoundToNearestStep(fp, SqrtMediumResolutionStep)];

            return data[RoundToNearestStep(fp, SqrtLowResolutionStep)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Sqrt2(FixedPoint fp)
        {
            var estimate = fp / FixedPoint.Two + FixedPoint.One;
            var n1       = (estimate + fp / estimate) / FixedPoint.Two;

            while (n1 < estimate)
            {
                estimate = n1;
                n1       = (estimate + fp / estimate) / FixedPoint.Two;
            }

            return estimate;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Tan(FixedPoint fp)
        {
            var sin = Sin(fp);
            var cos = Cos(fp);

            if (sin.Equals(FixedPoint.Zero) || cos.Equals(FixedPoint.Zero))
                return FixedPoint.Zero;

            return sin / cos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint Tanh(FixedPoint fp)
        {
            var sinh = Sinh(fp);
            var cosh = Cosh(fp);

            if (sinh.Equals(FixedPoint.Zero) || cosh.Equals(FixedPoint.Zero))
                return FixedPoint.Zero;

            return sinh / cosh;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static FixedPoint TrigonometricLookup(FixedPoint fp, IReadOnlyDictionary<FixedPoint, FixedPoint> data)
        {
            var value = Round(Abs(fp) / FixedPoint.Pi * TrigonometricSteps);

            var flip = (Floor(value / TrigonometricSteps) % FixedPoint.Two).Equals(FixedPoint.Zero);

            var result = flip
                ? data[value % TrigonometricSteps]
                : data[value % TrigonometricSteps] * FixedPoint.NegativeOne;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static FixedPoint HyperbolicLookup
            (FixedPoint fp, IReadOnlyDictionary<FixedPoint, FixedPoint> data, FixedPoint nanIfLessThan)
        {
            if (fp < nanIfLessThan)
                return FixedPoint.NaN;

            if (fp > FixedPoint.Zero && fp < HyperbolicMediumResolution)
                return data[RoundToNearestStep(fp, HyperbolicHighResolutionStep)];

            if (fp >= HyperbolicMediumResolution && fp < HyperbolicLowResolution)
                return data[RoundToNearestStep(fp, HyperbolicMediumResolutionStep)];

            return data[RoundToNearestStep(fp, HyperbolicLowResolutionStep)];
        }
    }
}