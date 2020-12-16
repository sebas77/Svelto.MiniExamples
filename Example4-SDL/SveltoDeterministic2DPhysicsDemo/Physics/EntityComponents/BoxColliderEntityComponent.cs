using Svelto.ECS;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public readonly struct BoxColliderEntityComponent : IEntityComponent
    {
        public static BoxColliderEntityComponent From(FixedPointVector2 size, FixedPointVector2 center)
        {
            return new BoxColliderEntityComponent(size, center);
        }

        internal readonly FixedPointVector2 _size;
        internal readonly FixedPointVector2 _center;

        BoxColliderEntityComponent(in FixedPointVector2 size, in FixedPointVector2 center)
        {
            _size   = size;
            _center = center;
        }
    }

    public static class BoxColliderEntityComponentUtility
    {
        public static AABB ToAABB(in this BoxColliderEntityComponent component, FixedPointVector2 point)
        {
            return AABB.From(point - component._center - component._size, point - component._center + component._size);
        }
    }
}