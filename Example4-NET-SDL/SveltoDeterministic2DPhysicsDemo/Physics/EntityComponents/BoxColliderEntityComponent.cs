using Svelto.ECS;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct BoxColliderEntityComponent : IEntityComponent
    {
        internal FixedPointVector2 Size;
        internal FixedPointVector2 Center;

        public BoxColliderEntityComponent(in FixedPointVector2 size, in FixedPointVector2 center)
        {
            Size   = size;
            Center = center;
        }
    }

    public static class BoxColliderEntityComponentUtility
    {
        public static AABB ToAABB(in this BoxColliderEntityComponent component, in FixedPointVector2 point)
        {
            return new AABB(point - component.Center - component.Size, point - component.Center + component.Size);
        }
    }
}