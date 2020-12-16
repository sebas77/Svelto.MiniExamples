using System.Collections.Generic;

namespace FixedMaths
{
    public class FixedPointEqualityComparer : IEqualityComparer<FixedPoint>
    {
        public static FixedPointEqualityComparer Instance = new FixedPointEqualityComparer();

        public bool Equals(FixedPoint x, FixedPoint y) { return x.Value == y.Value; }

        public int GetHashCode(FixedPoint obj) { return obj.Value; }
    }
}