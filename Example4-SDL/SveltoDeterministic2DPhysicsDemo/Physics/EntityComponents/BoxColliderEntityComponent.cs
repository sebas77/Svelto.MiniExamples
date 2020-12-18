using Svelto.ECS;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct BoxColliderEntityComponent : IEntityComponent
    {
        public static BoxColliderEntityComponent From(FixedPointVector2 size, FixedPointVector2 center)
        {
            return new BoxColliderEntityComponent(size, center);
        }

        internal FixedPointVector2 Size;
        internal FixedPointVector2 Center;

        BoxColliderEntityComponent(in FixedPointVector2 size, in FixedPointVector2 center)
        {
            Size   = size;
            Center = center;
        }
    }

    public static class BoxColliderEntityComponentUtility
    {
        public static AABB ToAABB(in this BoxColliderEntityComponent component, FixedPointVector2 point)
        {
            return AABB.From(point - component.Center - component.Size, point - component.Center + component.Size);
        }
    }
}