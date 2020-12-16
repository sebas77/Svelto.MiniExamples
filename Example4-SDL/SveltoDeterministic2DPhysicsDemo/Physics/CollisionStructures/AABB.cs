using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures
{
    public readonly struct AABB
    {
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