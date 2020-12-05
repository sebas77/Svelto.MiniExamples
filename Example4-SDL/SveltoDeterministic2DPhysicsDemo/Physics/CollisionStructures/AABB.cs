using SveltoDeterministic2DPhysicsDemo.Maths;

namespace SveltoDeterministic2DPhysicsDemo.Physics.CollisionStructures
{
    public readonly struct AABB
    {
        public static bool AABBvsAABB(AABB a, AABB b)
        {
            if (a.Max.X < b.Min.X || a.Min.X > b.Max.X)
                return false;

            if (a.Max.Y < b.Min.Y || a.Min.Y > b.Max.Y)
                return false;

            return true;
        }

        public static AABB From(FixedPointVector2 min, FixedPointVector2 max) { return new AABB(min, max); }

        public readonly FixedPointVector2 Min;
        public readonly FixedPointVector2 Max;

        AABB(FixedPointVector2 min, FixedPointVector2 max)
        {
            Min = min;
            Max = max;
        }
    }
}