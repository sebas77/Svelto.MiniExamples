namespace FixedMaths
{
    public static class MathFixedPointConstants
    {
        public const int TrigonometricSteps           = 4096;
        public const int HyperbolicHighResolutionStep = 1;
        public const int SqrtHighResolutionStep       = 1;

        public static readonly int HyperbolicHighResolution       = FixedPoint.Zero.Value;
        public static readonly int HyperbolicMediumResolution     = FixedPoint.From(100).Value;
        public static readonly int HyperbolicLowResolution        = FixedPoint.From(1000).Value;
        public static readonly int HyperbolicMediumResolutionStep = FixedPoint.One.Value;
        public static readonly int HyperbolicLowResolutionStep    = FixedPoint.Two.Value;

        public static readonly int SqrtHighResolution       = FixedPoint.Zero.Value;
        public static readonly int SqrtMediumResolution     = FixedPoint.From(100).Value;
        public static readonly int SqrtLowResolution        = FixedPoint.From(1000).Value;
        public static readonly int SqrtMediumResolutionStep = FixedPoint.One.Value;
        public static readonly int SqrtLowResolutionStep    = FixedPoint.Two.Value;
    }
}