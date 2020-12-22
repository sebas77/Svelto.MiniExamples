using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures
{
    public readonly struct AABB
    {
        public readonly FixedPointVector2 Min;
        public readonly FixedPointVector2 Max;

        public AABB(FixedPointVector2 min, FixedPointVector2 max)
        {
            Min = min;
            Max = max;
        }
    }
}